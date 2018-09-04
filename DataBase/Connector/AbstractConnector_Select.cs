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

        public void Select(Type[] types, Action<IEnumerable<object>> resultAction, LambdaExpression expr)
        {
            Select(types, resultAction, DataBase.ParseExpression(expr));
        }

        public void Select(Type type, Action<object> resultAction, IEnumerable<Condition.Condition> conditions = null)
        {
            Select(new[] { type }, (objects) => {
                if (resultAction != null && objects != null && objects.Any())
                {
                    var data = objects.First();
                    resultAction(data);
                }
            }, conditions);
        }

        public void Select(IEnumerable<Type> types, Action<IEnumerable<object>> resultAction, IEnumerable<Condition.Condition> conditions = null)
        {
            if (types != null)
            {
                Select(types.Select(x=> DataBase.LoadTable(x)), (fields, reader) => {
                    if (resultAction != null)
                    {
                        var index = 0;
                        List<object> objects = new List<object>();
                        foreach (var type in types)
                        {
                            var data = Activator.CreateInstance(type, new object[0]);
                            index = DataBase.Read(reader, data, index);
                            objects.Add(data);
                        }
                        resultAction(objects.ToArray());
                    }
                }, conditions);
            }
        }

        public void Select(Table.Table table, Action<IDictionary<int, string>, DbDataReader> readAction, IEnumerable<Condition.Condition> conditions = null)
        {
            Select(new[] { table }, readAction, conditions);
        }

        public void Select(IEnumerable<Table.Table> tables, Action<IDictionary<int, string>, DbDataReader> readAction, IEnumerable<Condition.Condition> conditions = null)
        {
            if (tables != null)
            {
                Dictionary<int, string> fields = new Dictionary<int, string>();
                int i = 0;
                foreach(var table in tables)
                {
                    var tablefields = table.GetSelectFields();
                    foreach (var kvp in tablefields)
                    {
                        fields.Add(i, kvp.Value);
                        i++;
                    }
                }
                Select(tables.Select(x=>x.Name), fields, readAction, conditions);
            }
        }

        public void Select(string tableName, IDictionary<int, string> fields, Action<IDictionary<int, string>, DbDataReader> readAction = null, IEnumerable<Condition.Condition> conditions = null)
        {
            Select(new[] { tableName }, fields, readAction, conditions);
        }

        public void Select(IEnumerable<string> tableNames, IDictionary<int, string> fields, Action<IDictionary<int, string>, DbDataReader> readAction = null, IEnumerable<Condition.Condition> conditions = null)
        {
            if(tableNames != null && tableNames.Any() && tableNames.Any(x=>!string.IsNullOrEmpty(x)))
            {
                using (var db = CreateConnection(_settings))
                {
                    db.Open();
                    try
                    {
                        using (var command = CreateSelectCommand(db, tableNames.Where(x => !string.IsNullOrEmpty(x)).Distinct(), fields, conditions))
                        {
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
        }
        #endregion
        #region count
        public long SelectCount(Type type, LambdaExpression expr)
        {
            return SelectCount(new[] { type }, expr);
        }

        public long SelectCount(IEnumerable<Type> types, LambdaExpression expr)
        {
            return SelectCount(types, DataBase.ParseExpression(expr));
        }

        public long SelectCount(Type type, IEnumerable<Condition.Condition> conditions = null)
        {
            return SelectCount(new[] { type }, conditions);
        }

        public long SelectCount(IEnumerable<Type> types, IEnumerable<Condition.Condition> conditions = null)
        {
            return (types != null) ? SelectCount(types.Select(x=>DataBase.LoadTable(x)), conditions) : 0;
        }

        public long SelectCount(Table.Table table, LambdaExpression expr)
        {
            return SelectCount(new[] { table }, expr);
        }

        public long SelectCount(IEnumerable<Table.Table> tables, LambdaExpression expr)
        {
            return SelectCount(tables, DataBase.ParseExpression(expr));
        }

        public long SelectCount(Table.Table table, IEnumerable<Condition.Condition> conditions = null)
        {
            return SelectCount(new[] { table.Name }, conditions);
        }

        public long SelectCount(IEnumerable<Table.Table> tables, IEnumerable<Condition.Condition> conditions = null)
        {
            return (tables != null) ? SelectCount(tables.Select(x=>x.Name), conditions) : 0;
        }

        public long SelectCount(string tableName, IEnumerable<Condition.Condition> conditions = null)
        {
            return SelectCount(new[] { tableName }, conditions);
        }
        public long SelectCount(IEnumerable<string> tableNames, IEnumerable<Condition.Condition> conditions = null)
        {
            long count = 0;
            if (tableNames != null && tableNames.Any() && tableNames.Any(x => !string.IsNullOrEmpty(x)))
            {
                using (var db = CreateConnection(_settings))
                {
                    db.Open();
                    try
                    {
                        var fields = new Dictionary<int, string>()
                        {
                            { 0, "count(*) as count"}
                        };
                        using (var command = CreateSelectCommand(db, tableNames.Where(x => !string.IsNullOrEmpty(x)).Distinct(), fields, conditions))
                        {
                            var data = command.ExecuteScalar();
                            count = (long)command.ExecuteScalar();
                        }
                    }
                    catch (Exception ex)
                    {
                        InitException(ex);
                    }
                }
            }
            return count;
        }
        #endregion
    }
}
