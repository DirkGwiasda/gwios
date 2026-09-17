#Requires -Version 5.1
<#
.SYNOPSIS
    Flasht Firmware und Konfiguration auf einen BME280-Satelliten.

.DESCRIPTION
    Erzeugt aus der CSV-Datei des Satelliten ein NVS-Image und schreibt es zusammen mit der Firmware aus
    dist\ auf das Board. Die Firmware wird in der VM gebaut und mit export-firmware.sh nach dist\ gelegt.
    Ablauf und Voraussetzungen stehen in README.md in diesem Ordner.

.PARAMETER Name
    Name des Satelliten. Die Konfiguration wird aus <ConfigDirectory>\<Name>.csv gelesen.

.PARAMETER ConfigDirectory
    Ordner mit den CSV-Dateien der Satelliten. Liegt außerhalb des Repos, weil die Dateien Geheimnisse
    enthalten.

.PARAMETER Port
    Serieller Port des Boards.

.PARAMETER Baud
    Geschwindigkeit beim Flashen. Höhere Werte sind schneller, aber weniger zuverlässig.

.PARAMETER IdfToolsPath
    Werkzeugordner der Windows-Installation von ESP-IDF; daraus wird deren Python verwendet.

.PARAMETER ConfigOnly
    Schreibt nur die Konfiguration, nicht die Firmware.

.PARAMETER Monitor
    Zeigt nach dem Flashen die Konsolenausgabe des Boards an.
#>
[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)]
    [string] $Name,

    [string] $ConfigDirectory = (Join-Path $env:USERPROFILE '.gwios\satellites'),

    [string] $Port = 'COM3',

    [int] $Baud = 115200,

    [string] $IdfToolsPath = (Join-Path $env:USERPROFILE '.espressif'),

    [switch] $ConfigOnly,

    [switch] $Monitor
)

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

$ConfigPartitionName = 'config'
$MonitorBaud = 115200
$NamePattern = '^[a-z0-9-]{1,32}$'

$DistDirectory = Join-Path $PSScriptRoot 'dist'
$PartitionTable = Join-Path $PSScriptRoot 'partitions.csv'
$FlasherArgsFile = Join-Path $DistDirectory 'flasher_args.json'

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

function Assert-ValidName
{
    # Dieselbe Regel wie in der Firmware (satellite_config_rules.h): Der Name steht in der URL des Pings.
    if ($Name -cnotmatch $NamePattern)
    {
        throw "Ungültiger Name '$Name': erlaubt sind 1 bis 32 Zeichen aus a-z, 0-9 und '-'."
    }
}

function Get-ConfigFile
{
    $path = Join-Path $ConfigDirectory "$Name.csv"
    if (-not (Test-Path -LiteralPath $path -PathType Leaf))
    {
        throw "Konfiguration '$path' nicht gefunden. Vorlage: config\satellite.example.csv."
    }

    return $path
}

function Assert-ConfigMatchesName([string] $ConfigFile)
{
    # Schützt davor, einem Board versehentlich die Identität eines anderen Satelliten zu geben.
    $nameRow = Import-Csv -LiteralPath $ConfigFile | Where-Object { $_.key -eq 'name' }
    if ($null -eq $nameRow)
    {
        throw "In '$ConfigFile' fehlt die Zeile 'name'."
    }
    if ($nameRow.value -cne $Name)
    {
        throw "In '$ConfigFile' steht der Name '$($nameRow.value)', erwartet war '$Name'."
    }
}

function Get-IdfPython
{
    # Je nach Installationsweg liegt die Python-Umgebung von ESP-IDF v6.1 an einer anderen Stelle:
    # install.ps1 legt python_env\idf6.1_py<Version>_env an, der Installation Manager tools\python\v6.1\venv.
    $candidates = @(
        (Join-Path $IdfToolsPath 'python_env\idf6.1_*_env\Scripts\python.exe'),
        (Join-Path $IdfToolsPath 'tools\python\v6.1\venv\Scripts\python.exe')
    )
    foreach ($candidate in $candidates)
    {
        $found = Get-ChildItem -Path $candidate -ErrorAction SilentlyContinue | Select-Object -First 1
        if ($null -ne $found)
        {
            return $found.FullName
        }
    }

    throw "Keine Python-Umgebung von ESP-IDF v6.1 unter '$IdfToolsPath' gefunden (Parameter -IdfToolsPath)."
}

function Get-ConfigPartition
{
    foreach ($line in Get-Content -LiteralPath $PartitionTable)
    {
        $trimmed = $line.Trim()
        if ($trimmed -eq '' -or $trimmed.StartsWith('#'))
        {
            continue
        }

        $columns = @($trimmed.Split(',') | ForEach-Object { $_.Trim() })
        if ($columns[0] -eq $ConfigPartitionName)
        {
            return @{ Offset = $columns[3]; Size = $columns[4] }
        }
    }

    throw "Partition '$ConfigPartitionName' fehlt in '$PartitionTable'."
}

function New-ConfigImage([string] $Python, [string] $ConfigFile, [hashtable] $Partition, [string] $OutputDirectory)
{
    $image = Join-Path $OutputDirectory 'config.bin'
    & $Python -m esp_idf_nvs_partition_gen generate $ConfigFile $image $Partition.Size | Out-Host
    Assert-ExitCode 'Erzeugen des NVS-Images'

    return $image
}

function Get-FirmwareFlashArguments
{
    if (-not (Test-Path -LiteralPath $FlasherArgsFile -PathType Leaf))
    {
        throw "Keine Firmware in '$DistDirectory'. In der VM bauen und ./export-firmware.sh ausführen."
    }

    $flasherArgs = Get-Content -LiteralPath $FlasherArgsFile -Raw | ConvertFrom-Json
    $arguments = @($flasherArgs.write_flash_args)
    foreach ($image in $flasherArgs.flash_files.PSObject.Properties)
    {
        $arguments += $image.Name
        $arguments += (Join-Path $DistDirectory $image.Value)
    }

    return $arguments
}

function Wait-ForDownloadMode
{
    # Auf diesem Board klappt der automatische Wechsel in den Download-Modus unter Windows nicht
    # ("Wrong boot mode detected"), siehe docs/devplan.md.
    Write-Host ''
    Write-Host 'Board in den Download-Modus bringen:' -ForegroundColor Yellow
    Write-Host '  1. BOOT gedrückt halten'
    Write-Host '  2. EN kurz drücken'
    Write-Host '  3. BOOT loslassen'
    Read-Host 'Danach Enter drücken' | Out-Null
}

function Write-Flash([string] $Python, [string[]] $FlashArguments)
{
    # --before no-reset: Ein Reset vor dem Flashen würde den Chip wieder aus dem Download-Modus holen.
    # Der Reset danach über RTS funktioniert und startet die neue Firmware.
    $arguments = @('-m', 'esptool', '--chip', 'esp32', '--port', $Port, '--baud', $Baud,
        '--before', 'no-reset', '--after', 'hard-reset', 'write-flash') + $FlashArguments
    & $Python @arguments | Out-Host
    Assert-ExitCode 'Flashen'
}

function Start-Monitor([string] $Python)
{
    Write-Host 'Beenden mit Strg+T, danach Strg+X.' -ForegroundColor Yellow
    & $Python -m esp_idf_monitor --port $Port --baud $MonitorBaud
}

Assert-ValidName
$configFile = Get-ConfigFile
Assert-ConfigMatchesName $configFile
$python = Get-IdfPython
$partition = Get-ConfigPartition

$flashArguments = @()
if (-not $ConfigOnly)
{
    $flashArguments += Get-FirmwareFlashArguments
}

# Das NVS-Image enthält Token und WLAN-Passwort im Klartext und wird deshalb in jedem Fall wieder gelöscht.
$workDirectory = Join-Path ([System.IO.Path]::GetTempPath()) ("gwios-satellite-" + [guid]::NewGuid())
New-Item -ItemType Directory -Path $workDirectory | Out-Null
try
{
    Write-Step "Konfiguration aus '$configFile' erzeugen"
    $image = New-ConfigImage $python $configFile $partition $workDirectory
    $flashArguments += $partition.Offset
    $flashArguments += $image

    Wait-ForDownloadMode

    if ($ConfigOnly)
    {
        Write-Step "Konfiguration auf $Port flashen"
    }
    else
    {
        Write-Step "Firmware und Konfiguration auf $Port flashen"
    }
    Write-Flash $python $flashArguments
}
finally
{
    Remove-Item -LiteralPath $workDirectory -Recurse -Force -ErrorAction SilentlyContinue
}

Write-Step "Satellit '$Name' geflasht"

if ($Monitor)
{
    Start-Monitor $python
}
