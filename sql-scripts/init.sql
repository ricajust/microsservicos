USE AlunosDb;
GO

-- Criação da tabela Usuarios
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = 'dbo' AND TABLE_NAME = 'Usuarios')
BEGIN
    CREATE TABLE Usuarios (
        Id VARCHAR(255) PRIMARY KEY,
        nome VARCHAR(255),
        cpf VARCHAR(20),
        data_nascimento DATETIME2,
        email VARCHAR(255),
        telefone VARCHAR(20),
        endereco VARCHAR(255),
        bairro VARCHAR(255),
        cidade VARCHAR(255),
        uf VARCHAR(2),
        cep VARCHAR(10),
        senha VARCHAR(255)
    );
END
GO

-- Criação da tabela Alunos (sem a chave estrangeira)
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = 'dbo' AND TABLE_NAME = 'Alunos')
BEGIN
    CREATE TABLE Alunos (
        Id VARCHAR(255) PRIMARY KEY
        -- Outros campos específicos de Aluno podem vir aqui
    );
END
GO

-- Importe os dados para a tabela Usuarios (ajuste o caminho e as opções conforme necessário)
BULK INSERT Usuarios
FROM '/import/alunos.csv'
WITH (
    FORMAT = 'CSV',
    FIRSTROW = 2, -- Use 2 quando o CSV tiver cabeçalho
    FIELDTERMINATOR = ',',
    ROWTERMINATOR = '\n',
    FIELDQUOTE = '"'
);
GO

EXEC sp_changedbowner 'sa';
GO