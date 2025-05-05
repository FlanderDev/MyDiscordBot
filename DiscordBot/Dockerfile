# syntax=docker/dockerfile:1

ARG DOTNET_VERSION=9.0

# Stage 1: Build the application
FROM mcr.microsoft.com/dotnet/sdk:${DOTNET_VERSION} AS builder
WORKDIR /app

# Copy the project file and restore dependencies
COPY --link ./DiscordBot.csproj ./
RUN --mount=type=cache,target=/root/.nuget/packages \
    --mount=type=cache,target=/root/.cache/msbuild \
    dotnet restore

# Copy the rest of the application source code
COPY --link . ./

# Build and publish the application
RUN dotnet publish -c Release -o /app/publish

# Stage 2: Create the runtime image
FROM mcr.microsoft.com/dotnet/aspnet:${DOTNET_VERSION}-alpine AS final
WORKDIR /app

# Install opus and libsodium
RUN apk add --no-cache ffmpeg libsodium opus

# Copy the published application from the builder stage
COPY --from=builder /app/publish .

# Create a non-root user for security
RUN addgroup -S appgroup && adduser -S appuser -G appgroup
USER appuser

# Set the entry point for the application
ENTRYPOINT ["dotnet", "DiscordBot"]