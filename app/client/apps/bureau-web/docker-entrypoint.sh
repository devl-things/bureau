#!/bin/sh
# Generates /usr/share/nginx/html/config.json from config.template.json
# by substituting environment variables via envsubst.
# Nginx then serves this file statically; the SPA loads it on startup.
#
# Place this script in /docker-entrypoint.d/ (Nginx Alpine runs all .sh files there).

set -e

TEMPLATE="/usr/share/nginx/html/config.template.json"
OUTPUT="/usr/share/nginx/html/config.json"

if [ -f "$TEMPLATE" ]; then
    envsubst < "$TEMPLATE" > "$OUTPUT"
    echo "bureau-web: config.json generated from template"
else
    echo "bureau-web: WARNING — config.template.json not found; config.json not generated" >&2
fi
