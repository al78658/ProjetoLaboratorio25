-- Remover a tabela EmparelhamentosEquipa_New se existir
IF EXISTS (SELECT * FROM sys.tables WHERE name = 'EmparelhamentosEquipa_New')
BEGIN
    DROP TABLE [EmparelhamentosEquipa_New];
END

-- Criar a nova tabela EmparelhamentosEquipa_New com a estrutura correta
CREATE TABLE [EmparelhamentosEquipa_New] (
    [Id] int NOT NULL IDENTITY(1,1),
    [Clube1] nvarchar(max) NOT NULL,
    [Clube2] nvarchar(max) NOT NULL,
    [DataJogo] datetime2 NOT NULL,
    [HoraJogo] time NOT NULL,
    [NomeCompeticao] nvarchar(max) NOT NULL,
    [CompeticaoId] int NOT NULL,
    CONSTRAINT [PK_EmparelhamentosEquipa_New] PRIMARY KEY ([Id])
);

-- Criar o índice para CompeticaoId
CREATE INDEX [IX_EmparelhamentosEquipa_New_CompeticaoId] ON [EmparelhamentosEquipa_New] ([CompeticaoId]);

-- Adicionar a chave estrangeira
ALTER TABLE [EmparelhamentosEquipa_New] ADD CONSTRAINT [FK_EmparelhamentosEquipa_New_Competicoes_CompeticaoId] 
FOREIGN KEY ([CompeticaoId]) REFERENCES [Competicoes] ([Id]) ON DELETE CASCADE;

-- Remover a tabela EmparelhamentosEquipa se existir
IF EXISTS (SELECT * FROM sys.tables WHERE name = 'EmparelhamentosEquipa')
BEGIN
    -- Remover as chaves estrangeiras primeiro
    DECLARE @constraint_name nvarchar(128)
    DECLARE constraint_cursor CURSOR FOR
    SELECT name FROM sys.foreign_keys
    WHERE parent_object_id = OBJECT_ID('EmparelhamentosEquipa')
    
    OPEN constraint_cursor
    FETCH NEXT FROM constraint_cursor INTO @constraint_name
    
    WHILE @@FETCH_STATUS = 0
    BEGIN
        DECLARE @sql nvarchar(500) = 'ALTER TABLE [EmparelhamentosEquipa] DROP CONSTRAINT [' + @constraint_name + ']'
        EXEC sp_executesql @sql
        FETCH NEXT FROM constraint_cursor INTO @constraint_name
    END
    
    CLOSE constraint_cursor
    DEALLOCATE constraint_cursor
    
    -- Remover os índices
    DECLARE @index_name nvarchar(128)
    DECLARE index_cursor CURSOR FOR
    SELECT name FROM sys.indexes
    WHERE object_id = OBJECT_ID('EmparelhamentosEquipa')
    AND is_primary_key = 0
    
    OPEN index_cursor
    FETCH NEXT FROM index_cursor INTO @index_name
    
    WHILE @@FETCH_STATUS = 0
    BEGIN
        DECLARE @index_sql nvarchar(500) = 'DROP INDEX [' + @index_name + '] ON [EmparelhamentosEquipa]'
        EXEC sp_executesql @index_sql
        FETCH NEXT FROM index_cursor INTO @index_name
    END
    
    CLOSE index_cursor
    DEALLOCATE index_cursor
    
    -- Agora podemos remover a tabela
    DROP TABLE [EmparelhamentosEquipa]
END

-- Renomear a tabela EmparelhamentosEquipa_New para EmparelhamentosEquipa
EXEC sp_rename 'EmparelhamentosEquipa_New', 'EmparelhamentosEquipa';

-- Renomear a chave primária
EXEC sp_rename 'PK_EmparelhamentosEquipa_New', 'PK_EmparelhamentosEquipa', 'OBJECT';

-- Renomear o índice
EXEC sp_rename 'IX_EmparelhamentosEquipa_New_CompeticaoId', 'IX_EmparelhamentosEquipa_CompeticaoId', 'INDEX';

-- Renomear a chave estrangeira
EXEC sp_rename 'FK_EmparelhamentosEquipa_New_Competicoes_CompeticaoId', 'FK_EmparelhamentosEquipa_Competicoes_CompeticaoId', 'OBJECT';