-- Verificar se existem colunas duplicadas e renomeá-las
DECLARE @sql nvarchar(max) = '';

-- Verificar se existem colunas duplicadas para Clube1
IF EXISTS (
    SELECT 1 
    FROM sys.columns 
    WHERE object_id = OBJECT_ID('EmparelhamentosEquipa') 
    AND name = 'Clube1' 
    AND column_id IN (
        SELECT column_id 
        FROM sys.columns 
        WHERE object_id = OBJECT_ID('EmparelhamentosEquipa') 
        GROUP BY column_id 
        HAVING COUNT(*) > 1
    )
)
BEGIN
    -- Renomear a coluna duplicada
    SET @sql = 'EXEC sp_rename ''EmparelhamentosEquipa.Clube1_1'', ''Clube1_Duplicada'', ''COLUMN''';
    EXEC sp_executesql @sql;
END

-- Verificar se existem colunas duplicadas para Clube2
IF EXISTS (
    SELECT 1 
    FROM sys.columns 
    WHERE object_id = OBJECT_ID('EmparelhamentosEquipa') 
    AND name = 'Clube2' 
    AND column_id IN (
        SELECT column_id 
        FROM sys.columns 
        WHERE object_id = OBJECT_ID('EmparelhamentosEquipa') 
        GROUP BY column_id 
        HAVING COUNT(*) > 1
    )
)
BEGIN
    -- Renomear a coluna duplicada
    SET @sql = 'EXEC sp_rename ''EmparelhamentosEquipa.Clube2_1'', ''Clube2_Duplicada'', ''COLUMN''';
    EXEC sp_executesql @sql;
END

-- Verificar se existem colunas duplicadas para NomeCompeticao
IF EXISTS (
    SELECT 1 
    FROM sys.columns 
    WHERE object_id = OBJECT_ID('EmparelhamentosEquipa') 
    AND name = 'NomeCompeticao' 
    AND column_id IN (
        SELECT column_id 
        FROM sys.columns 
        WHERE object_id = OBJECT_ID('EmparelhamentosEquipa') 
        GROUP BY column_id 
        HAVING COUNT(*) > 1
    )
)
BEGIN
    -- Renomear a coluna duplicada
    SET @sql = 'EXEC sp_rename ''EmparelhamentosEquipa.NomeCompeticao_1'', ''NomeCompeticao_Duplicada'', ''COLUMN''';
    EXEC sp_executesql @sql;
END