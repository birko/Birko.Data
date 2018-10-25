using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace Birko.Data.DataBase.Connector
{
    public abstract partial class AbstractConnector
    {
        public void DropTable(Type type)
        {
            DropTable(new[] { type });
        }

        public void DropTable(Table.Table table)
        {
            DropTable(new[] { table.Name });
        }

        public void DropTable(Type[] types)
        {
            DropTable(DataBase.LoadTables(types));
        }

        public void DropTable(IEnumerable<Table.Table> tables)
        {
            if (tables != null && tables.Any() && tables.Any(x => x != null))
            {
                DropTable(tables.Where(x => x != null).Select(x => x.Name));
            }
        }

        public void DropTable(IEnumerable<string> tables)
        {
            if (tables != null && tables.Any() && tables.Any(x => !string.IsNullOrEmpty(x)))
            {
                using (var db = CreateConnection(_settings))
                {
                    db.Open();
                    using (var transaction = db.BeginTransaction())
                    {
                        string commandText = null;
                        try
                        {
                            foreach (var tableName in tables.Where(x => !string.IsNullOrEmpty(x)))
                            {
                                using (var command = db.CreateCommand())
                                {
                                    command.CommandText = "DROP TABLE IF EXISTS " + tableName;
                                    commandText = DataBase.GetGeneratedQuery(command);
                                    command.ExecuteNonQuery();
                                }
                            }
                            transaction.Commit();
                        }
                        catch (Exception ex)
                        {
                            transaction.Rollback();
                            InitException(ex, commandText);
                        }
                    }
                }
            }
        }
    }
}
