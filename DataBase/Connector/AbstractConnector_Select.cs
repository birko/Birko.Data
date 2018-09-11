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
        public virtual DbCommand CreateSelectCommand(DbConnection db, IEnumerable<string> tableNames, IDictionary<int, string> fields, IEnumerable<Condition.Condition> conditions = null)
        {
            return CreateSelectCommand(db, tableNames, fields, null, conditions);
        }

        public virtual DbCommand CreateSelectCommand(DbConnection db, Table.View view, IEnumerable<Condition.Condition> conditions = null)
        {
            return CreateSelectCommand(db, view.Tables.Select(x => x.Name), view.GetSelectFields(), view.Join, conditions);
        }

        public virtual DbCommand CreateSelectCommand(DbConnection db, IEnumerable<string> tableNames, IDictionary<int, string> fields, IEnumerable<Condition.Join> joinconditions = null, IEnumerable<Condition.Condition> conditions = null)
        {
            var command = db.CreateCommand();
            command.CommandText = "SELECT " + string.Join(", ", fields.Values) + " FROM ";

            var joins = (joinconditions != null && joinconditions.Any())
                ? joinconditions.Where(x => !string.IsNullOrEmpty(x.Right)).Where(x => x != null).GroupBy(x => x.Left).ToDictionary(x => x.Key, x => x.AsEnumerable())
                : new Dictionary<string, IEnumerable<Condition.Join>>();

            int i = 0;
            foreach (var table in tableNames)
            {
                if (i > 0)
                {
                    command.CommandText += ", ";
                }
                command.CommandText += table;
                if (joins.ContainsKey(table))
                {
                    foreach (var join in joins[table])
                    {
                        switch (join.JoinType)
                        {
                            case Condition.JoinType.Inner:
                                command.CommandText += " INNER JOIN ";
                                break;
                            case Condition.JoinType.LeftOuter:
                                command.CommandText += " LEFT OUTER JOIN ";
                                break;
                            case Condition.JoinType.Cross:
                            default:
                                command.CommandText += " CROSS JOIN ";
                                break;
                        }
                        if (join.JoinType != Condition.JoinType.Cross && join.Conditions != null && join.Conditions.Any())
                        {
                            command.CommandText += " ON (";
                            command.CommandText += ConditionDefinition(conditions, command);
                            command.CommandText += ")";
                        }
                    }
                }
            }
            AddWhere(conditions, command);
            return command;
        }

        public void Select(Type type, Action<object> resultAction, LambdaExpression expr)
        {
            Select(type, resultAction, (expr != null) ? DataBase.ParseExpression(expr) : null);
        }

        public void Select(Type[] types, Action<IEnumerable<object>> resultAction, LambdaExpression expr)
        {
            Select(types, resultAction, (expr != null) ? DataBase.ParseExpression(expr) : null);
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
                        fields.Add(i, table.Name + "." + kvp.Value);
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
    }
}
