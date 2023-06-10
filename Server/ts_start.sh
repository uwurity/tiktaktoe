#!/bin/sh

/ts/tailscaled --state=/var/lib/tailscale/tailscaled.state --socket=/var/run/tailscale/tailscaled.sock &
/ts/tailscale up --authkey=${TAILSCALE_AUTHKEY} --hostname=tiktaktoe-server
