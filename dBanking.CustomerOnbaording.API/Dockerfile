
# -------- Base runtime --------
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
# Install curl for healthchecks
RUN apt-get update \
    && apt-get install -y --no-install-recommends curl ca-certificates \
    && rm -rf /var/lib/apt/lists/*
WORKDIR /app
EXPOSE 9090
ENV ASPNETCORE_URLS=http://0.0.0.0:9090

# -------- Build --------
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Copy sln and csproj files for better restore caching
COPY ["dBanking.CustomerManagment.sln", "./"]
COPY ["dBanking.CustomerOnbaording.API/dBanking.CustomerOnbaording.API.csproj", "dBanking.CustomerOnbaording.API/"]
COPY ["dBanking.CmsOnboarding.Core/dBanking.CmsOnboarding.Core.csproj", "dBanking.CmsOnboarding.Core/"]
COPY ["dBanking.CmsOnboarding.Infrastructure/dBanking.CmsOnboarding.Infrastructure.csproj", "dBanking.CmsOnboarding.Infrastructure/"]

RUN dotnet restore "dBanking.CustomerOnbaording.API/dBanking.CustomerOnbaording.API.csproj"

# Copy the rest of the source
COPY . .
WORKDIR "/src/dBanking.CustomerOnbaording.API"
RUN dotnet build "dBanking.CustomerOnbaording.API.csproj" -c $BUILD_CONFIGURATION -o /app/build

# -------- Publish --------
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "dBanking.CustomerOnbaording.API.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# -------- Final --------
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish ./
ENTRYPOINT ["dotnet", "dBanking.CustomerOnbaording.API.dll"]
