FROM mcr.microsoft.com/dotnet/runtime:6.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["SQR.csproj", "./"]
RUN dotnet restore "SQR.csproj"
COPY . .
WORKDIR "/src/"
RUN dotnet build "SQR.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "SQR.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "SQR.dll"]

FROM postgres:15.0 AS db
COPY ./Database/init-user-db.sh /docker-entrypoint-initdb.d/

FROM ibm-semeru-runtimes:open-17-jre-focal AS lavalink
RUN ["mkdir", "Lavalink"]
COPY ./Lavalink/* /Lavalink/
WORKDIR /Lavalink
CMD ["java -jar Lavalink.jar"]