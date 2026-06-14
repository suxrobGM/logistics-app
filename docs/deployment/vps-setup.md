# VPS Initial Setup

Prepare your Ubuntu/Debian VPS for LogisticsX deployment.

## Prerequisites

- Ubuntu 22.04+ LTS or Debian 12+
- Root or sudo access
- SSH access configured

## Step 1: System Update

```bash
sudo apt update && sudo apt upgrade -y
```

## Step 2: Create Deploy User

```bash
sudo adduser logistics
sudo usermod -aG sudo logistics
su - logistics
```

## Step 3: Install Docker

```bash
curl -fsSL https://get.docker.com | sudo sh
sudo usermod -aG docker $USER
newgrp docker

# Verify
docker --version
docker compose version
```

## Step 4: Install Nginx

```bash
sudo apt install nginx -y
sudo systemctl enable nginx
```

## Step 5: Install Certbot (SSL)

```bash
sudo apt install certbot python3-certbot-nginx -y
```

## Step 6: Configure Firewall

```bash
sudo ufw allow OpenSSH
sudo ufw allow 'Nginx Full'
sudo ufw enable
```

## Step 7: Deploy the Stack

Production deploys are normally handled by the `deploy.yml` GitHub Actions workflow
(push to the `prod` branch). For a manual first deploy:

```bash
mkdir -p ~/deploy/logistics && cd ~/deploy/logistics

# Copy deploy/docker-compose.yml from the repo here, then configure the environment
cp /path/to/repo/deploy/.env.example .env
nano .env  # Edit with your production values (PostgreSQL is external)

echo "$GHCR_PAT" | docker login ghcr.io -u <github-user> --password-stdin
docker compose pull && docker compose up -d
```

## Step 8: Configure Nginx

Copy the nginx configuration from `deploy/nginx/logisticsx.conf` in the repo:

```bash
sudo cp logisticsx.conf /etc/nginx/sites-available/logisticsx.conf
sudo ln -s /etc/nginx/sites-available/logisticsx.conf /etc/nginx/sites-enabled/
sudo nginx -t
sudo systemctl reload nginx
```

## Step 9: Obtain SSL Certificates

```bash
sudo certbot --nginx \
  -d api.yourdomain.com -d id.yourdomain.com -d admin.yourdomain.com \
  -d tms.yourdomain.com -d customer.yourdomain.com \
  -d portainer.yourdomain.com -d yourdomain.com
```

## Step 10: Deploy Portainer (once)

Portainer runs as its own compose project so main-stack redeploys never remove it:

```bash
mkdir -p ~/deploy/portainer && cd ~/deploy/portainer
# Copy deploy/docker-compose.portainer.yml here, then:
docker compose -f docker-compose.portainer.yml up -d
```

It is reachable only via `https://portainer.yourdomain.com` (nginx -> 127.0.0.1:9000).

## Security Recommendations

1. **Disable password SSH**:

   ```bash
   sudo sed -i 's/#PasswordAuthentication yes/PasswordAuthentication no/' /etc/ssh/sshd_config
   sudo systemctl restart sshd
   ```

2. **Install Fail2ban**:

   ```bash
   sudo apt install fail2ban -y
   sudo systemctl enable fail2ban
   ```

3. **Enable automatic updates**:

   ```bash
   sudo apt install unattended-upgrades -y
   sudo dpkg-reconfigure -plow unattended-upgrades
   ```

## Next Steps

- [Docker Compose Deployment](docker-deployment.md) - Service configuration details
- [Environment Variables](../configuration/environment-variables.md) - Configuration reference
