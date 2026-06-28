# Builder
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS builder
WORKDIR /App

COPY ["Pontuei.Api/Pontuei.Api.csproj", "Pontuei.Api/"]
COPY ["Pontuei.Shared/Pontuei.Shared.csproj", "Pontuei.Shared/"]

RUN dotnet restore Pontuei.Api/Pontuei.Api.csproj

COPY . .

ENV DOTNET_CONFIGURATION="${DOTNET_CONFIGURATION:-Debug}"
RUN dotnet publish Pontuei.Api/Pontuei.Api.csproj -c "${DOTNET_CONFIGURATION}" -o out

# Final image
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /App
ENV ASPNETCORE_ENVIRONMENT="${ASPNETCORE_ENVIRONMENT:-Development}"
ENV ASPNETCORE_URLS=http://+:5000

COPY --from=builder /App/out .

EXPOSE 5000
ENTRYPOINT ["dotnet", "Pontuei.Api.dll"]