# Use a imagem base do .NET Runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

# Use a imagem do .NET SDK para construir a aplicação
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["Alunos.API.csproj", "./"]
RUN dotnet restore "Alunos.API.csproj"
COPY . .
WORKDIR "/src/"
RUN dotnet build "Alunos.API.csproj" -c Release -o /app/build

# Publique a aplicação
FROM build AS publish
RUN dotnet publish "Alunos.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Crie a imagem final
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Alunos.API.dll"]
