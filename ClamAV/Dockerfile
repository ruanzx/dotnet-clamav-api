# See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

# Usage:
# docker build -t dotnet-clamav-rest .
# docker run -it --rm dotnet-clamav-rest -p 32772:8080  -e ASPNETCORE_HTTP_PORTS=8080 /bin/bash

# This stage is used when running from VS in fast mode (Default for Debug configuration)
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

# This stage is used to build the service project
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["ClamAV.csproj", "."]
RUN dotnet restore "./ClamAV.csproj"
COPY . .
WORKDIR "/src/."
RUN dotnet build "./ClamAV.csproj" -c $BUILD_CONFIGURATION -o /app/build

# This stage is used to publish the service project to be copied to the final stage
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./ClamAV.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# This stage is used in production or when running from VS in regular mode (Default when not using the Debug configuration)
FROM base AS final

RUN apt update \
  && apt install -y \
    gnupg \
    wget \
    supervisor \
    cron \
  && mkdir -p /etc/apt/keyrings \
  && wget -qO- https://ftp-master.debian.org/keys/archive-key-11.asc | gpg --dearmor > /etc/apt/keyrings/debian-archive.gpg \
  && wget -qO- https://ftp-master.debian.org/keys/archive-key-11-security.asc | gpg --dearmor > /etc/apt/keyrings/debian-security.gpg \
  && chmod 644 /etc/apt/keyrings/debian-archive.gpg /etc/apt/keyrings/debian-security.gpg \
  && echo "deb [signed-by=/etc/apt/keyrings/debian-archive.gpg] http://http.us.debian.org/debian stable main contrib non-free" > /etc/apt/sources.list.d/debian-nonfree.list \
  && apt update \
  && apt install -y \
    libclamunrar \
    clamav clamav-freshclam clamav-daemon \
  && apt clean \
  && apt autoremove \
  && rm -rf /var/cache/apt/archives/* /var/lib/apt/lists/*

VOLUME [ "/claimav_data", "/var/lib/clamav" ]

COPY ["./docker/clamav.conf", "/etc/clamav/clamav.conf"]
COPY ["./docker/supervisord.conf", "/etc/supervisor/conf.d/supervisord.conf"]

WORKDIR /app
COPY --from=publish /app/publish .

COPY ["./docker-entrypoint.sh", "/docker-entrypoint.sh"]
RUN chmod +x /docker-entrypoint.sh

EXPOSE 8080

ENTRYPOINT ["/docker-entrypoint.sh"]