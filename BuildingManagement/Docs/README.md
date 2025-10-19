# ?? Building Management - Deployment Guide

**One simple guide for deploying your Building Management System to Ubuntu server using Docker.**

---

## ?? Table of Contents

1. [Quick Overview](#-quick-overview)
2. [Prerequisites](#-prerequisites)
3. [Deployment Steps](#-deployment-steps)
4. [Updating Your Application](#-updating-your-application)
5. [Common Commands](#-common-commands)
6. [Troubleshooting](#-troubleshooting)

---

## ?? Quick Overview

**Your situation:** Can't clone repository on server

**Solution:** Build Docker image on your Windows PC ? Push to Docker Hub ? Pull and run on Ubuntu server

**Benefits:**
- ? No source code needed on server
- ? Fast deployments (30 seconds)
- ? Easy updates
- ? Secure

---

## ?? Prerequisites

### On Your Windows PC:
- Docker Desktop installed
- Docker Hub account (free): https://hub.docker.com

### On Your Ubuntu Server:
- Ubuntu 20.04 or newer
- SSH access
- Internet connection

---

## ?? Deployment Steps

### STEP 1: Setup Docker Hub (One-time, 2 minutes)

1. Create free account at https://hub.docker.com
2. Remember your username (e.g., `johndoe`)

### STEP 2: Build & Push Image (On Windows, 5 minutes first time)

```powershell
# Navigate to project
cd C:\Dev\BuildingManagement\BuildingManagement

# Edit docker-build-push.ps1
# Change line 13: $DockerHubUsername = "your-dockerhub-username"
# Replace with YOUR username, e.g.: $DockerHubUsername = "johndoe"

# Login to Docker Hub (first time only)
docker login

# Build and push
.\deployment\docker-build-push.ps1 build-push
```

? **Your image is now on Docker Hub!**

### STEP 3: Setup Ubuntu Server (One-time, 10 minutes)

**Transfer files to server:**

```powershell
# From Windows PowerShell
scp deployment\docker-compose.server.yml username@server-ip:/home/username/
scp deployment\.env.example username@server-ip:/home/username/
scp deployment\server-setup.sh username@server-ip:/home/username/
```

**SSH into server:**

```bash
ssh username@server-ip
```

**Run automated setup:**

```bash

./server-setup.sh
# Follow prompts, enter YOUR Docker Hub username when asked
```

**Edit passwords:**

```bash
nano .env
```

Change these 3 things:
- `MYSQL_ROOT_PASSWORD=` ? Use a strong password
- `MYSQL_PASSWORD=` ? Use a different strong password  
- `JWT_KEY=` ? Use a random 32+ character string

Save: `Ctrl+X`, then `Y`, then `Enter`

### STEP 4: Deploy! (2 minutes)

```bash
# Start everything
docker compose up -d

# Check status
docker compose ps

# View logs
docker compose logs -f

# Check health
curl http://localhost:8080/health
```

? **Your app is running!**

Access at: `http://your-server-ip:8080/swagger`

---

## ?? Updating Your Application

### When You Make Code Changes:

**On Windows:**

```powershell
cd C:\Dev\BuildingManagement\BuildingManagement

# Build and push new version
.\deployment\docker-build-push.ps1 build-push
```

**On Ubuntu Server:**

```bash
# Pull new image
docker compose pull

# Restart with new image
docker compose up -d

# Verify
docker compose logs -f api
```

**Done! Update deployed in under 2 minutes.**

---

## ?? Common Commands

### On Windows (Building & Pushing)

```powershell
# Build and push
.\deployment\docker-build-push.ps1 build-push

# Build with version tag
.\deployment\docker-build-push.ps1 build-push -Tag v1.0.0

# Just build (don't push)
.\deployment\docker-build-push.ps1 build

# Just push (already built)
.\deployment\docker-build-push.ps1 push

# Help
.\deployment\docker-build-push.ps1 help
```

### On Ubuntu Server (Managing)

```bash
# Start services
docker compose up -d

# Stop services
docker compose down

# Restart services
docker compose restart

# View all logs
docker compose logs -f

# View only API logs
docker compose logs -f api

# Check status
docker compose ps

# Update application
docker compose pull && docker compose up -d

# Backup database
docker compose exec -T mysql mysqldump -u root -p<password> binayati > backup.sql

# Restore database
docker compose exec -T mysql mysql -u root -p<password> binayati < backup.sql
```

---

## ?? Troubleshooting

### Issue: Build fails on Windows

```powershell
# Make sure Docker Desktop is running
# Check: docker --version

# Check if you're in the right directory
pwd  # Should show: C:\Dev\BuildingManagement\BuildingManagement
```

### Issue: Can't login to Docker Hub

```powershell
# Make sure you have an account at https://hub.docker.com
docker login
# Enter your Docker Hub username and password
```

### Issue: Server can't pull image

```bash
# Check internet connection
ping hub.docker.com

# If using private repository, login on server
docker login

# Check image name is correct in docker-compose.yml
cat docker-compose.yml | grep image:
```

### Issue: Containers won't start

```bash
# Check logs
docker compose logs

# Check if ports are already in use
sudo netstat -tulpn | grep 8080
sudo netstat -tulpn | grep 3306

# Remove everything and start fresh (WARNING: deletes data!)
docker compose down -v
docker compose up -d
```

### Issue: Can't connect to database

```bash
# Wait 30 seconds for MySQL to initialize (first startup)
docker compose logs mysql

# Check MySQL is running
docker compose ps

# Test MySQL connection
docker compose exec mysql mysql -u root -p
```

### Issue: Forgot passwords

```bash
# Edit .env file
nano .env

# Change passwords, then restart
docker compose down
docker compose up -d
```

---

## ?? Security Checklist

Before going live:

- [ ] Changed `MYSQL_ROOT_PASSWORD` in `.env`
- [ ] Changed `MYSQL_PASSWORD` in `.env`
- [ ] Set strong `JWT_KEY` in `.env` (32+ random characters)
- [ ] Added `.env` to `.gitignore` (already done)
- [ ] Setup firewall on server (server-setup.sh does this)
- [ ] Setup domain name
- [ ] Setup SSL certificate (Let's Encrypt)
- [ ] Regular database backups

---

## ?? Files in Deployment Folder

| File | Purpose |
|------|---------|
| `docker-build-push.ps1` | Build & push images (Windows) |
| `docker-build-push.sh` | Build & push images (Linux/Mac) |
| `docker-compose.server.yml` | Server deployment config |
| `.env.example` | Environment variables template |
| `server-setup.sh` | Automated server setup script |
| `deploy.ps1` | Local development helper (Windows) |
| `deploy.sh` | Local development helper (Linux/Mac) |

---

## ?? Quick Reference Card

### First Time Setup

```powershell
# Windows
1. Create Docker Hub account
2. Edit deployment\docker-build-push.ps1 (set username)
3. docker login
4. .\deployment\docker-build-push.ps1 build-push
```

```bash
# Ubuntu Server
1. Transfer files with scp
2. chmod +x server-setup.sh && ./server-setup.sh
3. nano .env (change passwords)
4. docker compose up -d
```

### Regular Updates

```powershell
# Windows: Build & push
.\deployment\docker-build-push.ps1 build-push
```

```bash
# Server: Pull & restart
docker compose pull && docker compose up -d
```

### Quick Commands

| Task | Command |
|------|---------|
| Check status | `docker compose ps` |
| View logs | `docker compose logs -f` |
| Restart | `docker compose restart` |
| Stop | `docker compose down` |
| Backup DB | `docker compose exec -T mysql mysqldump -u root -p<pwd> binayati > backup.sql` |
| Check health | `curl http://localhost:8080/health` |

---

## ?? Pro Tips

1. **Version your images**: Use tags like `v1.0.0`, `v1.1.0` etc.
   ```powershell
   .\deployment\docker-build-push.ps1 build-push -Tag v1.0.0
   ```

2. **Keep old versions**: Don't delete old image tags (useful for rollbacks)

3. **Automated backups**: Create a cron job on server
   ```bash
   crontab -e
   # Add: 0 2 * * * cd /home/username && docker compose exec -T mysql mysqldump -u root -p<pwd> binayati > backup_$(date +\%Y\%m\%d).sql
   ```

4. **Monitor logs**: Regularly check `docker compose logs` for errors

5. **Update regularly**: Keep Docker images up to date
   ```bash
   docker compose pull
   docker compose up -d
   ```

---

## ?? Need More Help?

- **Docker Hub**: https://hub.docker.com
- **Docker Docs**: https://docs.docker.com
- **Project Repo**: https://github.com/itani-rayan/BuildingManagement

---

**That's it! Everything you need in one place.** ??

Save this README - you can refer back anytime you forget the deployment steps!
