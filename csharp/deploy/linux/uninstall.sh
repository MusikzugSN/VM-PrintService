#!/bin/bash
# Entfernt den VM-Druckdienst wieder.  Aufruf:  sudo bash linux/uninstall.sh
set -e
DIENST=vm-druckdienst

if [ "$(id -u)" -ne 0 ]; then
    echo "Bitte mit sudo starten: sudo bash linux/uninstall.sh"
    exit 1
fi

systemctl disable --now "$DIENST" 2> /dev/null || true
rm -f /etc/systemd/system/$DIENST.service
systemctl daemon-reload
rm -rf /opt/vm-druckdienst
echo "Dienst entfernt. Logs bleiben in /var/log/$DIENST/ (bei Bedarf selbst löschen)."
