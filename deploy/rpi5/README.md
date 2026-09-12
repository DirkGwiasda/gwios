# Deploy der GwiOS WebUI auf den rpi5

Ein Skript erledigt alles: testen, Image bauen, übertragen, `.env` schreiben,
starten, prüfen. Es kann bei jedem Deploy erneut ausgeführt werden.

```powershell
.\deploy\rpi5\Deploy-WebUI.ps1              # mit Tests
.\deploy\rpi5\Deploy-WebUI.ps1 -SkipTests   # ohne Tests
```

Den Server selbst (Docker, Postgres, Keycloak, Traefik) beschreibt das Repo
`Infrastructure`, Ordner `rpi5`. Hier steht nur, was die WebUI betrifft.

---

## Dateien

| Datei | Läuft auf | Aufgabe |
|---|---|---|
| `Deploy-WebUI.ps1` | Windows | Secrets lesen, testen, Image bauen, übertragen, `apply.sh` starten, auf die App warten |
| `apply.sh` | rpi5 | Voraussetzungen prüfen, `.env` schreiben, Schlüsselverzeichnis anlegen, Image laden, Container starten, alte Images löschen |
| `docker-compose.yml` | rpi5 | Definition des Containers `gwios-webui` |

Auf dem Pi liegt danach unter `/home/tija/gwios-webui`:

```
docker-compose.yml   <- Kopie aus dem Repo
apply.sh             <- Kopie aus dem Repo
.env                 <- vom Skript bei jedem Deploy neu geschrieben, chmod 600
```

Direkte Änderungen dort werden beim nächsten Deploy überschrieben, auch an
der `.env`.

---

## Ablauf

1. **Secrets lesen** — bevor irgendetwas gebaut wird. Fehlt einer, bricht das
   Skript sofort ab.
2. **Tests** — `dotnet test --solution src/GwiOS.slnx`. Die Integrationstests
   brauchen wie immer `GwiOS.DB.TestConnectionstring`.
3. **Image bauen** — `dotnet publish /t:PublishContainer` für `linux-arm64`.
   Das .NET SDK baut das Image selbst: kein Dockerfile, kein Docker Desktop,
   keine Emulation. Ergebnis ist ein Archiv (~100 MB), keine Registry.
4. **Übertragen** — `docker-compose.yml`, `apply.sh` und das Archiv per `scp`.
5. **`apply.sh`** auf dem Pi, die `.env` bekommt es über stdin.
6. **Prüfen** — das Skript fragt `https://webui.gwios.gwiasda.net/` bis zu
   90 Sekunden lang ab. Antwortet die App nicht, zeigt es die letzten
   Log-Zeilen des Containers.

**Image-Tag** ist der Commit (`git rev-parse --short HEAD`). Bei nicht
committeten Änderungen warnt das Skript und hängt `-dirty` an.

---

## Secrets

Das Skript liest sie aus **Benutzer-Umgebungsvariablen** auf dem
Windows-Rechner und schreibt sie in die `.env` auf dem Pi. Im Repo stehen sie
nirgends.

| Benutzer-Umgebungsvariable | Wird in der `.env` zu | In der App |
|---|---|---|
| `GwiOS.DB.Connectionstring` | `GWIOS_DB_CONNECTIONSTRING` | `GwiOS.DB.Connectionstring` |
| `Keycloak__ClientSecret` | `GWIOS_WEBUI_CLIENT_SECRET` | `Keycloak:ClientSecret` |

Es sind dieselben Variablen, mit denen die App in der Entwicklung läuft — die
deployte App arbeitet also mit derselben Datenbank und demselben Keycloak-Client.
Die Variable der Tests (`GwiOS.DB.TestConnectionstring`) liest das Skript nicht.

- **Connection String:** Der Host muss auch **aus dem Container** erreichbar
  sein. `Host=rpi5` funktioniert — `docker-compose.yml` lässt den Namen per
  `extra_hosts` auf den Pi zeigen —, ebenso `Host=192.168.178.5`.
  `localhost` dagegen nicht: Das wäre im Container der Container selbst.
  Postgres läuft in einem anderen Compose-Projekt und Netz, der Name
  `postgres` ist von hier aus nicht erreichbar.
- **Client-Secret:** derselbe Wert wie `GWIOS_WEBUI_CLIENT_SECRET` in der
  `.env` unter `/home/tija/infrastructure` — Keycloak hat ihn beim Anlegen des
  Realms übernommen.
- **Kein `'`, kein `\` und kein Zeilenumbruch** in den Werten. In der `.env`
  steht jeder Wert in einfachen Anführungszeichen, damit Compose `$` und `#`
  nicht auswertet. Selbst dort macht Compose aber aus `\\` ein `\` — das
  Passwort käme verfälscht an. Das Skript lehnt solche Werte deshalb ab.
  Passwörter am einfachsten hex erzeugen: `openssl rand -hex 24`.
- Das Skript liest die Werte **aus dem Benutzerkonto**, nicht aus der
  Umgebung des Terminals — die hat in einem Terminal, das vor einer Änderung
  geöffnet wurde, noch den alten Wert. Nur wenn es im Benutzerkonto keinen
  Wert gibt, nimmt es den des Terminals.

### Warum über stdin

Die `.env` geht per `ssh` als Eingabe an `apply.sh`. So steht kein Secret in
einer Kommandozeile (auf dem Pi für jeden per `ps` sichtbar) und keines in
einer Datei auf dem Windows-Rechner. Auf dem Pi schreibt `apply.sh` sie mit
`umask 077` — nur `tija` darf sie lesen.

Unterwegs ist sie **base64-kodiert**, und `apply.sh` prüft ihre
SHA-256-Prüfsumme. Grund: Windows PowerShell reicht Text an Programme wie
`ssh` je nach Einstellung als ASCII weiter und macht dabei aus `§`, `ä` & Co.
stillschweigend ein `?`. Genau so ist beim ersten Deploy aus dem Passwort in
`GwiOS.DB.Connectionstring` ein falsches geworden. Base64 besteht nur aus
ASCII-Zeichen und kommt deshalb unverändert an; stimmt die Prüfsumme trotzdem
nicht, bricht `apply.sh` ab, ohne etwas zu ändern.

Wer in der Gruppe `docker` ist, sieht die Werte trotzdem über
`docker inspect gwios-webui`. Das ist bekannt und hingenommen: Die Gruppe
`docker` hat ohnehin Root-Rechte auf dem Pi.

---

## Voraussetzungen auf dem Server

Das Skript prüft die ersten vier selbst und bricht mit einer Meldung ab.

| Voraussetzung | Woher |
|---|---|
| Docker mit Compose, `tija` in der Gruppe `docker` | Infrastruktur-Repo |
| Docker-Netz `proxy` (entsteht mit Traefik) | Infrastruktur-Repo |
| SSD unter `/mnt/ssd` eingehängt | Infrastruktur-Repo |
| `sudo` ohne Passwort für `tija` — nur beim ersten Deploy, um `/mnt/ssd/gwios-webui/keys` anzulegen | Infrastruktur-Repo |
| SSH-Schlüssel des Windows-Rechners auf dem Pi freigeschaltet | Infrastruktur-Repo |
| Route `webui` in `traefik/routen.yml` aktiv, Ziel `http://gwios-webui:8080` | Infrastruktur-Repo, dort vorbereitet und auskommentiert |
| Datenbank `gwios_db` mit Benutzer `gwios_user` | von Hand, siehe unten |

Datenbank und Benutzer einmalig als `postgres` anlegen, z. B. in DBeaver. Die
App legt ihre Schemas und Tabellen beim Start selbst an und braucht dafür das
Recht `CREATE` auf der Datenbank — als Besitzer hat sie es.

```sql
CREATE ROLE gwios_user LOGIN PASSWORD '…';
CREATE DATABASE gwios_db OWNER gwios_user;
REVOKE ALL ON DATABASE gwios_db FROM PUBLIC;
```

Damit ein Neuaufbau des Pi sie wieder anlegt, gehört dasselbe als Skript nach
`postgres-init/` im Infrastruktur-Repo.

---

## Entscheidungen

- **Eigenes Compose-Projekt** `gwios-webui` in eigenem Ordner, nicht im
  Infrastruktur-Repo. Mit Traefik verbindet es nur das gemeinsame Netz
  `proxy`.
- **Data-Protection-Schlüssel auf der SSD** unter `/mnt/ssd/gwios-webui/keys`,
  eingebunden am Standardpfad `/home/app/.aspnet/DataProtection-Keys` — ASP.NET
  Core findet sie dort ohne Codeänderung. Ohne das entstünden mit jedem neuen
  Container neue Schlüssel, und alle wären nach jedem Deploy abgemeldet. Das
  Verzeichnis gehört dem Benutzer `app` der .NET-Images (UID `1654`).
- **`TZ=Europe/Berlin`**: Die App zeigt Uhrzeiten in der lokalen Zeitzone des
  Servers an, im Container wäre das UTC.
- **Keycloak über `host-gateway`**: Die App muss Keycloak unter
  `https://keycloak.gwios.gwiasda.net` erreichen, weil das der Aussteller in
  den Tokens ist. Der Eintrag in `extra_hosts` führt direkt zu Traefik auf dem
  Pi, statt vom DNS der FritzBox abzuhängen.
- **Keine Registry**: Das Image geht als Archiv per `scp`. Bei jedem Deploy
  werden so auch die Schichten des Basis-Images übertragen, im Heimnetz
  unerheblich.

---

## Zurückrollen

`apply.sh` behält die drei neuesten Images. Auf ein älteres zurück:

```bash
ssh tija@rpi5 'docker images gwios-webui'
```

Dann entweder den alten Commit auschecken und das Skript erneut ausführen,
oder auf dem Pi in `/home/tija/gwios-webui/.env` den Wert von
`GWIOS_WEBUI_TAG` ändern und `docker compose up -d` ausführen — bis zum
nächsten Deploy.

---

## Wenn es nicht klappt

| Meldung | Ursache |
|---|---|
| `404 - Route "webui" …` | Route in `traefik/routen.yml` nicht aktiv oder noch nicht auf den Pi übertragen |
| `502 - Traefik erreicht den Container nicht …` | Container läuft nicht oder startet immer wieder neu. Meist erreicht die App die Datenbank nicht — sie startet ohne Datenbank nicht. Die Log-Zeilen zeigen den Grund |
| Anmeldung scheitert mit *„Invalid parameter: redirect_uri"* | `ASPNETCORE_FORWARDEDHEADERS_ENABLED` fehlt oder wirkt nicht |
| `sudo ohne Passwort …` | Nur beim ersten Deploy: Schlüsselverzeichnis anlegen braucht `sudo` |
