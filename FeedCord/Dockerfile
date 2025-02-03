# Set the desired platform for the build stage
FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["FeedCord.csproj", "./"]
COPY ["src", "./src"]

RUN dotnet restore && \
    dotnet build -c Release -o /app/build && \
    dotnet publish -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "FeedCord.dll"]



