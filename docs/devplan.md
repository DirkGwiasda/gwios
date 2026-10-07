# Entwicklungsplan

## Logging (GwiOS.Core/CrossCutting/Logging)

Fertig: Domäne (`LogEntry`, `ILogger<T>`, `LogEntryManager`), Postgres-Repository, `EnsureStorageCreatedAsync`,
`AddGwiOSCore`, WebUI-Integration, Paging (`GetLogEntryPageAsync`/`GetLogEntryPageByAppAsync`).

- [ ] WebUI: `Program` (Composition Root) ist nicht durch Tests abgedeckt.

### Entscheidungen (gelten als Konvention für künftige Repositories)

- Datenzugriff über `NpgsqlDataSource` (Singleton, per `Npgsql.DependencyInjection`). Alle Repositories teilen
  sich diese Data Source.
- Die Datenbank wird nicht von der Anwendung angelegt, sie muss bereits existieren; Repositories stellen nur
  Schema/Tabelle/Index sicher (Recht `CREATE` auf der DB nötig). Jede Domäne bekommt ihr eigenes Schema.
- Paging geschieht in der Datenbank (`LIMIT`/`OFFSET` + `count(*)`), nicht in der WebUI.
- Connection Strings stehen nie im Repository, sondern in Umgebungsvariablen (`GwiOS.DB.Connectionstring` für
  die Anwendung, `GwiOS.DB.TestConnectionstring` für Integrationstests).
- Die WebUI legt Tabellen beim Start an; ist die Datenbank nicht erreichbar, startet sie nicht.
- Stolperstein: `NpgsqlDataReader` bietet seine `Get…("spaltenname")`-Overloads (statt Ordinalzahl) nur, wenn
  die Datei `using System.Data;` hat — fehlt es, meldet der Compiler bei jedem `reader.GetGuid("id")` &Auml;.
  `error CS1503: cannot convert from 'string' to 'int'`, weil er auf die Ordinal-Overload ausweicht. Jedes
  neue `*PostgresRepository.cs` bekommt dieses `using` von Anfang an (entdeckt bei `CategoryPostgresRepository`,
  18.09.2026).

### Tests (gilt projektweit)

- Stolperstein: In der VM nie mit Standard-Ausgabeordnern (`bin/`/`obj/`) bauen — die liegen im Freigabeordner,
  der CSS-Scope-Hash unterscheidet sich zwischen Linux- und Windows-Pfaden und wirft komponenteneigene Styles
  raus. Immer mit eigenem Ausgabeordner bauen: `dotnet test --project <Projekt> --artifacts-path ~/build/gwios-dotnet`.
- `dotnet test` läuft über die Microsoft Testing Platform (`global.json`, Pflicht für xunit v3 unter .NET 10):
  `dotnet test --solution src/GwiOS.slnx` bzw. je Projekt.
- Testverzeichnis/-namespace heißt wie die getestete Klasse (R5, CORE-3); weil der Namespace die Klasse
  verdeckt, Zugriff per `using`-Alias `<Klasse>UnderTest`.
- Implementierungen sind `internal` (DI-3), per `InternalsVisibleTo` für die Testprojekte freigegeben.
- Integrationstests laufen in gemeinsamen xunit-Collections nacheinander, mit eindeutigen Namen und Aufräumen
  der eigenen Daten. Fixtures liegen unter `tests/GwiOS.Core.Tests/TestInfrastructure/` und werden
  domänenübergreifend geteilt.
- Methoden mit `CancellationToken` bekommen in Tests `TestContext.Current.CancellationToken` (xUnit1051).
- Fehlt `GwiOS.DB.TestConnectionstring`, schlagen Integrationstests mit klarer Meldung fehl statt übersprungen
  zu werden.

### Offene Fragen

- Was soll `GwiOS.WebUI.Tests` für die Composition Root (`Program`) abdecken? Ein Start der App braucht
  Datenbank und Keycloak.

## Personen (GwiOS.Core/CrossCutting/Persons)

Fertig: Manager, Validator, Postgres-Repository, Exceptions (`PersonValidationException`,
`DuplicatePersonException`, `PersonNotFoundException`), Tests grün.

### Entscheidungen

- Eindeutigkeit (ID, Name, ShortName, IdentityUserId) erzwingt allein die Datenbank per Unique-Index, auch bei
  gleichzeitigen Zugriffen; das Repository übersetzt eine Verletzung in `DuplicatePersonException`.
- Der Validator prüft nur die Werte einer einzelnen Person, nicht die Eindeutigkeit.
- `UpdatePersonAsync` einer nicht gespeicherten Person wirft `PersonNotFoundException`;
  `DeletePersonAsync` ist idempotent.

### Offene Fragen

- Sollen Name und ShortName ohne Beachtung der Groß-/Kleinschreibung eindeutig sein? Derzeit sind „Anna“ und
  „anna“ zwei verschiedene Namen.

## ToDos (GwiOS.Core/ToDos)

Fertig: Manager, Validator, Postgres-Repository, Exceptions (`ToDoValidationException`, `ToDoNotFoundException`),
Tests grün.

### Entscheidungen

- `ToDo.DueDate` ist `DateOnly?` statt `DateTime?`; `CreatedAt`/`CompletedAt` bleiben UTC-`DateTime`.
- `CompleteToDoAsync` setzt `IsCompleted`/`CompletedAt` über den injizierten `TimeProvider` (DI-5).

### Offene Fragen

- Gehören ToDos einer Person (Zuständigkeit/Ersteller) oder sind sie familienweit? Derzeit familienweit.
- Soll die Reihenfolge per Drag & Drop änderbar sein (`Position`, wird derzeit nicht gesetzt), und soll ein
  erledigtes ToDo wieder geöffnet werden können?

## Haushaltsbuch (GwiOS.Core/Haushaltsbuch, WebUI unter `/haushaltsbuch`)

Architektur mit Dirk abgestimmt (18.09.2026), siehe `docs/architecture.md`, Abschnitt „Haushaltsbuch“. Noch
nichts umgesetzt.

- [x] `Category` (Model, Contracts, Manager, Validator, Postgres-Repository, Tests) — Code steht, DI-Registrierung
      und Startup-Aufruf (`Program.EnsureStorageCreatedAsync`) ergänzt. `dotnet test` lokal grün (435 Unit-Tests;
      die 11 neuen `CategoryPostgresRepository`-Integrationstests schlagen wie alle anderen Repository-Tests nur
      mangels `GwiOS.DB.TestConnectionstring` fehl, siehe R5-Stolperstein oben). .NET-10-SDK wurde dafür in dieser
      Sandbox per `dotnet-install.sh` nach `~/.dotnet` installiert (kein sudo nötig).
- [x] `RecurringItem` (Model, Contracts, Manager, Validator, Postgres-Repository, Tests) — Code steht, DI und
      Startup-Aufruf ergänzt. `dotnet test` lokal grün (470 Unit-Tests; die 8 neuen
      `RecurringItemPostgresRepository`-Integrationstests schlagen wie alle anderen nur mangels
      `GwiOS.DB.TestConnectionstring` fehl). Validator prüft nur die Werte des Postens selbst (wie
      `ToDoValidator`/`PersonValidator`), nicht ob `CategoryId` wirklich existiert oder zum Kategorietyp passt —
      das bleibt bewusst aus, weil `RecurringItemManager` laut Architektur keinen `CategoryManager` injiziert
      bekommen darf; die Datenbank sichert die Referenz per Fremdschlüssel ab.
- [x] `Booking` (Model, Contracts, Manager, Validator, Postgres-Repository, Tests) — Code steht, DI und
      Startup-Aufruf ergänzt. `dotnet test` lokal grün (499 Unit-Tests; die 10 neuen
      `BookingPostgresRepository`-Integrationstests schlagen wie alle anderen nur mangels
      `GwiOS.DB.TestConnectionstring` fehl). Lesen geschieht über `GetBookingsInRangeAsync(fromDate, toDate)`
      (inklusive, neueste zuerst) statt separater Monats-/Zeitraum-Methoden — ein Monat ist einfach der Bereich
      von dessen erstem bis letztem Tag. `RecurringItemId` ist rein informativ: Fremdschlüssel mit
      `ON DELETE SET NULL`, damit das freie Löschen eines Fixpostens (siehe RecurringItem-Entscheidung oben)
      bestehende Buchungen nicht blockiert.
- [x] Workflows (`RecurringBookingGenerationWorkflow`, `HaushaltsbuchOverviewWorkflow`, `CategoryDeletionWorkflow`)
      + `MonthReviewManagement` (Model, Contracts, Manager, Validator, Postgres-Repository) + Tests, DI ergänzt.
      `dotnet test` lokal grün (536 Unit-Tests; die 5 neuen `MonthReviewPostgresRepository`-Integrationstests
      schlagen wie alle anderen nur mangels `GwiOS.DB.TestConnectionstring` fehl). Ein eigenständiger
      `HaushaltsbuchReportWorkflow` erübrigte sich — `HaushaltsbuchOverviewWorkflow` nimmt jetzt `fromDate`/`toDate`
      statt eines einzelnen Monats und bedient damit Dashboard, Fixkosten, Kategorien und Berichte gleichermaßen.
      Beim Umsetzen fiel auf, dass Kategorie-Löschen zusätzlich zum Unterkategorien-Check (`CategoryManager`) auch
      die Verwendung in Buchungen/Fixposten prüfen muss — dafür kam `CategoryDeletionWorkflow` mit
      `CategoryInUseException` hinzu (siehe architecture.md).
- [x] WebUI-Grundgerüst (Nav-Eintrag, `HaushaltsbuchLayout`, `HaushaltsbuchHome`-Redirect, Routing) — `_Imports.razor`
      unter `Components/Pages/Haushaltsbuch` setzt `@layout HaushaltsbuchLayout`, analog zum Admin-Bereich.
- [x] Seite Kategorien — zwei Bäume (Einnahmen/Ausgaben) über die neue Komponente `GwiOSCategoryTree` (rekursiv,
      ein-/ausklappbar), Dialog mit `GwiOSCategorySelect` für die übergeordnete Kategorie und Farbwahl.
- [x] Seite Fixkosten — Tabelle mit `GwiOSCategorySelect`, Turnus-Auswahl inkl. neuem Feld „Startmonat“ (nur bei
      quartalsweise/jährlich sichtbar; im Mock-Dialog fehlte dieses Feld komplett, obwohl die Beispieldaten
      unterschiedliche Startmonate nutzen — ohne das Feld hätte ein neu angelegter quartalsweiser/jährlicher Posten
      nie richtig phasiert werden können).
- [x] Seite Dashboard — `GwiOSMonthSwitcher`, drei `GwiOSKpiTile`, Buchungstabelle und -dialog, Monatsstart-Popup
      (`RecurringBookingGenerationWorkflow`), Button „Fixbuchungen generieren“ als manueller Zusatzweg.
- [x] Seite Berichte — Zeitraum-Auswahl, Soll/Ist-Balken, `GwiOSDonutChart` (mit Drill-down), `GwiOSLineChart`.
      Ein eigener Monatsumschalter entfiel hier bewusst (siehe Entscheidung unten).
- [x] Neue Komponenten: `GwiOSMonthSwitcher`, `GwiOSKpiTile`/`GwiOSKpiAccent`, `GwiOSCategorySelect`,
      `GwiOSCategoryTree`, `GwiOSDonutChart`/`GwiOSDonutSegment`, `GwiOSLineChart`/`GwiOSLineSeries`. Alle mit
      `.razor.css`, Farben/Radien/Abstände über die vorhandenen `--gwios-*`-Variablen aus `app.css`, keine
      hartkodierten Werte außer der siebenteiligen Kategorie-Farbpalette.
      `dotnet build src/GwiOS.WebUI` fehlerfrei; die WebUI wurde in dieser Sitzung erstmals lokal gegen eine echte
      Postgres-Datenbank gestartet (siehe unten) und legt beim Start alle vier neuen Tabellen korrekt an.
- [x] Button „Fixbuchungen generieren“ übernimmt nicht mehr blind alle fälligen Fixposten, sondern öffnet denselben
      Auswahldialog wie die Monatsprüfung (alle vorausgewählt, einzeln abwählbar); anders als dort lässt er sich
      abbrechen, ohne etwas zu buchen (Vorgabe von Dirk, 19.09.2026). Zwei neue E2E-Tests in
      `DashboardPageTests`; `dotnet build` fehlerfrei, `GwiOS.WebUI.Tests` grün (366). **Die E2E-Tests sind noch
      nicht gelaufen** — in dieser Sitzung lag kein `GwiOS.DB.TestConnectionstring` vor.
- [x] Überarbeitung nach Abgleich mit der Vorlage (07.10.2026, Anlass: Kategorien-Ansicht nicht responsive, Boxen
      nicht in voller Breite, Schriftgrößen/Farben abweichend). Alle vier Tabs Text für Text gegen die gerenderte
      Vorlage geprüft (Schriftart, -größe, -stärke, Farbe) und bei 1280/900/390 px verglichen. Wesentliche Fehler:
      Kategorien-Raster `auto-fill` (dritte leere Spalte), `GwiOSSection` statt Kartenkopf mit Leiste, Ausgaben blau
      statt rot, keine Abstände zwischen den Blöcken (`.gwios-page` ohne `gap`), Monatsumschalter als Panel im Panel,
      Bearbeiten-Knöpfe leuchteten rot statt blau, Aktionen brachen um, Tabellenköpfe 13,5 statt 12,5 px. Außerdem
      zählte die Typsumme im Kategorien-Kopf Unterkategorien doppelt (Summe einer Kategorie enthält schon ihre
      Unterkategorien) — behoben. Neu: `GwiOSCard` (+ `GwiOSCardAccent`/`GwiOSCardVariant`), `GwiOSCategoryLabel`
      (Farbpunkt + Pfad, ersetzt die doppelte Pfadlogik in Dashboard/Fixkosten), `GwiOSLineChartLegend`; Varianten
      `GwiOSTabNavVariant.Pills` und `GwiOSPillGroupVariant.Toggles`; zentrale Klassen `gwios-toggle-button`,
      `gwios-toolbar-panel`, `gwios-stack--compact`, `gwios-table--compact`, `gwios-table-end`. bUnit-Tests für alle
      geänderten/neuen Komponenten, `GwiOS.WebUI.Tests` grün (413). **E2E-Tests nicht gelaufen** (kein
      `GwiOS.DB.TestConnectionstring`); der Regex für den Button „Fixbuchungen für … generieren“ wurde angepasst.
- [x] Fixkosten-Tabelle verschlankt (Vorgabe von Dirk, 07.10.2026): Spalten „Fälligkeit“, „pro Monat“ und
      „Status“ entfernt, Betrag in derselben Schriftgröße (13,5 px) wie die übrigen Spalten. Inaktive Posten stehen
      weiterhin in der Liste (sonst ließen sie sich nicht wieder aktivieren); ihren Zustand zeigt nur noch der Knopf
      „Aktivieren“/„Deaktivieren“. Fälligkeit und Startmonat bleiben im Bearbeiten-Dialog sichtbar.
- [x] Statt bUnit-Tests je Seite: echtes End-to-End-Testprojekt `tests/GwiOS.WebUI.E2E.Tests` mit Playwright
      gegen die real laufende WebUI (auf Wunsch von Dirk, 18.09.2026, als stärkerer Nachweis als Komponententests
      mit gerendertem Markup). Details siehe eigener Abschnitt unten.

### Entscheidungen

- Datenmodell: `Category` (Name, Typ Einnahme/Ausgabe, ParentId, Farbe nur an Wurzeln), `Booking` (Datum, Typ,
  Kategorie, Betrag, Beschreibung, Status gebucht/ausstehend, optionaler `RecurringItemId`-Verweis),
  `RecurringItem` (Name, Typ, Kategorie, Betrag, Turnus, Fälligkeitstag, Startmonat, aktiv). `Booking.Type` wird
  redundant gespeichert statt aus der Kategorie abgeleitet, weil sich der Typ einer Kategorie nach dem Anlegen
  nicht mehr ändert und das jede Monatsaggregation einen Join spart.
- Vier Manager (`CategoryManagement`, `BookingManagement`, `RecurringItemManagement`, `MonthReviewManagement`),
  drei Workflows (`RecurringBookingGenerationWorkflow`, `HaushaltsbuchOverviewWorkflow`, `CategoryDeletionWorkflow`);
  einzelne CRUD-Vorgänge laufen ohne Workflow direkt über den jeweiligen Manager (bestätigt von Dirk, 18.09.2026).
- Eigenes Schema `haushaltsbuch` mit den Tabellen `categories`, `bookings`, `recurring_items`, dazu
  `month_reviews(month date PK, reviewed_at)` für den Monatsstart-Check (bestätigt von Dirk, 18.09.2026).
- Kategorien sind beliebig verschachtelbar, begrenzt auf maximal 5 Ebenen; nur Wurzelkategorien tragen eine Farbe
  (bestätigt von Dirk, 18.09.2026).
- Familienweiter Zugriff ohne Rollen-/Personenbeschränkung, wie ToDos (bestätigt von Dirk, 18.09.2026).
- Kein Kontostand-Übertrag zwischen Monaten; Saldo = Einnahmen minus Ausgaben des gewählten Monats (bestätigt von
  Dirk, 18.09.2026).
- Statt eines reinen Buttons zeigt die Dashboard-Seite beim ersten Besuch eines ungeprüften Monats automatisch ein
  Popup mit den fälligen Fixposten zum Abhaken; erst eine Bestätigung markiert den Monat als geprüft. Der Button
  „Fixbuchungen generieren“ bleibt zusätzlich als manueller Weg bestehen (Vorgabe von Dirk, 18.09.2026).
- Der Monatsstart-Check löst gezielt beim Besuch des Dashboard-Tabs aus (dort lebt der Monatsumschalter), nicht
  allgemein beim Betreten des Haushaltsbuch-Bereichs; ein Verlassen des Popups ohne Bestätigen lässt den Monat
  ungeprüft, sodass es beim nächsten Besuch erneut erscheint (Vorschlag von Claude, so umgesetzt, 18.09.2026 —
  noch nicht von Dirk gegengeprüft).
- `RecurringBookingGenerationWorkflow.ConfirmAsync` markiert den Monat immer als geprüft, auch wenn keine
  Vorschläge ausgewählt wurden — sonst käme das Popup bei jedem Besuch wieder, obwohl bewusst nichts gebucht
  werden sollte.
- Der gewählte Monat wird nicht seitenübergreifend gemerkt: Dashboard und Kategorien öffnen beim Besuch immer den
  aktuellen Monat, Berichte hat einen eigenen Zeitraum-Umschalter (Aktuelles Jahr/Letzte 6/12 Monate) statt eines
  Monatsumschalters. Grund: Jede Tab ist eine eigene geroutete Blazor-Seite (keine gemeinsame In-Memory-Zustand
  über Seitenwechsel hinweg ohne zusätzlichen Dienst); ein Teilen des Monats zwischen Tabs war im Mock implizit
  (eine einzige SPA-Klasse), hier aber ein bewusster Mehraufwand, der zurückgestellt wurde (Vorschlag von Claude,
  so umgesetzt, 18.09.2026 — noch nicht von Dirk gegengeprüft).
- Alle vier Tabs holen ihre Daten über `IHaushaltsbuchOverviewWorkflow.GetOverviewAsync(fromDate, toDate)`, auch
  wenn eine Seite (z. B. Fixkosten) keine Buchungen braucht — dann wird einfach der kleinstmögliche Bereich (ein
  Tag) angefragt. Das hält die Architekturregel „Host ruft bei mehreren Managern einen Workflow, nie mehrere
  Manager direkt“ konsequent ein, statt für jede Seite eine eigene Ausnahme zu machen.

- Maßstab für die Optik ist die **gerenderte** Vorlage, nicht ihr Quelltext: Das Design-Tool verwirft
  `font-family:'Orbitron'` in per Skript gesetzten Styles, die Vorlage zeigt dort Inter. Deshalb stehen
  KPI-Werte, Monatsname, Fixkosten-Summe, Donut-Mitte und die Titel der Kategorienbäume in Inter (Vorschlag von
  Claude, so umgesetzt, 07.10.2026 — noch nicht von Dirk gegengeprüft).
- Bewusste Abweichungen von der Vorlage, weil sie dort selbst fehlerhaft ist (07.10.2026, noch nicht von Dirk
  gegengeprüft): Berichte — die zwei oberen Karten teilen sich die volle Breite (in der Vorlage erzwingt der
  Monatsverlauf über die ganze Zeile eine dritte, leere Spalte); Tabellen scrollen waagerecht, statt rechts
  abgeschnitten zu werden (Vorlage bei ca. 820–1000 px); der Monatsumschalter bleibt mobil eine Zeile (Vorlage stapelt
  ‹, Monat und › untereinander); Spaltenbeschriftungen der mobilen Tabellenkarten immer normal statt teils fett.

### Testinfrastruktur: lokale Postgres-DB und Playwright-E2E-Tests

Auf Wunsch von Dirk (18.09.2026) in dieser Sitzung aufgesetzt, damit die Haushaltsbuch-Umsetzung nicht nur gegen
Fakes, sondern gegen eine echte Datenbank und eine echte laufende WebUI geprüft werden konnte.

- Lokale Postgres 18 installiert (`sudo apt-get install -y postgresql`, von Dirk ausgeführt). Rolle `gwios_user`
  und zwei Datenbanken `gwios_db` (manueller Start) und `gwios_test_db` (Integrationstests **und** E2E-Tests,
  siehe unten) von Dirk per `sudo -u postgres psql` angelegt.
- Verbindungsstrings stehen **nicht** in `~/.bashrc` (Bash erlaubt keine Punkte in `export`-Namen); sie werden je
  Aufruf per `env 'GwiOS.DB.Connectionstring=...' dotnet ...` gesetzt. Das SDK selbst (`~/.dotnet`) bleibt wie
  gewohnt in `~/.bashrc`.
- Mit dieser echten DB liefen alle 632 Core-Tests grün (nicht nur die Fakes-gestützten Unit-Tests) — bestätigt
  Schema, Fremdschlüssel und `ON DELETE SET NULL` auch praktisch, nicht nur in der Theorie.
- Ein echter Start der WebUI (`dotnet run --project src/GwiOS.WebUI`) gegen `gwios_db` hat alle vier neuen
  Tabellen (`categories`, `recurring_items`, `bookings`, `month_reviews`) korrekt angelegt.
- Ein voller Browser-Klicktest scheitert an Keycloak: `appsettings.json` zeigt auf die echte Authority
  `https://keycloak.gwios.gwiasda.net`, zu der hier keine Zugangsdaten vorliegen. Dafür kam ein
  Test-Authentifizierungsschema hinzu: `GwiOS.WebUI.E2E.E2ETestAuthenticationHandler` authentifiziert jede
  Anfrage fest als `e2e-test-user`, **nur** wenn `Program` per `ASPNETCORE_ENVIRONMENT=E2E` gestartet wird — nie
  in Development oder Production. `Program.cs` verzweigt dafür bei der `AddAuthentication`-Registrierung.
- `tests/GwiOS.WebUI.E2E.Tests` (xunit v3 + Microsoft.Playwright, referenziert `GwiOS.Core` direkt für
  `TestDataSeeder`, das Kategorien für Tests vorab anlegt/löscht, ohne das über die Oberfläche zu tun).
  `GwiOSAppFixture` startet die echte WebUI als Subprozess (`dotnet run`, Umgebung `E2E`, `gwios_test_db`) und ein
  gemeinsames Chromium; alle Tests laufen in einer Collection nacheinander (dieselbe DB wie die
  Core-Integrationstests, deshalb kein eigenes `--artifacts-path`-Problem, aber gemeinsame Daten — Tests räumen
  selbst auf).
- Chromium-Installation brauchte einen Workaround: Playwright 1.55.0 kennt Ubuntu 26.04 noch nicht
  (`ERROR: Playwright does not support chromium on ubuntu26.04-x64`). Abhilfe:
  `PLAYWRIGHT_HOST_PLATFORM_OVERRIDE=ubuntu24.04-x64` vor `playwright install chromium` setzen (Node-seitige
  Prüfung in `hostPlatform.js`, keine echte Inkompatibilität). `Microsoft.Playwright.CLI` als globales Tool
  scheiterte an einer veralteten `netcoreapp3.1`-Zielversion; stattdessen ein Wegwerf-Konsolenprojekt mit
  `Microsoft.Playwright.Program.Main(["install", "chromium"])` verwendet, in derselben Paketversion wie das
  Testprojekt.
- E2E-Tests am 07.10.2026 erstmals gelaufen, zunächst 6 von 10 rot — zwei Ursachen, beide behoben: (1) `dotnet run`
  liefert die Static Web Assets (`blazor.web.js`, gebündelte Komponenten-Styles) von selbst nur in `Development`;
  in `E2E` antworteten sie mit 500, die Seiten wurden nie interaktiv. `Program` ruft deshalb in `E2E`
  `UseStaticWebAssets()` auf. (2) Die Tests klickten auf die nur vorgerenderte Seite, bevor die Verbindung zum
  Server stand. `MainLayout` setzt jetzt `data-interactive` (aus `RendererInfo.IsInteractive`), und
  `GwiOSAppFixture.OpenAsync` wartet nach dem Laden darauf; Seiten in E2E-Tests immer darüber öffnen. Danach
  dreimal hintereinander 10/10 grün. Vermutlich waren das auch die Fehlschläge von `dotnet test` beim Deploy.
- Docker ist seit 07.10.2026 in der VM installiert (`docker.io`, `docker-buildx`, `qemu-user-binfmt` für
  linux/arm64). Datenbanktests ohne die echte Test-DB: Wegwerf-Container `postgres:17` starten, Rolle `gwios_user`
  und Datenbank anlegen, `GwiOS.DB.TestConnectionstring` per `env` auf ihn setzen — so liefen alle 640 Core-Tests und
  die E2E-Tests grün. Stolpersteine: Die Sitzung sieht die Gruppe `docker` erst nach neuer Anmeldung (bis dahin
  `setfacl -m user:tija:rw /var/run/docker.sock`); `docker build` scheiterte einmal am IPv6-Download des
  Basis-Images (VM ohne IPv6-Route) — vorher `docker pull --platform linux/arm64 mcr.microsoft.com/dotnet/aspnet:10.0`.

### Offene Fragen

- Die neuen wiederverwendbaren Komponenten `GwiOSMonthSwitcher`, `GwiOSKpiTile`, `GwiOSCategoryTree`,
  `GwiOSDonutChart`, `GwiOSLineChart` sind vorgeschlagen, aber noch nicht im Detail mit Dirk abgestimmt.
- Fixposten sollen laut Dirk (07.10.2026) eigentlich nicht automatisch in die Buchungen einfließen, sondern nur als
  Übersicht neben dem manuellen Buchen dienen. Vorerst bleibt alles wie es ist (Monatsstart-Popup und Button
  „Fixbuchungen für … generieren“). Offen: Popup entfernen? Button behalten? Monatsprüfung (`month_reviews`) und
  ggf. `RecurringBookingGenerationWorkflow` dann aus dem Core entfernen?
- Seitentitel: Die Vorlage sagt „Haushaltsbuch HaBu“, umgesetzt ist „Haushaltsbuch“ (E2E-Tests prüfen darauf).
  Welcher soll gelten?
- Die Satellitenkacheln zeigen Name, Temperatur, Feuchte und Luftdruck in Orbitron, die gerenderte Vorlage dort
  Inter (siehe Entscheidung zur gerenderten Vorlage oben). Bewusst nicht umgestellt, weil die Kacheln mit Dirk
  abgestimmt wurden (Trendpfeil eigens wegen Orbitron). Soll es bei Orbitron bleiben?

## WebUI (Design-Vorlage `docs/UI/GwiOS.WebUI.Design.html`)

Fertig: Theme/Layout, GwiOS-Komponenten (`Components/GwiOS.Components`), Seiten `/todos`, `/admin/logging`,
`/admin/benutzer`, Statusleiste mit Verlauf, Testprojekt `tests/GwiOS.WebUI.Tests` (xunit v3 + bUnit 2).

- [x] Haushaltsbuch aus der Vorlage — umgesetzt und am 07.10.2026 gegen die Vorlage nachgearbeitet, siehe Abschnitt
      „Haushaltsbuch“ oben
- [x] ToDos, Satelliten und Admin gegen die gerenderte Vorlage geprüft (07.10.2026, gleiches Verfahren wie beim
      Haushaltsbuch). Aufbau und Raster stimmten; korrigiert: Tabellenzellen mittig statt oben (alle Tabellen),
      Log-Level-Plaketten, Pager-Seitenzahlen und Personen-Kürzel in Inter statt Orbitron, Status-Plaketten der
      Satellitenkacheln fett, Satellitennamen in Admin- und Kandidatenkachel in Inter statt Monospace, „Löschen“ in
      der Satellitenverwaltung gedämpft, Löschen-Knopf der Personenkachel fett, Statusleiste der Satelliten-Seite
      mobil untereinander mit voller Breite. Kachel-Raster mit `auto-fill` sind dort wie in der Vorlage gewollt
      (Kacheln behalten ihre Größe).

### Entscheidungen (gelten als Konvention für künftige UI-Arbeit)

- Razor macht aus dem Ordner `GwiOS.Components` sonst den Namespace `GwiOS_Components`; `_Imports.razor` setzt
  ihn per `@namespace` explizit, GwiOS-Usings stehen deshalb mit `global::`.
- Komponenten mit Logik haben eine Code-Behind-Datei (`.razor.cs`), reine Darstellungskomponenten einen
  `@code`-Block. Styles stehen in der `.razor.css` der Komponente, gemeinsame Grundelemente als `gwios-*`-Klassen
  zentral in `app.css`.
- Tabellen tragen `data-label` je Zelle; unter 820 px wird jede Zeile zur Karte.
- Löschen von Personen und Log-Einträgen verlangt einen zweiten Klick (`GwiOSConfirmButton`); ToDos werden
  sofort gelöscht.
- Statusmeldungen: Grau = neutral, Blau = Erfolg, Orange = Warnung, Rot = Fehler. `StatusMessageService` loggt
  nur das Level, nie den Text (kann Namen enthalten). Seiten loggen nicht selbst.
- Zeitstempel/Uhrzeiten immer über den injizierten `TimeProvider` in lokaler Zeitzone des Servers, einheitlich
  über `GwiOSTimestamp` (`dd.MM.yyyy HH:mm:ss`).
- Dialoge (`GwiOSDialog`) liegen über Kopfzeile und Statusleiste (`.gwios-main` hat deshalb keinen eigenen
  `z-index`) und werden unter 820 px zum Blatt von unten.

### Offene Fragen

- Sollen die Admin-Seiten auf eine Keycloak-Rolle beschränkt werden? Derzeit genügt die Anmeldung.
- Anmeldestatus und Abmelden fehlen im Header (`/logout` existiert, ist aber nicht verlinkt).
- Der Dialog hält den Tastatur-Fokus nicht fest (kein Focus-Trap) und sperrt das Scrollen der Seite nicht;
  beides bräuchte JavaScript.
- `wwwroot/lib/bootstrap` wird nicht mehr verwendet und kann entfernt werden.

## Datensicherung (GwiOS.Core/CrossCutting/Backup, WebUI unter `/admin/datensicherung`)

Im Admin-Bereich lädt „Sicherung herunterladen“ eine Sicherung der Datenbank herunter (Wunsch von Dirk, 07.10.2026).

Fertig: `DatabaseBackupManager`, `DatabaseBackupPgDumpRepository`, Endpunkt `GET /api/backup`, Seite und Tab
„Datensicherung“, Dockerfile mit `pg_dump` 17, `Deploy-WebUI.ps1` baut damit. Unit-Tests grün (Core 544,
WebUI 420).

- [x] Erster Deploy mit Dockerfile: Dirk hat deployt und in Produktion eine Sicherung heruntergeladen (07.10.2026).
- [x] Probe-Wiederherstellung (07.10.2026, in der VM mit Docker, nur Wegwerf-Container): Image wie
      `Deploy-WebUI.ps1` für linux/arm64 gebaut (Emulation, ~80 s; UID 1654, `pg_dump` 17.11), gegen Postgres 17.11
      mit Testdaten in allen Tabellen gestartet, `/api/backup` heruntergeladen (Dateiname in Ortszeit, temporäre
      Datei danach entfernt), mit `pg_restore` in eine leere Datenbank zurückgespielt: alle Fachtabellen inhaltlich
      gleich, Constraints und Indizes vollständig, Log-Tabelle vorhanden und leer; die App startet auf der
      wiederhergestellten Datenbank und zeigt die Daten. Fehlerfall (Datenbank gestoppt) leitet auf
      `?fehlgeschlagen=true` um.

### Entscheidungen

- Weg: `pg_dump` im Container statt eines eigenen Exports aus der App — Standardformat, Wiederherstellung mit
  `pg_restore` oder DBeaver (Dirk, 07.10.2026).
- Umfang: ganze Datenbank `gwios_db` ohne die Daten des Logs (`--exclude-table-data='logging.*'`) (Dirk).
- Zugriff: jeder angemeldete Benutzer, keine Keycloak-Rolle — es gibt nur Familienmitglieder als Benutzer (Dirk).
- Die Sicherung bleibt nicht auf dem Server: temporäre Datei im Container, gelöscht nach dem Senden.
- Schlägt die Sicherung fehl, leitet der Endpunkt auf `/admin/datensicherung?fehlgeschlagen=true` zurück, die Seite
  zeigt einen Hinweis; die Ursache steht im Log (der Link wird im Browser geöffnet, ein Fehlerstatus hinterließe eine
  leere Seite).
- `DatabaseBackupPgDumpRepository` hat keine Integrationstests (Vorgabe von Dirk, 07.10.2026): Sie bräuchten `pg_dump`
  auch auf dem Rechner, der testet und deployt. Geprüft wird die Sicherung durch den Download nach dem Deploy und
  eine Probe-Wiederherstellung.
- Stolperstein: Die Hauptversion von `pg_dump` (`$PostgresClientMajorVersion` in `Deploy-WebUI.ps1`) muss nach einem
  Postgres-Upgrade auf dem Pi mitgezogen werden.

## Deployment (deploy/rpi5)

Fertig: `Deploy-WebUI.ps1`, `apply.sh`, `docker-compose.yml`, `Dockerfile`; läuft unter
`https://webui.gwios.gwiasda.net`. Seit 07.10.2026 baut das Skript das Image mit Docker Desktop aus dem `Dockerfile`
(für `pg_dump`) statt mit `dotnet publish /t:PublishContainer`; Bedienung unverändert.

- [ ] Infrastruktur-Repo: `postgres-init/` um Benutzer `gwios_user` und Datenbank `gwios_db` ergänzen, damit
      ein Neuaufbau des Pi sie wieder anlegt.

### Entscheidungen

- Die WebUI wird ausschließlich aus diesem Repo deployt (Zielordner `/home/tija/gwios-webui`, eigenes
  Compose-Projekt, Verbindung zu Traefik nur über das Netz `proxy`).
- Secrets kommen aus Benutzer-Umgebungsvariablen (`GwiOS.DB.Connectionstring`, `Keycloak__ClientSecret`) und
  landen nur in der `.env` auf dem Pi (chmod 600), bei jedem Deploy neu geschrieben, per stdin übertragen.
- Die `.env` geht base64-kodiert mit SHA-256-Prüfsumme über stdin (PowerShell 5.1 verstümmelt sonst
  Sonderzeichen als ASCII). Eine abweichende Prüfsumme lässt `apply.sh` ohne Änderung abbrechen.
- Werte in der `.env` stehen in einfachen Anführungszeichen; Werte mit `'`, `\` oder Zeilenumbruch lehnt das
  Skript ab.
- `init: true` im Compose ist Pflicht: Als PID 1 beendet sich .NET nach einem unbehandelten Fehler nicht,
  `restart` griffe sonst nie.
- Postgres wird über den freigegebenen Port 5432 des Pi erreicht (`extra_hosts`: `rpi5`), Keycloak ebenso über
  `extra_hosts` bei Traefik.
- Data-Protection-Schlüssel liegen auf der SSD (`/mnt/ssd/gwios-webui/keys`), eingebunden am Standardpfad von
  ASP.NET Core.
- Image-Tag = kurzer Commit-Hash, bei nicht committeten Änderungen mit `-dirty`.

### Offene Fragen

- Skript und `apply.sh` sind nicht durch automatisierte Tests abgedeckt; geprüft werden sie durch den Deploy
  selbst.

## Satelliten (src/GwiOS.Satellites)

Kleine Hardwaremodule, die Messwerte über das LAN an GwiOS schicken. Erstes Modul: ESP32 mit BME280
(Temperatur, Luftfeuchte, Luftdruck).

Fertig: Firmware (I²C-Treiber, Konfiguration über NVS-Partition `config`, WLAN mit Neuverbindung,
Messwert-Server, Ping-Client, Status-LEDs), Architektur in `docs/architecture.md` bestätigt, GwiOS Core (Manager, Workflows)
und WebUI (Ping-API, Hintergrunddienst, Seiten „Satelliten“/„Satellitenverwaltung“) fertig und auf dem Pi
deployt; erster echter Satellit über die UI angelegt.

- [ ] Integrationstests `SatellitePostgresRepository` (11) laufen lassen — in der VM fehlt
      `GwiOS.DB.TestConnectionstring`
- [ ] `Flash-Satellite.ps1` unter Windows mit leerem Token prüfen (Token wird erzeugt und in die CSV
      geschrieben)
- [ ] Test am Board abschließen: Ping wird angenommen (`204`), Messwerte erscheinen in der Kachel, Offline
      nach 3 min ohne Strom
- [ ] Status-LEDs am Board prüfen: blau ab dem Einschalten, grün nach dem ersten angenommenen Ping, rotes
      Blitzen bei Ping und Abruf, dauerhaft rot nach 3 min ohne GwiOS (WLAN abschalten)

### Entscheidungen — Protokoll und Sicherheit

- Der Satellit sendet alle 60 s einen HTTP-Ping mit Id, Specs und der eigenen Server-Adresse; GwiOS holt
  Messwerte per Pull selbst ab (die Absender-IP des Pings wird nicht verwendet, sie ist hinter Traefik/Docker
  nicht verlässlich).
- Absicherung in beide Richtungen mit einem zufälligen Token pro Satellit (`Authorization: Bearer …`), das
  beide Seiten kennen; ein verlorener Satellit wird über sein Token gesperrt. Keycloak scheidet für Satelliten
  aus (zu schwer für den ESP32).
- Satelliten werden in der WebUI angelegt (Name = Id); GwiOS erzeugt das Token und zeigt es genau einmal.
  Pings unbekannter Namen oder mit falschem Token werden abgelehnt — keine Selbstanmeldung.
- Ein Token pro Satellit für beide Richtungen, mit ASP.NET Data Protection verschlüsselt in der DB gespeichert
  (Schlüssel auf dem Pi unter `/mnt/ssd/gwios-webui/keys` — gehen sie verloren, sind alle Tokens unlesbar und
  jeder Satellit braucht ein neues; die Schlüssel gehören ins Backup).
- Ping per HTTPS über Traefik (Zertifikatsprüfung mit dem CA-Bundle von ESP-IDF); Pull per unverschlüsseltem
  HTTP im LAN (nur durch WLAN-Verschlüsselung geschützt — bewusste Vereinfachung, kann später nachgerüstet
  werden).
- Konfiguration eines Satelliten (WLAN, Name, Token, GwiOS-Adresse) liegt nicht in der Firmware, sondern im
  NVS-Speicher, gespeist aus einer CSV-Datei pro Satellit (`%USERPROFILE%\.gwios\satellites\<name>.csv`,
  enthält Geheimnisse, nicht im Repo). Damit gibt es eine Firmware für alle Satelliten.
- API (Details siehe `docs/architecture.md`):
  - Ping: `POST https://webui.gwios.gwiasda.net/api/satellites/{name}/ping`, Body mit `endpoint`, `board`,
    `firmware`, `wifiRssiDbm`, `sensors`. Antworten `204`/`401`/`404`. Gemeldete Adresse muss
    `http://host[:port]` ohne Pfad/Query/Benutzerangaben sein; Board/Firmware 1–64 Zeichen, Signal
    −127…0 dBm, höchstens 8 Sensoren; Body höchstens 4 KB.
  - Pull: `GET http://<endpoint>/measurements` mit Token (Zeitlimit 5 s, Antwort höchstens 64 KB). Antwort mit
    `measurements[]` (Sensor, Messgröße, Wert, Einheit) und `ageSeconds`. Der Zeitstempel wird beim Abholen
    von GwiOS gesetzt (Satellit hat keine verlässliche Uhr).
  - Sensoren sind allgemein beschrieben (Typ + Messgrößen), jeder Messwert trägt seine Einheit — GwiOS
    braucht kein Wissen über einzelne Sensortypen.
- Messwerte werden nicht in der Datenbank gespeichert: GwiOS hält pro Sensor/Messgröße nur die letzten Werte
  im Speicher (kurzer Verlauf: 10 Werte/5 min; langer Verlauf: jeder 10. Wert, bis 100 über die Laufzeit);
  nach einem Neustart der WebUI ist der Verlauf leer. In der DB stehen nur Name, verschlüsseltes Token und
  Sperrstatus.
- Ein Satellit gilt als offline nach 3 Minuten ohne Ping (drei verpasste Pings) und wird dann nicht mehr
  abgefragt, bis er sich wieder meldet. Pull-Intervall 30 s.
- Das Abholen übernimmt vorerst ein Hintergrunddienst in der WebUI; später soll ein allgemeines System zur
  zeitgesteuerten Verarbeitung von Tasks entstehen, das diesen Dienst ablöst.
- Unbekannte Satelliten werden als Kandidat im Speicher gehalten (höchstens 20, verfällt nach 10 min ohne
  Ping; ungültige Namen/Tokens werden nicht aufgenommen). „Anlegen“ übernimmt das mitgeschickte Token, außer
  das Token wechselte zwischen zwei Pings desselben Namens (dann gesperrt, gegen Vorpreschen). „Blockieren“
  legt den Namen gesperrt ohne Token an.
- Name eines Satelliten (= Id, steht in der Ping-URL): nur Kleinbuchstaben `a–z`, Ziffern und `-`, 1–32
  Zeichen. Optionaler, nach dem Anlegen unveränderlicher Anzeigename.

### Entscheidungen — Anzeige (Kacheln/Satelliten-Seite)

- Temperatur mit zwei Nachkommastellen (wie der Sensor sie liefert); Trend, Luftdruck, absolute Feuchte mit
  einer.
- Sparkline-Balkenhöhe wird aus dem gerundet angezeigten Wert berechnet (nicht dem Rohwert), mit
  Mindestspanne von 0,5 K — sonst wirkt Sensorrauschen wie ein Sprung.
- Luftdruck-Balken deckt 954–1060 hPa ab mit Strich beim örtlichen Mittelwert 996 hPa (Stationsdruck, nicht
  auf Meereshöhe umgerechnet); Liniendiagramm (`GwiOSSparkLine`) beim Hover, Trendpfeil (`GwiOSTrendArrow`,
  eigene SVG-Komponente wegen zu kleiner Pfeil-Schriftzeichen in Orbitron) über 3 h nach meteorologischen
  Stufen (< 0,5 hPa gleichbleibend, bis 2 hPa steigend/fallend, darüber stark).
- Absolute Feuchte wird aus Temperatur und relativer Feuchte per Magnus-Formel berechnet und in Klammern
  hinter der relativen angezeigt, z. B. `44 % (8,8 g/m³)`.

### Entscheidungen — Firmware

- Host-Tests (Unity, ESP-IDF-frei) für reine Logik (Token-Prüfung, JSON, Konfiguration,
  BME280-Kompensation) unter `tests/GwiOS.Satellite.Bme280.Tests`, gespiegelte Struktur wie in R5.
  Hardwarenahes wird als Integrationstest am Board geprüft (Ablauf in der README des Satelliten).
- Messwert-Server: Port 80, `401` mit `WWW-Authenticate: Bearer` ohne gültiges Token, `503` vor der ersten
  Messung, Token-Vergleich in konstanter Zeit. Schlägt eine Messung fehl, bleibt die letzte gültige stehen
  und `ageSeconds` wächst weiter.
- Ping: erster sofort nach Verbindungsaufbau, danach alle 60 s; Weiterleitungen (3xx) werden nicht verfolgt
  (sonst ginge das Token an den Umleitungsziel-Server); Zeitlimit 10 s.
- Konfiguration liegt in eigener NVS-Partition `config` (vor der App, damit ihre Adresse nicht von der
  App-Größe abhängt), nicht in der Standard-Partition `nvs` (die nutzt der WLAN-Treiber). Fehlt sie oder ist
  sie ungültig, misst der Satellit trotzdem und gibt die Werte auf der Konsole aus, startet aber
  WLAN/Ping/Server nicht.
- Geflasht wird unter Windows mit `Flash-Satellite.ps1` (`esptool`, `esp_idf_nvs_partition_gen`);
  `export-firmware.sh` kopiert die Firmware-Images aus der VM nach `dist/`.
- Zugangspunkt-Wahl im Mesh: `WIFI_ALL_CHANNEL_SCAN` + `WIFI_CONNECT_AP_BY_SIGNAL` (statt der Voreinstellung,
  die den ersten gefundenen AP nimmt statt des stärksten) — kostet 1–2 s mehr pro Verbindung. Offen als
  Stufe 2, falls nötig: 802.11k/v, damit das Mesh laufend umlenken kann.
- Nach Verbindungsverlust: Neuversuch nach 1 s, Pause verdoppelt sich bis max. 60 s, fällt nach Erfolg auf
  1 s zurück. Mindestens WPA2 verlangt. Energiesparmodus aus (`WIFI_PS_NONE`), sonst verzögerte Anfragen von
  GwiOS.
- Status-LEDs (blau GPIO 25, grün GPIO 26, rot GPIO 27, je Vorwiderstand gegen GND): blau zeigt den Betrieb,
  grün eine erfolgreiche Kommunikation mit GwiOS in den letzten 3 Minuten, rot deren Fehlen — und solange
  grün leuchtet, stattdessen 500 ms je ein- oder ausgehendem Request. Rot trägt beides, weil zwei getrennte
  Lampen für Störung und Verkehr eine vierte LED gekostet hätten; die Störung hat Vorrang, sonst würde ein
  offline blinkender Satellit wie ein gesunder wirken. Die 3 Minuten sind dieselbe Frist, nach der GwiOS
  einen Satelliten für offline hält. Als Erfolg zählen ein angenommener Ping (2xx) und ein Abruf mit
  gültigem Token; abgewiesene Anfragen sind nur Verkehr.
- GPIO 25–27 für die LEDs, weil sie auf dem DevKit C frei sind: keine Strapping-Pins, keine Flash-Leitungen,
  beim Booten still. Die Belegung steht wie die des Sensors oben in `main/main.c`, die Lampen werden vor
  allem anderen gestartet (blau brennt ab dem Einschalten).
- Die Zustandslogik der LEDs liegt ESP-IDF-frei in `status_led_policy.c` (Host-Tests), die Ansteuerung in
  `status_led.c` (GPIOs, Mutex, Auffrischen alle 50 ms). Ping-Client und Messwert-Server melden jeden
  Request mit `status_led_note_request(erfolgreich)`.
- Landmine (Board/Flash): Der automatische Wechsel in den Download-Modus funktioniert auf diesem Board nicht
  (`idf.py flash` → `Wrong boot mode detected`) — vor dem Flashen BOOT halten, EN kurz drücken, BOOT
  loslassen, dann mit `--before no-reset` flashen.
- Landmine (Build): Das Build-Verzeichnis muss außerhalb des Freigabeordners liegen
  (`idf.py -B ~/build/gwios-satellite-bme280 …`), sonst `File name too long` (Windows-Pfadgrenze 260 Zeichen,
  ESP-IDF v6.1 erzeugt für mbedtls längere Objektpfade).
- Hardware: BME280 per I²C (VCC=3V3, GND, SDA=GPIO21, SCL=GPIO22), Adresse `0x77`. Versorgung ausschließlich
  mit 3,3 V (nicht 5 V, sonst liegen 5 V auf den I²C-Leitungen der GPIOs). Board ist ein klassisches ESP32
  DevKit C (ESP32-WROOM-32), kein S3 — Pin-Angaben aus S3-Anleitungen passen nicht.
- Entwickelt wird in der Ubuntu-VM (`/mnt/hgfs/Repos/gwios`), Toolchain ESP-IDF v6.1, Sprache C (nicht
  C++/Arduino), neuer I²C-Treiber (`driver/i2c_master.h`).
- `coding-guidelines.md` gilt nur für die .NET-Projekte; die Firmware folgt ESP-IDF-Konventionen
  (`snake_case`, `SCREAMING_SNAKE_CASE` für Makros, englische Bezeichner, Kommentare erklären das Warum).

### Offene Fragen

- Wie messen die Satelliten, ohne dass die Eigenwärme des ESP32 die Temperatur verfälscht (Abstand, Gehäuse,
  Korrekturwert)?
- Die Ping-API hat keine Begrenzung der Anfragerate; Kandidaten sind auf 20 begrenzt. Reicht das, falls die
  WebUI aus dem Internet erreichbar ist?
- Wer darf die Seiten „Satelliten“ und „Satellitenverwaltung“ sehen? Derzeit genügt die Anmeldung.
- Der Strich beim örtlichen Mittelwert steht fest auf 996 hPa. Soll er je Satellit einstellbar werden, oder
  soll GwiOS den Druck über eine Höhe je Satellit auf Meereshöhe umrechnen (dann wären 1013 hPa und der
  Vergleich mit dem Wetterbericht möglich)?
- Ist `webui.gwios.gwiasda.net` aus dem Internet erreichbar oder nur im LAN? (Für den Ping ist HTTPS
  ohnehin beschlossen; die Antwort bestimmt, wie streng der Endpunkt gegen Missbrauch zu schützen ist.)
