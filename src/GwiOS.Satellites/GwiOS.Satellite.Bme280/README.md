# GwiOS.Satellite.Bme280

Firmware für einen Satelliten aus ESP32 und BME280. Aktueller Stand: Der Satellit liest beim Start seine
Konfiguration, verbindet sich mit dem WLAN und misst alle fünf Sekunden Temperatur, Luftfeuchte und
Luftdruck. Die letzte Messung liefert er per HTTP an GwiOS aus und gibt sie auf der seriellen Konsole aus.
Alle 60 Sekunden meldet er sich per HTTPS bei GwiOS (Ping) mit Adresse, Board, Firmware-Version,
WLAN-Signalstärke und Sensoren. Drei LEDs zeigen ohne Konsole, ob er läuft und ob er GwiOS erreicht.

## Hardware

| Ader | Pin am Sensormodul | ESP32 DevKit C |
|---|---|---|
| rot | `VCC` | `3V3` |
| schwarz | `GND` | `GND` |
| gelb | `SDA/MOSI` | `GPIO 21` |
| blau | `SCL/SCK` | `GPIO 22` |
| grün | `CS` | offen |
| orange | `MISO/ADDR` | offen |

Dazu die drei Status-LEDs, jede mit eigenem Vorwiderstand (220–330 Ω) an `GND`:

| LED | ESP32 DevKit C |
|---|---|
| blau | `GPIO 25` |
| grün | `GPIO 26` |
| rot | `GPIO 27` |

Die langen Beine (Anode) gehen an die GPIOs, die kurzen über den Widerstand an `GND`.

Board: ESP32-WROOM-32 (DevKit C, CP2102). Sensor: seengreat BME280, I²C-Adresse `0x77`.
Die Pin-Belegung steht in `main/main.c` ganz oben; Begründungen in `docs/devplan.md`.

## Statusanzeige

| LED | Bedeutung |
|---|---|
| blau | leuchtet dauerhaft, solange die Firmware läuft |
| grün | leuchtet, solange in den letzten 3 Minuten eine erfolgreiche Kommunikation mit GwiOS stattfand |
| rot | leuchtet dauerhaft, solange das nicht der Fall ist; sonst 500 ms je ein- oder ausgehendem Request |

Als erfolgreich zählt ein Ping, den GwiOS annimmt (`204`), und ein Abruf der Messwerte mit gültigem Token.
Ein abgewiesener Ping oder ein Abruf ohne Token ist Verkehr, aber kein Erfolg: Rot blinkt, Grün bleibt
unberührt. Die 3 Minuten sind dieselbe Frist, nach der GwiOS einen Satelliten für offline hält.

Im Normalbetrieb leuchtet also blau und grün, und rot blitzt alle 30 Sekunden (Abruf durch GwiOS) und alle
60 Sekunden (Ping) kurz auf. Nur blau und rot heißt: keine Konfiguration, kein WLAN oder GwiOS nicht
erreichbar — der Satellit misst dann weiter, nur meldet er nichts.

## Ablauf im Überblick

1. **VM:** Firmware bauen und nach `dist/` exportieren.
2. **Windows:** Konfiguration des Satelliten als CSV-Datei anlegen (einmalig pro Satellit).
3. **Windows:** Firmware und Konfiguration mit `Flash-Satellite.ps1` flashen.

Das Board hängt dabei immer nur an einer Seite: VMware fragt beim Einstecken, ob es mit dem Host oder
der VM verbunden wird; umhängen geht über das VMware-Menü „Removable Devices“.

## Bauen und exportieren (VM)

Im ESP-IDF-Terminal (VS Code: `ESP-IDF: Open ESP-IDF Terminal`) im Ordner dieses Projekts:

```
idf.py -B ~/build/gwios-satellite-bme280 build
./export-firmware.sh
```

Das Build-Verzeichnis liegt bewusst außerhalb des Repos: Im VMware-Freigabeordner scheitert der Build an
zu langen Pfaden. `export-firmware.sh` kopiert nur die fertigen Images nach `dist/` (nicht im Repo).

Voraussetzung ist ESP-IDF v6.1, installiert über den Espressif Installation Manager
(https://dl.espressif.com/dl/eim/). Das Target `esp32` steht bereits in `sdkconfig`.

## Konfiguration (Windows)

Jeder Satellit bekommt eine eigene CSV-Datei unter `%USERPROFILE%\.gwios\satellites\<name>.csv`. Die
Dateien enthalten Token und WLAN-Passwort und gehören deshalb nicht ins Repo. Vorlage mit Dummy-Werten:
`config/satellite.example.csv`.

| Eintrag | Regel |
|---|---|
| `name` | 1–32 Zeichen aus `a–z`, `0–9`, `-`; muss dem Dateinamen entsprechen |
| `token` | 32–128 Zeichen aus `A–Z`, `a–z`, `0–9`, `-`, `_` |
| `wifi_ssid` | 1–32 Bytes |
| `wifi_password` | leer (offenes WLAN) oder 8–63 druckbare ASCII-Zeichen |
| `gwios_url` | beginnt mit `https://`, ohne `/` am Ende, höchstens 128 Zeichen |

Enthält ein Wert ein Komma, muss er in doppelte Anführungszeichen.

Das Token kann leer bleiben (`token,data,string,`). Dann erzeugt `Flash-Satellite.ps1` ein zufälliges Token
und speichert es in der CSV-Datei. Nach dem ersten Ping zeigt GwiOS den Satelliten auf der Seite „Satelliten“
unter „Unbekannte Satelliten“; „Anlegen“ übernimmt dort das Token. Alternativ legt man den Satelliten in GwiOS
unter „Admin → Satelliten“ an und trägt das dort einmal angezeigte Token in die CSV-Datei ein.

Die Konfiguration liegt in einer eigenen Partition `config` (siehe `partitions.csv`). Neue Firmware
überschreibt sie nicht, und eine neue Konfiguration lässt die Daten des WLAN-Treibers in `nvs` unberührt.

## Flashen (Windows)

Voraussetzung ist die Windows-Installation von ESP-IDF v6.1; das Skript verwendet deren Python mit
`esptool`, dem NVS-Generator und dem Monitor. In PowerShell im Ordner dieses Projekts:

```
.\Flash-Satellite.ps1 -Name wohnzimmer -Monitor
```

- `-Port COM3` ist die Vorgabe; `-ConfigOnly` schreibt nur die Konfiguration.
- Das Skript bittet vor dem Flashen um den Handgriff für den Download-Modus (BOOT halten, EN kurz
  drücken, BOOT loslassen): Unter Windows klappt der automatische Wechsel auf diesem Board nicht.
- Das erzeugte NVS-Image enthält die Geheimnisse im Klartext und wird nach dem Flashen gelöscht.
- Den Monitor beendet `Strg` + `T`, danach `Strg` + `X`.

Ohne Skript lässt sich die Konsole unter Windows auch mit der VS-Code-Erweiterung „Serial Monitor“ oder
mit PuTTY ansehen: Port `COM3`, 115200 Baud.

## Flashen und Monitor in der VM

Für schnelle Versuche ohne Konfiguration geht es auch direkt aus der VM:

```
idf.py -B ~/build/gwios-satellite-bme280 -p /dev/ttyUSB0 flash
idf.py -B ~/build/gwios-satellite-bme280 -p /dev/ttyUSB0 monitor
```

Scheitert das Flashen mit `Failed to start stub flasher`, hilft `-b 115200`. Den Monitor beendet
`Strg` + `]`; wo das nicht ankommt (deutsche Tastatur im VS-Code-Terminal), `Strg` + `T` und danach
`Strg` + `X`.

## Messwerte abrufen

GwiOS holt die Messwerte per `GET http://<ip>/measurements` mit `Authorization: Bearer <token>` ab. Von
Windows aus lässt sich das so nachstellen (Name und IP anpassen):

```
$token = (Import-Csv "$env:USERPROFILE\.gwios\satellites\arbeitszimmer.csv" | Where-Object key -eq token).value
curl.exe -i -H "Authorization: Bearer $token" http://192.168.178.33/measurements
```

Antworten: `200` mit JSON, `401` ohne oder mit falschem Token, `503` vor der ersten Messung.
`ageSeconds` gibt an, wie alt die Messung ist; bleibt der Sensor stumm, wächst der Wert weiter.

## Tests

Die reine Logik (BME280-Kompensation, Regeln der Konfiguration, Pausen bei der WLAN-Neuverbindung,
Token-Prüfung, JSON der Messwerte, URL und Inhalt des Pings, Zustand der Status-LEDs) wird mit Host-Tests
geprüft. Sie liegen unter
`tests/GwiOS.Satellite.Bme280.Tests`, verwenden Unity aus ESP-IDF und laufen ohne Board. Ausführen in
einem **normalen** Terminal der VM, nicht im ESP-IDF-Terminal, im Wurzelverzeichnis des Repos:

```
cmake -S tests/GwiOS.Satellite.Bme280.Tests -B ~/build/gwios-satellite-bme280-tests
cmake --build ~/build/gwios-satellite-bme280-tests
ctest --test-dir ~/build/gwios-satellite-bme280-tests --output-on-failure
```

Im ESP-IDF-Terminal scheitert schon `cmake`: Dort steht der Linker des ULP-Coprozessors vor dem des
Systems im `PATH`. Hardwarenahes (I²C, Lesen der Konfiguration, WLAN, HTTP-Server, Ping) wird am Board
geprüft, siehe „Erwartete Ausgabe“.

## Erwartete Ausgabe

Mit gültiger Konfiguration:

```
I (xxx) satellite: Configured as 'wohnzimmer', reporting to https://webui.gwios.gwiasda.net.
I (xxx) wifi_connection: Connecting to 'Mein WLAN' as 'gwios-wohnzimmer'.
I (xxx) measurement_server: Serving GET /measurements on port 80.
I (xxx) satellite: Sensor ready on SDA=GPIO21, SCL=GPIO22.
I (xxx) satellite: 24.20 C, 50.64 %rH, 996.37 hPa
I (xxx) wifi_connection: Connected, IP 192.168.1.57, signal -58 dBm.
I (xxx) ping_client: Ping accepted (signal -58 dBm).
```

Weist GwiOS den Ping ab, nennt der Satellit den Grund (`401` Token falsch, `404` Satellit unbekannt,
`3xx` Weiterleitung — wird aus Sicherheitsgründen nicht verfolgt). Ist GwiOS gar nicht erreichbar oder
passt das Zertifikat nicht, steht dort `Ping to … failed` mit dem Fehler.

Ist das WLAN nicht erreichbar oder das Passwort falsch, meldet der Satellit den Grund als Zahl und versucht
es nach 1, 2, 4 … höchstens 60 Sekunden erneut. Häufige Gründe: `201` WLAN nicht gefunden, `15` oder
`204` falsches Passwort.

Ohne oder mit ungültiger Konfiguration nennt der Satellit jeden betroffenen Eintrag (nie dessen Wert) und
misst trotzdem weiter:

```
E (xxx) satellite_config: No configuration flashed (namespace 'satellite' is empty).
W (xxx) satellite: No valid configuration, network disabled. Measuring locally only.
```

Die Messwertzeile wiederholt sich alle fünf Sekunden. Der Luftdruck ist der Druck am Standort, nicht auf
Meereshöhe umgerechnet.
