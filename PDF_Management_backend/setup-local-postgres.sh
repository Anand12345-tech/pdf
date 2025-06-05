#!/bin/bash

echo "Setting up local PostgreSQL database for PDF Management..."

# Check if Docker is installed
if ! command -v docker &> /dev/null; then
    echo "Docker is not installed. Please install Docker first."
    echo "Visit https://docs.docker.com/get-docker/ for installation instructions."
    exit 1
fi

# Check if container already exists
if docker ps -a --format '{{.Names}}' | grep -q "^pdf-postgres$"; then
    echo "PostgreSQL container already exists."
    
    # Check if it's running
    if docker ps --format '{{.Names}}' | grep -q "^pdf-postgres$"; then
        echo "PostgreSQL container is already running."
    else
        echo "Starting existing PostgreSQL container..."
        docker start pdf-postgres
    fi
else
    echo "Creating new PostgreSQL container..."
    docker run --name pdf-postgres -e POSTGRES_PASSWORD=postgres -e POSTGRES_DB=pdfdata -p 5432:5432 -d postgres:14
fi

# Wait for PostgreSQL to start
echo "Waiting for PostgreSQL to start..."
sleep 5

# Update .env file
echo "Updating .env file with local PostgreSQL connection string..."
cat > .env << EOL
# Database Connection
DB_CONNECTION_STRING=Host=localhost;Port=5432;Database=pdfdata;Username=postgres;Password=postgres;Trust Server Certificate=true

# JWT Settings
JWT_KEY=ThisIsMySecretKeyForPdfManagementApplication12345
JWT_ISSUER=PdfManagement.API
JWT_AUDIENCE=PdfManagementClient
JWT_EXPIRE_DAYS=7

# Storage Settings
STORAGE_PATH=/app/storage

# CORS Settings
ALLOWED_ORIGINS=http://localhost:3000,https://pdf-management-frontend.vercel.app
EOL

echo "Local PostgreSQL setup complete!"
echo "To apply migrations, run:"
echo "dotnet ef database update --project PdfManagement.Infrastructure --startup-project PdfManagement.API"
