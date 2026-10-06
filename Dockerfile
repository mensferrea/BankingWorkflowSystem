FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["BankingWorkflowSystem.slnx", "./"]
COPY ["src/BankingWorkflow.Domain/BankingWorkflow.Domain.csproj", "src/BankingWorkflow.Domain/"]
COPY ["src/BankingWorkflow.Application/BankingWorkflow.Application.csproj", "src/BankingWorkflow.Application/"]
COPY ["src/BankingWorkflow.Infrastructure/BankingWorkflow.Infrastructure.csproj", "src/BankingWorkflow.Infrastructure/"]
COPY ["src/BankingWorkflow.Web/BankingWorkflow.Web.csproj", "src/BankingWorkflow.Web/"]

RUN dotnet restore "src/BankingWorkflow.Web/BankingWorkflow.Web.csproj"

COPY . .
WORKDIR "/src/src/BankingWorkflow.Web"
RUN dotnet publish "BankingWorkflow.Web.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "BankingWorkflow.Web.dll"]
