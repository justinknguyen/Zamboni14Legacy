# ── Stage 1: Build ─────────────────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

RUN apt-get update && apt-get install -y git && rm -rf /var/lib/apt/lists/*

WORKDIR /src

# Clone the sibling repos that Zamboni14Legacy.csproj references via ProjectReference.
# These must be at the same directory level as Zamboni14Legacy/ for the relative paths to resolve.
RUN git clone --branch nhl14legacy-compatability \
        https://github.com/ZamboniDevelopment/BlazeSDK.git BlazeSDK
RUN git clone --branch master \
        https://github.com/Aim4kill/PSN.git PSN

# Copy the game server source last so the layer above is cached across source changes.
COPY . Zamboni14Legacy/

WORKDIR /src/Zamboni14Legacy
RUN dotnet publish -c Release -o /app/publish

# ── Stage 2: Runtime ───────────────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime

WORKDIR /app

COPY --from=build /app/publish .

# OpenSSL compat so the server accepts the old TLS ciphers used by PS3 / NHL 14.
COPY openssl-zamboni.cnf /etc/openssl-zamboni.cnf
ENV OPENSSL_CONF=/etc/openssl-zamboni.cnf

# Entrypoint generates zamboni-config.yml from environment variables at startup.
# sed strips Windows \r so the script works even when built on a Windows host.
COPY docker-entrypoint.sh /entrypoint.sh
RUN sed -i 's/\r$//' /entrypoint.sh && chmod +x /entrypoint.sh

# Ports:
#   42127 TCP  Redirector (RPCS3 discovery)
#   16767 TCP  Blaze game server
#   8082  TCP  REST API
EXPOSE 42127 16767 8082

ENTRYPOINT ["/entrypoint.sh"]
