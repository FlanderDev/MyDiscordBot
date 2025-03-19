# syntax=docker/dockerfile:1

ARG DOTNET_VERSION=9.0

FROM mcr.microsoft.com/dotnet/sdk:${DOTNET_VERSION} AS build
WORKDIR /src

COPY DiscordBot/DiscordBot.csproj ./DiscordBot/
RUN --mount=type=cache,target=/root/.nuget/packages \
    --mount=type=cache,target=/root/.cache/msbuild \
    dotnet restore ./DiscordBot/DiscordBot.csproj

COPY DiscordBot/ ./DiscordBot/
WORKDIR /src/DiscordBot
RUN --mount=type=cache,target=/root/.nuget/packages \
    --mount=type=cache,target=/root/.cache/msbuild \
    dotnet build -c Release -o /app/build

RUN --mount=type=cache,target=/root/.nuget/packages \
    --mount=type=cache,target=/root/.cache/msbuild \
    dotnet publish -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:${DOTNET_VERSION}-alpine AS runtime
WORKDIR /app

COPY --from=build /app/publish .

RUN addgroup -S appgroup && adduser -S appuser -G appgroup
USER appuser

ENTRYPOINT ["dotnet", "DiscordBot.dll"]