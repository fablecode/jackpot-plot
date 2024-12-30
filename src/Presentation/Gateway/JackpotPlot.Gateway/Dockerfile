# See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

# This stage is used when running from VS in fast mode (Default for Debug configuration)
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER app
WORKDIR /app


# This stage is used to build the service project
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /

COPY . .
RUN dotnet restore "JackpotPlot.sln"

# This stage is used to publish the service project to be copied to the final stage
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./src/Presentation/Gateway/JackpotPlot.Gateway/JackpotPlot.Gateway.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false --no-restore

# This stage is used in production or when running from VS in regular mode (Default when not using the Debug configuration)
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Expose the default port
EXPOSE 5001

ENTRYPOINT ["dotnet", "JackpotPlot.Gateway.dll"]













# See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

# # Use a .NET runtime image
# FROM mcr.microsoft.com/dotnet/aspnet:8.0

# # Set working directory
# WORKDIR /app

# # Copy the Gateway project files
# COPY . .

# # Expose the gateway's port
# EXPOSE 8080

# # Run the Gateway application
# CMD ["dotnet", "JackpotPlot.Gateway.dll"]


# This stage is used when running from VS in fast mode (Default for Debug configuration)
#FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
#USER app
#WORKDIR /app
#EXPOSE 8080
#EXPOSE 8081
#
#
## This stage is used to build the service project
#FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
#ARG BUILD_CONFIGURATION=Release
#WORKDIR /src
#COPY ["src/Presentation/Gateway/JackpotPlot.Gateway/JackpotPlot.Gateway.csproj", "src/Presentation/Gateway/JackpotPlot.Gateway/"]
#RUN dotnet restore "./src/Presentation/Gateway/JackpotPlot.Gateway/JackpotPlot.Gateway.csproj"
#COPY . .
#WORKDIR "/src/src/Presentation/Gateway/JackpotPlot.Gateway"
#RUN dotnet build "./JackpotPlot.Gateway.csproj" -c $BUILD_CONFIGURATION -o /app/build
#
## This stage is used to publish the service project to be copied to the final stage
#FROM build AS publish
#ARG BUILD_CONFIGURATION=Release
#RUN dotnet publish "./JackpotPlot.Gateway.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false
#
## This stage is used in production or when running from VS in regular mode (Default when not using the Debug configuration)
#FROM base AS final
#WORKDIR /app
#COPY --from=publish /app/publish .
#ENTRYPOINT ["dotnet", "JackpotPlot.Gateway.dll"]