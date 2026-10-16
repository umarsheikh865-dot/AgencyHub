#!/bin/sh
# Reload periodically so renewed certificates are picked up
while :; do sleep 6h; nginx -s reload; done &
