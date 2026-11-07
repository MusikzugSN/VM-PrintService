#!/bin/bash
# Installiert oder aktualisiert den VM-Druckdienst als systemd-Dienst.
# Aufruf im veröffentlichten Ordner:  sudo bash linux/install.sh
set -e

ZIEL=/opt/vm-druckdienst
DIENST=vm-druckdienst
QUELLE="$(cd "$(dirname "$0")/.." && pwd)"

if [ "$(id -u)" -ne 0 ]; then
    echo "Bitte mit sudo starten: sudo bash linux/install.sh"
    exit 1
fi

if [ ! -f "$QUELLE/VM.Druckdienst.dll" ]; then
    echo "VM.Druckdienst.dll nicht gefunden in $QUELLE"
    exit 1
fi

if ! command -v lp > /dev/null; then
    echo "CUPS fehlt. Zuerst installieren: sudo apt install cups"
    exit 1
fi

echo "1/4 Benutzer $DIENST"
id "$DIENST" &> /dev/null || useradd --system --no-create-home --shell /usr/sbin/nologin "$DIENST"

echo "2/4 Dienst anhalten (falls er schon läuft)"
systemctl stop "$DIENST" 2> /dev/null || true

echo "3/4 Dateien nach $ZIEL kopieren"
mkdir -p "$ZIEL"
SICHERUNG=""
if [ -f "$ZIEL/appsettings.json" ]; then
    SICHERUNG=$(mktemp)
    cp "$ZIEL/appsettings.json" "$SICHERUNG"
fi
cp -r "$QUELLE"/. "$ZIEL"/
if [ -n "$SICHERUNG" ]; then
    mv "$SICHERUNG" "$ZIEL/appsettings.json"
    echo "    vorhandene appsettings.json behalten"
fi
chown -R root:root "$ZIEL"
chmod 755 "$ZIEL/VM.Druckdienst"
chmod 644 "$ZIEL/appsettings.json"

echo "4/4 systemd-Dienst einrichten und starten"
cp "$ZIEL/linux/$DIENST.service" /etc/systemd/system/
systemctl daemon-reload
systemctl enable --now "$DIENST"

sleep 3
systemctl --no-pager --lines=0 status "$DIENST" || true
if command -v curl > /dev/null; then
    curl -s http://127.0.0.1:8416/api/v1/health && echo
fi
echo "Fertig. Logs: /var/log/$DIENST/  und  journalctl -u $DIENST"
