#!/bin/bash

# Load environment variables from .env file if it exists
if [ -f .env ]; then
  export $(grep -v '^#' .env | xargs)
fi

# Define the project names to clean up
PROJECTS=("${PROJECT_NAME}")

for PROJECT in "${PROJECTS[@]}"; do
  echo "🧹 Cleaning up Docker Compose project '$PROJECT'..."
  docker compose --project-name "$PROJECT" down --remove-orphans --volumes
  echo "✅ Project '$PROJECT' cleaned (stopped containers, volumes, and orphans removed)."
done