#!/bin/bash

echo "Stopping any running services..."
pkill -f "dotnet run" || true
pkill -f "npm start" || true

echo "Starting API service..."
cd /mnt/d/pdf_management/repo/PDF_Management.Net-8/PdfManagement.API
dotnet run &
API_PID=$!

echo "Waiting for API to initialize (10 seconds)..."
sleep 10

echo "Starting frontend service..."
cd /mnt/d/pdf_management/repo/frontend
npm start &
FRONTEND_PID=$!

echo "Services started!"
echo "API running with PID: $API_PID"
echo "Frontend running with PID: $FRONTEND_PID"
echo ""
echo "To stop services, run: pkill -f 'dotnet run'; pkill -f 'npm start'"
