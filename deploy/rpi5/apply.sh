#!/usr/bin/env bash
#
# Server-Teil des Deploys der GwiOS WebUI. Läuft auf dem rpi5 und wird von
# Deploy-WebUI.ps1 aufgerufen, nicht von Hand:
#
#   bash apply.sh <image-archiv> <schlüsselverzeichnis> <sha256-der-.env>
#   (stdin: Inhalt der .env, base64-kodiert)
#
# Die .env kommt über stdin, damit die Secrets weder in einer Kommandozeile
# (sichtbar in "ps") noch als Datei auf dem Windows-Rechner landen.

set -euo pipefail

readonly image_name=gwios-webui
readonly ssd_mount=/mnt/ssd
# Benutzer "app" der .NET-Images. Ihm muss das Schlüsselverzeichnis gehören.
readonly container_user_id=1654
# So viele der neuesten Images bleiben für ein Zurückrollen liegen.
readonly images_to_keep=3

fail() {
    echo "FEHLER: $*" >&2
    exit 1
}

[[ $# -eq 3 ]] || fail "Aufruf: bash apply.sh <image-archiv> <schlüsselverzeichnis> <sha256-der-.env>"

target_directory="$(cd "$(dirname "$0")" && pwd)"
readonly target_directory
readonly image_archive="$1"
readonly keys_directory="$2"
readonly expected_env_file_hash="$3"

check_prerequisites() {
    docker info >/dev/null 2>&1 \
        || fail "Docker ist nicht erreichbar - installiert und $(whoami) in der Gruppe docker?"
    docker compose version >/dev/null 2>&1 \
        || fail "Docker Compose fehlt."
    docker network inspect proxy >/dev/null 2>&1 \
        || fail "Das Docker-Netz 'proxy' fehlt - läuft die Infrastruktur mit Traefik?"
    findmnt "$ssd_mount" >/dev/null \
        || fail "Die SSD ist nicht unter $ssd_mount eingehängt."
}

write_env_file() {
    local new_env_file="$target_directory/.env.new"
    # umask 077: Nur der eigene Benutzer darf die .env lesen. Erst eine neue
    # Datei schreiben, dann umbenennen - bei einem Abbruch bleibt so nie eine
    # halbe .env zurück. Die .env kommt base64-kodiert, damit keine
    # Zeichenkodierung unterwegs etwas verändert; --ignore-garbage überliest
    # die BOM und die Zeilenumbrüche, die Windows PowerShell hinzufügt.
    (umask 077 && base64 --decode --ignore-garbage > "$new_env_file")
    if [[ "$(sha256sum < "$new_env_file" | cut -d ' ' -f 1)" != "$expected_env_file_hash" ]]; then
        rm -f "$new_env_file"
        fail "Die .env ist verfälscht angekommen (Prüfsumme stimmt nicht) - nichts geändert."
    fi
    mv "$new_env_file" "$target_directory/.env"
}

ensure_keys_directory() {
    # sudo braucht es nur beim ersten Deploy, solange das Verzeichnis fehlt
    # oder noch nicht dem Container-Benutzer gehört.
    if [[ "$(stat -c %u "$keys_directory" 2>/dev/null)" != "$container_user_id" ]]; then
        sudo -n mkdir -p "$keys_directory" \
            && sudo -n chown "$container_user_id:$container_user_id" "$keys_directory" \
            && sudo -n chmod 700 "$keys_directory" \
            || fail "$keys_directory ließ sich nicht anlegen - sudo ohne Passwort für $(whoami) eingerichtet?"
    fi
}

load_image() {
    docker load --quiet --input "$image_archive"
    rm -f "$image_archive"
}

start_container() {
    # Compose erzeugt den Container nur neu, wenn sich Image oder Definition
    # geändert haben.
    cd "$target_directory"
    docker compose up --detach --remove-orphans
}

remove_old_images() {
    # Ein Image, das noch ein Container benutzt, lässt docker ohnehin nicht
    # löschen - Fehler dabei sind deshalb unkritisch.
    docker images "$image_name" --format '{{.CreatedAt}}|{{.ID}}' \
        | sort --reverse \
        | tail -n "+$((images_to_keep + 1))" \
        | cut -d '|' -f 2 \
        | xargs --no-run-if-empty docker rmi >/dev/null 2>&1 \
        || true
}

check_prerequisites
write_env_file
ensure_keys_directory
load_image
start_container
remove_old_images
