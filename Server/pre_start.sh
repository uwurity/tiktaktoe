#!/bin/sh

set -eu

echo "Preparing to Fly..."

echo "Fetching cluster cert..."
curl -o /nakama/db.crt "https://cockroachlabs.cloud/clusters/$CLUSTER_ID/cert"

echo "Migrating database..."
/nakama/nakama migrate up --database.address "$DATABASE_URL"
