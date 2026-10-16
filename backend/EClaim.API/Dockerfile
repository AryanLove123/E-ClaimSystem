# ---------- Stage 1: build ------------
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY backend/ backend/
RUN dotnet restore backend/EClaim.API/EClaim.API.csproj
RUN dotnet publish backend/EClaim.API/EClaim.API.csproj -c Release -o /app/publish --no-restore

# ---------- Stage 2: runtime -----------
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app


RUN apt-get update \
 && apt-get install -y --no-install-recommends libfontconfig1 fonts-dejavu-core \
 && rm -rf /var/lib/apt/lists/*

COPY --from=build /app/publish .

# Inside a container the app must listen on 0.0.0.0 on a fixed port.
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "EClaim.API.dll"]
