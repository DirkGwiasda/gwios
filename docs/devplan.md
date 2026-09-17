# Entwicklungsplan

## Logging (GwiOS.Core/CrossCutting/Logging)

- [x] Domäne: `LogEntry`, `LogLevel`, `ILogger<T>` / `DefaultLogger<T>`, `ILogEntryManager` / `LogEntryManager`
- [x] `LogEntryPostgresRepository` mit direktem SQL über Npgsql (kein Entity Framework)
- [x] `ILogEntryRepository.EnsureStorageCreatedAsync()` — legt idempotent die Logging-Tabellen an (nicht die Datenbank)
- [x] `AddGwiOSCore(connectionString)` — `IServiceCollection`-Extension für GwiOS.Core
- [x] WebUI: `AddGwiOSCore` in `Program.cs`, `EnsureStorageCreatedAsync()` beim Start,
      Connection String aus der Umgebungsvariable `GwiOS.DB.Connectionstring`
- [x] DI-6 in `LogEntryManager` und `DefaultLogger<T>` umgesetzt
- [x] Unit-Tests (`tests/GwiOS.Core.Tests`): `LogEntry`, `LogEntryManager`, `DefaultLogger<T>`, `AddGwiOSCore` — 33 grün
- [x] Integrationstests für `LogEntryPostgresRepository` — 12 grün
- [ ] WebUI: `Program` (Composition Root) ist nicht durch Tests abgedeckt (das Testprojekt `GwiOS.WebUI.Tests`
      deckt bisher nur Komponenten, Seiten und Dienste ab)

### Entscheidungen

- Datenzugriff über `NpgsqlDataSource` (Singleton, per `Npgsql.DependencyInjection` registriert). Alle künftigen
  Repositories teilen sich diese Data Source der GwiOS-Datenbank.
- Die Datenbank wird nicht von der Anwendung angelegt, sie muss bereits existieren. Das Repository stellt nur Schema,
  Tabelle und Index sicher; der Benutzer braucht dafür das Recht `CREATE` auf der Datenbank.
- Logging-Tabellen liegen im Schema `logging`: Tabelle `logging.log_entries`, Index auf `(app_name, timestamp DESC)`.
- `ContextData` wird als `jsonb` gespeichert, `LogLevel` als `smallint` (numerischer Enum-Wert).
- Log-Einträge einer App werden neueste zuerst geliefert, App-Namen alphabetisch.
- `LogLevel.None` ist nur der Default-Wert eines nicht gesetzten Levels und wird für echte Log-Einträge nicht genutzt.
- Connection Strings stehen nie im Repository, sondern in Umgebungsvariablen: `GwiOS.DB.Connectionstring` für die
  Anwendung, `GwiOS.DB.TestConnectionstring` für die Integrationstests. Die WebUI liest ihre Variable über
  `IConfiguration` (Umgebungsvariablen sind dort unter ihrem Namen als Schlüssel enthalten).
- Die WebUI legt die Tabellen beim Start an; ist die Datenbank nicht erreichbar, startet sie nicht.

### Tests

- `dotnet test` läuft über die Microsoft Testing Platform (`global.json`, Pflicht für xunit v3 unter dem .NET-10-SDK):
  `dotnet test --solution src/GwiOS.slnx` bzw. `dotnet test --project tests/GwiOS.Core.Tests/GwiOS.Core.Tests.csproj`.
- Nach R5 heißt das Testverzeichnis wie die getestete Klasse und nach CORE-3 dann auch der Namespace. Weil der
  Namespace so die gleichnamige Klasse verdeckt, wird sie per `using`-Alias `<Klasse>UnderTest` angesprochen.
- Implementierungen sind `internal` (DI-3); `GwiOS.Core` gibt sie per `InternalsVisibleTo` für `GwiOS.Core.Tests` frei.
- Integrationstests laufen in einer gemeinsamen xunit-Collection (`PostgresTestDatabase`) nacheinander. Sie verwenden
  eindeutige App-Namen und räumen ihre Daten selbst wieder weg. Die Fixture ruft `EnsureStorageCreatedAsync()` einmal
  vor allen Integrationstests auf.
- Fixture und Collection liegen unter `tests/GwiOS.Core.Tests/TestInfrastructure/`, weil Logging-, Person- und
  ToDo-Integrationstests sie teilen. Die Fixture verdrahtet wie `AddGwiOSCore`, ersetzt aber `ILogger<T>` durch
  `LoggerFake<T>`, damit die Repositories unter Test keine Log-Einträge in der Test-DB hinterlassen.
- `LoggerFake<T>` (`tests/.../CrossCutting/Logging/Contracts/`) zeichnet Log-Einträge synchron auf; damit prüfen
  Unit-Tests das Logging von Klassen mit öffentlicher Schnittstelle.
- Methoden mit `CancellationToken` bekommen in Tests `TestContext.Current.CancellationToken` (xunit-Analyzer xUnit1051).
- Fehlt `GwiOS.DB.TestConnectionstring`, schlagen die Integrationstests mit einer klaren Meldung fehl, statt
  übersprungen zu werden.
- Die Test-Fixture liest `GwiOS.DB.TestConnectionstring` zuerst aus dem Prozess und auf Windows ersatzweise direkt aus
  dem Benutzerkonto (`EnvironmentVariableTarget.User`): Laufende IDEs und Terminals kannten neu gesetzte
  Variablen nicht, auch ein Neustart von Visual Studio half nicht.

### Offene Fragen

- Was soll `GwiOS.WebUI.Tests` für die Composition Root (`Program`) abdecken? Ein Start der App braucht Datenbank und
  Keycloak.

## Personen (GwiOS.Core/CrossCutting/Persons)

- [x] `IPersonManager` / `PersonManager`: Anlegen, Ändern, Lesen (alle, per Identity-User-ID), Löschen
- [x] `IPersonValidator` / `PersonValidator`: Name und ShortName nicht leer, IdentityUserId `null` oder nicht leer
- [x] `IPersonRepository` / `PersonPostgresRepository` mit direktem SQL über Npgsql, inkl. `EnsureStorageCreatedAsync()`
- [x] Exceptions: `PersonValidationException`, `DuplicatePersonException`, `PersonNotFoundException`
- [x] Registrierung in `AddGwiOSCore`; WebUI legt die Personen-Tabellen beim Start an
- [x] Contracts dokumentiert (DOC-1/DOC-2); `UpdatePerson`/`DeletePerson` in `…Async` umbenannt (MS-45)
- [x] Unit-Tests: `PersonManager`, `PersonValidator`, Exceptions, `Person`, `AddGwiOSCore` — 42 neu
- [x] Integrationstests für `PersonPostgresRepository` — 21 neu (gesamt: 108 grün)

### Entscheidungen

- Tabelle `persons.persons` (Schema `persons`): `id uuid` (PK `pk_persons`), `name`, `short_name`,
  `identity_user_id` (nullable), alle `text`.
- Eindeutigkeit von ID, Name, ShortName und IdentityUserId erzwingt allein die Datenbank (Unique-Indizes
  `ux_persons_name`, `ux_persons_short_name`, `ux_persons_identity_user_id`); das ist auch bei gleichzeitigen Zugriffen
  sicher. Mehrere Personen ohne IdentityUserId sind erlaubt (NULL-Werte gelten im Unique-Index als verschieden).
  Das Repository übersetzt eine Verletzung in `DuplicatePersonException` mit dem betroffenen Property-Namen.
- Der Validator prüft nur die Werte einer einzelnen Person, nicht die Eindeutigkeit.
- Ungültige Personen: `PersonManager` wirft `PersonValidationException` mit allen Validierungsfehlern.
- `UpdatePersonAsync` einer nicht gespeicherten Person wirft `PersonNotFoundException`; `DeletePersonAsync` ist
  idempotent und tut dann nichts.
- `GetAllPersonsAsync` liefert die Personen nach Namen sortiert.
- Logging: `PersonManager` loggt Anlegen, Ändern und Löschen (Information, mit `PersonId`); `PersonValidator` loggt
  abgelehnte Personen, `PersonPostgresRepository` Duplikate und fehlende Personen beim Ändern (jeweils Warning). Namen
  werden nicht geloggt, nur die ID.

### Offene Fragen

- Sollen Name und ShortName ohne Beachtung der Groß-/Kleinschreibung eindeutig sein? Derzeit sind „Anna“ und „anna“
  zwei verschiedene Namen.

## ToDos (GwiOS.Core/ToDos)

- [x] `IToDoManager` / `ToDoManager`: Anlegen, Erledigen, Lesen (alle), Löschen
- [x] `IToDoValidator` / `ToDoValidator`: Titel nicht leer
- [x] `IToDoRepository` / `ToDoPostgresRepository` mit direktem SQL über Npgsql, inkl. `EnsureStorageCreatedAsync()`
- [x] Exceptions: `ToDoValidationException`, `ToDoNotFoundException`
- [x] Registrierung in `AddGwiOSCore` (inkl. `TimeProvider.System`); WebUI legt die ToDo-Tabelle beim Start an
- [x] Unit-Tests: `ToDoManager`, `ToDoValidator`, Exceptions, `ToDo`, `AddGwiOSCore`; Integrationstests für
      `ToDoPostgresRepository` — 48 neu (Core gesamt: 156 grün)

### Entscheidungen

- Tabelle `todos.todos` (Schema `todos`): `id uuid` (PK `pk_todos`), `title text`, `description text NULL`,
  `is_completed boolean`, `due_date date NULL`, `created_at timestamptz`, `completed_at timestamptz NULL`,
  `position integer`. `UpdateAsync` überschreibt `created_at` nicht.
- `ToDo.DueDate` ist `DateOnly?` statt `DateTime?`: Ein Fälligkeitstag hat keine Uhrzeit und keine Zeitzone.
  `CreatedAt`/`CompletedAt` bleiben UTC-`DateTime`.
- ToDos werden nach `Position`, bei gleicher Position nach `CreatedAt` sortiert. Die Position setzt derzeit niemand
  (alle 0), die Reihenfolge ist also die des Anlegens.
- `CompleteToDoAsync` setzt `IsCompleted` und `CompletedAt` (aus dem injizierten `TimeProvider`, DI-5: Uhr hinter
  Abstraktion) und speichert; ein nicht gespeichertes ToDo führt zu `ToDoNotFoundException`.
- Logging wie bei Personen: Manager loggt Anlegen, Erledigen, Löschen (Information, nur `ToDoId`), Validator
  abgelehnte ToDos, Repository fehlende ToDos beim Ändern (Warning). Titel werden nicht geloggt.

### Offene Fragen

- Gehören ToDos einer Person (Zuständigkeit/Ersteller) oder sind sie familienweit? Derzeit familienweit.
- Soll die Reihenfolge per Drag & Drop änderbar sein (`Position`), und soll ein erledigtes ToDo wieder geöffnet
  werden können?

## WebUI (Design-Vorlage `docs/UI/GwiOS.WebUI.Design.html`)

- [x] Theme: Design-Tokens, Schriften (Inter, Orbitron, lokal unter `wwwroot/fonts`), Logo, zentral gestylte
      Elemente in `wwwroot/app.css`; Bootstrap wird nicht mehr eingebunden
- [x] Layout: `MainLayout` (Hintergrund, Header, Statusleiste), `AppHeader` (Navigation, Burger-Menü unter 820 px),
      `AdminLayout` (Admin-Tabs)
- [x] GwiOS-Komponenten (`Components/GwiOS.Components`): `GwiOSTextBox`, `GwiOSDateBox`, `GwiOSSection`,
      `GwiOSTabNav`, `GwiOSPillGroup`, `GwiOSConfirmButton`, `GwiOSLogLevelBadge`, `GwiOSToDoTile`,
      `GwiOSPersonTile`, `GwiOSStatusBar`
- [x] Seiten: `/todos` (ToDo-Übersicht), `/admin/logging` (System-Logs), `/admin/benutzer` (Benutzerverwaltung);
      `/` und `/admin` leiten weiter
- [x] Statusleiste mit Verlauf: `IStatusMessageService` / `StatusMessageService` (scoped je Sitzung, max. 50 Meldungen)
- [x] Testprojekt `tests/GwiOS.WebUI.Tests` (xunit v3 + bUnit 2) — 127 grün
- [ ] Haushaltsbuch aus der Vorlage (vorerst ausgenommen)

### Entscheidungen

- Razor würde aus dem Ordner `GwiOS.Components` den Namespace `GwiOS_Components` machen; nach CORE-3 setzt
  `Components/GwiOS.Components/_Imports.razor` ihn per `@namespace` auf `GwiOS.WebUI.Components.GwiOS.Components`.
  Weil dieser Namespace in allen Komponenten unter `GwiOS.WebUI.Components` den Wurzel-Namespace `GwiOS` verdeckt,
  stehen die GwiOS-Usings in `Components/_Imports.razor` mit `global::`. Testklassen sprechen die Komponenten wie
  im Core per Alias `<Komponente>UnderTest` an.
- Das implizite `using Microsoft.Extensions.Logging` ist im WebUI-Projekt entfernt: Es machte `ILogger<T>` und
  `LogLevel` mehrdeutig, und nur der GwiOS-Logger ist erlaubt.
- Komponenten mit Logik haben eine Code-Behind-Datei (`.razor.cs`), reine Darstellungskomponenten einen `@code`-Block.
  Styles einer Komponente stehen in ihrer `.razor.css`, gemeinsame Grundelemente (Eingabefeld, Schaltflächen,
  Tabelle, Panel, Plaketten) als Klassen `gwios-*` zentral in `app.css` (Ausnahme der UI-Regeln für einfache,
  zentral gestylte HTML-Elemente).
- Tabellen tragen `data-label` je Zelle; unter 820 px wird jede Zeile zur Karte (wie in der Vorlage).
- Benutzerverwaltung zeigt Personen (Kürzel-Avatar, Name, Kurzname, Konto ja/nein). Die Rollen der Vorlage
  (Admin/Mitglied/Kind) entfallen, bis es Rollen gibt. Löschen von Personen und Log-Einträgen verlangt einen
  zweiten Klick (`GwiOSConfirmButton`); ToDos werden wie in der Vorlage sofort gelöscht.
- System-Logs zeigen je Anwendung (Auswahl als Pills) Zeitstempel, Level und Nachricht, neueste zuerst; dazu
  „Aktualisieren“ und „Einträge löschen“.
- Statusmeldungen: Grau = neutral, Blau = Erfolg, Orange = Warnung, Rot = Fehler. `StatusMessageService` loggt nur
  das Level (Debug), nicht den Text, da er Namen enthalten kann. Seiten loggen nicht selbst; Manager, Validatoren
  und Repositories loggen die fachlichen Vorgänge bereits.
- Uhrzeiten werden über den injizierten `TimeProvider` in der lokalen Zeitzone des Servers angezeigt.

### Offene Fragen

- Sollen die Admin-Seiten auf eine Keycloak-Rolle beschränkt werden? Derzeit genügt die Anmeldung.
- Anmeldestatus und Abmelden sind in der Vorlage nicht vorgesehen und fehlen daher im Header (`/logout` existiert).
- Die System-Logs laden alle Einträge einer Anwendung; bei großen Mengen braucht es Paging oder Virtualisierung.
- `wwwroot/lib/bootstrap` wird nicht mehr verwendet und kann entfernt werden.

## Deployment (deploy/rpi5)

- [x] `Deploy-WebUI.ps1` (Windows): Secrets lesen, Tests, arm64-Image per `dotnet publish /t:PublishContainer`
      als Archiv bauen, per `scp` übertragen, `apply.sh` starten, auf `https://webui.gwios.gwiasda.net/` warten
- [x] `apply.sh` (rpi5): Voraussetzungen prüfen, `.env` von stdin schreiben, Schlüsselverzeichnis anlegen,
      Image laden, `docker compose up`, alte Images löschen (drei bleiben)
- [x] `docker-compose.yml`: Projekt `gwios-webui`, Container `gwios-webui` im Netz `proxy`
- [x] Image-Bau lokal geprüft (arm64, Benutzer 1654, Port 8080); Compose-Datei und `.env`-Quoting mit
      `docker compose config` geprüft
- [x] Erster Deploy auf den Pi (2026-09-12) — läuft unter `https://webui.gwios.gwiasda.net`
- [x] Infrastruktur-Repo: Route `webui` in `traefik/routen.yml` aktiviert, README nachgezogen
- [ ] Infrastruktur-Repo: `postgres-init/` um Benutzer `gwios_user` und Datenbank `gwios_db` ergänzen, damit ein
      Neuaufbau des Pi sie wieder anlegt

### Entscheidungen

- Die WebUI wird ausschließlich aus diesem Repo deployt, nicht im Infrastruktur-Repo beschrieben. Eigener
  Zielordner `/home/tija/gwios-webui`, eigenes Compose-Projekt; Verbindung zu Traefik nur über das Netz `proxy`.
- Secrets kommen aus denselben Benutzer-Umgebungsvariablen wie in der Entwicklung (`GwiOS.DB.Connectionstring`,
  `Keycloak__ClientSecret`), nicht aus denen der Tests, und landen nur in der `.env` auf dem Pi (chmod 600).
  Die `.env` wird bei jedem Deploy neu geschrieben und per stdin übertragen, nie über eine Kommandozeile.
- Die `.env` geht base64-kodiert mit SHA-256-Prüfsumme über stdin: Windows PowerShell 5.1 reichte sie beim ersten
  Deploy als ASCII an `ssh` weiter und machte aus `§` im DB-Passwort ein `?` (Folge: „password authentication
  failed“). Base64 ist reines ASCII; eine abweichende Prüfsumme lässt `apply.sh` ohne Änderung abbrechen.
- Werte stehen in der `.env` in einfachen Anführungszeichen (keine Ersetzung von `$` durch Compose); Werte
  mit `'`, `\` oder Zeilenumbruch lehnt das Skript ab (Compose macht auch dort aus `\\` ein `\`).
- `init: true` im Compose: Als PID 1 beendet sich .NET nach einem unbehandelten Fehler nicht (beobachtet beim
  ersten Deploy: Absturz beim DB-Login, Container blieb „running“, `restart` griff nie).
- Postgres wird aus dem Container über den freigegebenen Port 5432 des Pi erreicht (`extra_hosts`: `rpi5` →
  Pi), Keycloak über einen `extra_hosts`-Eintrag direkt bei Traefik auf dem Pi.
- Data-Protection-Schlüssel liegen auf der SSD (`/mnt/ssd/gwios-webui/keys`), eingebunden am Standardpfad
  von ASP.NET Core — keine Codeänderung nötig. `TZ=Europe/Berlin` für die Anzeige lokaler Uhrzeiten.
- Image-Tag = kurzer Commit-Hash, bei nicht committeten Änderungen mit `-dirty`.

### Offene Fragen

- Skript und `apply.sh` sind nicht durch automatisierte Tests abgedeckt (R5 zielt auf .NET-Code unter
  `tests/`); geprüft werden sie durch den Deploy selbst.

## Satelliten (src/GwiOS.Satellites)

Kleine Hardwaremodule, die Messwerte über das LAN an GwiOS schicken. Erstes Modul: ESP32 mit BME280
(Temperatur, Luftfeuchte, Luftdruck).

- [x] Schaltung: BME280 per I²C an den ESP32 — vier Adern: VCC = 3V3, GND = GND, SDA = GPIO 21,
      SCL = GPIO 22; CS und ADDR bleiben unverbunden (Herstellerangabe `NC` für I²C). Adresse `0x77`,
      durch den I²C-Scan auf dem Board bestätigt
- [x] Toolchain ESP-IDF v6.1 unter `D:/esp/v6.1/esp-idf`, Werkzeuge unter `C:/Users/dirk/.espressif`
- [x] Erstes Programm: I²C-Scan (`src/GwiOS.Satellites/GwiOS.Satellite.Bme280`) — baut fehlerfrei
      für Target `esp32` (Image 164 KB)
- [x] CP210x-Treiber (Silicon Labs) installiert; das Board meldet sich als COM3
- [x] I²C-Scan geflasht und ausgeführt: genau ein Gerät, Adresse `0x77` — Verkabelung, Versorgung
      und Adresse sind damit am lebenden Objekt bestätigt
- [x] Entwicklungsumgebung in der Ubuntu-VM (26.04) eingerichtet: Systempakete, Gruppe `dialout`,
      ESP-IDF v6.1 unter `~/.espressif/v6.1/esp-idf` (Werkzeuge `~/.espressif/tools`), in VS Code
      ausgewählt; Board per USB-Durchreichung als `/dev/ttyUSB0` sichtbar
- [x] BME280 auslesen (Kalibrierdaten, Kompensation, Temperatur/Feuchte/Druck): Treiber
      `main/bme280.c`, unter Linux gebaut (Image 168 KB), geflasht und am Board geprüft. Werte über
      100 s stabil und plausibel: 24,1–24,2 °C, 50–52 %rH, 996,4–996,5 hPa (Stationsdruck)
- [x] Schnittstelle zum Portal festgelegt: Ping plus Pull, Adressierung, Token, Verschlüsselung,
      Konfiguration im NVS, Datenformat, Verhalten von GwiOS (siehe Entscheidungen)
- [x] Satellit: Host-Tests eingerichtet (`tests/GwiOS.Satellite.Bme280.Tests`, CMake + CTest, Unity aus ESP-IDF).
      BME280-Kompensation in `main/bme280_compensation.{c,h}` herausgelöst (rein, ohne ESP-IDF) — 18 Tests grün,
      Firmware baut ohne Warnungen; Gegenprobe am Board am 17.09.2026: 22,2–22,3 °C, 51 %rH, 995,3 hPa (plausibel)
- [x] Satellit: Konfiguration aus eigener NVS-Partition `config` (`main/satellite_config.{c,h}`), Regeln in
      `main/satellite_config_rules.{c,h}` mit 35 Host-Tests (gesamt 53 grün); CSV-Vorlage
      `config/satellite.example.csv`, `export-firmware.sh` (VM → `dist/`), `Flash-Satellite.ps1` (Windows).
      Am Board geprüft (17.09.2026): Flash-Skript unter Windows, Konfiguration `arbeitszimmer` gelesen, Messung läuft
- [x] Satellit: WLAN (`main/wifi_connection.{c,h}`) mit Neuverbindung nach Pausen aus
      `main/wifi_reconnect_policy.{c,h}` (5 Host-Tests, gesamt 58 grün); Hostname `gwios-<name>`.
      Partition `config` vor die App verlegt (App 1,9 MB; Firmware mit WLAN 800 KB). Am Board geprüft
      (17.09.2026): verbunden nach 3,4 s, IP 192.168.178.33, Signal -70 dBm im Arbeitszimmer
- [x] Satellit: Messwert-Server `GET /measurements` (`main/measurement_server.{c,h}`) mit Token-Prüfung
      (`main/bearer_token.{c,h}`), JSON-Antwort (`main/measurements_json.{c,h}`) und letzter Messung
      (`main/measurement_store.{c,h}`); 17 Host-Tests neu, gesamt 75 grün. Baut ohne Warnungen (850 KB).
      Am Board geprüft (17.09.2026) mit `curl.exe` von Windows: `200` mit JSON, ohne Token `401`
- [x] Satellit: Ping per HTTPS alle 60 s (`main/ping_client.{c,h}`), URL und Body in `main/ping_url.{c,h}` und
      `main/ping_payload.{c,h}` mit 10 Host-Tests (gesamt 85 grün). Firmware-Version `0.1.0` fest in
      `CMakeLists.txt`, Board-Name in `main.c`. Baut ohne Warnungen (1,0 MB). Am Board geprüft (17.09.2026):
      HTTPS und Zertifikat gegen `webui.gwios.gwiasda.net` funktionieren; die WebUI antwortet mangels
      Endpunkt mit `302` (Weiterleitung zum Login), die der Satellit wie vorgesehen nicht verfolgt
- [ ] GwiOS: Satelliten verwalten, Ping-Endpunkt, Pull-Dienst, Seite „Satelliten“ — nach dem Satelliten
      Hinweis aus dem Test des Satelliten: `/api/satellites/…` darf nicht unter die Cookie-/OpenID-Anmeldung
      fallen. Ohne eigene Regel leitet die WebUI den Ping mit `302` zum Keycloak-Login um; die API muss
      stattdessen selbst mit `204`, `401` oder `404` antworten.

### Entscheidungen

- Schnittstelle zum Portal (Vorgabe von Dirk, 16.09.2026): Der Satellit sendet alle 60 Sekunden einen
  HTTP-Request als Ping an GwiOS und übermittelt dabei seine Id (= Name) und seine Specs (z. B. welche
  Sensoren er hat). GwiOS holt die Messwerte dann regelmäßig selbst per HTTP-Request beim Satelliten ab
  (Pull). Der Satellit betreibt dafür einen eigenen HTTP-Server.
- Der Satellit schreibt die Adresse seines HTTP-Servers ausdrücklich in den Ping (z. B.
  `http://192.168.1.57:80`); GwiOS verwendet für den Pull die zuletzt gemeldete Adresse. Die Absender-IP
  des Requests wird nicht verwendet: Hinter Traefik und Docker ist sie nicht verlässlich die des
  Satelliten. Wechselt die IP per DHCP, kennt GwiOS die neue nach spätestens 60 Sekunden.
- Absicherung in beide Richtungen mit einem zufälligen Token pro Satellit, das Satellit und GwiOS kennen:
  Der Satellit schickt es beim Ping als `Authorization: Bearer …`, GwiOS schickt es beim Pull ebenso;
  die Gegenseite lehnt bei falschem Token ab. Ein verlorener Satellit wird über sein Token gesperrt.
  Keycloak scheidet für Satelliten aus (für Menschen im Browser gedacht, für den ESP32 zu schwer).
- Ein Satellit wird in der WebUI angelegt (Name = Id). GwiOS erzeugt dabei das Token und zeigt es genau
  einmal an; es wird von Hand in die Konfiguration des Satelliten eingetragen. Pings unbekannter Namen
  oder mit falschem Token werden abgelehnt — keine Selbstanmeldung.
- Es gibt genau ein Token pro Satellit für beide Richtungen. Weil GwiOS es für den Pull im Klartext
  braucht, wird es nicht gehasht, sondern mit ASP.NET Data Protection verschlüsselt in der Datenbank
  gespeichert (Schlüssel auf dem Pi unter `/mnt/ssd/gwios-webui/keys`). Gehen diese Schlüssel verloren,
  sind alle Tokens unlesbar und jeder Satellit braucht ein neues — die Schlüssel gehören ins Backup.
- Der Ping geht per HTTPS über Traefik an `https://webui.gwios.gwiasda.net/…`; der Satellit prüft das
  Zertifikat mit dem CA-Bundle von ESP-IDF. Der Pull geht per unverschlüsseltem HTTP im LAN: Das Token
  ist dort nur durch die WLAN-Verschlüsselung geschützt; HTTPS auf dem ESP32 (Zertifikat pro Satellit)
  ist für den Anfang zu aufwendig und kann später nachgerüstet werden.
- Die Konfiguration eines Satelliten (WLAN-Name, WLAN-Passwort, Name, Token, GwiOS-Adresse) liegt nicht in
  der Firmware, sondern im NVS-Speicher des ESP32. Quelle ist eine CSV-Datei pro Satellit, aus der ein
  NVS-Image erzeugt und zusätzlich geflasht wird. Damit gibt es eine Firmware für alle Satelliten; ein
  Firmware-Update lässt die Konfiguration unberührt. Die CSV-Dateien enthalten Geheimnisse und kommen
  nicht ins Repo. Einrichtung über ein eigenes WLAN des Satelliten (Captive Portal) ist verworfen.
- Protokoll (Entwurf von Claude, von Dirk bestätigt am 17.09.2026). Die API der WebUI liegt unter
  `/api/satellites/…`.
  - Ping: `POST https://webui.gwios.gwiasda.net/api/satellites/{name}/ping` mit
    `Authorization: Bearer <token>` und dem Body
    `{"endpoint":"http://192.168.1.57","board":"ESP32 DevKit C","firmware":"0.1.0","wifiRssiDbm":-58,"sensors":[{"type":"bme280","quantities":["temperature","humidity","pressure"]}]}`.
    Antworten: `204` bei Erfolg, `401` bei falschem Token, `404` bei unbekanntem Namen.
  - `board` ist der Typname des Boards (nicht des Sensors), `wifiRssiDbm` die aktuelle WLAN-Signalstärke
    in dBm, vor jedem Ping frisch abgefragt (Ergänzung von Dirk, 17.09.2026). Der Board-Typname steht fest
    in der Firmware neben der Pin-Belegung, die ohnehin nur zu diesem Board passt, und ist für die Anzeige
    lesbar geschrieben (`ESP32 DevKit C`), nicht maschinenlesbar.
  - Pull: `GET http://<endpoint>/measurements` mit `Authorization: Bearer <token>`. Antwort:
    `{"measurements":[{"sensor":"bme280","quantity":"temperature","value":24.20,"unit":"°C"},{"sensor":"bme280","quantity":"humidity","value":50.64,"unit":"%"},{"sensor":"bme280","quantity":"pressure","value":996.37,"unit":"hPa"}],"ageSeconds":3}`.
  - Der Name steht im Pfad, nicht im Body, damit Token und Name sichtbar zusammengehören.
  - Sensoren werden allgemein beschrieben (Typ + Messgrößen), jeder Messwert trägt seine Einheit: GwiOS
    braucht kein Wissen über einzelne Sensortypen, weitere Sensoren ändern das Format nicht.
  - Der Satellit misst selbst im Takt und liefert beim Pull die letzte Messung; `ageSeconds` gibt ihr
    Alter an. Der Satellit sendet keine Uhrzeit (keine verlässliche Uhr); den Zeitstempel setzt GwiOS
    beim Abholen.
- Messwerte werden vorerst nicht in der Datenbank gespeichert (Vorgabe von Dirk, 17.09.2026). GwiOS
  hält pro Sensor und Messgröße die letzten 10 abgeholten Werte im Speicher und zeigt daraus den aktuellen
  Wert und einen kurzen Verlauf an. Nach einem Neustart der WebUI ist der Verlauf leer.
- Pull-Intervall 30 Sekunden (einstellbar). Zwei Verläufe pro Sensor und Messgröße, beide nur im Speicher:
  - kurz: die letzten 10 abgeholten Werte (5 Minuten), wird direkt angezeigt;
  - lang: jeder 10. abgeholte Wert, höchstens 100 Werte über die Laufzeit der WebUI (gut 8 Stunden,
    der älteste fällt heraus), wird auf Klick angezeigt.
- In der Datenbank stehen nur die Satelliten selbst (Name, verschlüsseltes Token, gesperrt). Letzter Ping,
  gemeldete Adresse, Firmware, Sensoren und Verläufe liegen nur im Speicher; nach einem Neustart der WebUI
  kennt GwiOS die Adressen erst wieder mit dem nächsten Ping (spätestens nach 60 Sekunden).
- Ein Satellit gilt als offline, wenn 3 Minuten lang kein Ping kam (drei verpasste Pings), und wird dann
  nicht mehr abgefragt, bis er sich wieder meldet.
- Das Abholen übernimmt vorerst ein Hintergrunddienst in der WebUI (kein eigener Container). Später soll
  ein allgemeines System zur zeitgesteuerten Verarbeitung von Tasks entstehen, das diesen Dienst ablöst
  (Vorgabe von Dirk, 17.09.2026).
- Tests der Firmware (Entscheidung von Dirk zu R5, 17.09.2026): Host-Tests mit Unity für die reine Logik
  (Token-Prüfung, JSON von Ping und Messwerten, Konfiguration prüfen, BME280-Kompensation), lauffähig in
  der VM ohne Board. Ablage unter `tests/GwiOS.Satellite.Bme280.Tests` mit gespiegelter Struktur wie in R5.
  Hardwarenahes (WLAN, I²C, HTTP-Server, Ping) wird als Integrationstest am Board geprüft; der Ablauf
  steht in der README des Satelliten.
- Umsetzung der Firmware-Tests: Pro getestetem Modul ein Verzeichnis, pro getesteter Funktion eine Testdatei
  `<funktion>_tests.c` mit eigenem `main` und eigenem CTest-Eintrag. Testbare Logik steht in Modulen ohne
  ESP-IDF-Abhängigkeit; hardwarenahe Module rufen sie nur auf. Die Formeln werden gegen das Rechenbeispiel
  des Datenblatts geprüft (25,08 °C, 100653 Pa), die Feuchte gegen die Gleitkomma-Referenzformel des
  Datenblatts, weil es dafür kein Rechenbeispiel gibt. Unity braucht dafür `UNITY_INCLUDE_DOUBLE`.
- Host-Tests werden in einem normalen Terminal gebaut, nicht im ESP-IDF-Terminal: Dessen `PATH` stellt
  `esp32ulp-elf/bin/ld` vor den System-Linker, `cmake` scheitert dann schon am Compiler-Test.
- Name eines Satelliten (= Id, steht in der URL des Pings): nur Kleinbuchstaben `a–z`, Ziffern und `-`,
  1–32 Zeichen. Ein lesbarer Anzeigename kann später in GwiOS dazukommen.
- Die CSV-Dateien mit der Konfiguration liegen außerhalb der VM auf dem Windows-Rechner, geflasht wird dort
  (Vorgabe von Dirk, 17.09.2026). Im Repo liegt nur eine Vorlage mit Dummy-Werten.
- Fehlt die Konfiguration oder ist sie ungültig, misst der Satellit trotzdem und gibt die Werte auf der
  Konsole aus; er meldet deutlich, welcher Wert fehlt oder ungültig ist, und startet WLAN, Ping und Server
  nicht.
- Die Konfiguration liegt in einer eigenen NVS-Partition `config` (0x10000, 12 KB, `partitions.csv`),
  nicht in der Standard-Partition `nvs`: Die nutzt der WLAN-Treiber für eigene Daten, die beim Einspielen
  einer Konfiguration sonst mit überschrieben würden. Namespace `satellite`, Schlüssel `name`, `token`,
  `wifi_ssid`, `wifi_password`, `gwios_url`.
- `config` liegt vor der App, nicht dahinter: So hängt ihre Adresse nicht von der Größe der App-Partition ab.
  Die App beginnt deshalb erst bei 0x20000 (App-Partitionen müssen auf 64 KB ausgerichtet sein) und hat
  1,9 MB statt 1 MB, weil WLAN, HTTPS und HTTP-Server die Firmware auf etwa 1 MB bringen. Umgestellt vor
  Schritt 3; die Konfiguration musste dafür einmal neu geflasht werden.
- Regeln der Konfiguration (Entscheidung von Claude, soweit nicht oben vorgegeben): Token 32–128 Zeichen im
  base64url-Alphabet (passt ohne Kodierung in den HTTP-Header; GwiOS muss Tokens in diesem Format
  erzeugen); WLAN-Passwort leer oder 8–63 druckbare ASCII-Zeichen; `gwios_url` nur `https://`, ohne `/` am
  Ende. Fehler werden mit dem Schlüssel geloggt, nie mit dem Wert.
- Geflasht wird unter Windows mit `Flash-Satellite.ps1`, das das Python der Windows-Installation von ESP-IDF
  v6.1 nutzt (`esptool`, `esp_idf_nvs_partition_gen`, `esp_idf_monitor`; Entscheidung von Dirk). Die
  Firmware baut die VM; `export-firmware.sh` kopiert die Images laut `flasher_args.json` nach `dist/`
  (in `.gitignore`), weil das Build-Verzeichnis nicht im Freigabeordner liegen kann. Das Skript liest
  Offset und Größe von `config` aus `partitions.csv`, fordert zum Handgriff für den Download-Modus auf,
  flasht mit `--before no-reset` und 115200 Baud und löscht das NVS-Image mit den Geheimnissen danach.
  Die CSV-Dateien liegen unter `%USERPROFILE%\.gwios\satellites\<name>.csv`.
- WLAN (Entscheidungen von Claude): Nach einem Verbindungsverlust versucht der Satellit es nach 1 s erneut,
  die Pause verdoppelt sich bis höchstens 60 s und fällt nach einer erfolgreichen Verbindung auf 1 s zurück.
  Mit Passwort wird mindestens WPA2 verlangt, damit kein schwächer gesichertes Netz gleichen Namens genommen
  wird. Energiesparmodus des Funkmoduls aus (`WIFI_PS_NONE`): Er verzögert eingehende Anfragen von GwiOS
  bis zum nächsten Beacon; der Satellit hängt am USB-Netzteil. Hostname `gwios-<name>` für die
  Geräteliste des Routers. Die Standard-Partition `nvs` wird gelöscht, wenn sie unbrauchbar ist — dort
  liegen nur Daten des WLAN-Treibers.
- Messwert-Server (Entscheidungen von Claude): Port 80. Ohne gültiges Token `401` mit
  `WWW-Authenticate: Bearer`, vor der ersten Messung `503`. Das Schema `Bearer` wird ohne Beachtung der
  Groß-/Kleinschreibung erkannt (HTTP-Regel), das Token selbst in konstanter Zeit verglichen. Ein
  Authorization-Header, der länger ist als jedes gültige Token, wird gar nicht erst gelesen. Schlägt eine
  Messung fehl, bleibt die letzte gültige stehen und `ageSeconds` wächst weiter — so erkennt GwiOS veraltete
  Werte. Das JSON wird mit `snprintf` erzeugt statt mit einer JSON-Bibliothek: Alle Zeichenketten sind feste
  Bezeichner. Die Messstruktur steht in `bme280_measurement.h`, damit die Host-Tests sie ohne I²C-Treiber
  nutzen können.
- Ping (Entscheidungen von Claude): Der erste Ping geht sofort nach dem Verbindungsaufbau raus, danach alle
  60 s; ohne Verbindung prüft der Ping-Task jede Sekunde. Fehlgeschlagene Pings werden nur geloggt, der
  nächste reguläre Ping ist der neue Versuch. Weiterleitungen (3xx) werden nicht verfolgt, sonst ginge das
  Token an den Zielserver der Weiterleitung (z. B. den Keycloak-Login). Zeitlimit 10 s. Das Zertifikat wird
  gegen das CA-Bundle von ESP-IDF geprüft, ohne Prüfung des Ablaufdatums (der Satellit hat keine Uhrzeit).
  Die Firmware-Version steht fest in `CMakeLists.txt` (`PROJECT_VER`), weil `git describe` im
  Freigabeordner scheitert. Board-Name und Firmware-Version werden nicht JSON-kodiert, sondern bei
  Anführungszeichen, Backslash oder Steuerzeichen abgelehnt — es sind Konstanten der Firmware.
- Reihenfolge: Erst wird der Satellit fertig, die GwiOS-Seite (API, Hintergrunddienst, Seite „Satelliten“)
  folgt danach gemeinsam mit Dirk. Für die Seite gibt es bereits eine Designvorlage
  (`docs/UI/GwiOS.WebUI.Design.html`); Inhalt und Berechtigungen werden dann anhand der Vorlage geklärt.

- Satelliten-Quellcode liegt unter `src/GwiOS.Satellites`, getrennt von den .NET-Projekten.
- Der ESP32 wird in C programmiert, nicht in C++/Arduino-Sketch-Form.
- Entwickelt wird inzwischen in einer Ubuntu-VM (VMware, Repo über den freigegebenen Ordner
  `/mnt/hgfs/Repos/gwios`), nicht mehr auf dem Windows-Rechner. Die Windows-Einträge unten bleiben
  als Historie stehen.
- Linux-Voraussetzungen für ESP-IDF per apt: `git wget flex bison gperf python3 python3-pip
  python3-venv cmake ninja-build ccache libffi-dev libssl-dev dfu-util libusb-1.0-0`. Ohne sie
  bricht der Installation Manager ab. Zugriff auf `/dev/ttyUSB0` erst nach Aufnahme in `dialout`
  und neuer Anmeldung.
- ESP-IDF nicht über das Offline-Archiv des Installation Managers installieren: Er entpackt die
  2,5 GB nach `/tmp`, das unter Ubuntu 26.04 ein tmpfs im RAM ist (3,7 GB), und scheitert am Platz.
  Stattdessen online und nur für den ESP32:
  `TMPDIR=~/.cache/eim-tmp ~/.espressif/eim_gui/eim install -i v6.1 -t esp32 -p ~/.espressif --idf-features ide -n true`.
  Bricht dabei die Python-Umgebung wegen eines Netzaussetzers ab, vervollständigt
  `eim fix -p ~/.espressif/v6.1/esp-idf -n true` die Installation (mit `PIP_DEFAULT_TIMEOUT=120 PIP_RETRIES=10`).
- Nach dem abgebrochenen ersten Versuch zeigte `idfSelectedId` in `~/.espressif/tools/eim_idf.json`
  ins Leere; `eim select v6.1` behebt das, danach zeigt VS Code die Version an.
- Das Build-Verzeichnis liegt außerhalb des Freigabeordners: `idf.py -B ~/build/gwios-satellite-bme280 …`.
  Im Freigabeordner scheitert der Build mit `File name too long`, weil die Dateien auf dem Windows-Host
  liegen (Pfadgrenze 260 Zeichen) und ESP-IDF v6.1 für mbedtls Objektpfade von 269 Zeichen erzeugt.
  Das `-B` gehört deshalb zu jedem `idf.py`-Aufruf (build, flash, monitor).
- Unter Linux klappt der automatische Wechsel in den Download-Modus: `idf.py … -p /dev/ttyUSB0 flash`
  flasht ohne Handgriff an BOOT/EN (anders als unter Windows, siehe unten).
- Flashen mit 460800 Baud kann über die USB-Durchreichung der VM scheitern (`Failed to start stub flasher`,
  unsinnige Quarzfrequenz wie „15.55 MHz“). Mit `-b 115200` läuft es zuverlässig, dauert aber länger.
- Den Monitor beendet `Strg` + `]`, auf deutscher Tastatur im VS-Code-Terminal kommt das aber nicht an.
  Alternative: `Strg` + `T`, dann `Strg` + `X` — oder das Terminal schließen.
- Der Build meldet `detected dubious ownership` für das Repo (Freigabeordner gehört `root`); damit fällt
  `PROJECT_VER` auf `1` zurück. Für den Build ohne Folgen, noch nicht behoben.
- Das Board ist ein klassisches ESP32 DevKit C (ESP32-WROOM-32, USB-Seriell über CP2102), kein ESP32-S3.
  Daher `SDA` = GPIO 21 und `SCL` = GPIO 22 nach üblicher Konvention. Die Herstelleranleitung nennt
  IO17/IO18, das gilt aber für den S3 — auf dem existiert GPIO 22 nicht (nur GPIO 0–21 und 26–48).
  Beim ESP32 ist I²C ohnehin über die I/O-Matrix frei zuweisbar.
- Entwickelt wird mit ESP-IDF (Espressifs eigenem Framework, reines C), nicht mit dem Arduino-Core.
  Installation über den Espressif Installation Manager, https://dl.espressif.com/dl/eim/.
- Verwendet wird der neue I²C-Treiber (`driver/i2c_master.h`), nicht der alte `driver/i2c.h`.
  Deshalb hängt `main` an der Komponente `esp_driver_i2c`.
- Der EIM hatte nur das Repository geklont, ohne Toolchain und Python-Umgebung; `export.ps1` scheiterte
  daran. Nachgeholt mit `install.ps1 esp32` aus dem Repo — das Argument beschränkt den Download auf den
  Compiler für den klassischen ESP32.
- Der automatische Wechsel in den Download-Modus funktioniert auf diesem Board nicht: `idf.py flash`
  scheitert mit `Wrong boot mode detected (0x13)`. Vor dem Flashen deshalb von Hand: BOOT halten,
  EN kurz drücken, BOOT loslassen. Danach flashen mit `--before no-reset`, sonst fällt der Chip sofort
  wieder heraus. Das Zurücksetzen über RTS nach dem Flashen funktioniert dagegen.
- Entwickelt wird in VS Code mit der Erweiterung `espressif.esp-idf-extension` (2.2.0), die auf die
  vorhandene Installation zeigt — kein zweites ESP-IDF.
- Version 2.2.0 hat keinen Einrichtungsassistenten mehr (`ESP-IDF: Configure ESP-IDF Extension`
  existiert nicht), sondern liest eine `eim_idf.json`, die der Installation Manager hätte schreiben
  sollen. Da sein Lauf abgebrochen war, ist sie von Hand angelegt: `C:/Users/dirk/.espressif/eim_idf.json`,
  verdrahtet über die VS-Code-Einstellung `idf.eimIdfJsonPath`. Format aus dem Code der Erweiterung
  abgeleitet, nicht aus einer Dokumentation.
- Die Erweiterung war anfangs unsichtbar, weil VS Code sie in einem nicht vertrauenswürdigen Workspace
  (Workspace Trust) deaktiviert hatte. Kein einziger Befehl erscheint dann in der Palette.
- Ein Satellit ist ein eigenes ESP-IDF-Projekt in einem eigenen Ordner unter `src/GwiOS.Satellites`.
  Ordnername nach Repo-Konvention (`GwiOS.Satellite.Bme280`), CMake-Projektname ohne Punkte
  (`gwios_satellite_bme280`), weil daraus die Namen der Build-Artefakte entstehen.
- Die `coding-guidelines.md` gelten nur für die .NET-Projekte (Anweisung von Dirk, 16.09.2026); der
  Geltungsbereich steht jetzt so im Dokument. Die Firmware folgt den ESP-IDF-Konventionen:
  `snake_case` für Funktionen und Variablen, `SCREAMING_SNAKE_CASE` für Makros, englische
  Bezeichner, kleine Funktionen mit je einer Aufgabe, Kommentare erklären das Warum.
- Der BME280 wird über I²C angebunden (vier Leitungen), nicht über SPI: weniger Leitungen, und das
  Modul ist der einzige Teilnehmer am Bus.
- Versorgung des Sensors aus dem 3V3-Pin des ESP32, nicht aus 5 V — der BME280 selbst ist ein reiner
  3,3-V-Baustein.
- Verwendet wird ein 6-poliges Kombimodul (VCC, GND, SCL/SCK, SDA/MOSI, MISO/ADDR, CS) mit
  Steckverbinder und konfektioniertem Kabel; es beherrscht I²C und SPI.
- `CS` hat auf dem Modul einen Pull-up nach `VCC` (gemessen 4,69 kΩ, stromlos zwischen grüner und
  roter Ader). Das Modul startet damit von selbst im I²C-Modus, die grüne Ader bleibt unverbunden.
  Das ist auch praktisch nötig, weil das DevKit nur einen einzigen `3V3`-Pin hat.
- `ADDR` bleibt unverbunden: Die Herstelleranleitung gibt für den I²C-Betrieb bei `MISO/ADDR` und `CS`
  ausdrücklich `NC` an, die Onboard-Widerstände legen beide Pins fest. Aus den Messwerten allein war
  das nicht ableitbar (9,4 kΩ gegen `GND`, aber auch 4,7 kΩ gegen `VCC`).
- Versorgung ausschließlich mit 3,3 V, obwohl der Hersteller „3.3V/5V“ angibt: Die I²C-Pull-ups liegen
  an `VCC`, bei 5 V lägen also 5 V auf `SDA`/`SCL` und die ESP32-GPIOs (max. 3,6 V) nehmen Schaden.
- Die I²C-Pull-ups auf `SDA` und `SCL` sitzen auf dem Modul (je 4,7 kΩ gegen `VCC` gemessen);
  externe Widerstände sind nicht nötig.
- Die I²C-Adresse ist `0x77`. Das Hersteller-Wiki (seengreat, https://seengreat.com/wiki/87/bme280-environmental-sensor)
  schreibt: „ADDR is connected to high level by default, and the address is 0X77.“ Das deckt sich mit der
  Messung von 4,7 kΩ zwischen `ADDR` und `VCC`. Für einen zweiten BME280 am selben Bus legt man dessen
  `ADDR` auf `GND`, der bekommt dann `0x76`.

### Offene Fragen

- Wie messen die Satelliten, ohne dass die Eigenwärme des ESP32 die Temperatur verfälscht (Abstand,
  Gehäuse, Korrekturwert)?
- Ist `webui.gwios.gwiasda.net` aus dem Internet erreichbar oder nur im LAN? (Für den Ping ist HTTPS
  ohnehin beschlossen; die Antwort bestimmt, wie streng der Endpunkt gegen Missbrauch zu schützen ist.)
