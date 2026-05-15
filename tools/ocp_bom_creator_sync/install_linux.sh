#!/usr/bin/env bash
set -euo pipefail

APP_DIR="${APP_DIR:-/home/d3h/ocp-bom-creator-sync}"
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"

mkdir -p "$APP_DIR"
cp "$SCRIPT_DIR/sync_bom_creator.py" "$APP_DIR/"
cp "$SCRIPT_DIR/requirements.txt" "$APP_DIR/"
if [ ! -f "$APP_DIR/.env" ]; then
  cp "$SCRIPT_DIR/env.example" "$APP_DIR/.env"
fi

python3 -m venv "$APP_DIR/.venv"
"$APP_DIR/.venv/bin/python" -m pip install --upgrade pip
"$APP_DIR/.venv/bin/pip" install -r "$APP_DIR/requirements.txt"

mkdir -p "$APP_DIR/runs" "$APP_DIR/logs"

echo "Installed to $APP_DIR"
echo "Edit $APP_DIR/.env before enabling the timer."
echo "Then copy systemd files:"
echo "  sudo cp $SCRIPT_DIR/systemd/ocp-bom-creator-sync.* /etc/systemd/system/"
echo "  sudo systemctl daemon-reload"
echo "  sudo systemctl enable --now ocp-bom-creator-sync.timer"
