CREATE TABLE [Competicoes] (
    [Id] int NOT NULL IDENTITY,
    [Nome] nvarchar(100) NOT NULL,
    [TipoCompeticao] nvarchar(50) NOT NULL,
    [NumJogadores] int NOT NULL,
    [NumEquipas] int NOT NULL,
    [PontosVitoria] int NOT NULL,
    [PontosEmpate] int NOT NULL,
    CONSTRAINT [PK_Competicoes] PRIMARY KEY ([Id])
);
GO


CREATE TABLE [Utilizadores] (
    [Id] int NOT NULL IDENTITY,
    [UtilizadorNome] nvarchar(100) NOT NULL,
    [Email] nvarchar(100) NOT NULL,
    [Senha] nvarchar(100) NOT NULL,
    CONSTRAINT [PK_Utilizadores] PRIMARY KEY ([Id])
);
GO


CREATE TABLE [ConfiguracoesFase] (
    [Id] int NOT NULL IDENTITY,
    [FaseNumero] int NOT NULL,
    [NumJogosPorFase] int NOT NULL,
    [Formato] nvarchar(50) NOT NULL,
    [PontosVitoria] int NOT NULL,
    [PontosEmpate] int NOT NULL,
    [PontosDerrota] int NOT NULL,
    [PontosFaltaComparencia] int NOT NULL,
    [PontosDesclassificacao] int NOT NULL,
    [PontosExtra] int NOT NULL,
    [CriteriosDesempate] nvarchar(max) NOT NULL,
    [CompeticaoId] int NOT NULL,
    CONSTRAINT [PK_ConfiguracoesFase] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_ConfiguracoesFase_Competicoes_CompeticaoId] FOREIGN KEY ([CompeticaoId]) REFERENCES [Competicoes] ([Id]) ON DELETE CASCADE
);
GO


CREATE TABLE [EmparelhamentosBase] (
    [Id] int NOT NULL IDENTITY,
    [Clube1] nvarchar(max) NOT NULL,
    [Clube2] nvarchar(max) NOT NULL,
    [DataJogo] datetime2 NOT NULL,
    [HoraJogo] time NOT NULL,
    [CompeticaoId] int NOT NULL,
    [NomeCompeticao] nvarchar(max) NULL,
    CONSTRAINT [PK_EmparelhamentosBase] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_EmparelhamentosBase_Competicoes_CompeticaoId] FOREIGN KEY ([CompeticaoId]) REFERENCES [Competicoes] ([Id]) ON DELETE CASCADE
);
GO


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
GO


CREATE TABLE [Jogadores] (
    [Id] int NOT NULL IDENTITY,
    [Nome] nvarchar(max) NOT NULL,
    [Codigo] nvarchar(max) NOT NULL,
    [DataNascimento] datetime2 NOT NULL,
    [Categoria] nvarchar(max) NOT NULL,
    [Clube] nvarchar(max) NOT NULL,
    [CompeticaoId] int NOT NULL,
    CONSTRAINT [PK_Jogadores] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Jogadores_Competicoes_CompeticaoId] FOREIGN KEY ([CompeticaoId]) REFERENCES [Competicoes] ([Id]) ON DELETE CASCADE
);
GO


CREATE TABLE [JogosEmparelhados] (
    [Id] int NOT NULL IDENTITY,
    [Jogador1Id] int NOT NULL,
    [Jogador2Id] int NOT NULL,
    [DataJogo] datetime2 NOT NULL,
    [HoraJogo] time NOT NULL,
    [CompeticaoId] int NOT NULL,
    CONSTRAINT [PK_JogosEmparelhados] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_JogosEmparelhados_Competicoes_CompeticaoId] FOREIGN KEY ([CompeticaoId]) REFERENCES [Competicoes] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_JogosEmparelhados_Jogadores_Jogador1Id] FOREIGN KEY ([Jogador1Id]) REFERENCES [Jogadores] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_JogosEmparelhados_Jogadores_Jogador2Id] FOREIGN KEY ([Jogador2Id]) REFERENCES [Jogadores] ([Id]) ON DELETE NO ACTION
);
GO


CREATE INDEX [IX_ConfiguracoesFase_CompeticaoId] ON [ConfiguracoesFase] ([CompeticaoId]);
GO


CREATE INDEX [IX_EmparelhamentosBase_CompeticaoId] ON [EmparelhamentosBase] ([CompeticaoId]);
GO


CREATE INDEX [IX_EmparelhamentosEquipa_CompeticaoId] ON [EmparelhamentosEquipa] ([CompeticaoId]);
GO


CREATE INDEX [IX_Jogadores_CompeticaoId] ON [Jogadores] ([CompeticaoId]);
GO


CREATE INDEX [IX_JogosEmparelhados_CompeticaoId] ON [JogosEmparelhados] ([CompeticaoId]);
GO


CREATE INDEX [IX_JogosEmparelhados_Jogador1Id] ON [JogosEmparelhados] ([Jogador1Id]);
GO


CREATE INDEX [IX_JogosEmparelhados_Jogador2Id] ON [JogosEmparelhados] ([Jogador2Id]);
GO


