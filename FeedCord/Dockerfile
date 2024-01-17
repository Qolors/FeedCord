# Accept the PORT build argument
ARG PORT=5150

# Use the SDK image to build the app
FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["FeedCord.csproj", "./"]
COPY ["src", "./src"]

RUN dotnet restore
RUN dotnet build -c Release -o /app/build
RUN dotnet publish -c Release -o /app/publish

# Use the aspnet runtime image to run the app
FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
ENV ASPNETCORE_URLS=http://*:$PORT

ENTRYPOINT ["dotnet", "FeedCord.dll"]


