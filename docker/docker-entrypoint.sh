#!/bin/sh
set -e

# Validate required env vars
: "${GAME_SERVER_IP:?GAME_SERVER_IP environment variable is required}"
: "${DB_PASSWORD:?DB_PASSWORD environment variable is required}"

# Generate zamboni-config.yml at container start so env vars take effect
cat > /app/zamboni-config.yml <<EOF
GameServerIp: "${GAME_SERVER_IP}"
GameServerPort: ${GAME_SERVER_PORT:-16767}
LogLevel: "${LOG_LEVEL:-Info}"
DatabaseConnectionString: "Host=postgres;Port=5432;Username=zamboni;Password=${DB_PASSWORD};Database=zamboni"
HostRedirectorInstance: true
ApiServerIdentifier: "nhl14"
ApiServerPort: "8082"
EOF

exec dotnet Zamboni14Legacy.dll
