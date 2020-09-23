using System.Data;

namespace AutoPocoIO.test.DynamicSchema.Db
{
    public static class SchmeaBuilderDataTableBuilder
    {

        public static DataTable CreateColumnTable()
        {
            var dt = new DataTable();
            //Table attr
            dt.Columns.Add("ObjectType");
            dt.Columns.Add("TableSchema");
            dt.Columns.Add("TableName");
            dt.Columns.Add("DatabaseName");
            dt.Columns.Add("PKColumnName");
            //Column attr
            dt.Columns.Add("ColumnName");
            dt.Columns.Add("ColumnType");
            dt.Columns.Add("ColumnLength");
            dt.Columns.Add("ColumnIsNullable");
            dt.Columns.Add("PKName");
            dt.Columns.Add("PKPosition");
            dt.Columns.Add("PKIsIdentity");
            dt.Columns.Add("IsComputed");
            dt.Columns.Add("FKName");
            dt.Columns.Add("ReferencedSchema");
            dt.Columns.Add("ReferencedTable");
            dt.Columns.Add("ReferencedColumn");

            return dt;
        }

        public static DataTable CreatProcTable()
        {
            var dt = new DataTable();
            //Table attr
            dt.Columns.Add("ProcSchema");
            dt.Columns.Add("ProcName");
            dt.Columns.Add("DatabaseName");
            //Params attr
            dt.Columns.Add("ParamName");
            dt.Columns.Add("ParamType");
            dt.Columns.Add("IsOutput");
            dt.Columns.Add("IsNullable");

            return dt;
        }

        public static DataRow CreateRowWithColReqValues(this DataTable tbl)
        {
            var row = tbl.NewRow();
            //Required
            row["ColumnLength"] = "1";
            row["ColumnIsNullable"] = "false";
            row["PKPosition"] = "0";
            row["PKIsIdentity"] = "false";
            row["IsComputed"] = "false";

            return row;
        }
    }
}
