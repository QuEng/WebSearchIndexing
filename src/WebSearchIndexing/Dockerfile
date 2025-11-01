#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
USER app
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["src/WebSearchIndexing/WebSearchIndexing.csproj", "src/WebSearchIndexing/"]
COPY ["src/WebSearchIndexing.Data/WebSearchIndexing.Data.csproj", "src/WebSearchIndexing.Data/"]
COPY ["src/WebSearchIndexing.Domain/WebSearchIndexing.Domain.csproj", "src/WebSearchIndexing.Domain/"]
RUN dotnet restore "./src/WebSearchIndexing/WebSearchIndexing.csproj"
COPY . .
WORKDIR "/src/src/WebSearchIndexing"
RUN dotnet build "./WebSearchIndexing.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./WebSearchIndexing.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "WebSearchIndexing.dll"]
