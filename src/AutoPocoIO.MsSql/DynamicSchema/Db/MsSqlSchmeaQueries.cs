using AutoPocoIO.DynamicSchema.Models;
using System.Diagnostics.CodeAnalysis;

namespace AutoPocoIO.DynamicSchema.Db
{
    [ExcludeFromCodeCoverage]
    internal class MsSqlSchmeaQueries : ISchemaQueries
    {
        private readonly Config _config;

        public MsSqlSchmeaQueries(Config config)
        {
            _config = config;
        }
        public virtual string BuildColumns()
        {
            return $@"
            with allfksfromparent as
            (
            SELECT 
               distinct 
               (fk.referenced_object_id) refobjid,
               fk.referenced_object_id,
               fk.parent_object_id 

            FROM 
               sys.foreign_keys fk  
            WHERE 
               fk.parent_object_id  = Object_ID('{_config.FilterSchema}.{_config.IncludedTable}')
                or fk.referenced_object_id  = Object_ID('{_config.FilterSchema}.{_config.IncludedTable}') 
                or fk.parent_object_id  in ({_config.JoinsAsString})
                or fk.referenced_object_id  in ({_config.JoinsAsString})
            UNION ALL
            SELECT 
   
               (fk2.referenced_object_id) refobjid,
               fk2.parent_object_id,
               fk2.referenced_object_id
            FROM 
               sys.foreign_keys fk2  
            JOIN allfksfromparent on
            --fk2.parent_object_id = allfksfromreftables.referenced_object_id --AND fk2.parent_object_id <> fk2.referenced_object_id and allfksfromreftables.referenced_object_id <> allfksfromreftables.parent_object_id
            fk2.parent_object_id = allfksfromparent.parent_object_id and allfksfromparent.referenced_object_id <> fk2.parent_object_id
            )
            ,
            distinctfks as
            (
            select distinct object_id from (
            SELECT distinct refobjid as object_id from allfksfromparent
            UNION ALL
            SELECT distinct referenced_object_id as object_id from allfksfromparent
            UNION ALL
            select Object_ID('{_config.FilterSchema}.{_config.IncludedTable}') 
            union all select object_id from sys.tables where object_id in ({_config.JoinsAsString})
            ) as dobjids
            )
            select
                schema_name(t.schema_id) TableSchema, t.name TableName, t.type ObjectType, db_name() DatabaseName,
                c.name ColumnName, null ColumnDescription, type_name(isnull(cast(dt.system_type_id as int), c.user_type_id)) as ColumnType,	c.max_length as ColumnLength, c.is_nullable as ColumnIsNullable,
	            pk.constraint_name PKName, pk_column_name PKColumnName, isnull(ordinal_position,0) PKPosition, isnull(pk.is_identity,0) PKIsIdentity,
                fk.fk_name FKName, fk.reference_schema ReferencedSchema, fk.referenced_object ReferencedTable, fk.referenced_column ReferencedColumn,
                c.is_computed as IsComputed
	
            from sys.objects t
            inner join distinctfks dfk
            ON t.object_id = dfk.object_id
            left outer join 
            sys.columns c on t.object_id = c.object_id
            left outer join sys.types dt
            ON dt.user_type_id = c.user_type_id AND is_user_defined = 1
            left outer join 
            (select 
	            object_name(constraint_object_id) fk_name	
	            ,fkc.parent_object_id
	            ,fkc.parent_column_id
	            , schema_name(t.schema_id) reference_schema
	            ,object_name(referenced_object_id) referenced_object
	            ,(select name from sys.columns c where c.object_id = fkc.referenced_object_id and c.column_id = fkc.referenced_column_id) as referenced_column
             from sys.foreign_key_columns fkc inner join sys.tables t on t.object_id = fkc.referenced_object_id
            ) fk 
            on fk.parent_object_id = t.object_id and c.column_id = fk.parent_column_id
            left outer join 
            (select 
	            c.is_identity as is_identity,
	            c.is_rowguidcol as is_rowguidcol,
	            t.object_id as table_object_id, s.name as table_schema, t.name as table_name
                , k.name as constraint_name, k.type_desc as constraint_type
                , c.name as pk_column_name, ic.key_ordinal AS ordinal_position          
             from sys.key_constraints as k
             join sys.tables as t
             on t.object_id = k.parent_object_id
             join sys.schemas as s
             on s.schema_id = t.schema_id
             join sys.index_columns as ic
             on ic.object_id = t.object_id
             and ic.index_id = k.unique_index_id
             join sys.columns as c
             on c.object_id = t.object_id
             and c.column_id = ic.column_id
             where k.type_desc = 'PRIMARY_KEY_CONSTRAINT'
            ) pk on pk.table_object_id = t.object_id and pk.pk_column_name = c.name
            where t.name <> 'sysdiagrams' and t.type in ('V', 'U')
            order by TableSchema, TableName";
        }

        public virtual string BuildStoredProcedureCommand()
        {
            return $@" select
            objs.name ProcName,
            schema_name(objs.schema_id) ProcSchema,
            params.name ParamName,
            type_name(user_type_id) ParamType,
            is_output IsOutput,
            is_nullable IsNullable,
            db_name() DatabaseName
            from sys.objects objs
            left join sys.parameters params on objs.object_id = params.object_id 
            where objs.type = 'P' and schema_name(objs.schema_id) = '{_config.FilterSchema}'
                and ('{_config.IncludedStoredProcedure}' = '' or  objs.name = '{_config.IncludedStoredProcedure}')
            order by objs.object_id, params.parameter_id";
        }

        public virtual string BuildTablesViewCommand()
        {

            string schemaFilter = _config.FilterSchema == null ? "!= 'sys'" : $"= '{_config.FilterSchema}'";

            return $@"
WITH mycte AS 
(
SELECT SCHEMA_NAME(o.schema_id) AS 'TableSchema'
					, OBJECT_NAME(i2.object_id) AS 'TableName'
					, STUFF(
						(SELECT ',' + COL_NAME(ic.object_id,ic.column_id) 
						FROM sys.indexes i1
							INNER JOIN sys.index_columns ic ON i1.object_id = ic.object_id AND i1.index_id = ic.index_id
						WHERE i1.is_primary_key = 1
							AND i1.object_id = i2.object_id	AND i1.index_id = i2.index_id
						FOR XML PATH('')),1,1,'') AS PrimaryKeys
FROM sys.indexes i2
	INNER JOIN sys.objects o ON i2.object_id = o.object_id
WHERE i2.is_primary_key = 1
	AND o.type_desc = 'USER_TABLE'
),
tableinfo as (
SELECT t.TABLE_NAME as TableName,t.TABLE_SCHEMA as TableSchema, TABLE_CATALOG DatabaseName,
            case
            when t.TABLE_TYPE = 'BASE TABLE' then 'U'
            when t.TABLE_TYPE = 'VIEW' then 'V'
            end as objectType
			,mycte.PrimaryKeys			
            FROM INFORMATION_SCHEMA.TABLES t
			left outer join mycte on mycte.TableSchema = TABLE_SCHEMA and mycte.TableName = TABLE_NAME

)
select TableName, TableSchema, DatabaseName,  objectType, c.COLUMN_NAME ColumnName, null ColumnDescription, c.data_type ColumnType, isnull(c.character_maximum_length,0) ColumnLength,
case
 when c.IS_NULLABLE = 'YES' then 1
 when c.IS_NULLABLE = 'NO' then 0
end as ColumnIsNullable,
null PKName, PrimaryKeys PKColumnName, 0 PKPosition, 0 PKIsIdentity, 0 IsComputed, null FKName
from INFORMATION_SCHEMA.COLUMNS c
inner join tableinfo on tableinfo.DatabaseName = c.TABLE_CATALOG and tableinfo.TableName = c.TABLE_NAME and tableinfo.TableSchema = c.TABLE_SCHEMA
WHERE TableSchema {schemaFilter}
order by TableSchema, TableName
            ";
        }
    }
}
