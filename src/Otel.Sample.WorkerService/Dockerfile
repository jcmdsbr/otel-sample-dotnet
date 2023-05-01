#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/runtime:7.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["Otel.Sample.WorkerService/Otel.Sample.WorkerService.csproj", "Otel.Sample.WorkerService/"]
COPY ["Otel.Sample.SharedKernel/Otel.Sample.SharedKernel.csproj", "Otel.Sample.SharedKernel/"]
RUN dotnet restore "Otel.Sample.WorkerService/Otel.Sample.WorkerService.csproj"
COPY . .
WORKDIR "/src/Otel.Sample.WorkerService"
RUN dotnet build "Otel.Sample.WorkerService.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Otel.Sample.WorkerService.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Otel.Sample.WorkerService.dll"]