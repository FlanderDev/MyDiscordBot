# Running the Project with Docker

This section provides instructions for setting up and running the project using Docker.

## Prerequisites

- Ensure Docker and Docker Compose are installed on your system.
- The project requires .NET version 9.0 as specified in the Dockerfile.

## Setup Instructions

1. **Build and Run the Project**

   Navigate to the project root directory and execute the following command:

   ```bash
   docker-compose up --build
   ```

   This command builds the Docker image and starts the container.

2. **Environment Variables**

   - If required, create a `.env` file in the project root directory to specify environment variables.
   - Uncomment the `env_file` line in the `docker-compose.yml` file to enable this feature.

3. **Network Configuration**

   The project uses a custom Docker network named `discordbot_network` with the `bridge` driver.

4. **Exposed Ports**

   - The project does not explicitly expose any ports in the provided configuration. Update the `docker-compose.yml` file if port mapping is needed.

## Additional Notes

- The application runs under a non-root user for enhanced security.
- Ensure all dependencies are correctly installed and accessible within the Docker environment.

For further details, refer to the project's documentation or contact the development team.