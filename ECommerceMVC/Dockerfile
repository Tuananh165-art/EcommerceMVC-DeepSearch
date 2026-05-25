# ===========================================================
# ASP.NET 8 MVC — Multi-stage Dockerfile
# ===========================================================
# Stage 1: Restore & Build (SDK image)
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy csproj first (layer caching)
COPY *.csproj ./
RUN dotnet restore

# Copy everything else and publish
COPY . ./
RUN dotnet publish -c Release -o /app/publish --no-restore

# ===========================================================
# Stage 2: Runtime (ASP.NET image — small)
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

# Copy published output from build stage
COPY --from=build /app/publish .

# Create a non-root user for security
RUN groupadd -r appuser && useradd -r -g appuser appuser \
    && chown -R appuser:appuser /app
USER appuser

EXPOSE 8080

ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

ENTRYPOINT ["dotnet", "ECommerceMVC.dll"]
