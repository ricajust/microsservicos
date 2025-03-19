# Banco de dados

**Porta:** 1435

**Usuario:** sa

**Senha:** Edug0@2025!

```Shell
#Comando para subir o banco de dados
docker run -d --name sql-alunos -p 1435:1433 -e ACCEPT_EULA=Y -e SA_PASSWORD=Edug0@2025! -e MSSQL_PID=Express mcr.microsoft.com/mssql/server:2022-latest
```

# Para instalar o sqlcmd no ubuntu 24.04
```Shell
# Get signing key for repository from Microsoft
curl https://packages.microsoft.com/keys/microsoft.asc | sudo gpg --dearmor -o /usr/share/keyrings/microsoft-prod.gpg

# Obtain the configuration for the repositories and add it to the system.
curl https://packages.microsoft.com/config/ubuntu/$(lsb_release -rs)/prod.list | sudo tee /etc/apt/sources.list.d/mssql-release.list

# Update package lists
sudo apt update

# Install MSSQL ODBC driver version 18
sudo ACCEPT_EULA=Y apt-get install -y msodbcsql18

# optional: for bcp and sqlcmd
sudo ACCEPT_EULA=Y apt-get install -y mssql-tools18
echo 'export PATH="$PATH:/opt/mssql-tools18/bin"' >> ~/.bashrc
source ~/.bashrc
```

# Para conectar ao banco de dados via sqlcmd
```Shell
#
sqlcmd -S localhost,1435 -U sa -P Edug0@2025! -C
``` 