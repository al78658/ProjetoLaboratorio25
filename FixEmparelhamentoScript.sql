-- Script to create EmparelhamentosEquipa table if it doesn't exist
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'EmparelhamentosEquipa')
BEGIN
    CREATE TABLE [EmparelhamentosEquipa] (
        [Id] int NOT NULL IDENTITY,
        [Clube1] nvarchar(max) NOT NULL,
        [Clube2] nvarchar(max) NOT NULL,
        [DataJogo] datetime2 NOT NULL,
        [HoraJogo] time NOT NULL,
        [NomeCompeticao] nvarchar(max) NOT NULL,
        [CompeticaoId] int NOT NULL,
        CONSTRAINT [PK_EmparelhamentosEquipa] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_EmparelhamentosEquipa_Competicoes_CompeticaoId] FOREIGN KEY ([CompeticaoId]) REFERENCES [Competicoes] ([Id]) ON DELETE CASCADE
    );

    CREATE INDEX [IX_EmparelhamentosEquipa_CompeticaoId] ON [EmparelhamentosEquipa] ([CompeticaoId]);
END

-- Add CompeticaoId1 column to JogosEmparelhados if it doesn't exist
IF NOT EXISTS (SELECT * FROM sys.columns WHERE name = 'CompeticaoId1' AND object_id = OBJECT_ID('JogosEmparelhados'))
BEGIN
    ALTER TABLE [JogosEmparelhados] ADD [CompeticaoId1] int NULL;
    CREATE INDEX [IX_JogosEmparelhados_CompeticaoId1] ON [JogosEmparelhados] ([CompeticaoId1]);
    ALTER TABLE [JogosEmparelhados] ADD CONSTRAINT [FK_JogosEmparelhados_Competicoes_CompeticaoId1] 
    FOREIGN KEY ([CompeticaoId1]) REFERENCES [Competicoes] ([Id]);
END

-- Create index on JogosEmparelhados.CompeticaoId if it doesn't exist
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_JogosEmparelhados_CompeticaoId' AND object_id = OBJECT_ID('JogosEmparelhados'))
BEGIN
    CREATE INDEX [IX_JogosEmparelhados_CompeticaoId] ON [JogosEmparelhados] ([CompeticaoId]);
END

-- Add foreign key to JogosEmparelhados.CompeticaoId if it doesn't exist
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_JogosEmparelhados_Competicoes_CompeticaoId' AND parent_object_id = OBJECT_ID('JogosEmparelhados'))
BEGIN
    ALTER TABLE [JogosEmparelhados] ADD CONSTRAINT [FK_JogosEmparelhados_Competicoes_CompeticaoId] 
    FOREIGN KEY ([CompeticaoId]) REFERENCES [Competicoes] ([Id]) ON DELETE CASCADE;
END

-- Insert the migration record into __EFMigrationsHistory
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