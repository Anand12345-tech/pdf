#!/bin/bash

# Production deployment script for PDF Management Backend

# Exit on error
set -e

echo "Starting production deployment..."

# Load production environment variables
if [ -f .env.production ]; then
    echo "Loading production environment variables..."
    export $(grep -v '^#' .env.production | xargs)
else
    echo "Error: .env.production file not found!"
    exit 1
fi

# Set environment to Production
export ASPNETCORE_ENVIRONMENT=Production

# Build the application
echo "Building application in Release mode..."
dotnet build -c Release

# Run database migrations
echo "Running database migrations..."
dotnet ef database update --project PdfManagement.Infrastructure --startup-project PdfManagement.API

# Publish the application
echo "Publishing application..."
dotnet publish -c Release -o ./publish

echo "Deployment preparation complete!"
echo "The application has been built and published to the ./publish directory"
echo "You can now deploy these files to your production server"

# Instructions for deployment
echo ""
echo "=== DEPLOYMENT INSTRUCTIONS ==="
echo "1. Copy the contents of the ./publish directory to your production server"
echo "2. Make sure your production server has the .NET runtime installed"
echo "3. Set up your environment variables on the production server"
echo "4. Configure your web server (Nginx, Apache, etc.) to proxy requests to the application"
echo "5. Start the application with: dotnet PdfManagement.API.dll"
echo "=== END INSTRUCTIONS ==="
