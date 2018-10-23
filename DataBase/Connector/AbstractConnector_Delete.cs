using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace Birko.Data.DataBase.Connector
{
    public abstract partial class AbstractConnector
    {
        public void Delete(Type type, LambdaExpression expr)
        {
            Delete(type, DataBase.ParseConditionExpression(expr));
        }

        public void Delete(Type type, IEnumerable<Condition.Condition> conditions = null)
        {
            Delete(DataBase.LoadTable(type), conditions);
        }

        public void Delete(Table.Table table, LambdaExpression expr)
        {
            Delete(table, DataBase.ParseConditionExpression(expr));
        }

        public void Delete(Table.Table table, IEnumerable<Condition.Condition> conditions = null)
        {
            var tableName = table.Name;
            Delete(tableName, conditions);
        }

        public void Delete(string tableName, LambdaExpression expr)
        {
            Delete(tableName, DataBase.ParseConditionExpression(expr));
        }

        private void Delete(string tableName, IEnumerable<Condition.Condition> conditions = null)
        {
            using (var db = CreateConnection(_settings))
            {
                db.Open();
                var transaction = db.BeginTransaction();
                try
                {
                    using (var command = db.CreateCommand())
                    {
                        command.CommandText = "DELETE FROM " + tableName;
                        AddWhere(conditions, command);
                        command.ExecuteNonQuery();
                    }
                    transaction.Commit();
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    InitException(ex);
                }
            }
        }
    }
}
