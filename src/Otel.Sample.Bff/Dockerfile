#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["Otel.Sample.Bff/Otel.Sample.Bff.csproj", "Otel.Sample.Bff/"]
COPY ["Otel.Sample.SharedKernel/Otel.Sample.SharedKernel.csproj", "Otel.Sample.SharedKernel/"]
RUN dotnet restore "Otel.Sample.Bff/Otel.Sample.Bff.csproj"
COPY . .
WORKDIR "/src/Otel.Sample.Bff"
RUN dotnet build "Otel.Sample.Bff.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Otel.Sample.Bff.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Otel.Sample.Bff.dll"]