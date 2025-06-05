FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["PdfManagement.API/PdfManagement.API.csproj", "PdfManagement.API/"]
COPY ["PdfManagement.Core/PdfManagement.Core.csproj", "PdfManagement.Core/"]
COPY ["PdfManagement.Infrastructure/PdfManagement.Infrastructure.csproj", "PdfManagement.Infrastructure/"]
RUN dotnet restore "PdfManagement.API/PdfManagement.API.csproj"
COPY . .
WORKDIR "/src/PdfManagement.API"
RUN dotnet build "PdfManagement.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "PdfManagement.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Create directory for local file storage (if needed as fallback)
RUN mkdir -p /app/Storage/Uploads

# Set environment to Production
ENV ASPNETCORE_ENVIRONMENT=Production

# Set the entry point
ENTRYPOINT ["dotnet", "PdfManagement.API.dll"]
