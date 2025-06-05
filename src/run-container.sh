#!/bin/bash

# Load environment variables from .env file if it exists
if [ -f .env ]; then
  export $(grep -v '^#' .env | xargs)
fi

# Add "-all" suffix to project name
PROJECT_NAME_WITH_SUFFIX="${PROJECT_NAME}"

# Print status message
echo "▶ Starting project '$PROJECT_NAME_WITH_SUFFIX'"

# Run docker compose with build and detached mode
docker compose --project-name "$PROJECT_NAME_WITH_SUFFIX" --file docker-compose.yaml up --build -d

# 🧹 Prune dangling images (like <none>:<none>)
echo "🧹 Cleaning up dangling Docker images..."
docker image prune -f