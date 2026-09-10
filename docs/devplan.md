# Entwicklungsplan

## Logging (GwiOS.Core/CrossCutting/Logging)

- [x] Domäne: `LogEntry`, `LogLevel`, `ILogger<T>` / `DefaultLogger<T>`, `ILogEntryManager` / `LogEntryManager`
- [x] `LogEntryPostgresRepository` mit direktem SQL über Npgsql (kein Entity Framework)
- [x] `ILogEntryRepository.EnsureStorageCreatedAsync()` — legt idempotent die GwiOS-Datenbank und die Logging-Tabellen an
- [x] `AddGwiOSCore(connectionString)` — `IServiceCollection`-Extension für GwiOS.Core
- [ ] WebUI: `AddGwiOSCore` in `Program.cs` aufrufen, Connection String konfigurieren (Passwort per User Secrets),
      `EnsureStorageCreatedAsync()` beim Start ausführen
- [ ] Integrationstest des Repositorys gegen eine echte PostgreSQL-Instanz

### Entscheidungen

- Datenzugriff über `NpgsqlDataSource` (Singleton, per `Npgsql.DependencyInjection` registriert). Alle künftigen
  Repositories teilen sich diese Data Source der GwiOS-Datenbank.
- Zum Anlegen der Datenbank gibt es eine zweite, keyed registrierte Data Source auf die Wartungsdatenbank `postgres`
  (ohne Pooling), abgeleitet aus demselben Connection String. Der Name der GwiOS-Datenbank kommt aus `Database=...`.
- Logging-Tabellen liegen im Schema `logging`: Tabelle `logging.log_entries`, Index auf `(app_name, timestamp DESC)`.
- `ContextData` wird als `jsonb` gespeichert, `LogLevel` als `smallint` (numerischer Enum-Wert).
- Log-Einträge einer App werden neueste zuerst geliefert, App-Namen alphabetisch.

### Offene Fragen

- Bedeutung von `LogLevel.None` (für die XML-Doku nach DOC-5 zu klären).
- `LogEntryManager` und `DefaultLogger<T>` verwenden Primärkonstruktor-Parameter direkt statt eines
  `private readonly`-Felds (DI-6) — anpassen?
