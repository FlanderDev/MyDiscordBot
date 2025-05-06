# This stage is used when running from VS in fast mode (Default for Debug configuration)
FROM mcr.microsoft.com/dotnet/runtime:9.0 AS base
USER $APP_UID
WORKDIR /app

# This stage is used to build the service project
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

RUN apt-get update 
RUN apt-get install -y ffmpeg #libopus-dev libsodium-dev #libsodium23 libopus0
RUN mkdir -p /app/RunningSpace/ffmpeg
RUN ln -s "$(find /usr/bin/ -type f -name 'ffmpeg' | head -n 1)" /app/RunningSpace/ffmpeg
#RUN ln -s "$(find /usr/lib/ -type f -name 'libopus.so*' | head -n 1)" /app/RunningSpace/libopus.so
#RUN ln -s "$(find /usr/lib/ -type f -name 'libsodium.so*' | head -n 1)" /app/RunningSpace/libsodium.so

COPY ["DiscordBot/DiscordBot.csproj", "DiscordBot/"]
RUN dotnet restore "./DiscordBot/DiscordBot.csproj"
COPY . .
WORKDIR "/src/DiscordBot"
RUN dotnet build "./DiscordBot.csproj" -c $BUILD_CONFIGURATION -o /app/build

# This stage is used to publish the service project to be copied to the final stage
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./DiscordBot.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# This stage is used in production or when running from VS in regular mode (Default when not using the Debug configuration)
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "DiscordBot.dll"]
