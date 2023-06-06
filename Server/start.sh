#!/bin/sh

set -eu

sh /nakama/pre_start.sh

sh /ts/start.sh

TS_IP=$(/ts/tailscale ip -4)

echo "Starting to Fly..."

cd /nakama/
mkdir -p data/modules
mv index.js data/modules

exec /nakama/nakama --name tiktaktoe1 \
  --database.address "$DATABASE_URL" \
  --logger.level DEBUG \
  --runtime.js_entrypoint "./index.js" \
  --session.token_expiry_sec 7200 \
  --metrics.prometheus_port 9091 \
  --console.address "$TS_IP" \
  --console.port 80
