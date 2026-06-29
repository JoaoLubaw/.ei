# Build stage for the Android app using .NET MAUI
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build-env
WORKDIR /src

RUN apt-get update && apt-get install -y --no-install-recommends \
    openjdk-17-jdk-headless \
    unzip \
    && rm -rf /var/lib/apt/lists/*

RUN dotnet workload install android

ARG MAUI_API_BASE_URL
ARG MAUI_STORAGE_BASE_URL
ARG MAUI_GOOGLE_WEB_CLIENT_ID
ARG GOOGLE_SERVICES_BASE64

COPY . .

# 1. Creates the appsettings.Local.json file with the provided environment variables
RUN printf '{\n\
  "Api": {\n\
    "BaseUrl": "%s"\n\
  },\n\
  "Storage": {\n\
    "BaseUrl": "%s"\n\
  },\n\
  "Google": {\n\
    "WebClientId": "%s"\n\
  }\n\
}\n' "$MAUI_API_BASE_URL" "$MAUI_STORAGE_BASE_URL" "$MAUI_GOOGLE_WEB_CLIENT_ID" > Pontuei.App/appsettings.Local.json

# 2. Decodes the base64-encoded google-services.json and saves it to the appropriate location if the variable is set
RUN if [ -n "$GOOGLE_SERVICES_BASE64" ]; then \
        echo "$GOOGLE_SERVICES_BASE64" | base64 -d > Pontuei.App/google-services.json; \
    fi

RUN dotnet restore Pontuei.App/Pontuei.App.csproj

RUN dotnet publish Pontuei.App/Pontuei.App.csproj \
    -c Release \
    -f net10.0-android \
    -o /artifacts