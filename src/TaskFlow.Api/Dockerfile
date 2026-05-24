FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY src/TaskFlow.Api/TaskFlow.Api.csproj TaskFlow.Api/
COPY src/TaskFlow.Application/TaskFlow.Application.csproj TaskFlow.Application/
COPY src/TaskFlow.Domain/TaskFlow.Domain.csproj TaskFlow.Domain/
COPY src/TaskFlow.Infrastructure/TaskFlow.Infrastructure.csproj TaskFlow.Infrastructure/

RUN dotnet restore TaskFlow.Api/TaskFlow.Api.csproj

COPY src/TaskFlow.Api/ TaskFlow.Api/
COPY src/TaskFlow.Application/ TaskFlow.Application/
COPY src/TaskFlow.Domain/ TaskFlow.Domain/
COPY src/TaskFlow.Infrastructure/ TaskFlow.Infrastructure/

RUN dotnet publish TaskFlow.Api/TaskFlow.Api.csproj -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "TaskFlow.Api.dll"]
