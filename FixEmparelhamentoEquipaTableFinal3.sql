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

-- Criar a tabela EmparelhamentosEquipa com a estrutura correta
CREATE TABLE [EmparelhamentosEquipa] (
    [Id] int NOT NULL IDENTITY(1,1),
    [Clube1] nvarchar(max) NOT NULL,
    [Clube2] nvarchar(max) NOT NULL,
    [DataJogo] datetime2 NOT NULL,
    [HoraJogo] time NOT NULL,
    [NomeCompeticao] nvarchar(max) NOT NULL,
    [CompeticaoId] int NOT NULL,
    CONSTRAINT [PK_EmparelhamentosEquipa] PRIMARY KEY ([Id])
);

-- Criar o índice para CompeticaoId
CREATE INDEX [IX_EmparelhamentosEquipa_CompeticaoId] ON [EmparelhamentosEquipa] ([CompeticaoId]);

-- Adicionar a chave estrangeira
ALTER TABLE [EmparelhamentosEquipa] ADD CONSTRAINT [FK_EmparelhamentosEquipa_Competicoes_CompeticaoId] 
FOREIGN KEY ([CompeticaoId]) REFERENCES [Competicoes] ([Id]) ON DELETE CASCADE;

-- Verificar se a tabela JogosEmparelhados tem a coluna CompeticaoId1
IF EXISTS (SELECT * FROM sys.columns WHERE name = 'CompeticaoId1' AND object_id = OBJECT_ID('JogosEmparelhados'))
BEGIN
    -- Verificar se existe uma chave estrangeira para CompeticaoId1
    IF EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_JogosEmparelhados_Competicoes_CompeticaoId1')
    BEGIN
        ALTER TABLE [JogosEmparelhados] DROP CONSTRAINT [FK_JogosEmparelhados_Competicoes_CompeticaoId1];
    END
    
    -- Verificar se existe um índice para CompeticaoId1
    IF EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_JogosEmparelhados_CompeticaoId1')
    BEGIN
        DROP INDEX [IX_JogosEmparelhados_CompeticaoId1] ON [JogosEmparelhados];
    END
    
    -- Remover a coluna CompeticaoId1
    ALTER TABLE [JogosEmparelhados] DROP COLUMN [CompeticaoId1];
END

-- Verificar se existe um índice para CompeticaoId em JogosEmparelhados
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_JogosEmparelhados_CompeticaoId' AND object_id = OBJECT_ID('JogosEmparelhados'))
BEGIN
    CREATE INDEX [IX_JogosEmparelhados_CompeticaoId] ON [JogosEmparelhados] ([CompeticaoId]);
END

-- Verificar se existe uma chave estrangeira para CompeticaoId em JogosEmparelhados
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_JogosEmparelhados_Competicoes_CompeticaoId' AND parent_object_id = OBJECT_ID('JogosEmparelhados'))
BEGIN
    ALTER TABLE [JogosEmparelhados] ADD CONSTRAINT [FK_JogosEmparelhados_Competicoes_CompeticaoId] 
    FOREIGN KEY ([CompeticaoId]) REFERENCES [Competicoes] ([Id]) ON DELETE CASCADE;
END

-- Atualizar a tabela __EFMigrationsHistory
IF NOT EXISTS (SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = '20250521115558_AddCompeticaoIdToEmparelhamentoEquipa')
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES ('20250521115558_AddCompeticaoIdToEmparelhamentoEquipa', '7.0.5');
END

IF NOT EXISTS (SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = '20250521124105_ReintroduceCompeticaoId')
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES ('20250521124105_ReintroduceCompeticaoId', '7.0.5');
END

IF NOT EXISTS (SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = '20250521133517_AddNomeCompeticaoToEmparelhamentoEquipa')
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES ('20250521133517_AddNomeCompeticaoToEmparelhamentoEquipa', '7.0.5');
END

IF NOT EXISTS (SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = '20250521150133_AddNomeCompeticaoToEmparelhamentoBase')
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES ('20250521150133_AddNomeCompeticaoToEmparelhamentoBase', '7.0.5');
END

IF NOT EXISTS (SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = '20250524113115_FixEmparelhamento')
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES ('20250524113115_FixEmparelhamento', '7.0.5');
END