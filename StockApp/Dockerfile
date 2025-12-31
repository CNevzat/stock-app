# Build stage
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy csproj and restore dependencies
COPY ["StockApp.csproj", "./"]
RUN dotnet restore "StockApp.csproj"

# Copy everything else and build
COPY . .
RUN dotnet build "StockApp.csproj" -c Release -o /app/build

# Publish stage
FROM build AS publish
RUN dotnet publish "StockApp.csproj" -c Release -o /app/publish

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Expose port
EXPOSE 5134

ENTRYPOINT ["dotnet", "StockApp.dll"]


