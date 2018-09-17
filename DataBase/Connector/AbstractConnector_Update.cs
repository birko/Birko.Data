using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace Birko.Data.DataBase.Connector
{
    public abstract partial class AbstractConnector
    {
        public void Update(object model, LambdaExpression expr)
        {
            Update(model, DataBase.ParseExpression(expr));
        }

        public void Update(object model, IEnumerable<Condition.Condition> conditions = null)
        {
            if (model != null)
            {
                Update(model.GetType(), model, conditions);
            }
        }

        public void Update(Type type, object model, LambdaExpression expr)
        {
            Update(DataBase.LoadTable(type), model, DataBase.ParseExpression(expr));
        }

        public void Update(Type type, object model, IEnumerable<Condition.Condition> conditions = null)
        {
            Update(DataBase.LoadTable(type), model, conditions);
        }

        public void Update(Table.Table table, object model, LambdaExpression expr)
        {
            Update(table, model, DataBase.ParseExpression(expr));
        }

        public void Update(Table.Table table, object model, IEnumerable<Condition.Condition> conditions = null)
        {
            if (model != null)
            {
                Update(table.Name, table.GetSelectFields(), DataBase.Write(table.Fields.Select(f => f.Value), model), conditions);
            }
        }

        public void Update<T, P>(Type type, IDictionary<Expression<Func<T, P>>, object> expresions, LambdaExpression expr)
        {
            Update(type, expresions, DataBase.ParseExpression(expr));
        }

        public void Update<T,P>(Type type, IDictionary<Expression<Func<T, P>>, object> expresions, IEnumerable<Condition.Condition> conditions = null)
        {
            var table = DataBase.LoadTable(type);
            Update(table, expresions, conditions);
        }

        public void Update<T, P>(Table.Table table, IDictionary<Expression<Func<T, P>>, object> expresions, Expression expr)
        {
                Update(table, expresions, DataBase.ParseExpression(expr));
        }

        public void Update<T, P>(Table.Table table, IDictionary<Expression<Func<T, P>>, object> expresions, IEnumerable<Condition.Condition> conditions = null)
        {
            if (table != null)
            {
                Update(table.Name, expresions, conditions);
            }
        }

        public void Update<T, P>(string tableName, IDictionary<Expression<Func<T, P>>, object> expresions, IEnumerable<Condition.Condition> conditions = null)
        {
            var fields = new Dictionary<int, string>();
            var values = new Dictionary<string, object>();
            int i = 0;
            foreach (var kvp in expresions)
            {
                var field = DataBase.GetField(kvp.Key);
                fields.Add(i, field.Name);
                values.Add(field.Name, kvp.Value);
                i++;
            }
            Update(tableName, fields, values, conditions);
        }

        public void Update(Table.Table table, IDictionary<string, object> values, IEnumerable<Condition.Condition> conditions = null)
        {
            var tableName = table.Name;
            IDictionary<int, string> fields = table.GetSelectFields();
            Update(tableName, fields, values, conditions);
        }

        public void Update(string tableName, IDictionary<int, string> fields, IDictionary<string, object> values, IEnumerable<Condition.Condition> conditions = null)
        {
            if (values != null && values.Any())
            {
                using (var db = CreateConnection(_settings))
                {
                    db.Open();
                    var transaction = db.BeginTransaction();
                    try
                    {
                        using (var command = db.CreateCommand())
                        {
                            command.CommandText = "UPDATE " + tableName + " SET " + string.Join(", ", fields.Values.Select(x => x + "= @SET" + x.Replace(".", string.Empty)));
                            AddWhere(conditions, command);
                            foreach (var kvp in values)
                            {
                                AddParameter(command, "@SET" + kvp.Key.Replace(".", string.Empty), kvp.Value);
                            }
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
}
