# Docker Deployment Guide

This guide explains how to deploy the GTA6fans API using Docker and Docker Compose.

## Prerequisites

- Docker Desktop (Windows/Mac) or Docker Engine (Linux)
- Docker Compose
- Git (for cloning the repository)

## Quick Start with Docker Compose

### 1. Clone and Navigate to the Project
```bash
git clone <repository-url>
cd GTA6fans.Backend
```

### 2. Start the Application Stack
```bash
docker-compose up -d
```

This command will:
- Build the GTA6fans API Docker image
- Start MongoDB with authentication
- Initialize the database with sample data
- Start the API with environment-based configuration

### 3. Verify the Deployment
- **API Health Check**: http://localhost:8080/health
- **API Documentation**: http://localhost:8080 (Swagger UI)
- **Detailed Health Check**: http://localhost:8080/health/detailed

## Environment Variables

The application supports the following environment variables for configuration:

### MongoDB Configuration
- `MONGODB_CONNECTION_STRING`: MongoDB connection string
- `MONGODB_DATABASE_NAME`: Database name to use

### Logging Configuration
- `LOG_LEVEL_DEFAULT`: Default logging level (Debug, Information, Warning, Error)
- `LOG_LEVEL_MICROSOFT`: Microsoft components logging level

### Security Configuration
- `ALLOWED_HOSTS`: Allowed hosts for the application
- `CORS_ORIGINS`: Comma-separated list of allowed CORS origins

### JWT Configuration (for future authentication)
- `JWT_SECRET`: Secret key for JWT token signing
- `JWT_ISSUER`: JWT token issuer
- `JWT_AUDIENCE`: JWT token audience
- `JWT_EXPIRE_MINUTES`: Token expiration time in minutes

## Docker Commands

### Build the API Image
```bash
docker build -t gta6fans-api:latest .
```

### Run the API Container Manually
```bash
docker run -d \
  --name gta6fans-api \
  -p 8080:8080 \
  -e MONGODB_CONNECTION_STRING="mongodb://localhost:27017" \
  -e MONGODB_DATABASE_NAME="gta6fans" \
  gta6fans-api:latest
```

### Run with MongoDB
```bash
# Start MongoDB
docker run -d \
  --name mongodb \
  -p 27017:27017 \
  -e MONGO_INITDB_ROOT_USERNAME=admin \
  -e MONGO_INITDB_ROOT_PASSWORD=password123 \
  mongo:7.0

# Start API
docker run -d \
  --name gta6fans-api \
  --link mongodb:mongodb \
  -p 8080:8080 \
  -e MONGODB_CONNECTION_STRING="mongodb://admin:password123@mongodb:27017/gta6fans?authSource=admin" \
  -e MONGODB_DATABASE_NAME="gta6fans" \
  gta6fans-api:latest
```

## Production Deployment

### Environment Variables for Production
Create a `.env` file:
```bash
# MongoDB Configuration
MONGODB_CONNECTION_STRING=mongodb://username:password@mongodb-host:27017/gta6fans?authSource=admin
MONGODB_DATABASE_NAME=gta6fans

# Security (Change these!)
JWT_SECRET=your-super-secure-jwt-secret-key-change-me
ALLOWED_HOSTS=yourdomain.com,www.yourdomain.com
CORS_ORIGINS=https://yourdomain.com,https://www.yourdomain.com

# Logging
LOG_LEVEL_DEFAULT=Warning
LOG_LEVEL_MICROSOFT=Error

# Application
ASPNETCORE_ENVIRONMENT=Production
```

### Production Docker Compose
```yaml
version: '3.8'
services:
  gta6fans-api:
    build: .
    ports:
      - "80:8080"
      - "443:8081"
    env_file:
      - .env
    restart: unless-stopped
    depends_on:
      - mongodb
    
  mongodb:
    image: mongo:7.0
    restart: unless-stopped
    volumes:
      - /data/mongodb:/data/db
    environment:
      MONGO_INITDB_ROOT_USERNAME: ${MONGO_USERNAME}
      MONGO_INITDB_ROOT_PASSWORD: ${MONGO_PASSWORD}
    ports:
      - "27017:27017"
```

## Monitoring and Logs

### View Application Logs
```bash
# Docker Compose logs
docker-compose logs -f gta6fans-api

# Direct container logs
docker logs -f gta6fans-api
```

### Health Monitoring
The application provides health check endpoints:

- **Basic Health**: `GET /health`
- **Detailed Health**: `GET /health/detailed`

Example response:
```json
{
  "Status": "Healthy",
  "Timestamp": "2024-01-01T12:00:00.000Z",
  "Service": "GTA6fans.Api",
  "Version": "1.0.0",
  "Environment": "Production",
  "Database": {
    "Status": "Connected",
    "DatabaseName": "gta6fans",
    "Message": "Database connection is healthy"
  }
}
```

## Troubleshooting

### Common Issues

1. **MongoDB Connection Failed**
   ```bash
   # Check if MongoDB is running
   docker-compose ps
   
   # Check MongoDB logs
   docker-compose logs mongodb
   
   # Restart MongoDB
   docker-compose restart mongodb
   ```

2. **Application Won't Start**
   ```bash
   # Check application logs
   docker-compose logs gta6fans-api
   
   # Rebuild the image
   docker-compose build --no-cache gta6fans-api
   docker-compose up -d gta6fans-api
   ```

3. **Port Already in Use**
   ```bash
   # Change ports in docker-compose.yml
   ports:
     - "8081:8080"  # Change first port to available port
   ```

4. **Database Initialization Issues**
   ```bash
   # Remove and recreate volumes
   docker-compose down -v
   docker-compose up -d
   ```

### Debugging

1. **Access Container Shell**
   ```bash
   docker exec -it gta6fans-api bash
   ```

2. **Check Configuration**
   ```bash
   # Inside container
   cat appsettings.json
   ```

3. **Test Database Connection**
   ```bash
   # Connect to MongoDB
   docker exec -it gta6fans-mongodb mongosh -u admin -p password123
   ```

## Performance Tuning

### MongoDB Optimization
```javascript
// Inside MongoDB shell
use gta6fans;

// Check index usage
db.users.explain("executionStats").find({username: "admin"});

// Add compound indexes for common queries
db.users.createIndex({isActive: 1, isDeleted: 1});
db.users.createIndex({createdAt: -1, isDeleted: 1});
```

### API Optimization
- Use environment variable `ASPNETCORE_ENVIRONMENT=Production`
- Configure appropriate logging levels
- Set up reverse proxy (nginx) for SSL termination
- Configure connection pooling for MongoDB

## Security Best Practices

1. **Change Default Credentials**
   - Update MongoDB username/password
   - Use strong JWT secret
   - Configure proper CORS origins

2. **Network Security**
   - Don't expose MongoDB port in production
   - Use Docker networks for internal communication
   - Configure firewall rules

3. **Container Security**
   - Application runs as non-root user
   - Minimal base image (aspnet runtime)
   - No sensitive data in image layers

## Scaling

### Horizontal Scaling
```yaml
version: '3.8'
services:
  gta6fans-api:
    build: .
    deploy:
      replicas: 3
    ports:
      - "8080-8082:8080"
```

### Load Balancer (nginx)
```nginx
upstream gta6fans_api {
    server localhost:8080;
    server localhost:8081;
    server localhost:8082;
}

server {
    listen 80;
    location / {
        proxy_pass http://gta6fans_api;
    }
}
```

## Backup and Recovery

### Database Backup
```bash
# Create backup
docker exec gta6fans-mongodb mongodump --username admin --password password123 --authenticationDatabase admin --out /backup

# Restore backup
docker exec gta6fans-mongodb mongorestore --username admin --password password123 --authenticationDatabase admin /backup
```

### Volume Backup
```bash
# Backup MongoDB data volume
docker run --rm -v gta6fans_backend_mongodb_data:/data -v $(pwd):/backup alpine tar czf /backup/mongodb_backup.tar.gz /data
```