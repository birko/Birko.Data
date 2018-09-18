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
            var leftTables = view.Join?.Select(x => x.Left).Distinct().Where(x => !string.IsNullOrEmpty(x)).ToList();
            if (leftTables != null)
            {
                foreach (var tableName in view.Join.Select(x => x.Right).Distinct().Where(x => !string.IsNullOrEmpty(x)))
                {
                    leftTables.Remove(tableName);
                }
            }

            return CreateSelectCommand(db, leftTables ?? view.Tables.Select(x => x.Name), view.GetSelectFields(), view.Join, conditions, view.HasAggregateFields() ? view.GetSelectFields(true) : null);
        }


        public virtual DbCommand CreateSelectCommand(DbConnection db, IEnumerable<string> tableNames, IDictionary<int, string> fields, IEnumerable<Condition.Join> joinconditions = null, IEnumerable<Condition.Condition> conditions = null, IDictionary<int, string> groupFields = null)
        {
            var command = db.CreateCommand();
            command.CommandText = "SELECT " + string.Join(", ", fields.Values) + " FROM ";

            Dictionary<string, List<Condition.Join>> joins = new Dictionary<string, List<Condition.Join>>();
            if (joinconditions != null && joinconditions.Any())
            {
                string prevleft = null;
                string prevright = null;
                foreach (var join in joinconditions)
                {
                    if (!string.IsNullOrEmpty(prevleft) && !string.IsNullOrEmpty(prevright) && !joins.ContainsKey(join.Left) && prevright == join.Left && joins.ContainsKey(prevleft))
                    {
                        joins[prevleft].Add(join);
                    }
                    else
                    {
                        if (!joins.ContainsKey(join.Left))
                        {
                            joins.Add(join.Left, new List<Condition.Join>());
                        }
                        joins[join.Left].Add(join);
                        prevleft = join.Left;
                    }
                    prevright = join.Right;
                }
            }

            int i = 0;
            foreach (var table in tableNames.Distinct())
            {
                if (i > 0)
                {
                    command.CommandText += ", ";
                }
                command.CommandText += table;
                if (joins != null && joins.ContainsKey(table))
                {
                    var joingroups = joins[table].GroupBy(x => new { x.Right, x.JoinType }).ToDictionary(x => x.Key, x => x.SelectMany(y => y.Conditions).Where(z => z != null));
                    foreach (var joingroup in joingroups.Where(x=>x.Value.Any()))
                    {
                        switch (joingroup.Key.JoinType)
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
                        command.CommandText += joingroup.Key.Right;
                        if (joingroup.Key.JoinType != Condition.JoinType.Cross && joingroup.Value != null && joingroup.Value.Any())
                        {
                            command.CommandText += " ON (";
                            command.CommandText += ConditionDefinition(joingroup.Value, command);
                            command.CommandText += ")";
                        }
                    }
                }
            }
            AddWhere(conditions, command);
            if (groupFields != null && groupFields.Any())
            {
                command.CommandText += " GROUP BY " + string.Join(", ", groupFields.Values);
            }
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
                foreach(var table in tables.Where(x=> x != null))
                {
                    var tablefields = table.GetSelectFields();
                    foreach (var kvp in tablefields)
                    {
                        fields.Add(i, kvp.Value);
                        i++;
                    }
                }
                Select(tables.Where(x => x != null).Select(x=>x.Name), fields, readAction, conditions);
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
