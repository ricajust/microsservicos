# Use a imagem base do .NET SDK para construir a aplicação
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["Alunos.API.csproj", "./"]
RUN dotnet restore "Alunos.API.csproj"
COPY . .
WORKDIR "/src/"
RUN dotnet build "Alunos.API.csproj" -c Release -o /app/build
RUN dotnet tool install --global dotnet-ef

# Altera a imagem base para o SDK, aumenta o tamanho da sua imagem final, pois o SDK é maior que o runtime
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS publish
WORKDIR /app
COPY --from=build /app/build .
ENTRYPOINT ["dotnet", "Alunos.API.dll"]