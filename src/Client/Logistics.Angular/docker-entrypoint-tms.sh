#!/bin/sh
set -e

# Replace placeholder strings in compiled JS bundles with environment variable values
for file in /usr/share/nginx/html/*.js; do
  [ -f "$file" ] || continue
  sed -i "s|\${MAPBOX_TOKEN}|${MAPBOX_TOKEN}|g" "$file"
  sed -i "s|\${STRIPE_PUBLISHABLE_KEY}|${STRIPE_PUBLISHABLE_KEY}|g" "$file"
done

exec nginx -g 'daemon off;'
