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
