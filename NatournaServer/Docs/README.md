# 🚀 Natourna Server - Deployment User Manual

Complete user manual for deploying production and development environments on the same Ubuntu server.

---

## 📋 Overview

This deployment system allows you to run **both production and development environments** simultaneously on your Ubuntu server using Docker containers. Each environment is completely isolated with its own database and runs on different ports.

### 🎯 Environment Setup

| Environment | API Port | MySQL Port | Container Prefix | Use Case |
|-------------|----------|------------|------------------|----------|
| **Production** | 8080 | 3306 | `-prod` | Live production system |
| **Development** | 9080 | 3307 | `-dev` | Testing and development |

### 📦 Containers per Environment

Each environment runs **2 containers**:
1. **MySQL Database** - Database server
2. **.NET API Application** - Backend API (from Docker Hub)

**Total: 4 containers** running on your server (2 prod + 2 dev)

---

## 📁 Folder Structure on Server

```
/opt/deployment/
├── production/                          🏭 Production Environment
│   ├── docker-compose.yml              ⚙️  Production config
│   ├── production.env                  📄 Environment template
│   ├── .env                            🔐 Active config (copy from production.env)
│   └── logs/                           📝 Application logs
│
└── development/                         🧪 Development Environment
    ├── docker-compose.yml              ⚙️  Development config
    ├── development.env                 📄 Environment template
    ├── .env                            🔐 Active config (copy from development.env)
    └── logs/                           📝 Application logs
```

---

## 🚀 Initial Server Setup

### Step 1: Install Docker on Ubuntu Server

```bash
# Install Docker
curl -fsSL https://get.docker.com -o get-docker.sh
sudo sh get-docker.sh

# Add user to docker group
sudo usermod -aG docker $USER
newgrp docker

# Install Docker Compose plugin
sudo apt install docker-compose-plugin -y

# Verify installation
docker --version
docker compose version
```

### Step 2: Create Deployment Directories

```bash
# Create directories
sudo mkdir -p /opt/deployment/production
sudo mkdir -p /opt/deployment/development

# Set ownership
sudo chown -R $USER:$USER /opt/deployment
```

### Step 3: Transfer Files from Windows to Server

**From your Windows machine**, run PowerShell and transfer files:

```powershell
# Set your server IP
$SERVER = "your-server-ip"

# Transfer production files
scp deployment\production\docker-compose.yml root@${SERVER}:/opt/deployment/production/
scp deployment\production\production.env root@${SERVER}:/opt/deployment/production/

# Transfer development files
scp deployment\development\docker-compose.yml root@${SERVER}:/opt/deployment/development/
scp deployment\development\development.env root@${SERVER}:/opt/deployment/development/
```

### Step 4: Configure Environment Files on Server

**SSH into your server** and set up environment files:

```bash
ssh root@your-server-ip

# Configure Production
cd /opt/deployment/production
cp production.env .env
nano .env  # Edit passwords if needed

# Configure Development
cd /opt/deployment/development
cp development.env .env
nano .env  # Edit passwords if needed
```

---

## 🏭 Production Environment

### Deploy Production

```bash
cd /opt/deployment/production
docker compose up -d
```

### Check Production Status

```bash
cd /opt/deployment/production

# View running containers
docker compose ps

# View logs
docker compose logs -f

# Test API health
curl http://localhost:8080/health
```

### Update Production

When you've built and pushed a new Docker image:

```bash
cd /opt/deployment/production

# Pull latest image
docker compose pull

# Restart with new image
docker compose up -d

# View logs
docker compose logs -f api
```

### Stop Production

```bash
cd /opt/deployment/production
docker compose down
```

### Production Environment Variables

Located in `/opt/deployment/production/.env`:

```env
MYSQL_ROOT_PASSWORD=ProdRoot@2025!
MYSQL_DATABASE=binayati
MYSQL_USER=buildinguser
MYSQL_PASSWORD=ProdUser@2025!
ASPNETCORE_ENVIRONMENT=Production
JwtSettings__SecretKey=rlsLBB/CRUksCnfLErhe1kS1DzRr+wzt
JwtSettings__Issuer=NatournaServer
JwtSettings__Audience=NatournaServerApp
JwtSettings__ExpirationMinutes=1440
```

---

## 🧪 Development Environment

### Deploy Development

```bash
cd /opt/deployment/development
docker compose up -d
```

### Check Development Status

```bash
cd /opt/deployment/development

# View running containers
docker compose ps

# View logs
docker compose logs -f

# Test API health
curl http://localhost:9080/health
```

### Update Development

When you want to test new changes:

```bash
cd /opt/deployment/development

# Pull latest image
docker compose pull

# Restart with new image
docker compose up -d

# View logs
docker compose logs -f api
```

### Stop Development

```bash
cd /opt/deployment/development
docker compose down
```

---

## 🔄 Build & Push Docker Image (Windows)

### Understanding Image Tags

Your deployment uses **Docker image tags** to control which version runs in each environment:

| Environment | Docker Image Tag | Purpose |
|-------------|------------------|---------|
| **Production** | `:latest` | Stable, production-ready releases |
| **Development** | `:dev` | Testing and development builds |

### Build and Push Commands

**All builds happen on your Windows machine**, then you pull them on the server.

#### Option 1: Build for Development Only

Use this when testing new features:

```powershell
# Navigate to your project directory
cd C:\Dev\NatournaServer

# Build and push with 'dev' tag
.\deployment\docker-build-push.ps1 build-push -Tag "dev"
```

**Result:** Pushes to Docker Hub as `itanirayan/natourna-server-api:dev`

#### Option 2: Build for Production Only

Use this for stable releases:

```powershell
# Navigate to your project directory
cd C:\Dev\NatournaServer

# Build and push with 'latest' tag (default)
.\deployment\docker-build-push.ps1 build-push
```

**Result:** Pushes to Docker Hub as `itanirayan/natourna-server-api:latest`

#### Option 3: Build for Both Environments

Use this to deploy the same code to both environments:

```powershell
# Build and push dev version first
.\deployment\docker-build-push.ps1 build-push -Tag "dev"

# Build and push production version
.\deployment\docker-build-push.ps1 build-push
```

#### Option 4: Build with Semantic Versioning

Use this for version tracking:

```powershell
# Build specific version
.\deployment\docker-build-push.ps1 build-push -Tag "v1.2.0"

# Build and also tag as latest
.\deployment\docker-build-push.ps1 build-push -Tag "v1.2.0"
.\deployment\docker-build-push.ps1 build-push -Tag "latest"
```

---

### Deployment Workflow from Windows to Server

#### Scenario 1: Deploy to Development Only

**On Windows:**
```powershell
# 1. Build and push dev image
.\deployment\docker-build-push.ps1 build-push -Tag "dev"
```

**On Server:**
```bash
# 2. SSH to your server
ssh root@your-server-ip

# 3. Update ONLY development environment
cd /opt/deployment/development
docker compose pull
docker compose up -d

# 4. Verify
docker compose logs -f api
curl http://localhost:9080/health
```

**Production is NOT affected** ✅

---

#### Scenario 2: Deploy to Production Only

**On Windows:**
```powershell
# 1. Build and push production image
.\deployment\docker-build-push.ps1 build-push
```

**On Server:**
```bash
# 2. SSH to your server
ssh root@your-server-ip

# 3. Backup production database first!
cd /opt/deployment/production
docker compose exec -T mysql mysqldump -u root -pProdRoot@2025! binayati > backup_before_update_$(date +%Y%m%d_%H%M%S).sql

# 4. Update ONLY production environment
docker compose pull
docker compose up -d

# 5. Verify
docker compose logs -f api
curl http://localhost:8080/health
```

**Development is NOT affected** ✅

---

#### Scenario 3: Test in Dev, Then Deploy to Prod

**Step 1: Deploy to Development**

On Windows:
```powershell
# Build and push dev version
.\deployment\docker-build-push.ps1 build-push -Tag "dev"
```

On Server:
```bash
# Update development
cd /opt/deployment/development
docker compose pull && docker compose up -d

# Test thoroughly
curl http://localhost:9080/health
curl http://localhost:9080/swagger
```

**Step 2: If Tests Pass, Deploy to Production**

On Windows:
```powershell
# Build and push production version (same code)
.\deployment\docker-build-push.ps1 build-push
```

On Server:
```bash
# Backup production database
cd /opt/deployment/production
docker compose exec -T mysql mysqldump -u root -pProdRoot@2025! binayati > backup_$(date +%Y%m%d_%H%M%S).sql

# Update production
docker compose pull && docker compose up -d

# Verify
curl http://localhost:8080/health
```

---

#### Scenario 4: Different Versions in Dev and Prod

**On Windows:**
```powershell
# Keep production on v1.0.0
# Deploy new v1.1.0 to dev only

# Build and push to dev
.\deployment\docker-build-push.ps1 build-push -Tag "dev"
```

**On Server:**
```bash
# Only update development
cd /opt/deployment/development
docker compose pull && docker compose up -d

# Production still runs v1.0.0 (or :latest from before)
cd /opt/deployment/production
docker compose ps  # Shows old version still running
```

---

### Quick Reference: Windows Build Commands

```powershell
# Development builds
.\deployment\docker-build-push.ps1 build-push -Tag "dev"
.\deployment\docker-build-push.ps1 build-push -Tag "v1.0.0-dev"
.\deployment\docker-build-push.ps1 build-push -Tag "v1.0.0-beta"

# Production builds
.\deployment\docker-build-push.ps1 build-push
.\deployment\docker-build-push.ps1 build-push -Tag "latest"
.\deployment\docker-build-push.ps1 build-push -Tag "v1.0.0"

# Both environments (same code)
.\deployment\docker-build-push.ps1 build-push -Tag "dev"
.\deployment\docker-build-push.ps1 build-push -Tag "latest"
```

### Quick Reference: Server Pull Commands

```bash
# Update development only
cd /opt/deployment/development && docker compose pull && docker compose up -d

# Update production only
cd /opt/deployment/production && docker compose pull && docker compose up -d

# Update both (if you pushed both tags)
cd /opt/deployment/development && docker compose pull && docker compose up -d
cd /opt/deployment/production && docker compose pull && docker compose up -d
```

---

### Understanding the Image Tag System

```
Your Windows Machine          Docker Hub                    Your Server
──────────────────           ──────────                    ───────────

Build with -Tag "dev"   ──>   :dev tag         ──>   Development pulls :dev
Build with -Tag "latest" ──>  :latest tag      ──>   Production pulls :latest

                              Same image can
                              have multiple tags!
```

**Key Points:**
- ✅ Development docker-compose.yml uses `image: itanirayan/natourna-server-api:dev`
- ✅ Production docker-compose.yml uses `image: itanirayan/natourna-server-api:latest`
- ✅ Each environment only pulls its configured tag
- ✅ Building on Windows and pushing updates Docker Hub
- ✅ Server pulls from Docker Hub when you run `docker compose pull`

---

## 🗄️ Database Management

### Access Production Database

```bash
cd /opt/deployment/production

# Access MySQL CLI
docker compose exec mysql mysql -u buildinguser -p
# Password: ProdUser@2025! (from your .env file)

# Or as root
docker compose exec mysql mysql -u root -p
# Password: ProdRoot@2025! (from your .env file)
```

### Access Development Database

```bash
cd /opt/deployment/development

# Access MySQL CLI
docker compose exec mysql mysql -u buildinguser -p
# Password: DevUser@2025! (from your .env file)
```

### Backup Production Database

```bash
cd /opt/deployment/production

# Create backup
docker compose exec -T mysql mysqldump \
  -u root -pProdRoot@2025! \
  binayati > backup_prod_$(date +%Y%m%d_%H%M%S).sql

# Backup is saved in current directory
ls -lh backup_*.sql
```

### Backup Development Database

```bash
cd /opt/deployment/development

# Create backup
docker compose exec -T mysql mysqldump \
  -u root -pDevRoot@2025! \
  binayati > backup_dev_$(date +%Y%m%d_%H%M%S).sql
```

### Restore Database

```bash
# For production
cd /opt/deployment/production
docker compose exec -T mysql mysql \
  -u root -pProdRoot@2025! \
  binayati < backup_file.sql

# For development
cd /opt/deployment/development
docker compose exec -T mysql mysql \
  -u root -pDevRoot@2025! \
  binayati < backup_file.sql
```

### Copy Development Data to Production

```bash
# Step 1: Backup development database
cd /opt/deployment/development
docker compose exec -T mysql mysqldump \
  -u root -pDevRoot@2025! \
  binayati > dev_to_prod_export.sql

# Step 2: Restore to production
cd /opt/deployment/production
docker compose exec -T mysql mysql \
  -u root -pProdRoot@2025! \
  binayati < ../development/dev_to_prod_export.sql
```

---

## 🔍 Monitoring & Troubleshooting

### View All Running Containers

```bash
# See all Natourna Server containers
docker ps --filter name=natourna-server

# Expected output:
# natourna-server-api-prod
# natourna-server-mysql-prod
# natourna-server-api-dev
# natourna-server-mysql-dev
```

### Check Container Logs

```bash
# Production API logs (console output)
docker logs natourna-server-api-prod -f

# Production MySQL logs
docker logs natourna-server-mysql-prod -f

# Development API logs (console output)
docker logs natourna-server-api-dev -f

# Development MySQL logs
docker logs natourna-server-mysql-dev -f

# Or using docker compose (from deployment directory)
cd /opt/deployment/production
docker compose logs -f api

cd /opt/deployment/development
docker compose logs -f api
```

### Check Application Log Files

The application writes detailed logs to files on the host machine:

```bash
# View production log files
cd /opt/deployment/production/logs
ls -lh

# Tail production logs in real-time
tail -f /opt/deployment/production/logs/NatournaServer-$(date +%Y%m%d).log

# Search for errors in production logs
grep -i "error" /opt/deployment/production/logs/*.log

# View development log files
cd /opt/deployment/development/logs
ls -lh

# Tail development logs in real-time
tail -f /opt/deployment/development/logs/NatournaServer-$(date +%Y%m%d).log

# View logs from specific date
cat /opt/deployment/production/logs/NatournaServer-20250125.log
```

### Health Checks

```bash
# Production API
curl http://localhost:8080/health

# Production API Swagger
curl http://localhost:8080/swagger

# Development API
curl http://localhost:9080/health

# Development API Swagger
curl http://localhost:9080/swagger
```

### Check Container Status

```bash
# Production
cd /opt/deployment/production
docker compose ps

# Development
cd /opt/deployment/development
docker compose ps
```

### Restart Containers

```bash
# Restart production API only
cd /opt/deployment/production
docker compose restart api

# Restart entire production environment
docker compose restart

# Restart development environment
cd /opt/deployment/development
docker compose restart
```

### Common Issues

#### 1. Container Won't Start

```bash
# Check logs
docker compose logs

# Remove and recreate
docker compose down
docker compose up -d
```

#### 2. Port Already in Use

```bash
# Check what's using the port
sudo netstat -tulpn | grep 8080
sudo netstat -tulpn | grep 9080
sudo netstat -tulpn | grep 3306
sudo netstat -tulpn | grep 3307

# Stop conflicting service or change port in docker-compose.yml
```

#### 3. Database Connection Failed

```bash
# Check MySQL is healthy
docker compose exec mysql mysqladmin ping -h localhost

# Verify credentials in .env file
cat .env

# Check connection string in container
docker compose exec api env | grep ConnectionStrings
```

#### 4. Can't Pull Latest Image

```bash
# Login to Docker Hub if private repo
docker login

# Pull manually
docker pull itanirayan/natourna-server-api:latest

# Then restart
docker compose up -d
```

---

## 🌐 External Access Setup

### Configure Firewall

```bash
# Allow production API
sudo ufw allow 8080/tcp

# Allow development API
sudo ufw allow 9080/tcp

# Allow SSH (if not already)
sudo ufw allow 22/tcp

# Enable firewall
sudo ufw enable

# Check status
sudo ufw status
```

### Setup Nginx Reverse Proxy (Optional)

Install Nginx:

```bash
sudo apt update
sudo apt install nginx -y
```

**Production configuration** (`/etc/nginx/sites-available/natourna-server-prod`):

```nginx
server {
    listen 80;
    server_name yourdomain.com;

    location / {
        proxy_pass http://localhost:8080;
        proxy_set_header Host $host;
        proxy_set_header X-Real-IP $remote_addr;
        proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto $scheme;
    }
}
```

**Development configuration** (`/etc/nginx/sites-available/natourna-server-dev`):

```nginx
server {
    listen 80;
    server_name dev.yourdomain.com;

    location / {
        proxy_pass http://localhost:9080;
        proxy_set_header Host $host;
        proxy_set_header X-Real-IP $remote_addr;
        proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto $scheme;
    }
}
```

Enable sites:

```bash
# Enable production
sudo ln -s /etc/nginx/sites-available/natourna-server-prod /etc/nginx/sites-enabled/

# Enable development
sudo ln -s /etc/nginx/sites-available/natourna-server-dev /etc/nginx/sites-enabled/

# Test configuration
sudo nginx -t

# Restart Nginx
sudo systemctl restart nginx
```

### Setup SSL with Let's Encrypt (Optional)

```bash
# Install Certbot
sudo apt install certbot python3-certbot-nginx -y

# Get SSL certificate for production
sudo certbot --nginx -d yourdomain.com

# Get SSL certificate for development
sudo certbot --nginx -d dev.yourdomain.com

# Auto-renewal is configured automatically
# Test renewal
sudo certbot renew --dry-run
```

---

## 📊 Quick Reference Commands

### Production Commands

```bash
# Navigate to production
cd /opt/deployment/production

# Start
docker compose up -d

# Stop
docker compose down

# Update
docker compose pull && docker compose up -d

# Logs (container console output)
docker compose logs -f

# Application logs (file-based)
tail -f logs/NatournaServer-$(date +%Y%m%d).log

# Status
docker compose ps

# Backup DB
docker compose exec -T mysql mysqldump -u root -pProdRoot@2025! binayati > backup.sql

# Access MySQL
docker compose exec mysql mysql -u buildinguser -pProdUser@2025!
```

### Development Commands

```bash
# Navigate to development
cd /opt/deployment/development

# Start
docker compose up -d

# Stop
docker compose down

# Update
docker compose pull && docker compose up -d

# Logs (container console output)
docker compose logs -f

# Application logs (file-based)
tail -f logs/NatournaServer-$(date +%Y%m%d).log

# Status
docker compose ps

# Backup DB
docker compose exec -T mysql mysqldump -u root -pDevRoot@2025! binayati > backup.sql

# Access MySQL
docker compose exec mysql mysql -u buildinguser -pDevUser@2025!
```

### System-Wide Commands

```bash
# View all containers
docker ps -a --filter name=natourna-server

# View all volumes
docker volume ls | grep natourna-server

# View all networks
docker network ls | grep building

# System cleanup (careful!)
docker system prune -f

# Remove unused volumes (careful!)
docker volume prune -f
```

---

## 🔒 Security Best Practices

### 1. Change Default Passwords

Edit `.env` files and change all passwords:

```bash
# Production
nano /opt/deployment/production/.env

# Development
nano /opt/deployment/development/.env

# After changing, restart containers
docker compose down && docker compose up -d
```

### 2. Secure Secret Keys

Generate new JWT secret keys:

```bash
# Generate a secure random key
openssl rand -base64 32

# Update in .env files
JwtSettings__SecretKey=<your-new-secret>
```

### 3. File Permissions

```bash
# Restrict access to .env files
chmod 600 /opt/deployment/production/.env
chmod 600 /opt/deployment/development/.env

# Set directory permissions
chmod 750 /opt/deployment/production
chmod 750 /opt/deployment/development
```

### 4. Firewall Configuration

```bash
# Only allow necessary ports
sudo ufw default deny incoming
sudo ufw default allow outgoing
sudo ufw allow 22/tcp    # SSH
sudo ufw allow 80/tcp    # HTTP
sudo ufw allow 443/tcp   # HTTPS
sudo ufw allow 8080/tcp  # Production API
sudo ufw allow 9080/tcp  # Development API
sudo ufw enable
```

### 5. Regular Backups

Create a backup cron job:

```bash
# Edit crontab
crontab -e

# Add daily backup at 2 AM for production
0 2 * * * cd /opt/deployment/production && docker compose exec -T mysql mysqldump -u root -pProdRoot@2025! binayati > /opt/backups/prod_$(date +\%Y\%m\%d).sql

# Add daily backup at 3 AM for development
0 3 * * * cd /opt/deployment/development && docker compose exec -T mysql mysqldump -u root -pDevRoot@2025! binayati > /opt/backups/dev_$(date +\%Y\%m\%d).sql
```

---

## 📝 Typical Workflows

### Complete Deployment Flow Diagram

```
┌─────────────────────────────────────────────────────────────────┐
│                    YOUR WINDOWS MACHINE                         │
│                                                                 │
│  1. Make code changes                                          │
│  2. Run: .\deployment\docker-build-push.ps1 build-push        │
│     Options:                                                    │
│     • -Tag "dev"    → Builds for development                   │
│     • (no tag)      → Builds for production (latest)           │
│                                                                 │
└────────────────────────┬────────────────────────────────────────┘
                         │
                         │ Pushes image to
                         ↓
┌─────────────────────────────────────────────────────────────────┐
│                       DOCKER HUB                                │
│                                                                 │
│  itanirayan/natourna-server-api:dev     ← Development       │
│  itanirayan/natourna-server-api:latest  ← Production        │
│                                                                 │
└────────────────┬───────────────────┬────────────────────────────┘
                 │                   │
                 │                   │
        Pulls :dev                 Pulls :latest
                 │                   │
                 ↓                   ↓
┌────────────────────────┐  ┌────────────────────────┐
│   DEVELOPMENT ENV      │  │   PRODUCTION ENV       │
│   Port: 9080           │  │   Port: 8080           │
│   MySQL: 3307          │  │   MySQL: 3306          │
│                        │  │                        │
│   docker compose pull  │  │   docker compose pull  │
│   docker compose up -d │  │   docker compose up -d │
└────────────────────────┘  └────────────────────────┘
```

### Workflow 1: Update Development Only

**Purpose:** Test new features without affecting production

```powershell
# ===== ON WINDOWS =====
# Navigate to project
cd C:\Dev\NatournaServer

# Build and push to Docker Hub with 'dev' tag
.\deployment\docker-build-push.ps1 build-push -Tag "dev"
```

```bash
# ===== ON SERVER =====
# SSH to server
ssh root@your-server-ip

# Update ONLY development
cd /opt/deployment/development
docker compose pull
docker compose up -d

# Check logs
docker compose logs -f api

# Test the API
curl http://localhost:9080/health
curl http://localhost:9080/swagger
```

**Result:** 
- ✅ Development updated to new version
- ✅ Production unchanged

---

### Workflow 2: Update Production Only

**Purpose:** Deploy stable release to production

```powershell
# ===== ON WINDOWS =====
cd C:\Dev\NatournaServer

# Build and push to Docker Hub with 'latest' tag
.\deployment\docker-build-push.ps1 build-push
```

```bash
# ===== ON SERVER =====
ssh root@your-server-ip

# Backup production database first!
cd /opt/deployment/production
docker compose exec -T mysql mysqldump \
  -u root -pProdRoot@2025! \
  binayati > backup_before_update_$(date +%Y%m%d_%H%M%S).sql

# Update production
docker compose pull
docker compose up -d

# Check logs
docker compose logs -f api

# Test the API
curl http://localhost:8080/health
```

**Result:**
- ✅ Production updated to new version
- ✅ Development unchanged
- ✅ Database backed up before update

---

### Workflow 3: Test in Dev First, Then Promote to Prod

**Purpose:** Safe deployment - test thoroughly before production

**Step 1: Build and Deploy to Development**

```powershell
# ===== ON WINDOWS =====
cd C:\Dev\NatournaServer

# Build and push dev version
.\deployment\docker-build-push.ps1 build-push -Tag "dev"
```

```bash
# ===== ON SERVER =====
ssh root@your-server-ip

# Deploy to development
cd /opt/deployment/development
docker compose pull && docker compose up -d

# Monitor logs
docker compose logs -f api
```

**Step 2: Test Development Environment**

```bash
# Still on server, test thoroughly
curl http://localhost:9080/health
curl http://localhost:9080/swagger

# Test all endpoints
curl -X POST http://localhost:9080/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"email":"admin@example.com","password":"Admin@123"}'

# Check logs for errors
cd /opt/deployment/development
grep -i "error" logs/NatournaServer-$(date +%Y%m%d).log
```

**Step 3: If Tests Pass, Deploy to Production**

```powershell
# ===== ON WINDOWS =====
# Build same code for production
.\deployment\docker-build-push.ps1 build-push
```

```bash
# ===== ON SERVER =====
# Backup production
cd /opt/deployment/production
docker compose exec -T mysql mysqldump \
  -u root -pProdRoot@2025! \
  binayati > backup_$(date +%Y%m%d_%H%M%S).sql

# Deploy to production
docker compose pull && docker compose up -d

# Verify production
curl http://localhost:8080/health
docker compose logs -f api
```

**Result:**
- ✅ Tested in development first
- ✅ Production deployed only after successful tests
- ✅ Both environments now run the same code

---

### Workflow 4: Copy Data from Production to Development

```bash
# 1. Backup production
cd /opt/deployment/production
docker compose exec -T mysql mysqldump -u root -pProdRoot@2025! binayati > prod_backup.sql

# 2. Restore to development
cd /opt/deployment/development
docker compose exec -T mysql mysql -u root -pDevRoot@2025! binayati < ../production/prod_backup.sql

# 3. Verify
docker compose exec mysql mysql -u buildinguser -pDevUser@2025! -e "USE binayati; SHOW TABLES;"
```

---

## 🎓 Access Points Summary

### Production Environment

| Service | Local Access | External Access (with Nginx) |
|---------|-------------|------------------------------|
| API | `http://localhost:8080` | `http://yourdomain.com` |
| Swagger | `http://localhost:8080/swagger` | `http://yourdomain.com/swagger` |
| MySQL | `localhost:3306` | Not exposed |

### Development Environment

| Service | Local Access | External Access (with Nginx) |
|---------|-------------|------------------------------|
| API | `http://localhost:9080` | `http://dev.yourdomain.com` |
| Swagger | `http://localhost:9080/swagger` | `http://dev.yourdomain.com/swagger` |
| MySQL | `localhost:3307` | Not exposed |

### Container Names

| Environment | API Container | MySQL Container |
|-------------|--------------|-----------------|
| Production | `natourna-server-api-prod` | `natourna-server-mysql-prod` |
| Development | `natourna-server-api-dev` | `natourna-server-mysql-dev` |

### Volume Names

| Environment | MySQL Volume | App Data Volume |
|-------------|-------------|-----------------|
| Production | `mysql-prod-data` | `app-prod-data` |
| Development | `mysql-dev-data` | `app-dev-data` |

### Network Names

| Environment | Network Name |
|-------------|--------------|
| Production | `building-prod-network` |
| Development | `building-dev-network` |

---

## 💡 Pro Tips

1. **Always backup before updates**: Create a database backup before pulling new images
2. **Test in development first**: Deploy to development, test, then deploy to production
3. **Monitor logs after updates**: Use `docker compose logs -f` to watch for errors
4. **Keep .env files secure**: Never commit these files to git, use proper permissions
5. **Use version tags**: Tag your Docker images with versions for easy rollback
6. **Regular backups**: Set up automated daily backups with cron jobs
7. **Health checks**: Monitor `/health` endpoint to ensure services are running
8. **Resource monitoring**: Use `docker stats` to monitor container resource usage

---

## 🆘 Emergency Procedures

### Roll Back to Previous Version

```bash
# 1. Stop current containers
cd /opt/deployment/production
docker compose down

# 2. Pull specific version
docker pull itanirayan/natourna-server-api:v1.0.0

# 3. Update docker-compose.yml to use specific tag
nano docker-compose.yml
# Change: image: itanirayan/natourna-server-api:v1.0.0

# 4. Start with old version
docker compose up -d

# 5. Restore database if needed
docker compose exec -T mysql mysql -u root -pProdRoot@2025! binayati < backup.sql
```

### Complete Reset

```bash
# WARNING: This deletes all data!

# Stop and remove everything
cd /opt/deployment/production
docker compose down -v  # -v removes volumes

# Start fresh
docker compose up -d
```

### Disk Space Issues

```bash
# Check disk usage
df -h

# Check Docker disk usage
docker system df

# Clean up unused containers, images, networks
docker system prune -a

# Clean up unused volumes (careful!)
docker volume prune
```

---

## 📞 Support & Maintenance

### Logs Location

**Application Logs (Serilog - File Logs):**
- Production logs: `/opt/deployment/production/logs/`
- Development logs: `/opt/deployment/development/logs/`

**Docker Container Logs (Console Output):**
```bash
# View with docker compose (from the deployment directory)
docker compose logs

# View specific container logs
docker logs natourna-server-api-prod -f
docker logs natourna-server-api-dev -f
```

**Log File Naming:**
- Format: `NatournaServer-YYYYMMDD.log`
- Example: `NatournaServer-20250125.log`
- Rolling: Daily (new file each day)
- Retention: 30 days

**Access Log Files:**
```bash
# Production logs
cd /opt/deployment/production/logs
ls -lh

# View latest production log
tail -f /opt/deployment/production/logs/NatournaServer-$(date +%Y%m%d).log

# Development logs
cd /opt/deployment/development/logs
ls -lh

# View latest development log
tail -f /opt/deployment/development/logs/NatournaServer-$(date +%Y%m%d).log
```

### Monitoring Commands

```bash
# System resources
docker stats

# Disk usage
docker system df

# Container health
docker ps --filter "health=unhealthy"

# Network inspection
docker network inspect building-prod-network
docker network inspect building-dev-network
```

### Maintenance Schedule

- **Daily**: Check logs for errors
- **Weekly**: Review container resource usage
- **Monthly**: Update base images and clean up old images
- **Quarterly**: Review and update passwords/secrets

---

## 🎉 Summary

You now have a complete deployment system with:

✅ **4 Docker containers** (2 production + 2 development)  
✅ **Isolated environments** with separate databases and networks  
✅ **Different ports** (Production: 8080/3306, Development: 9080/3307)  
✅ **Easy updates** via Docker Hub image pulls  
✅ **Database backup/restore** procedures  
✅ **Security configurations** and best practices  
✅ **Monitoring and troubleshooting** tools  

### Quick Start Reminder

```bash
# Production
cd /opt/deployment/production
docker compose up -d

# Development
cd /opt/deployment/development
docker compose up -d

# Check everything is running
docker ps --filter name=natourna-server
```

---

*Last updated: January 2025*  
*Project: Natourna Server System*  
*Docker Hub: itanirayan/natourna-server-api*  
*Server Setup: Ubuntu 20.04+ with Docker*
