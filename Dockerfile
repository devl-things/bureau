# Use the official .NET SDK image to build the project
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /source

# Copy the project files and restore dependencies
COPY ./src/Bureau.UI.Web/ .
RUN dotnet restore

# Copy the rest of the project files and build the project
COPY . .
RUN dotnet publish -c Release -o /app

# # Use the official .NET runtime image to run the application
# FROM mcr.microsoft.com/dotnet/aspnet:6.0
# WORKDIR /app
# COPY --from=build /app .

# # Expose the port the app runs on
# EXPOSE 80

# # Run the application
# ENTRYPOINT ["dotnet", "Bureau.UI.Web.dll"]



# See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

# This stage is used when running from VS in fast mode (Default for Debug configuration)
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081


# # This stage is used to build the service project
# FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
# ARG BUILD_CONFIGURATION=Release
# WORKDIR /src
# COPY ["src/Bureau.UI.API/Bureau.UI.API.csproj", "src/Bureau.UI.API/"]
# RUN dotnet restore "./src/Bureau.UI.API/Bureau.UI.API.csproj"
# COPY . .
# WORKDIR "/src/src/Bureau.UI.API"
# RUN dotnet build "./Bureau.UI.API.csproj" -c $BUILD_CONFIGURATION -o /app/build

# # This stage is used to publish the service project to be copied to the final stage
# FROM build AS publish
# ARG BUILD_CONFIGURATION=Release
# RUN dotnet publish "./Bureau.UI.API.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# # This stage is used in production or when running from VS in regular mode (Default when not using the Debug configuration)
# FROM base AS final
# WORKDIR /app
# COPY --from=publish /app/publish .
# ENTRYPOINT ["dotnet", "Bureau.UI.API.dll"]