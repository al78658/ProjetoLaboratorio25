-- Script para consolidar os emparelhamentos na tabela EmparelhamentosBase
-- e remover as tabelas JogosEmparelhados e EmparelhamentosEquipa

-- 1. Primeiro, vamos verificar se a tabela EmparelhamentosBase existe
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'EmparelhamentosBase')
BEGIN
    -- Criar a tabela EmparelhamentosBase se não existir
    CREATE TABLE [EmparelhamentosBase] (
        [Id] int NOT NULL IDENTITY(1,1),
        [Clube1] nvarchar(max) NOT NULL,
        [Clube2] nvarchar(max) NOT NULL,
        [DataJogo] datetime2 NOT NULL,
        [HoraJogo] time NOT NULL,
        [CompeticaoId] int NOT NULL,
        [NomeCompeticao] nvarchar(max) NULL,
        CONSTRAINT [PK_EmparelhamentosBase] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_EmparelhamentosBase_Competicoes_CompeticaoId] 
            FOREIGN KEY ([CompeticaoId]) REFERENCES [Competicoes] ([Id]) ON DELETE CASCADE
    );

    -- Criar índice para CompeticaoId
    CREATE INDEX [IX_EmparelhamentosBase_CompeticaoId] ON [EmparelhamentosBase] ([CompeticaoId]);
END

-- 2. Migrar dados de JogosEmparelhados para EmparelhamentosBase (se a tabela existir)
IF EXISTS (SELECT * FROM sys.tables WHERE name = 'JogosEmparelhados')
BEGIN
    -- Inserir dados de JogosEmparelhados em EmparelhamentosBase
    INSERT INTO [EmparelhamentosBase] ([Clube1], [Clube2], [DataJogo], [HoraJogo], [CompeticaoId], [NomeCompeticao])
    SELECT 
        j1.Nome, -- Clube1 (nome do jogador 1)
        j2.Nome, -- Clube2 (nome do jogador 2)
        je.DataJogo,
        je.HoraJogo,
        je.CompeticaoId,
        c.Nome -- Nome da competição
    FROM [JogosEmparelhados] je
    INNER JOIN [Jogadores] j1 ON je.Jogador1Id = j1.Id
    INNER JOIN [Jogadores] j2 ON je.Jogador2Id = j2.Id
    LEFT JOIN [Competicoes] c ON je.CompeticaoId = c.Id
    WHERE NOT EXISTS (
        -- Evitar duplicatas
        SELECT 1 FROM [EmparelhamentosBase] eb 
        WHERE eb.Clube1 = j1.Nome AND eb.Clube2 = j2.Nome 
        AND eb.DataJogo = je.DataJogo AND eb.CompeticaoId = je.CompeticaoId
    );
END

-- 3. Migrar dados de EmparelhamentosEquipa para EmparelhamentosBase (se a tabela existir)
IF EXISTS (SELECT * FROM sys.tables WHERE name = 'EmparelhamentosEquipa')
BEGIN
    -- Inserir dados de EmparelhamentosEquipa em EmparelhamentosBase
    INSERT INTO [EmparelhamentosBase] ([Clube1], [Clube2], [DataJogo], [HoraJogo], [CompeticaoId], [NomeCompeticao])
    SELECT 
        ee.Clube1,
        ee.Clube2,
        ee.DataJogo,
        ee.HoraJogo,
        ee.CompeticaoId,
        ee.NomeCompeticao
    FROM [EmparelhamentosEquipa] ee
    WHERE NOT EXISTS (
        -- Evitar duplicatas
        SELECT 1 FROM [EmparelhamentosBase] eb 
        WHERE eb.Clube1 = ee.Clube1 AND eb.Clube2 = ee.Clube2 
        AND eb.DataJogo = ee.DataJogo AND eb.CompeticaoId = ee.CompeticaoId
    );
END

-- 4. Remover as tabelas antigas (se existirem)
-- Primeiro, remover as chaves estrangeiras
IF EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_JogosEmparelhados_Competicoes_CompeticaoId')
BEGIN
    ALTER TABLE [JogosEmparelhados] DROP CONSTRAINT [FK_JogosEmparelhados_Competicoes_CompeticaoId];
END

IF EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_JogosEmparelhados_Jogadores_Jogador1Id')
BEGIN
    ALTER TABLE [JogosEmparelhados] DROP CONSTRAINT [FK_JogosEmparelhados_Jogadores_Jogador1Id];
END

IF EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_JogosEmparelhados_Jogadores_Jogador2Id')
BEGIN
    ALTER TABLE [JogosEmparelhados] DROP CONSTRAINT [FK_JogosEmparelhados_Jogadores_Jogador2Id];
END

IF EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_EmparelhamentosEquipa_Competicoes_CompeticaoId')
BEGIN
    ALTER TABLE [EmparelhamentosEquipa] DROP CONSTRAINT [FK_EmparelhamentosEquipa_Competicoes_CompeticaoId];
END

-- Agora, remover as tabelas
IF EXISTS (SELECT * FROM sys.tables WHERE name = 'JogosEmparelhados')
BEGIN
    DROP TABLE [JogosEmparelhados];
END

IF EXISTS (SELECT * FROM sys.tables WHERE name = 'EmparelhamentosEquipa')
BEGIN
    DROP TABLE [EmparelhamentosEquipa];
END

-- 5. Atualizar a tabela __EFMigrationsHistory para registrar esta alteração
IF NOT EXISTS (SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = '20250524120000_ConsolidarEmparelhamentoBase')
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES ('20250524120000_ConsolidarEmparelhamentoBase', '7.0.5');
END

-- 6. Verificar se há colunas duplicadas na tabela EmparelhamentosBase
-- Verificar se existem colunas duplicadas para Clube1
IF EXISTS (
    SELECT 1 
    FROM sys.columns 
    WHERE object_id = OBJECT_ID('EmparelhamentosBase') 
    AND name = 'Clube1' 
    AND column_id IN (
        SELECT column_id 
        FROM sys.columns 
        WHERE object_id = OBJECT_ID('EmparelhamentosBase') 
        GROUP BY column_id 
        HAVING COUNT(*) > 1
    )
)
BEGIN
    -- Renomear a coluna duplicada
    DECLARE @sql nvarchar(max) = 'EXEC sp_rename ''EmparelhamentosBase.Clube1_1'', ''Clube1_Duplicada'', ''COLUMN''';
    EXEC sp_executesql @sql;
END

-- Verificar se existem colunas duplicadas para Clube2
IF EXISTS (
    SELECT 1 
    FROM sys.columns 
    WHERE object_id = OBJECT_ID('EmparelhamentosBase') 
    AND name = 'Clube2' 
    AND column_id IN (
        SELECT column_id 
        FROM sys.columns 
        WHERE object_id = OBJECT_ID('EmparelhamentosBase') 
        GROUP BY column_id 
        HAVING COUNT(*) > 1
    )
)
BEGIN
    -- Renomear a coluna duplicada
    DECLARE @sql2 nvarchar(max) = 'EXEC sp_rename ''EmparelhamentosBase.Clube2_1'', ''Clube2_Duplicada'', ''COLUMN''';
    EXEC sp_executesql @sql2;
END

-- Verificar se existem colunas duplicadas para NomeCompeticao
IF EXISTS (
    SELECT 1 
    FROM sys.columns 
    WHERE object_id = OBJECT_ID('EmparelhamentosBase') 
    AND name = 'NomeCompeticao' 
    AND column_id IN (
        SELECT column_id 
        FROM sys.columns 
        WHERE object_id = OBJECT_ID('EmparelhamentosBase') 
        GROUP BY column_id 
        HAVING COUNT(*) > 1
    )
)
BEGIN
    -- Renomear a coluna duplicada
    DECLARE @sql3 nvarchar(max) = 'EXEC sp_rename ''EmparelhamentosBase.NomeCompeticao_1'', ''NomeCompeticao_Duplicada'', ''COLUMN''';
    EXEC sp_executesql @sql3;
END