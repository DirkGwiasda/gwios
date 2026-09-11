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
- [ ] WebUI: kein Testprojekt vorhanden; `Program` (Composition Root) ist daher nicht durch Tests abgedeckt

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
- Fixture und Collection liegen unter `tests/GwiOS.Core.Tests/TestInfrastructure/`, weil Logging- und
  Person-Integrationstests sie teilen. Die Fixture verdrahtet wie `AddGwiOSCore`, ersetzt aber `ILogger<T>` durch
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

- Soll ein Testprojekt `GwiOS.WebUI.Tests` angelegt werden (R5), und was soll es für die Composition Root abdecken?

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
