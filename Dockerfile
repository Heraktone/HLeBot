# Dockerfile

FROM mcr.microsoft.com/dotnet/core/sdk:3.1 AS build-env
WORKDIR /app

# Copy csproj and restore as distinct layers
COPY *.csproj ./
RUN dotnet restore

# Copy everything else and build
COPY . .
RUN dotnet publish -c Release -o out

# Build runtime image
FROM mcr.microsoft.com/dotnet/core/aspnet:3.1
WORKDIR /app
COPY --from=build-env /app/out .

ENV TZ=America/New_York

# Run the app on container startup
# Use your project name for the second parameter
# e.g. MyProject.dll
# ENTRYPOINT [ "dotnet", "HLeBot.dll" ]

CMD GOOGLE_CALENDAR=$GOOGLE_CALENDAR DISCORD_TOKEN=$DISCORD_TOKEN GOOGLE_API_KEY=$GOOGLE_API_KEY dotnet HLeBot.dll