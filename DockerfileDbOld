FROM mcr.microsoft.com/mssql/server:2022-latest

USER root

RUN apt-get update && \
    apt-get install -y --no-install-recommends curl wget sudo gnupg software-properties-common && \
    rm -rf /var/lib/apt/lists/*

RUN curl https://packages.microsoft.com/keys/microsoft.asc | gpg --dearmor | tee /etc/apt/trusted.gpg.d/microsoft.gpg > /dev/null && \
    add-apt-repository "$(wget -qO- https://packages.microsoft.com/config/ubuntu/$(lsb_release -rs)/prod.list)"

ENV DEBIAN_FRONTEND=noninteractive
ENV ACCEPT_EULA=Y

RUN apt-get update && \
    apt-get install -y --no-install-recommends mssql-tools && \
    rm -rf /var/lib/apt/lists/*

ENV PATH="/opt/mssql-tools/bin:${PATH}"

USER mssql

## PARA EXECUTAR O CONTAINER 
#para limpar o volume
#docker volume rm $(docker volume ls -qf name=tcc_edugo_microsservico_Alunos.API_sql-server-data) # Substitua pelo nome correto do volume se este não for o padrão
#docker run -d --name sql-server -p 1499:1433 -e ACCEPT_EULA=Y -e SA_PASSWORD="SenhaMuitoForte@123" -v ./sql-server-data:/var/opt/mssql/data sql-server:latest
#docker run -d --name sql-server -p 1499:1433 \
#-e ACCEPT_EULA=Y \
#-e SA_PASSWORD="SenhaMuitoForte@123" \
#-v $(pwd)/init.sql:/docker-entrypoint-initdb.d/init.sql \
#-v ./sql-server-data:/var/opt/mssql/data \
#sql-server:latest


#PARA ACESSAR O SQLCMD
#sqlcmd -S localhost -U sa -P "SenhaMuitoForte@123"