FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER root
WORKDIR /app
EXPOSE 8080
EXPOSE 8081
RUN apt-get update
RUN apt-get install -y ffmpeg libgdiplus

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["OpenMediaServer/OpenMediaServer.csproj", "OpenMediaServer/"]
RUN dotnet restore "OpenMediaServer/OpenMediaServer.csproj"
COPY . .
WORKDIR "/src/OpenMediaServer"
RUN dotnet build "OpenMediaServer.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "OpenMediaServer.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "OpenMediaServer.dll"]
