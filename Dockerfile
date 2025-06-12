# ----------- Base runtime image for .NET 8 -----------
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

# ----------- Build stage using .NET SDK 8.0 -----------
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Copy the .csproj file and restore dependencies
COPY ["FinTrackHub/FinTrackHub.csproj", "FinTrackHub/"]
RUN dotnet restore "FinTrackHub/FinTrackHub.csproj"

# Copy the entire project
COPY . .
WORKDIR "/src/FinTrackHub"

# Build and publish the application
RUN dotnet publish "FinTrackHub.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# ----------- Final image setup -----------
FROM base AS final
WORKDIR /app

# Copy the published output from the build stage
COPY --from=build /app/publish .

# Set the entry point to run the application
ENTRYPOINT ["dotnet", "FinTrackHub.dll"]
