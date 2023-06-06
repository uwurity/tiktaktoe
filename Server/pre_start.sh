#!/bin/sh

set -eu

echo "Preparing to Fly..."

echo -n "Fetching cluster cert... "
curl -o /nakama/db.crt "https://cockroachlabs.cloud/clusters/$CLUSTER_ID/cert" -s
echo "done!"

echo -n "Migrating database... "
/nakama/nakama migrate up --database.address "$DATABASE_URL"
echo "done!"
