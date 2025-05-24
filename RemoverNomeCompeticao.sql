-- Remover a coluna NomeCompeticao da tabela EmparelhamentosBase
IF EXISTS (SELECT * FROM sys.columns WHERE name = 'NomeCompeticao' AND object_id = OBJECT_ID('EmparelhamentosBase'))
BEGIN
    ALTER TABLE [EmparelhamentosBase] DROP COLUMN [NomeCompeticao];
END

-- Atualizar a tabela __EFMigrationsHistory para registrar esta alteração
IF NOT EXISTS (SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = '20250524130000_RemoverNomeCompeticao')
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES ('20250524130000_RemoverNomeCompeticao', '7.0.5');
END