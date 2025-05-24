-- Verificar se a tabela EmparelhamentosEquipa existe e recriá-la se necessário
IF EXISTS (SELECT * FROM sys.tables WHERE name = 'EmparelhamentosEquipa')
BEGIN
    -- Remover a tabela existente
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
    CONSTRAINT [PK_EmparelhamentosEquipa] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_EmparelhamentosEquipa_Competicoes_CompeticaoId] FOREIGN KEY ([CompeticaoId]) REFERENCES [Competicoes] ([Id]) ON DELETE CASCADE
);

-- Criar o índice para CompeticaoId
CREATE INDEX [IX_EmparelhamentosEquipa_CompeticaoId] ON [EmparelhamentosEquipa] ([CompeticaoId]);