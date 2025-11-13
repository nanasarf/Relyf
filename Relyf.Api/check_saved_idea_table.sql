-- Check if SavedIdea table exists
SELECT 
    OBJECT_NAME(object_id) AS TableName,
    name AS ColumnName,
    TYPE_NAME(user_type_id) AS DataType
FROM sys.columns
WHERE OBJECT_NAME(object_id) IN ('SavedIdea', 'Save')
ORDER BY TableName, column_id;

-- Check for any tables with 'Save' in the name
SELECT 
    SCHEMA_NAME(schema_id) AS SchemaName,
    name AS TableName
FROM sys.tables
WHERE name LIKE '%Save%'
ORDER BY SchemaName, name;
