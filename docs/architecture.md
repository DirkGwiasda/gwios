# Software-Architektur

Beschreibt den Aufbau von GwiOS: Projekte, Domänen, Manager und Workflows sowie die Regeln für ihre
Abhängigkeiten. Die Regeln für einzelne Codezeilen stehen in [coding-guidelines.md](coding-guidelines.md),
der Arbeitsstand und die fachlichen Entscheidungen in [devplan.md](devplan.md).

## Projekte

| Projekt | Aufgabe |
|---|---|
| `src/GwiOS.Core` | Fachlogik aller Domänen und ihr Datenzugriff (Manager, Workflows, Repositories). Kennt keine UI und kein HTTP-Hosting. |
| `src/GwiOS.WebUI` | Host: Blazor-Oberfläche, HTTP-API und Hintergrunddienste. Enthält keine Fachlogik, sondern ruft den Core auf. |
| `src/GwiOS.Satellites/*` | Firmware der Satelliten (ESP-IDF, C). Außerhalb der .NET-Architektur; spricht mit GwiOS nur über das HTTP-Protokoll im devplan. |
| `tests/*.Tests` | Tests, Struktur gespiegelt nach R5. |

Abhängigkeiten zeigen nur in eine Richtung: `GwiOS.WebUI` → `GwiOS.Core`. Der Core kennt seine Hosts nicht.

## Domänen

Eine Domäne ist ein fachlicher Bereich von GwiOS, z. B. `ToDos`, `Haushaltsbuch` oder `Satellites`. Jede Domäne
ist ein Ordner direkt unter `GwiOS.Core`. Was mehrere Domänen brauchen, liegt unter
`GwiOS.Core/CrossCutting` (derzeit `Logging` und `Persons`) und darf von allen Domänen genutzt werden.
Eine fachliche Domäne nutzt nie eine andere fachliche Domäne direkt.

Aufbau einer Domäne:

```
GwiOS.Core/<Domäne>/
├── Domain/
│   ├── Managers/
│   │   └── <Entität>Management/
│   │       ├── Contracts/
│   │       │   ├── I<Entität>Manager.cs
│   │       │   ├── I<Entität>Repository.cs
│   │       │   ├── I<Entität>Validator.cs       (weitere Helfer des Managers ebenso)
│   │       │   ├── Exceptions/
│   │       │   └── Models/
│   │       ├── <Entität>Manager.cs
│   │       └── <Entität>Validator.cs
│   └── Workflows/
│       └── <Aufgabe>/
│           ├── Contracts/
│           │   ├── I<Aufgabe>Workflow.cs
│           │   └── Models/
│           └── <Aufgabe>Workflow.cs
└── Infrastructure/
    └── <Entität><Technik>Repository.cs          (z. B. ToDoPostgresRepository)
```

`ToDos` und `CrossCutting/Persons` folgen diesem Aufbau bereits (ohne Workflows, weil sie keine brauchen).
`CrossCutting/Logging` ist älter und weicht ab (Contracts auf oberster Ebene, kein `LogEntryManagement`-Ordner);
es wird nicht umgebaut, solange es keinen fachlichen Anlass gibt.

## Manager

- **Ein Manager ist für genau eine Domänen-Entität zuständig** (`ToDoManager` für `ToDo`, `PersonManager` für
  `Person`). Er ist die einzige Stelle, die diese Entität anlegt, ändert, liest oder löscht, und setzt ihre
  fachlichen Regeln durch.
- **Manager hängen nicht voneinander ab.** Kein Manager bekommt einen anderen Manager injiziert, auch nicht aus
  einer anderen Domäne.
- Ein Manager darf nutzen: seine eigenen Helfer (Repository, Validator, weitere Schnittstellen in seinem
  `Contracts`-Ordner), `TimeProvider` und `ILogger<T>`.
- Die Helfer eines Managers gehören zu ihm und werden nur von ihm verwendet. Ein Repository liefert und speichert
  nur die Entität seines Managers.
- Die Modelle eines Managers (`Contracts/Models`) beschreiben seine Entität; seine Exceptions
  (`Contracts/Exceptions`) seine fachlichen Fehler.

## Workflows

- **Muss eine Aufgabe mehrere Manager orchestrieren, übernimmt das ein Workflow** unter `Domain/Workflows`.
- Ein Workflow hat genau eine Aufgabe (CORE-1) und ist nach ihr benannt (z. B. `SatellitePingWorkflow` im Ordner
  `Workflows/SatellitePing`). Der Ordner trägt den Namen ohne `Workflow`, so wie `PersonManagement` den
  `PersonManager` enthält: Hieße der Namespace wie die Klasse, würde er sie verdecken.
- **Ein Workflow läuft nur innerhalb seiner Domäne.** Er orchestriert die Manager seiner eigenen Domäne und darf
  dazu Manager aus `CrossCutting` nutzen, nie Manager einer anderen fachlichen Domäne.
- **Workflows rufen sich nicht gegenseitig auf.** Wird ein Ablauf mehrfach gebraucht, gehört das Wiederverwendbare
  in einen Manager.
- Ein Workflow bekommt die Manager, die er braucht, als Schnittstellen injiziert, dazu `TimeProvider` und
  `ILogger<T>`. Er greift nie direkt auf Repositories oder andere Helfer eines Managers zu.
- Ein Workflow enthält die Ablauflogik (Reihenfolge, Verzweigungen, Fehlerbehandlung über Manager hinweg), aber
  keine Regeln einer einzelnen Entität — die bleiben im Manager.
- Liefert ein Workflow ein zusammengesetztes Ergebnis aus mehreren Entitäten, beschreibt er es mit eigenen
  Modellen in `Contracts/Models`.

## Hosts (WebUI)

- Seiten, API-Endpunkte und Hintergrunddienste enthalten keine Fachlogik. Sie übersetzen zwischen ihrer Technik
  (Blazor, HTTP/JSON, Timer) und dem Core.
- Braucht ein Anwendungsfall nur einen Manager, ruft der Host ihn direkt auf (so arbeiten die bisherigen Seiten).
  Braucht er mehrere, ruft der Host einen Workflow auf und nie mehrere Manager nacheinander.
- Hintergrunddienste (`BackgroundService`) sind nur Taktgeber: Sie rufen im Takt einen Workflow auf. So kann ein
  späteres allgemeines Task-System sie ersetzen, ohne dass Fachlogik umzieht.

## Lebensdauer im DI-Container

- Manager, Workflows, Validatoren und Datenbank-Repositories sind `Scoped`.
- Zustand, der nur im Speicher gehalten wird und die Anfrage überdauern muss, liegt in einem Repository mit
  der Lebensdauer `Singleton` (z. B. `…InMemoryRepository`). Der Manager bleibt `Scoped` und sieht den
  Unterschied nicht; das Repository ist dann thread-sicher.
- Registriert wird je Domäne in einer privaten Methode von `AddGwiOSCore` (`AddToDos`, `AddPersons`, …).

## Satelliten

Anwendung der Regeln oben auf die Domäne `GwiOS.Core/Satellites`. Fachliche Vorgaben (Protokoll, Token,
Intervalle, Verläufe) stehen im devplan, Abschnitt „Satelliten“. Bestätigt von Dirk (17.09.2026).

### Manager

| Manager | Entität | Aufgaben | Helfer |
|---|---|---|---|
| `SatelliteManagement` | `Satellite`: Name, Anzeigename, verschlüsseltes Token, gesperrt | Anlegen mit neuem Token (liefert es einmal im Klartext), Anlegen mit übernommenem Token, Anlegen als gesperrt ohne Token, Anzeigename ändern, Sperren, Entsperren, neues Token, Löschen, Lesen, Token prüfen, Token für den Pull entschlüsseln | Validator, Token-Generator, Token-Protector (Data Protection), `SatellitePostgresRepository` |
| `SatellitePresenceManagement` | `SatellitePresence`: letzter Ping, Adresse, Board, Firmware, Signal, Sensoren | Ping festhalten, online/offline bestimmen (3 min), lesen | Validator, `SatellitePresenceInMemoryRepository` |
| `SatelliteCandidateManagement` | `SatelliteCandidate`: unbekannter Satellit mit Adresse, Board, Firmware, Signal, erstem und letztem Ping | Ping festhalten (höchstens 20, verfallen nach 10 min ohne Ping, Token-Wechsel markieren), lesen, Token zur Übernahme herausgeben, entfernen | Validator, `SatelliteCandidateInMemoryRepository` (hält das Token getrennt vom Modell) |
| `MeasurementManagement` | `MeasurementHistory`: Verlauf je Satellit, Sensor und Messgröße | Messwerte beim Satelliten abholen, in kurzen und langen Verlauf aufnehmen, Verläufe lesen | Validator, HTTP-Client zum Satelliten, `MeasurementHistoryInMemoryRepository` |

### Workflows

| Workflow | Orchestriert | Ablauf |
|---|---|---|
| `SatellitePingWorkflow` | Satellite, SatellitePresence, SatelliteCandidate | Token prüfen; unbekannter Name → als Kandidat festhalten und `SatelliteNotFoundException` weiterwerfen; falsches Token oder gesperrt → `SatelliteTokenRejectedException`; sonst Ping festhalten |
| `MeasurementPollingWorkflow` | Satellite, SatellitePresence, Measurement | für jeden nicht gesperrten Satelliten, der online ist: Token entschlüsseln, Messwerte abholen und in den Verlauf aufnehmen; ein Fehler bei einem Satelliten wird geloggt und hält die anderen nicht auf |
| `SatelliteOverviewWorkflow` | Satellite, SatellitePresence, SatelliteCandidate, Measurement | Daten für die Seiten „Satelliten“ und „Satellitenverwaltung“ zusammenstellen (Satelliten mit Presence, online/offline und Verläufen; unbekannte Satelliten ohne Token) |
| `SatelliteAdoptionWorkflow` | SatelliteCandidate, Satellite | Token des Kandidaten holen (nicht bei Token-Wechsel), Satellit mit diesem Token anlegen, Kandidat entfernen |
| `SatelliteBlockingWorkflow` | SatelliteCandidate, Satellite | Satellit gesperrt und ohne Token anlegen, Kandidat entfernen |

### Hosts

- API `POST /api/satellites/{name}/ping`: Minimal-API ohne Cookie-/OpenID-Anmeldung und ohne Umleitung auf die
  Not-found-Seite, ruft `SatellitePingWorkflow`. Erfolg → `204`, fehlendes Token oder
  `SatelliteTokenRejectedException` → `401`, `SatelliteNotFoundException` → `404`, ungültiger Body → `400`;
  alle anderen Fehler bleiben `500`.
- Hintergrunddienst: alle 30 s (einstellbar) `MeasurementPollingWorkflow`.
- Seite „Satelliten“ (`/satelliten`): liest über `SatelliteOverviewWorkflow`, bei eingeschalteter
  Auto-Aktualisierung alle 5 s über einen eigenen Timer. Eine Checkbox schaltet die Auto-Aktualisierung aus und
  wieder an. „Jetzt abfragen“ stößt sofort `MeasurementPollingWorkflow` an und liest danach neu. Unbekannte
  Satelliten erscheinen darunter mit „Anlegen“ (`SatelliteAdoptionWorkflow`) und „Blockieren“
  (`SatelliteBlockingWorkflow`).
- Seite „Satellitenverwaltung“ (`/admin/satelliten`): liest über `SatelliteOverviewWorkflow`; Anlegen, Bearbeiten,
  Sperren, Entsperren, neues Token und Löschen betreffen nur `Satellite` und gehen direkt an den
  `SatelliteManager`.

## Entscheidungen

- Regeln für Manager, Workflows und Hosts von Dirk vorgegeben bzw. bestätigt (17.09.2026): ein Manager je
  Entität, keine Abhängigkeiten zwischen Managern, Orchestrierung nur in Workflows, Workflows rufen sich nicht
  auf und bleiben in ihrer Domäne, Domänenübergreifendes liegt in `CrossCutting`, Ordnernamen im Plural
  (`Managers`, `Workflows`), Hosts dürfen bei nur einem beteiligten Manager direkt auf ihn zugreifen.
- Ordner und Namespace-Segment sind immer gleich geschrieben, gruppierende Ordner stehen im Backend im Plural. Auf
  Anweisung von Dirk in CORE-3 der Coding Guidelines ergänzt, das Beispiel dort auf `Domain/Workflows` korrigiert
  (17.09.2026).
- Satelliten: drei Manager — `SatelliteManagement` (Stammdaten und Token, Postgres),
  `SatellitePresenceManagement` (Ping-Zustand, nur im Speicher) und `MeasurementManagement` (Abholen per HTTP und
  Verläufe, nur im Speicher). Die Presence ist nicht Teil von `Satellite`, damit kein Repository Datenbank und
  Speicher mischt (bestätigt von Dirk, 17.09.2026).
- Satelliten: drei Workflows — `SatellitePingWorkflow`, `MeasurementPollingWorkflow` und
  `SatelliteOverviewWorkflow`. Anlegen und Sperren betreffen nur `Satellite` und laufen ohne Workflow direkt über
  den `SatelliteManager`. Token-Vergleich und online/offline bleiben in den Managern, der Workflow bestimmt nur
  den Ablauf (bestätigt von Dirk, 17.09.2026).
- Satelliten: „unbekannt“ und „abgelehnt“ werden über Exceptions gemeldet, nicht über Ergebniswerte (CC-23).
  Der `SatelliteManager` wirft `SatelliteNotFoundException` (unbekannter Name) und
  `SatelliteTokenRejectedException` (falsches Token oder gesperrt, also `401` statt `404`). Der
  `SatellitePingWorkflow` lässt sie durch, die API übersetzt nur diese beiden; der `MeasurementPollingWorkflow`
  fängt sie je Satellit ab und loggt sie (bestätigt von Dirk, 17.09.2026).
- Satelliten: Die Seite fragt selbst im Takt nach (Timer 5 s, nur Lesen im Speicher), der Core meldet nichts an
  die Seite — keine Ereignisse oder Abonnements über Scopes hinweg. Die Auto-Aktualisierung lässt sich per
  Checkbox aus- und wieder einschalten; „Aktualisieren“ fragt die Satelliten sofort ab (Empfehlung von Claude,
  Checkbox von Dirk ergänzt, 17.09.2026). Beim Öffnen der Seite ist sie eingeschaltet; die Einstellung wird nicht
  gemerkt (kein Speichern je Benutzer, kein Local Storage).
- Satelliten: Unbekannte Satelliten werden als Kandidaten im Speicher gehalten und auf der Seite „Satelliten“
  angezeigt. „Anlegen“ übernimmt das Token, mit dem der Satellit pingt; `Flash-Satellite.ps1` erzeugt es, wenn die
  CSV keins enthält. „Blockieren“ legt den Namen gesperrt ohne Token an. Satelliten bekommen einen optionalen
  Anzeigenamen. Dafür kommen der Manager `SatelliteCandidateManagement` und die Workflows
  `SatelliteAdoptionWorkflow` und `SatelliteBlockingWorkflow` hinzu (bestätigt von Dirk, 17.09.2026).
- Die Regeln dieses Dokuments zu Domänen, Managern und Workflows gelten vorerst nur für das Backend
  (`GwiOS.Core`). Für die UI kommen eigene Regeln (Vorgabe von Dirk, 17.09.2026).
