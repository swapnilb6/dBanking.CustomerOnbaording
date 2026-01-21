# See https://aka.ms/customizecontainer to learn how to customize your debug container
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
ARG APP_UID=1000
# Install curl for healthchecks
RUN apt-get update \
    && apt-get install -y --no-install-recommends curl ca-certificates \
    && rm -rf /var/lib/apt/lists/*
USER ${APP_UID}
WORKDIR /app
EXPOSE 8080
ENV ASPNETCORE_URLS=http://0.0.0.0:8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Copy only the project file (assumes build context is the project folder)
COPY ["dBanking.CustomerOnbaording.API.csproj", "./"]
RUN dotnet restore "./dBanking.CustomerOnbaording.API.csproj"

# Copy rest of the project files and build
COPY . .
RUN dotnet build "./dBanking.CustomerOnbaording.API.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./dBanking.CustomerOnbaording.API.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "dBanking.CustomerOnbaording.API.dll"]