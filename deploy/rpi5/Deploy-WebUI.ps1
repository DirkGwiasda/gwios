#Requires -Version 5.1
<#
.SYNOPSIS
    Baut die GwiOS WebUI als Container-Image und deployt sie auf den rpi5.

.DESCRIPTION
    Kann bei jedem Deploy erneut ausgeführt werden. Ablauf, Voraussetzungen und die
    benötigten Benutzer-Umgebungsvariablen stehen in README.md in diesem Ordner.

.PARAMETER SkipTests
    Überspringt "dotnet test" vor dem Bauen.
#>
[CmdletBinding()]
param(
    [switch] $SkipTests
)

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

$Server = 'tija@rpi5'
$ServerDirectory = '/home/tija/gwios-webui'
$KeysDirectory = '/mnt/ssd/gwios-webui/keys'
$ImageName = 'gwios-webui'
$HealthCheckUrl = 'https://webui.gwios.gwiasda.net/'
$HealthCheckTimeoutSeconds = 90

# Schlüssel in der .env auf dem Server -> Benutzer-Umgebungsvariable auf diesem Rechner. Es sind
# dieselben Variablen, mit denen die App in der Entwicklung läuft.
$SecretVariables = [ordered]@{
    'GWIOS_DB_CONNECTIONSTRING' = 'GwiOS.DB.Connectionstring'
    'GWIOS_WEBUI_CLIENT_SECRET' = 'Keycloak__ClientSecret'
}

$RepositoryRoot = (Resolve-Path (Join-Path $PSScriptRoot '..\..')).Path
$WebUIProject = Join-Path $RepositoryRoot 'src\GwiOS.WebUI\GwiOS.WebUI.csproj'
$Solution = Join-Path $RepositoryRoot 'src\GwiOS.slnx'
$Utf8WithoutBom = New-Object System.Text.UTF8Encoding $false

Add-Type -AssemblyName System.Net.Http

function Write-Step([string] $Message)
{
    Write-Host "==> $Message" -ForegroundColor Cyan
}

function Assert-ExitCode([string] $Action)
{
    if ($LASTEXITCODE -ne 0)
    {
        throw "$Action ist fehlgeschlagen (Exit-Code $LASTEXITCODE)."
    }
}

function Assert-ToolsAvailable
{
    foreach ($tool in 'dotnet', 'git', 'ssh', 'scp')
    {
        if (-not (Get-Command $tool -ErrorAction SilentlyContinue))
        {
            throw "'$tool' wurde nicht gefunden."
        }
    }
}

function Get-Secret([string] $VariableName)
{
    # Zuerst das Benutzerkonto: Ein Terminal, das vor einer Änderung geöffnet wurde, hat in seiner
    # Prozessumgebung noch den alten Wert. Der Prozess nur, falls es im Benutzerkonto keinen gibt.
    $value = [Environment]::GetEnvironmentVariable($VariableName, 'User')
    if ([string]::IsNullOrEmpty($value))
    {
        $value = [Environment]::GetEnvironmentVariable($VariableName, 'Process')
    }
    if ([string]::IsNullOrEmpty($value))
    {
        throw "Die Benutzer-Umgebungsvariable '$VariableName' ist nicht gesetzt - siehe deploy/rpi5/README.md."
    }
    # In der .env steht jeder Wert in einfachen Anführungszeichen, damit Compose nichts darin
    # ersetzt. Ein einfaches Anführungszeichen oder ein Zeilenumbruch lässt sich so nicht
    # abbilden, und Compose macht selbst dort aus "\\" ein "\" - der Wert käme verfälscht an.
    if ($value -match "['\\`r`n]")
    {
        throw "Der Wert von '$VariableName' enthält ein einfaches Anführungszeichen, einen Backslash oder einen Zeilenumbruch."
    }
    return $value
}

function New-EnvFileContent([string] $ImageTag)
{
    $lines = @(
        '# Erzeugt von deploy/rpi5/Deploy-WebUI.ps1 bei jedem Deploy - Änderungen hier gehen verloren.'
        "GWIOS_WEBUI_TAG='$ImageTag'"
        "GWIOS_WEBUI_KEYS_DIRECTORY='$KeysDirectory'"
    )
    foreach ($entry in $SecretVariables.GetEnumerator())
    {
        $lines += "$($entry.Key)='$(Get-Secret $entry.Value)'"
    }
    return ($lines -join "`n") + "`n"
}

function Get-ImageTag
{
    $commit = git -C $RepositoryRoot rev-parse --short HEAD
    Assert-ExitCode 'git rev-parse'
    $changes = git -C $RepositoryRoot status --porcelain
    Assert-ExitCode 'git status'
    if ($changes)
    {
        Write-Warning 'Das Arbeitsverzeichnis hat nicht committete Änderungen - sie werden mit deployt.'
        return "$commit-dirty"
    }
    return $commit
}

function Invoke-Tests
{
    dotnet test --solution $Solution
    Assert-ExitCode 'dotnet test'
}

function New-StagingDirectory
{
    $directory = Join-Path ([IO.Path]::GetTempPath()) 'gwios-webui-deploy'
    Remove-StagingDirectory $directory
    New-Item -ItemType Directory -Path $directory | Out-Null
    Copy-Item (Join-Path $PSScriptRoot 'docker-compose.yml') $directory
    # Git checkt Dateien unter Windows ggf. mit CRLF aus; bash auf dem Pi verlangt LF.
    $applyScript = [IO.File]::ReadAllText((Join-Path $PSScriptRoot 'apply.sh')) -replace "`r`n", "`n"
    [IO.File]::WriteAllText((Join-Path $directory 'apply.sh'), $applyScript, $Utf8WithoutBom)
    return $directory
}

function Remove-StagingDirectory([string] $Directory)
{
    if (Test-Path $Directory)
    {
        Remove-Item $Directory -Recurse -Force
    }
}

function New-ContainerImageArchive([string] $ImageTag, [string] $ArchivePath)
{
    # Das .NET SDK baut das Image selbst - ohne Dockerfile, ohne Docker und ohne Emulation.
    dotnet publish $WebUIProject -c Release --os linux --arch arm64 /t:PublishContainer `
        "-p:ContainerRepository=$ImageName" `
        "-p:ContainerImageTag=$ImageTag" `
        "-p:ContainerArchiveOutputPath=$ArchivePath"
    Assert-ExitCode 'dotnet publish'
}

function Invoke-OnServer([string] $Command)
{
    ssh -o BatchMode=yes $Server $Command
    Assert-ExitCode "ssh $Server '$Command'"
}

function Copy-StagingDirectoryToServer([string] $Directory)
{
    # Relative Namen, damit scp einen Laufwerksbuchstaben nicht für einen Hostnamen hält.
    Push-Location $Directory
    try
    {
        $files = @(Get-ChildItem -File -Name)
        scp -o BatchMode=yes -q $files "${Server}:$ServerDirectory/"
        Assert-ExitCode 'scp'
    }
    finally
    {
        Pop-Location
    }
}

function Install-OnServer([string] $EnvFileContent, [string] $ArchiveName)
{
    # Die .env geht über stdin, nicht über die Kommandozeile - siehe apply.sh. Base64, weil Windows
    # PowerShell Text an native Programme je nach Einstellung als ASCII weiterreicht und dabei
    # Zeichen wie "§" durch "?" ersetzt. Die Prüfsumme lässt apply.sh eine verfälschte .env erkennen.
    $envFileBytes = $Utf8WithoutBom.GetBytes($EnvFileContent)
    $encodedEnvFile = [Convert]::ToBase64String($envFileBytes)
    $envFileHash = Get-Sha256Hex $envFileBytes
    $encodedEnvFile | ssh -o BatchMode=yes $Server `
        "bash $ServerDirectory/apply.sh $ServerDirectory/$ArchiveName $KeysDirectory $envFileHash"
    Assert-ExitCode 'apply.sh auf dem Server'
}

function Get-Sha256Hex([byte[]] $Bytes)
{
    $sha256 = [Security.Cryptography.SHA256]::Create()
    try
    {
        return ([BitConverter]::ToString($sha256.ComputeHash($Bytes)) -replace '-', '').ToLowerInvariant()
    }
    finally
    {
        $sha256.Dispose()
    }
}

function Get-WebUIStatusCode([System.Net.Http.HttpClient] $Client)
{
    try
    {
        $response = $Client.GetAsync($HealthCheckUrl).GetAwaiter().GetResult()
        $statusCode = [int] $response.StatusCode
        $response.Dispose()
        return $statusCode
    }
    catch [System.Management.Automation.MethodInvocationException]
    {
        # Nicht erreichbar oder Zeitüberschreitung - der Container startet womöglich noch.
        return 0
    }
}

function Wait-ForWebUI
{
    [Net.ServicePointManager]::SecurityProtocol = [Net.ServicePointManager]::SecurityProtocol `
        -bor [Net.SecurityProtocolType]::Tls12
    $handler = New-Object System.Net.Http.HttpClientHandler
    # Eine Weiterleitung (etwa zur Anmeldung bei Keycloak) zählt schon als Antwort der App.
    $handler.AllowAutoRedirect = $false
    $client = New-Object System.Net.Http.HttpClient $handler
    $client.Timeout = [TimeSpan]::FromSeconds(10)
    $deadline = (Get-Date).AddSeconds($HealthCheckTimeoutSeconds)
    $statusCode = 0
    try
    {
        while ((Get-Date) -lt $deadline)
        {
            $statusCode = Get-WebUIStatusCode $client
            if (($statusCode -ge 200) -and ($statusCode -lt 400))
            {
                return
            }
            Start-Sleep -Seconds 3
        }
    }
    finally
    {
        $client.Dispose()
    }
    Show-ServerLogs
    throw "$HealthCheckUrl antwortet nach $HealthCheckTimeoutSeconds s nicht (zuletzt: $(Get-StatusHint $statusCode))."
}

function Get-StatusHint([int] $StatusCode)
{
    switch ($StatusCode)
    {
        0 { return 'keine Verbindung' }
        404 { return '404 - Route "webui" in traefik/routen.yml aktiv?' }
        502 { return '502 - Traefik erreicht den Container nicht, er läuft nicht oder startet neu' }
        default { return "HTTP $StatusCode" }
    }
}

function Show-ServerLogs
{
    Write-Host 'Letzte Log-Zeilen des Containers:' -ForegroundColor Yellow
    ssh -o BatchMode=yes $Server 'docker logs --tail 50 gwios-webui'
}

Write-Step 'Werkzeuge und Secrets prüfen'
Assert-ToolsAvailable
$imageTag = Get-ImageTag
$envFileContent = New-EnvFileContent $imageTag

if (-not $SkipTests)
{
    Write-Step 'Tests ausführen'
    Invoke-Tests
}

$stagingDirectory = New-StagingDirectory
try
{
    $archiveName = "$ImageName-$imageTag.tar"
    Write-Step "Image ${ImageName}:$imageTag für linux-arm64 bauen"
    New-ContainerImageArchive $imageTag (Join-Path $stagingDirectory $archiveName)

    Write-Step "Dateien nach ${Server}:$ServerDirectory übertragen"
    Invoke-OnServer "mkdir -p $ServerDirectory"
    Copy-StagingDirectoryToServer $stagingDirectory

    Write-Step 'Auf dem Server installieren und starten'
    Install-OnServer $envFileContent $archiveName
}
finally
{
    Remove-StagingDirectory $stagingDirectory
}

Write-Step "Auf $HealthCheckUrl warten"
Wait-ForWebUI
Write-Host "Fertig: ${ImageName}:$imageTag läuft." -ForegroundColor Green
