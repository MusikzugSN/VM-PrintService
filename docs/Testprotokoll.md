# Testprotokoll – VM-Druckdienst

**Testumgebung:** Windows 11, Visual Studio 2026, .NET 10 · CUPS in WSL2 (Ubuntu 24.04) mit PDF-Drucker `PDF` · Test-API `VM.TestApi` auf Port 8417 · Druckdienst auf Port 8416
**Testdaten:** `csharp/TestPdfs/` (A4, A3, A5, A4 quer, gemischt A4+A3, Letter, kaputt)
**Prüfung der Ausdrucke:** `~/PDF` in WSL mit `pdfinfo` (Seitengröße und Reihenfolge)

## Gesamttest über die Test-API – 29.10.2026

| Nr. | Test | Anfrage | Erwartet | Ergebnis | OK? |
|---|---|---|---|---|---|
| 1 | Normaler Druckauftrag | `POST /print/abc123`, Drucker `PDF` | 200, 5 von 5 Seiten, Reihenfolge A4, A4, A3, A3, A5 | 200, `gesamt 5, erfolgreich 5, fehlgeschlagen 0`. Ausdrucke: `10` A4 · `40` S.1 A4 · `30` A3 · `40` S.2 A3 · `20` A5 | ✅ |
| 2 | Kein Drucker angegeben | `POST /print/abc123`, Body `{ }` | 400, nichts gedruckt | 400 „Kein Drucker angegeben.“, nichts gedruckt | ✅ |
| 3 | Ungültige Zeichen im Druckernamen | Drucker `PDF; rm -rf /` | 400, nichts gedruckt | 400 „Der Druckername enthält ungültige Zeichen.“, nichts gedruckt | ✅ |
| 4 | Unbekannter Drucker | Drucker `GibtEsNicht` | 400, nichts gedruckt | 400 „Drucker 'GibtEsNicht' gibt es nicht.“, nichts gedruckt | ✅ |
| 5 | Druckerliste | `GET /printers` | 200, Drucker `PDF` als Standarddrucker | 200, `PDF`, `istStandard: true` | ✅ |
| 6 | Ungültiger Token | `POST /print/falsch` | 404, nichts gedruckt | 404 „Token ungültig oder Datei fehlt“, nichts gedruckt | ✅ |
| 7 | Notendatei fehlt (Datei 99) | `POST /print/kaputt` | 404, **gar nichts** gedruckt (auch Note 1 nicht) | 404, `~/PDF` leer | ✅ |
| 8 | Datei ist kein PDF (Datei 50) | `POST /print/pdf-kaputt` | 422, **gar nichts** gedruckt | 422 „Note 2 (Datei 50) ist kein lesbares PDF. Es wurde nichts gedruckt.“, `~/PDF` leer | ✅ (nach Fehlerbehebung, siehe unten) |
| 9 | Unbekanntes Format (US Letter) | `POST /print/letter` | 200, 1 von 2 Seiten, Fehler „unbekanntes Papierformat“ | 200, `gesamt 2, erfolgreich 1, fehlgeschlagen 1`, 1 Ausdruck A4 | ✅ |
| 10 | Ungültige Zeichen im Token | `POST /print/abc.123` | 400, nichts gedruckt | 400 „Der Token enthält ungültige Zeichen.“, nichts gedruckt | ✅ |
| 11 | API nicht erreichbar | Test-API beendet, dann `POST /print/abc123` | 502, nichts gedruckt | 502 „API nicht erreichbar“ (`localhost:8417` verweigert), nichts gedruckt | ✅ |
| 12 | Logdatei | `bin/Debug/net10.0/logs/druckdienst-20261029.log` | Für jeden Test eine passende Zeile | Alle Tests 1–11 protokolliert (Start, Ergebnis, Warnungen, Fehler mit Details) | ✅ |
| – | Temporäre Dateien | `%TEMP%\vm-druckdienst` nach jedem Test | Ordner leer | Ordner leer | ✅ |

## Installation als Linux-Dienst (WSL, Ubuntu 24.04) – 09.11.2026

Getestet auf dem Laptop von Ismail. Dienst mit `dotnet publish -r linux-x64 --self-contained` veröffentlicht und mit `sudo bash linux/install.sh` installiert. Test-API ebenfalls in Ubuntu gestartet. Ausdrucke vom PDF-Drucker in `/home/vm-druckdienst/PDF/`.

| Nr. | Test | Erwartet | Ergebnis | OK? |
|---|---|---|---|---|
| I1 | sudo bash linux/install.sh | 4 Schritte, Dienst active, Health ok | 4 Schritte, active (running), `{"status":"ok","version":"1.0.0"}` | ✅ |
| 17 | GET /health | status ok, version 1.0.0 | `{"status":"ok","version":"1.0.0"}` | ✅ |
| 5 | GET /printers | PDF, Standarddrucker | `[{"name":"PDF","istStandard":true,"papierfaecher":[]}]` | ✅ |
| 1 | POST /print/abc123 | 5 von 5, A4, A4, A3, A3, A5 | 5 von 5, (A4) (A4) (A3) (A3) (A5) | ✅ |
| 13 | POST /print/duplex | 3 von 3, Note 2 doppelseitig | 3 von 3, (A4) (A4)x2 | ✅ |
| 6 | POST /print/falsch | 404, nichts gedruckt | 404, nichts gedruckt | ✅ |
| 8 | POST /print/pdf-kaputt | 422, nichts gedruckt | 422 „Note 2 (Datei 50) ist kein lesbares PDF“, nichts gedruckt | ✅ |
| 15 | POST /print/gross | 500 von 500 | 500 von 500 in 7,3 s | ✅ |
| L1 | Logdatei | Datei mit heutigem Datum in /var/log/vm-druckdienst | `druckdienst-20261109.log`, letzte Zeile „500 von 500 Seiten gedruckt“ | ✅ |
| L2 | journalctl -u vm-druckdienst | gleiche Meldungen | gleiche Meldungen | ✅ |
| R1 | Absturz (systemctl kill) | nach 5 s wieder active | active | ✅ |
| R2 | Neustart (wsl --shutdown) | startet von selbst | active, Health ok | ✅ |
| C | Fehler in appsettings.json (ApiBasisUrl ohne http) | Dienst startet nicht, Meldung im Journal | „Der Druckdienst startet nicht … keine gültige http/https-Adresse“, status=1/FAILURE | ✅ |
| I2 | Update mit install.sh | vorhandene appsettings.json bleibt | „vorhandene appsettings.json behalten“, ApiTimeoutSekunden blieb 25 | ✅ |
| I3 | uninstall.sh | Dienst und /opt/vm-druckdienst entfernt | inactive, Ordner entfernt; Neuinstallation danach ok | ✅ |

## Gefundene Fehler

| Datum | Test | Fehler | Ursache | Behebung |
|---|---|---|---|---|
| 29.10.2026 | Gesamttest vor 10:03 | Druckauftrag druckte nichts (`0 von 0 Seiten`) | `Druckauftrag.Ausfuehren()` rief `GruppenDrucken()` nie auf, Sortieren und Gruppieren fehlten | `Ausfuehren()` neu geschrieben: PDFs lesen → sortieren → gruppieren → drucken (Commit `0ccce18`) |
| 29.10.2026 | Test 8, 12:36 | `pdf-kaputt` lieferte 500 „Druckauftrag fehlgeschlagen“ statt 422 | PDFs wurden zweimal gelesen, das erste Mal ohne Fehlerbehandlung | Erste Schleife entfernt (Commit `f6431ee`), Test 8 danach 422 ✅ |
| 29.10.2026 | Logdatei | Pro API-Aufruf 4 zusätzliche Zeilen vom `HttpClient` | Standard-Loglevel `Information` für `System.Net.Http.HttpClient` | Loglevel für `HttpClient` auf `Warning` gesetzt |
| 09.11.2026 | Test C | Bei einem Fehler in appsettings.json startet systemd den Dienst alle 5 Sekunden neu | `Restart=always` startet auch nach einem Konfigurationsfehler neu, den nur ein Mensch beheben kann | `RestartPreventExitStatus=1` in `vm-druckdienst.service`: Exit-Code 1 (Konfigurationsfehler) → kein Neustart, echte Abstürze weiter neu gestartet |

## Offene Punkte

- Test 9: Soll bei einer Seite mit unbekanntem Format der Rest gedruckt werden (aktuell) oder gar nichts? → Frage an Herrn Mertens
- Noch nicht getestet: echter Drucker mit A3 und mehreren Papierfächern
- Erledigt: API-Timeout 30 s (30.10.), Duplex (30.10.), großer Test mit 500 Seiten (04.11. und 09.11.), Installation als Linux-Dienst (09.11.)
