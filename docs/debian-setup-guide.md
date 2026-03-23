# Zamboni NHL14 Legacy Server — Debian Headless Mini PC Setup Guide

This guide has **two paths**. Pick one:

- **Docker (recommended)** — fewer steps, easy updates, self-contained. See the section directly below.
- **Manual (bare-metal)** — no Docker dependency, full control. Skip to [Manual Install](#manual-install).

Because the mini PC sits behind a home router (NAT), both paths require port forwarding and optionally a dynamic DNS service if your ISP changes your public IP.

---

## Docker Install

### RPCS3 Client Setup

In RPCS3 go to **Configuration -> Network** and under IP/Host Switches add:

```
gosredirector.ea.com=YOUR_PUBLIC_IP
```

Replace `YOUR_PUBLIC_IP` with the public IP of your mini PC's router.

### Prerequisites

- Debian 12 (Bookworm) on the mini PC with SSH access
- Docker Engine and Docker Compose plugin installed (see below)
- The `gosredirector_mod.pfx` certificate file

### Port Forward on Your Router

Forward these ports from your router to the mini PC's local IP:

| Port | Protocol | Service |
|---|---|---|
| 42127 | TCP | Redirector (RPCS3 discovery) |
| 16767 | TCP | Blaze game server |
| 8082 | TCP | REST API |
| 17502 | TCP | QoS server |
| 17499 | UDP | QoS |
| 17500 | UDP | QoS |
| 17501 | UDP | QoS |

### Install Docker on Debian

```bash
apt-get update
apt-get install -y ca-certificates curl
install -m 0755 -d /etc/apt/keyrings
curl -fsSL https://download.docker.com/linux/debian/gpg -o /etc/apt/keyrings/docker.asc
chmod a+r /etc/apt/keyrings/docker.asc

echo "deb [arch=$(dpkg --print-architecture) signed-by=/etc/apt/keyrings/docker.asc] \
  https://download.docker.com/linux/debian $(. /etc/os-release && echo "$VERSION_CODENAME") stable" \
  | tee /etc/apt/sources.list.d/docker.list > /dev/null

apt-get update
apt-get install -y docker-ce docker-ce-cli containerd.io docker-compose-plugin
```

### Clone the repo and set up config

```bash
cd /opt
git clone --branch master https://github.com/ZamboniDevelopment/Zamboni14Legacy.git zamboni
cd /opt/zamboni

cp .env.example .env
nano .env          # set GAME_SERVER_IP and DB_PASSWORD
```

### TLS Certificate (`gosredirector_mod.pfx`)

The redirector server (port 42127) requires the EA `gosredirector.ea.com` certificate packaged as a `.pfx` file. A self-signed certificate will **not** work — the PS3 game client validates against the real EA cert.

**Get the cert from the openBlase repo:**

```bash
git clone https://github.com/openBlase/openBlase.git /tmp/openBlase
```

Then package it into the `.pfx` format Zamboni expects:

```bash
openssl pkcs12 -export \
  -out /opt/zamboni/gosredirector_mod.pfx \
  -inkey /tmp/openBlase/BlaseProxy/gosredirector.ea.com.key \
  -in /tmp/openBlase/BlaseProxy/gosredirector.ea.com.crt \
  -passout pass:123456

rm -rf /tmp/openBlase
```

### Build and start

The first build clones BlazeSDK, PSN, and the QoS server — it takes a few minutes.

```bash
docker compose up -d --build
```

### Verify

```bash
docker compose ps
docker compose logs -f zamboni14
curl http://127.0.0.1:8082/nhl14/status
```

### Update

```bash
cd /opt/zamboni
git pull
docker compose up -d --build
```

### Useful commands

```bash
docker compose down          # stop all services
docker compose logs zamboni14 -f
docker compose logs qos -f
docker compose logs postgres -f
docker compose restart zamboni14
```

> **Tip:** If your public IP changes, update `GAME_SERVER_IP` in `.env` and run `docker compose up -d` (no rebuild needed — the entrypoint regenerates the config from env vars at each start).

---

## Manual Install

After installing, in RPCS3 go to **Configuration → Network** and set
`gosredirector.ea.com` → your public IP (or DDNS hostname resolved to IP).

---

## Before You Start

| What you need | Notes |
|---|---|
| Debian 12 (Bookworm) on the mini PC | Other Debian versions work but package names may differ |
| SSH access to the mini PC | `ssh user@<local-ip>` from another machine on your network |
| Router admin access | To set a static LAN IP and port-forward |
| A static public IP **or** a DDNS service | So RPCS3 clients can always find your server |

Replace the following values everywhere in this guide:

- `192.168.1.X` → the mini PC's local IP address
- `YOUR_PUBLIC_IP` → your router's public IP (or DDNS hostname)
- `CHANGE_ME_STRONG_PASSWORD` → your PostgreSQL password

---

## Step 1 — Give the Mini PC a Static Local IP

Set a DHCP reservation in your router's admin panel so the mini PC always gets the same local IP (e.g. `192.168.1.X`).
This prevents port-forward rules from breaking after a reboot.

---

## Step 2 — Port Forward on Your Router

Forward these ports from your router's public IP to `192.168.1.X`:

| Port | Protocol | Service |
|---|---|---|
| 42127 | TCP | Redirector (RPCS3 discovery) |
| 16767 | TCP | Blaze game server |
| 8082 | TCP | REST API |
| 17502 | TCP | QoS server |
| 17499 | UDP | QoS |
| 17500 | UDP | QoS |
| 17501 | UDP | QoS |

The exact steps depend on your router brand. Look for "Port Forwarding" or "Virtual Servers" in its admin panel.

---

## Step 3 — Connect to the Mini PC

```bash
ssh user@192.168.1.X
```

Switch to root for the install steps (or prefix every command with `sudo`):

```bash
sudo -i
```

---

## Step 4 — Install Base Packages

```bash
apt update && apt upgrade -y
apt install -y git curl wget gnupg ca-certificates ufw \
    postgresql postgresql-contrib apt-transport-https
```

---

## Step 5 — Install .NET 8 SDK

Debian 12 does not include the Microsoft package feed by default.

```bash
wget https://packages.microsoft.com/config/debian/12/packages-microsoft-prod.deb
dpkg -i packages-microsoft-prod.deb
rm packages-microsoft-prod.deb

apt update
apt install -y dotnet-sdk-8.0
```

Verify:

```bash
dotnet --list-sdks
dotnet --list-runtimes
```

---

## Step 6 — Create Service User and Folders

```bash
useradd -r -m -d /opt/zamboni -s /bin/bash zamboni
mkdir -p /opt/zamboni/publish
chown -R zamboni:zamboni /opt/zamboni
```

---

## Step 7 — Create the PostgreSQL Database

```bash
sudo -u postgres psql -c "CREATE USER zamboni WITH PASSWORD 'CHANGE_ME_STRONG_PASSWORD';"
sudo -u postgres psql -c "CREATE DATABASE zamboni OWNER zamboni;"
```

---

## Step 8 — Clone Repositories

```bash
sudo -u zamboni -H bash -lc '
cd /opt/zamboni
git clone --branch master https://github.com/ZamboniDevelopment/Zamboni14Legacy.git
git clone --branch nhl14legacy-compatability https://github.com/ZamboniDevelopment/BlazeSDK.git
git clone --branch master https://github.com/Aim4kill/PSN.git
git clone --branch nhl10-compatability-lazy https://github.com/ZamboniDevelopment/Skateboard3Server.Qos.git
'
```

---

## Step 9 — Generate the TLS Certificate

The redirector server requires the EA `gosredirector.ea.com` certificate. A self-signed certificate will **not** work — the PS3 game client validates against the real EA cert.

Get the cert from the openBlase repo and package it:

```bash
git clone https://github.com/openBlase/openBlase.git /tmp/openBlase

openssl pkcs12 -export \
  -out /opt/zamboni/Zamboni14Legacy/gosredirector_mod.pfx \
  -inkey /tmp/openBlase/BlaseProxy/gosredirector.ea.com.key \
  -in /tmp/openBlase/BlaseProxy/gosredirector.ea.com.crt \
  -passout pass:123456

rm -rf /tmp/openBlase

chown zamboni:zamboni /opt/zamboni/Zamboni14Legacy/gosredirector_mod.pfx
chmod 600 /opt/zamboni/Zamboni14Legacy/gosredirector_mod.pfx
```

---

## Step 10 — Create the Zamboni Config

```bash
sudo -u zamboni tee /opt/zamboni/Zamboni14Legacy/zamboni-config.yml > /dev/null <<'EOF'
GameServerIp: "YOUR_PUBLIC_IP"
GameServerPort: 16767
LogLevel: "Info"
DatabaseConnectionString: "Host=localhost;Port=5432;Username=zamboni;Password=CHANGE_ME_STRONG_PASSWORD;Database=zamboni"
HostRedirectorInstance: true
ApiServerIdentifier: "nhl14"
ApiServerPort: "8082"
EOF
```

> **Tip:** If your ISP changes your public IP regularly, look into a free DDNS service (e.g. DuckDNS, No-IP).
> Put the current public IP in `GameServerIp` and keep it updated, or run a DDNS client on the mini PC.

---

## Step 11 — Create the QoS Config

```bash
cat > /opt/zamboni/Skateboard3Server.Qos/src/Skateboard3Server.Qos/appsettings.json <<'EOF'
{
  "Logging": {
    "LogLevel": {
      "Default": "Trace",
      "Microsoft": "Information",
      "Microsoft.Hosting.Lifetime": "Information"
    }
  },
  "AllowedHosts": "*",
  "Qos": {
    "QosIp": "YOUR_PUBLIC_IP",
    "FirewallPrimaryIp": "YOUR_PUBLIC_IP",
    "FirewallSecondaryIp": "YOUR_PUBLIC_IP"
  }
}
EOF
```

---

## Step 12 — OpenSSL Compatibility Config

NHL 14 / PS3 uses older TLS ciphers. This config lets the server accept them:

```bash
cat > /opt/zamboni/openssl-zamboni.cnf <<'EOF'
openssl_conf = openssl_init

[openssl_init]
providers = provider_sect
alg_section = algorithm_sect
ssl_conf = ssl_sect

[provider_sect]
default = default_sect
legacy = legacy_sect

[default_sect]
activate = 1

[legacy_sect]
activate = 1

[algorithm_sect]

[ssl_sect]
system_default = system_default_sect

[system_default_sect]
CipherString = DEFAULT:@SECLEVEL=0
MinProtocol = TLSv1
EOF
```

---

## Step 13 — Build the Server

```bash
sudo -u zamboni -H bash -lc '
export DOTNET_CLI_HOME=/opt/zamboni/.dotnet
export NUGET_PACKAGES=/opt/zamboni/.nuget/packages

cd /opt/zamboni/Zamboni14Legacy
dotnet publish -c Release -o /opt/zamboni/publish/z14

cd /opt/zamboni/Skateboard3Server.Qos/src/Skateboard3Server.Qos
dotnet publish -c Release -r linux-x64 --self-contained true -o /opt/zamboni/publish/qos
'
```

---

## Step 14 — Create systemd Services

### Zamboni game server

```bash
cat > /etc/systemd/system/zamboni14.service <<'EOF'
[Unit]
Description=Zamboni NHL14 Legacy Server
After=network.target postgresql.service
Wants=postgresql.service

[Service]
Type=simple
User=zamboni
WorkingDirectory=/opt/zamboni/publish/z14
ExecStart=/usr/bin/dotnet /opt/zamboni/publish/z14/Zamboni14Legacy.dll
Restart=on-failure
RestartSec=5
Environment=DOTNET_CLI_HOME=/opt/zamboni/.dotnet
Environment=OPENSSL_CONF=/opt/zamboni/openssl-zamboni.cnf

[Install]
WantedBy=multi-user.target
EOF
```

### QoS server

```bash
cat > /etc/systemd/system/qos.service <<'EOF'
[Unit]
Description=Zamboni QoS Server
After=network.target

[Service]
Type=simple
User=zamboni
WorkingDirectory=/opt/zamboni/publish/qos
ExecStart=/opt/zamboni/publish/qos/Skateboard3Server.Qos
Restart=on-failure
RestartSec=5

[Install]
WantedBy=multi-user.target
EOF
```

---

## Step 15 — Configure Firewall

```bash
ufw allow OpenSSH
ufw allow 42127/tcp
ufw allow 16767/tcp
ufw allow 8082/tcp
ufw allow 17502/tcp
ufw allow 17499/udp
ufw allow 17500/udp
ufw allow 17501/udp
ufw --force enable
ufw status
```

---

## Step 16 — Enable and Start Services

```bash
systemctl daemon-reload

systemctl enable postgresql
systemctl enable zamboni14.service
systemctl enable qos.service

systemctl start postgresql
systemctl start zamboni14.service
systemctl start qos.service
```

---

## Step 17 — Verify Installation

Check service status:

```bash
systemctl status zamboni14.service --no-pager
systemctl status qos.service --no-pager
systemctl status postgresql --no-pager
```

Check ports are listening:

```bash
ss -ltnup | grep -E '42127|16767|8082|17502|17499|17500|17501|5432'
```

Test the API:

```bash
curl http://127.0.0.1:8082/nhl14/status
```

View live logs:

```bash
journalctl -u zamboni14.service -f
journalctl -u qos.service -f
```

---

## Updating the Server

```bash
sudo -u zamboni -H bash -lc '
cd /opt/zamboni/Zamboni14Legacy && git pull
cd /opt/zamboni/BlazeSDK && git pull
cd /opt/zamboni/PSN && git pull
cd /opt/zamboni/Skateboard3Server.Qos && git pull
'

sudo -u zamboni -H bash -lc '
export DOTNET_CLI_HOME=/opt/zamboni/.dotnet
export NUGET_PACKAGES=/opt/zamboni/.nuget/packages

cd /opt/zamboni/Zamboni14Legacy
dotnet publish -c Release -o /opt/zamboni/publish/z14

cd /opt/zamboni/Skateboard3Server.Qos/src/Skateboard3Server.Qos
dotnet publish -c Release -r linux-x64 --self-contained true -o /opt/zamboni/publish/qos
'

systemctl restart zamboni14.service
systemctl restart qos.service
```

---

## Keeping GameServerIp Up to Date (Dynamic Public IP)

If your ISP assigns a new public IP periodically, you must update `zamboni-config.yml` and the QoS `appsettings.json`, then restart the services. A simple approach is to install a DDNS client and point `GameServerIp` to the IP resolved by your DDNS hostname whenever you rebuild.

Alternatively, install `ddclient`:

```bash
apt install -y ddclient
```

Configure it for your DDNS provider (DuckDNS, No-IP, Cloudflare, etc.) and it will keep your DNS record current automatically.

---

## Clean Uninstall

```bash
systemctl stop zamboni14.service qos.service 2>/dev/null || true
systemctl disable zamboni14.service qos.service 2>/dev/null || true

rm -f /etc/systemd/system/zamboni14.service
rm -f /etc/systemd/system/qos.service
systemctl daemon-reload

rm -rf /opt/zamboni
userdel -r zamboni 2>/dev/null || true

sudo -u postgres psql -c "DROP DATABASE IF EXISTS zamboni;"
sudo -u postgres psql -c "DROP ROLE IF EXISTS zamboni;"

ufw delete allow 42127/tcp
ufw delete allow 16767/tcp
ufw delete allow 8082/tcp
ufw delete allow 17502/tcp
ufw delete allow 17499/udp
ufw delete allow 17500/udp
ufw delete allow 17501/udp
```

---

## Final Notes

- Replace `192.168.1.X` with your mini PC's local IP
- Replace `YOUR_PUBLIC_IP` with your router's public IP or DDNS-resolved IP
- Replace `CHANGE_ME_STRONG_PASSWORD` with a strong PostgreSQL password
- The mini PC must be powered on and reachable for remote players to connect
- **Online Team Play** (OTP) is supported — up to 6 players per team (12 total per lobby)
