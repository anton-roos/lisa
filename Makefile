# Makefile for Lisa Project

# Run the app inside DevContainer
dev:
	dotnet watch run --project Lisa/Lisa.csproj --no-launch-profile --urls http://0.0.0.0:80

# Build only
build:
	dotnet build Lisa/Lisa.csproj

# Create a new migration
migration:
	dotnet ef migrations add $(NAME) --project Lisa/Lisa.csproj --startup-project Lisa/Lisa.csproj --output-dir Migrations

# Apply migrations
migrate:
	dotnet ef database update --project Lisa/Lisa.csproj --startup-project Lisa/Lisa.csproj

# Start only the database
db-up:
	docker compose up -d db

# Stop the database
db-down:
	docker compose down

# Check database health
db-health:
	docker exec lisa-postgres pg_isready -U lisauser

# Build production Docker image
docker-build:
	docker build -t lisa:latest .

# Clean up docker system safely
docker-clean:
	docker system prune -a --volumes
