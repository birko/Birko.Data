using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace Birko.Data.DataBase.Connector
{
    public abstract partial class AbstractConnector
    {
        #region select
        public void Select(Type type, Action<object> resultAction, LambdaExpression expr)
        {
            Select(type, resultAction, DataBase.ParseExpression(expr));
        }

        public void Select(Type type, Action<object> resultAction, IEnumerable<Condition.Condition> conditions = null)
        {
            var table = DataBase.LoadTable(type);
            Select(table, (fields, reader) => {
                if (resultAction != null)
                {
                    var data = Activator.CreateInstance(type, new object[0]);
                    var index = DataBase.Read(reader, data);
                    resultAction(data);
                }
            }, conditions);
        }

        public void Select(Table.Table table, Action<IDictionary<int, string>, DbDataReader> readAction, IEnumerable<Condition.Condition> conditions = null)
        {
            if (table != null)
            {
                IDictionary<int, string> fields = table.GetSelectFields();
                var tableName = table.Name;
                Select(tableName, fields, readAction, conditions);
            }
        }

        public void Select(string tableName, IDictionary<int, string> fields, Action<IDictionary<int, string>, DbDataReader> readAction = null, IEnumerable<Condition.Condition> conditions = null)
        {
            using (var db = CreateConnection(_settings))
            {
                db.Open();
                try
                {
                    using (var command = db.CreateCommand())
                    {
                        command.CommandText = "SELECT "
                            + string.Join(", ", fields.Values)
                            + " FROM "
                            + tableName;
                        AddWhere(conditions, command);
                        var reader = command.ExecuteReader();
                        if (reader.HasRows)
                        {
                            bool isNext = reader.Read();
                            while (isNext)
                            {
                                readAction?.Invoke(fields, reader);
                                isNext = reader.Read();
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    InitException(ex);
                }
            }
        }
        #endregion
        #region count
        public long SelectCount(Type type, LambdaExpression expr)
        {
            return SelectCount(type, DataBase.ParseExpression(expr));
        }

        public long SelectCount(Type type, IEnumerable<Condition.Condition> conditions = null)
        {
            var table = DataBase.LoadTable(type);
            return SelectCount(table, conditions);
        }

        public long SelectCount(Table.Table table, LambdaExpression expr)
        {
            return (table != null) ? SelectCount(table.Name, DataBase.ParseExpression(expr)) : 0;
        }

        public long SelectCount(Table.Table table, IEnumerable<Condition.Condition> conditions = null)
        {
            return (table != null) ? SelectCount(table.Name, conditions) : 0;
        }

        public long SelectCount(string tableName, IEnumerable<Condition.Condition> conditions = null)
        {
            long count = 0;
            using (var db = CreateConnection(_settings))
            {
                db.Open();
                try
                {
                    using (var command = db.CreateCommand())
                    {
                        command.CommandText = "SELECT "
                            + "count(*) as count"
                            + " FROM "
                            + tableName;
                        AddWhere(conditions, command);
                        var data = command.ExecuteScalar();
                        count = (long)command.ExecuteScalar();
                    }
                }
                catch (Exception ex)
                {
                    InitException(ex);
                }
            }
            return count;
        }
        #endregion
    }
}
