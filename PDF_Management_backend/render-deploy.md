# Deploying to Render

This document provides instructions for deploying your PDF Management backend to Render.

## Prerequisites

1. Create a Render account at https://render.com if you don't already have one.
2. Connect your GitHub repository to Render.

## Deployment Steps

1. **Create a new Web Service**:
   - Go to the Render dashboard
   - Click "New" and select "Web Service"
   - Connect your GitHub repository
   - Select the repository containing your PDF Management backend

2. **Configure the Web Service**:
   - Name: `pdf-management-backend` (or your preferred name)
   - Environment: `Docker`
   - Branch: `main` (or your default branch)
   - Root Directory: Leave empty if your Dockerfile is in the root directory
   - Build Command: Leave empty (uses Dockerfile)
   - Start Command: Leave empty (uses Dockerfile)
   - Plan: Select the appropriate plan (Free tier is available)

3. **Set Environment Variables**:
   In the "Environment" section, add the following environment variables:

   ```
   ASPNETCORE_ENVIRONMENT=Production
   ASPNETCORE_URLS=http://+:10000
   DB_CONNECTION_STRING=your_database_connection_string
   JWT_KEY=your_jwt_key
   JWT_ISSUER=your_jwt_issuer
   JWT_AUDIENCE=your_jwt_audience
   JWT_EXPIRE_DAYS=7
   GOOGLE_DRIVE_FOLDER_ID=your_folder_id
   GOOGLE_CLIENT_EMAIL=your_client_email
   GOOGLE_CLIENT_ID=your_client_id
   GOOGLE_PRIVATE_KEY=your_private_key
   GOOGLE_TYPE=service_account
   GOOGLE_PROJECT_ID=your_project_id
   ALLOWED_ORIGINS=https://your-frontend-url.com
   FILE_STORAGE_PROVIDER=GoogleDrive
   ```

   Replace the placeholder values with your actual credentials.

4. **Create the Web Service**:
   - Click "Create Web Service"
   - Render will automatically build and deploy your application

5. **Verify Deployment**:
   - Once deployment is complete, click on the URL provided by Render
   - You should see your API running
   - Test the health endpoint at `/health` to verify the service is working correctly

## Updating Your Application

When you push changes to your GitHub repository, Render will automatically rebuild and redeploy your application.

## Troubleshooting

If you encounter any issues:

1. Check the deployment logs in the Render dashboard
2. Verify that all environment variables are set correctly
3. Make sure your application is listening on port 10000
4. Check that your Dockerfile is correctly configured

## Important Notes

- Render's free tier has limitations on usage and may sleep after periods of inactivity
- For production use, consider upgrading to a paid plan
- Make sure your database is accessible from Render's servers
- Ensure your CORS settings allow requests from your frontend domain
