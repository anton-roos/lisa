FROM mcr.microsoft.com/dotnet/sdk:9.0

# Install system tools
RUN apt-get update && apt-get install -y libpq-dev
RUN apt update && apt install -y make

# Set working directory
WORKDIR /workspace
