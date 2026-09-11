# Verbindliche Regeln für die Arbeit an GwiOS

Diese Regeln sind ausnahmslos zu befolgen. Sie wachsen im Lauf der Entwicklung.

## R1 — Der Entwicklungsplan ist die erste Anlaufstelle

`docs/devplan.md` wird zu Beginn jeder Session gelesen — insbesondere nach einer Compaction
oder einem vollständigen Kontext-Reset — bevor mit der Arbeit begonnen wird.

## R2 — Der Entwicklungsplan wird aktuell gehalten

`docs/devplan.md` spiegelt jederzeit den tatsächlichen Stand wider: erledigte Punkte
abhaken, neue Erkenntnisse, Entscheidungen und offene Fragen eintragen.

## R3 — Regeln ändern sich nur auf ausdrückliche Anweisung

Diese Datei, `CLAUDE.md` und `coding-guidelines.md` werden ausschließlich dann geändert,
wenn Dirk es explizit anweist. Keine eigenmächtigen Ergänzungen, Umformulierungen oder
Streichungen von Regeln.

## R4 — Die Coding Guidelines gelten für jede Codeänderung

[coding-guidelines.md](coding-guidelines.md) ist verbindlich und wird bei jedem Schreiben,
Ändern und Prüfen von Code berücksichtigt — ohne gesonderte Aufforderung.

## R5 - Tests sind Pflicht
Es müssen immer Unit- und ggfs. Integrationstests geschrieben werden, die den Code abdecken. Tests müssen erfolgreich sein, bevor eine Aufgabe abgeschlossen werden kann.
Die Test-Projekte liegen im Dateisystem unter `tests/` und heißen wie das getestete Projekt mit dem Suffix `.Tests`. Die Tests werden mit `dotnet test` ausgeführt.
Die Ordner-Struktur der Testprojekte muss der Struktur der zu testenden Projekte entsprechen. Jede getestete Klasse bekommt ein eigenes Directory und jede getestete Methode eine eigene Testklasse. Die Testmethoden drücken die Funktionalität der getesteten Methoden aus.

## Logging
Für das Logging wird ausschließlich das Interface GwiOS.Core.CrossCutting.Logging.Contracts.ILogger verwendet. Die Implementierung wird über Dependency Injection bereitgestellt. Es dürfen keine eigenen Logger implementiert werden. Logging ist in allen Klassen verpflichtend, die eine öffentliche Schnittstelle haben.