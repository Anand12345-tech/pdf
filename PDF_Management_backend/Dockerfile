FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy csproj files and restore dependencies
COPY ["PdfManagement.API/PdfManagement.API.csproj", "PdfManagement.API/"]
COPY ["PdfManagement.Core/PdfManagement.Core.csproj", "PdfManagement.Core/"]
COPY ["PdfManagement.Infrastructure/PdfManagement.Infrastructure.csproj", "PdfManagement.Infrastructure/"]
COPY ["PdfManagement.Models/PdfManagement.Models.csproj", "PdfManagement.Models/"]
COPY ["PdfManagement.Data/PdfManagement.Data.csproj", "PdfManagement.Data/"]
COPY ["PdfManagement.Services/PdfManagement.Services.csproj", "PdfManagement.Services/"]

RUN dotnet restore "PdfManagement.API/PdfManagement.API.csproj"

# Copy all files and build the project
COPY . .
WORKDIR "/src/PdfManagement.API"
RUN dotnet build "PdfManagement.API.csproj" -c Release -o /app/build

# Publish stage
FROM build AS publish
RUN dotnet publish "PdfManagement.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Final stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Set environment variables
ENV ASPNETCORE_ENVIRONMENT=Production
ENV ASPNETCORE_URLS=http://+:10000

# Expose port 10000 (Render uses this port)
EXPOSE 10000

ENTRYPOINT ["dotnet", "PdfManagement.API.dll"]
