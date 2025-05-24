-- Remover a tabela EmparelhamentosEquipa se existir
IF EXISTS (SELECT * FROM sys.tables WHERE name = 'EmparelhamentosEquipa')
BEGIN
    DROP TABLE [EmparelhamentosEquipa];
END

-- Criar a tabela EmparelhamentosEquipa com a estrutura correta
CREATE TABLE [EmparelhamentosEquipa] (
    [Id] int NOT NULL IDENTITY,
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