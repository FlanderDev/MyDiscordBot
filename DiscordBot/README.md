# Running the Project with Docker

This section provides instructions to build and run the project using Docker.

## Prerequisites

- Docker and Docker Compose installed on your system.
- Ensure the `.NET` version specified in the Dockerfile (`9.0`) is supported by your environment.

## Environment Variables

- `DOTNET_VERSION`: Specifies the .NET version to use (default: `9.0`).
- Database service:
  - `POSTGRES_USER`: Database username (default: `user`).
  - `POSTGRES_PASSWORD`: Database password (default: `password`).
  - `POSTGRES_DB`: Database name (default: `discordbot`).

## Build and Run Instructions

1. Clone the repository and navigate to the project root directory.
2. Build and start the services using Docker Compose:
   ```bash
   docker-compose up --build
   ```
3. The application will be built and hosted as per the configurations.

## Special Configuration

- The application uses a PostgreSQL database service. Ensure the environment variables are correctly set for database connectivity.
- Data persistence is managed through Docker volumes (`discordbot_data` and `database_data`).

## Exposed Ports

- No specific ports are exposed in the provided configuration. Ensure to update the `docker-compose.yml` file if external access is required.

For further details, refer to the project documentation or contact the maintainers.