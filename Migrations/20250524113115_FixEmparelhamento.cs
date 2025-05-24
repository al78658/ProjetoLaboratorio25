﻿using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProjetoLaboratorio25.Migrations
{
    /// <inheritdoc />
    public partial class FixEmparelhamento : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Execute o script SQL personalizado
            migrationBuilder.Sql(@"
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
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Remover a tabela EmparelhamentosEquipa se existir
            migrationBuilder.Sql(@"
IF EXISTS (SELECT * FROM sys.tables WHERE name = 'EmparelhamentosEquipa')
BEGIN
    DROP TABLE [EmparelhamentosEquipa];
END

-- Remover a coluna CompeticaoId1 de JogosEmparelhados se existir
IF EXISTS (SELECT * FROM sys.columns WHERE name = 'CompeticaoId1' AND object_id = OBJECT_ID('JogosEmparelhados'))
BEGIN
    IF EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_JogosEmparelhados_Competicoes_CompeticaoId1')
    BEGIN
        ALTER TABLE [JogosEmparelhados] DROP CONSTRAINT [FK_JogosEmparelhados_Competicoes_CompeticaoId1];
    END
    
    IF EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_JogosEmparelhados_CompeticaoId1')
    BEGIN
        DROP INDEX [IX_JogosEmparelhados_CompeticaoId1] ON [JogosEmparelhados];
    END
    
    ALTER TABLE [JogosEmparelhados] DROP COLUMN [CompeticaoId1];
END

-- Remover o índice em JogosEmparelhados.CompeticaoId se existir
IF EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_JogosEmparelhados_CompeticaoId')
BEGIN
    DROP INDEX [IX_JogosEmparelhados_CompeticaoId] ON [JogosEmparelhados];
END

-- Remover a chave estrangeira em JogosEmparelhados.CompeticaoId se existir
IF EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_JogosEmparelhados_Competicoes_CompeticaoId')
BEGIN
    ALTER TABLE [JogosEmparelhados] DROP CONSTRAINT [FK_JogosEmparelhados_Competicoes_CompeticaoId];
END
            ");
        }
    }
}
