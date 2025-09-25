# Use the official .NET 8 SDK image for building
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build

# Set the working directory
WORKDIR /app

# Copy solution file
COPY *.sln ./

# Copy project files
COPY GTA6fans.Domain/*.csproj ./GTA6fans.Domain/
COPY GTA6fans.Application/*.csproj ./GTA6fans.Application/
COPY GTA6fans.Infrastructure/*.csproj ./GTA6fans.Infrastructure/
COPY GTA6fans.Api/*.csproj ./GTA6fans.Api/

# Restore dependencies
RUN dotnet restore

# Copy the rest of the source code
COPY . ./

# Build the application
RUN dotnet build -c Release --no-restore

# Publish the application
RUN dotnet publish GTA6fans.Api/GTA6fans.Api.csproj -c Release -o /app/publish --no-restore

# Use the official .NET 8 runtime image for the final stage
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime

# Install bash for the entrypoint script
RUN apt-get update && apt-get install -y bash && rm -rf /var/lib/apt/lists/*

# Set the working directory
WORKDIR /app

# Copy the published application from the build stage
COPY --from=build /app/publish .

# Copy the entrypoint script
COPY entrypoint.sh ./
RUN chmod +x ./entrypoint.sh

# Create a non-root user for security
RUN addgroup --system --gid 1001 dotnetgroup
RUN adduser --system --uid 1001 --ingroup dotnetgroup dotnetuser

# Change ownership of the app directory to the non-root user
RUN chown -R dotnetuser:dotnetgroup /app

# Switch to the non-root user
USER dotnetuser

# Expose the port the app runs on
EXPOSE 8080
EXPOSE 8081

# Set environment variables
ENV ASPNETCORE_ENVIRONMENT=Production
ENV ASPNETCORE_URLS=http://+:8080

# Use the entrypoint script
ENTRYPOINT ["./entrypoint.sh"]