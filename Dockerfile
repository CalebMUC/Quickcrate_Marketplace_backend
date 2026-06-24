# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy .csproj and restore dependencies
COPY ["MinimartApi.csproj", "./"]
RUN dotnet restore "MinimartApi.csproj"

# Copy everything else and build
COPY . .
WORKDIR "/src"
RUN dotnet build "MinimartApi.csproj" -c Release -o /app/build

# Publish the app
RUN dotnet publish "MinimartApi.csproj" -c Release -o /app/publish

# Final stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "MinimartApi.dll"]
