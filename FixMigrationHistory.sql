-- Adicionar as migrações pendentes à tabela __EFMigrationsHistory
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