-- Script para corrigir a relação entre Competicao e EmparelhamentoBase

-- 1. Verificar se existe a coluna CompeticaoId1
IF EXISTS (SELECT * FROM sys.columns WHERE name = 'CompeticaoId1' AND object_id = OBJECT_ID('EmparelhamentosBase'))
BEGIN
    -- 2. Remover o índice associado à coluna CompeticaoId1, se existir
    IF EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_EmparelhamentosBase_CompeticaoId1')
    BEGIN
        DROP INDEX [IX_EmparelhamentosBase_CompeticaoId1] ON [EmparelhamentosBase];
    END

    -- 3. Remover a chave estrangeira associada à coluna CompeticaoId1, se existir
    IF EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_EmparelhamentosBase_Competicoes_CompeticaoId1')
    BEGIN
        ALTER TABLE [EmparelhamentosBase] DROP CONSTRAINT [FK_EmparelhamentosBase_Competicoes_CompeticaoId1];
    END

    -- 4. Remover a coluna CompeticaoId1
    ALTER TABLE [EmparelhamentosBase] DROP COLUMN [CompeticaoId1];
END

-- 5. Verificar se existe a chave estrangeira para CompeticaoId
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_EmparelhamentosBase_Competicoes_CompeticaoId')
BEGIN
    -- 6. Adicionar a chave estrangeira para CompeticaoId
    ALTER TABLE [EmparelhamentosBase] ADD CONSTRAINT [FK_EmparelhamentosBase_Competicoes_CompeticaoId]
    FOREIGN KEY ([CompeticaoId]) REFERENCES [Competicoes] ([Id]) ON DELETE CASCADE;
END

-- 7. Verificar se existe o índice para CompeticaoId
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_EmparelhamentosBase_CompeticaoId')
BEGIN
    -- 8. Adicionar o índice para CompeticaoId
    CREATE INDEX [IX_EmparelhamentosBase_CompeticaoId] ON [EmparelhamentosBase] ([CompeticaoId]);
END

-- 9. Atualizar a tabela __EFMigrationsHistory para registrar esta alteração
IF NOT EXISTS (SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = '20250524130100_CorrigirRelacaoCompeticao')
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES ('20250524130100_CorrigirRelacaoCompeticao', '7.0.5');
END