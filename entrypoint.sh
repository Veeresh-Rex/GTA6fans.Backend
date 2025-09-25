#!/bin/bash
set -e

echo "Starting GTA6fans API with environment configuration..."

# Function to update JSON configuration
update_config() {
    local config_file="$1"
    local json_path="$2"
    local env_value="$3"
    
    if [ -n "$env_value" ]; then
        echo "Updating $json_path with value from environment variable"
        # Create temporary file with updated JSON
        python3 -c "
import json
import sys

try:
    with open('$config_file', 'r') as f:
        config = json.load(f)
    
    # Split the path and navigate to the correct location
    path_parts = '$json_path'.split('.')
    current = config
    
    # Navigate to the parent of the target key
    for part in path_parts[:-1]:
        if part not in current:
            current[part] = {}
        current = current[part]
    
    # Set the final value
    current[path_parts[-1]] = '$env_value'
    
    with open('$config_file', 'w') as f:
        json.dump(config, f, indent=2)
        
    print('Successfully updated $json_path')
except Exception as e:
    print(f'Error updating configuration: {e}', file=sys.stderr)
    sys.exit(1)
"
    fi
}

# Install Python if not available (for JSON manipulation)
if ! command -v python3 &> /dev/null; then
    echo "Installing Python for JSON configuration..."
    apt-get update && apt-get install -y python3 && rm -rf /var/lib/apt/lists/*
fi

# Configuration file path
CONFIG_FILE="appsettings.json"

# Create backup of original configuration
cp "$CONFIG_FILE" "${CONFIG_FILE}.backup"

echo "Injecting environment variables into configuration..."

# MongoDB Configuration
if [ -n "$MONGODB_CONNECTION_STRING" ]; then
    update_config "$CONFIG_FILE" "MongoDbSettings.ConnectionString" "$MONGODB_CONNECTION_STRING"
fi

if [ -n "$MONGODB_DATABASE_NAME" ]; then
    update_config "$CONFIG_FILE" "MongoDbSettings.DatabaseName" "$MONGODB_DATABASE_NAME"
fi

# Logging Configuration
if [ -n "$LOG_LEVEL_DEFAULT" ]; then
    update_config "$CONFIG_FILE" "Logging.LogLevel.Default" "$LOG_LEVEL_DEFAULT"
fi

if [ -n "$LOG_LEVEL_MICROSOFT" ]; then
    update_config "$CONFIG_FILE" "Logging.LogLevel.Microsoft.AspNetCore" "$LOG_LEVEL_MICROSOFT"
fi

# CORS Configuration (if you want to make AllowedHosts configurable)
if [ -n "$ALLOWED_HOSTS" ]; then
    update_config "$CONFIG_FILE" "AllowedHosts" "$ALLOWED_HOSTS"
fi

# Custom Application Settings (add more as needed)
if [ -n "$JWT_SECRET" ]; then
    update_config "$CONFIG_FILE" "JwtSettings.Secret" "$JWT_SECRET"
fi

if [ -n "$JWT_ISSUER" ]; then
    update_config "$CONFIG_FILE" "JwtSettings.Issuer" "$JWT_ISSUER"
fi

if [ -n "$JWT_AUDIENCE" ]; then
    update_config "$CONFIG_FILE" "JwtSettings.Audience" "$JWT_AUDIENCE"
fi

if [ -n "$JWT_EXPIRE_MINUTES" ]; then
    update_config "$CONFIG_FILE" "JwtSettings.ExpireMinutes" "$JWT_EXPIRE_MINUTES"
fi

# CORS Origins (for production CORS policy)
if [ -n "$CORS_ORIGINS" ]; then
    update_config "$CONFIG_FILE" "CorsSettings.AllowedOrigins" "$CORS_ORIGINS"
fi

echo "Configuration injection completed. Final configuration:"
echo "=================================================="
cat "$CONFIG_FILE"
echo "=================================================="

echo "Starting the application..."

# Start the .NET application
exec dotnet GTA6fans.Api.dll