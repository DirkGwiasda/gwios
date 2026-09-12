# Verbindliche Regeln für die Arbeit GwiOS UI

Diese Regeln sind ausnahmslos zu befolgen. Sie wachsen im Lauf der Entwicklung.

## Kapselung in Komponenten
Immer wenn möglich, sollten UI-Elemente in wiederverwendbare Komponenten gekapselt werden. Dies fördert die Wartbarkeit und Konsistenz der Benutzeroberfläche.
Ausnahmen davon sind einfach Elemente, die über einfache, zentral gestylete HTML-Elemente abbildbar sind.
WWebUI-Komponenten befinden sich unter D:\Repos\gwios\src\GwiOS.WebUI\Components\GwiOS.Components.
Beispiele für Komponenten sind:
- GiwOSTextBox mit Validierungs-, Formatierungslogik und Logik zur vereinfachten Eingabe von Daten.