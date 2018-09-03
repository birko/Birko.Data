using Birko.Data.DataBase.Condition;
using Birko.Data.DataBase.Connector;
using Birko.Data.DataBase.Field;
using Birko.Data.Model;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace Birko.Data.DataBase
{
    public static class DataBase
    {
        private static Dictionary<Type, Dictionary<string, AbstractConnector>> _connectors = null;
        private static Dictionary<Type, Table.Table> _tableCache = null;
        private static Dictionary<Type, IEnumerable<Field.AbstractField>> _fieldsCache = null;

        public static AbstractConnector GetConnector<T>(Store.Settings settings) where T: AbstractConnector
        {
            if (_connectors == null)
            {
                _connectors = new Dictionary<Type, Dictionary<string, AbstractConnector>>();
            }
            if (!_connectors.ContainsKey(typeof(T)))
            {
                _connectors.Add(typeof(T), new Dictionary<string, AbstractConnector>());
            }
            if (!_connectors[typeof(T)].ContainsKey(settings.GetId()))
            {
                _connectors[typeof(T)].Add(settings.GetId(), (AbstractConnector)Activator.CreateInstance(typeof(T), new object[] { settings }));
            }
            return _connectors[typeof(T)][settings.GetId()];
        }

        #region Attributes
        public static string GetGeneratedQuery(DbCommand dbCommand)
        {
            var query = dbCommand.CommandText;
            foreach (DbParameter parameter in dbCommand.Parameters)
            {
                query = query.Replace(parameter.ParameterName, parameter.Value.ToString());
            }

            return query;
        }

        public static IEnumerable<Table.Table> LoadTables(IEnumerable<Type> types)
        {
            if (types != null && types.Any())
            {
                List<Table.Table> tables = new List<Table.Table>();
                foreach (Type type in types)
                {
                    var table  = LoadTable(type);
                    if (table != null && table.Fields != null && table.Fields.Any())
                    {
                        tables.Add(table);
                    }
                }
                return tables.ToArray();
            }
            else
            {
                throw new Exceptions.TableAttributeException("Types enum is empty ot null");
            }
        }

        public static Table.Table LoadTable(Type type)
        {
            if (_tableCache == null)
            {
                _tableCache = new Dictionary<Type, Table.Table>();
            }
            if (!_tableCache.ContainsKey(type))
            {
                object[] attrs = type.GetCustomAttributes(typeof(Attribute.Table), true);
                if (attrs != null)
                {
                    foreach (Attribute.Table attr in attrs)
                    {
                        Table.Table table = new Table.Table()
                        {
                            Name = attr.Name,
                            Fields = LoadFields(type).ToDictionary(x => x.Name),
                        };
                        if (table.Fields != null && table.Fields.Any())
                        {
                            _tableCache.Add(type, table);
                            return table;
                        }
                    }
                    return null;
                }
                else
                {
                    throw new Exceptions.TableAttributeException("No table attributes in type");
                }
            }
            return _tableCache[type];
        }

        public static IEnumerable<AbstractField> GetPrimaryFields(Type type)
        {
            var table = LoadTable(type);
            return table?.GetPrimaryFields() ?? new AbstractField[0];
        }

        public static Condition.Condition CreateCondition(AbstractField field, object value)
        {
            return new Condition.Condition(field.Name, new[] { field.Property.GetValue(value, null) });
        }

        private static IEnumerable<AbstractField> LoadFields(Type type)
        {
            if (_fieldsCache == null)
            {
                _fieldsCache = new Dictionary<Type, IEnumerable<Field.AbstractField>>();
            }
            if (!_fieldsCache.ContainsKey(type))
            {
                List<Field.AbstractField> list = new List<Field.AbstractField>();
                foreach (var field in type.GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance))
                {
                    object[] fieldAttrs = field.GetCustomAttributes(typeof(Attribute.Field), true);
                    if (fieldAttrs != null)
                    {
                        foreach (Attribute.Field fieldAttr in fieldAttrs)
                        {
                            string name = !string.IsNullOrEmpty(fieldAttr.Name) ? fieldAttr.Name : field.Name;
                            var tableField = Field.AbstractField.FromAttribute(field, fieldAttr);

                            if (tableField != null)
                            {
                                list.Add(tableField);
                            }
                        }
                    }
                    else
                    {
                        throw new Exceptions.FieldAttributeException("No field attributes in type");
                    }
                }
                _fieldsCache.Add(type, list.ToArray());
            }
            return _fieldsCache[type];
        }

        public static Dictionary<string, object> Write(object data)
        {
            var type = data.GetType();
            List<Field.AbstractField> tableFields = new List<Field.AbstractField>();
            foreach (var field in type.GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance))
            {
                object[] fieldAttrs = field.GetCustomAttributes(typeof(Attribute.Field), true);
                if (fieldAttrs != null)
                {
                    foreach (Attribute.Field fieldAttr in fieldAttrs)
                    {
                        tableFields.Add(Field.AbstractField.FromAttribute(field, fieldAttr));
                    }
                }
            }
            return Write(tableFields, data);
        }

        public static Dictionary<string, object> Write(IEnumerable<Field.AbstractField> fields, object data)
        {
            Dictionary<string, object> result = new Dictionary<string, object>();
            if (fields != null)
            {
                foreach (var tableField in fields)
                {
                    result.Add(tableField.Name, tableField.Write(data));
                }
            }
            return result;
        }

        public static int Read(DbDataReader reader, object data, int index = 0)
        {
            var type = data.GetType();
            List<Field.AbstractField> tableFields = new List<Field.AbstractField>();
            foreach (var field in type.GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance))
            {
                object[] fieldAttrs = field.GetCustomAttributes(typeof(Attribute.Field), true);
                if (fieldAttrs != null)
                {
                    foreach (Attribute.Field fieldAttr in fieldAttrs)
                    {
                        tableFields.Add(Field.AbstractField.FromAttribute(field, fieldAttr));
                    }
                }
            }
            return Read(tableFields, reader, data, index);
        }

        public static int Read(IEnumerable<Field.AbstractField> fields, DbDataReader reader, object data, int index = 0)
        {
            if (fields != null)
            {
                foreach (var tableField in fields)
                {
                    tableField.Read(data, reader, index);
                    index++;
                }
            }
            return index;
        }

        public static IEnumerable<Condition.Condition> ParseExpression(Expression expr, Condition.Condition parent = null)
        {
            if (expr != null)
            {
                if (expr.NodeType == ExpressionType.Lambda)
                {
                    LambdaExpression lambda = (LambdaExpression)expr;
                    return ParseExpression(lambda.Body);
                }
                var basecondition = new Condition.Condition(null, null);
                if (expr is BinaryExpression)
                {
                    var binnary = expr as BinaryExpression;
                    if (expr.NodeType == ExpressionType.AndAlso || expr.NodeType == ExpressionType.OrElse)
                    {

                        var listSub = new List<Condition.Condition>();
                        listSub.AddRange(ParseExpression(binnary.Left));
                        var rightConds = ParseExpression(binnary.Right);
                        if (rightConds != null && rightConds.Any())
                        {
                            foreach (var right in rightConds)
                            {
                                right.IsOr = expr.NodeType == ExpressionType.OrElse;
                                listSub.Add(right);
                            }
                        }
                        basecondition.SubConditions = listSub.AsEnumerable();
                        return basecondition.SubConditions;
                    }
                    else if (
                        expr.NodeType == ExpressionType.Equal
                        || expr.NodeType == ExpressionType.LessThan
                        || expr.NodeType == ExpressionType.LessThanOrEqual
                        || expr.NodeType == ExpressionType.GreaterThan
                        || expr.NodeType == ExpressionType.GreaterThanOrEqual
                        || expr.NodeType == ExpressionType.Not
                        || expr.NodeType == ExpressionType.NotEqual
                    )
                    {
                        if (expr.NodeType == ExpressionType.Equal)
                        {
                            basecondition.Type = ConditionType.Equal;
                        }
                        if (expr.NodeType == ExpressionType.NotEqual)
                        {
                            basecondition.Type = ConditionType.Equal;
                            basecondition.IsNot = true;
                        }
                        if (expr.NodeType == ExpressionType.LessThan)
                        {
                            basecondition.Type = ConditionType.Less;
                        }
                        if (expr.NodeType == ExpressionType.LessThanOrEqual)
                        {
                            basecondition.Type = ConditionType.LessAndEqual;
                        }
                        if (expr.NodeType == ExpressionType.GreaterThan)
                        {
                            basecondition.Type = ConditionType.Greather;
                        }
                        if (expr.NodeType == ExpressionType.GreaterThanOrEqual)
                        {
                            basecondition.Type = ConditionType.GreatherAndEqual;
                        }
                        ParseExpression(binnary.Left, basecondition);
                        ParseExpression(binnary.Right, basecondition);
                        return new[] { basecondition };
                    }
                }
                else if (parent != null)
                {
                    if (expr.NodeType == ExpressionType.NewArrayBounds)
                    {
                        var array = (expr as NewArrayExpression);
                    }
                    if (expr.NodeType == ExpressionType.NewArrayInit)
                    {
                        var array = (expr as NewArrayExpression);
                        foreach (var arg in array.Expressions)
                        {
                            ParseExpression(arg, parent);
                        }
                    }
                    if (expr.NodeType == ExpressionType.Constant)
                    {
                        List<object> vals = new List<object>();
                        var f = Expression.Lambda(expr).Compile();
                        var value = f.DynamicInvoke();
                        var valueType = value.GetType();
                        if (valueType.IsPrimitive || valueType == typeof(string))
                        {
                            vals.Add(value);
                        }
                        else if (valueType.IsArray)
                        {
                            foreach (var item in (Array)value)
                            {
                                vals.Add(item);
                            }
                        }
                        else
                        {
                            var fields = valueType.GetFields();
                            if (fields.Any())
                            {
                                foreach (var field in fields)
                                {
                                    vals.Add(field.GetValue(value));
                                }
                            }
                        }
                        if (parent.Values != null)
                        {
                            foreach (var o in parent.Values)
                            {
                                vals.Add(o);
                            }
                        }
                        parent.Values = vals.ToArray();
                    }
                    if (expr.NodeType == ExpressionType.MemberAccess)
                    {
                        var member = (expr as MemberExpression);
                        if (member.Expression?.NodeType == ExpressionType.Parameter)
                        {
                            var fields = LoadFields(member.Member.ReflectedType);
                            var field = fields.FirstOrDefault(x => x.Property.Name == member.Member.Name);
                            parent.Name = field?.Name;
                        }
                        else
                        {
                            ParseExpression(member.Expression, parent);
                        }
                    }
                    //like
                    //in
                    //starts with
                    // ends with
                }
                else if (expr.NodeType == ExpressionType.Call)
                {
                    var member = (expr as MethodCallExpression);
                    if (member.Method.Name == "StartsWith")
                    {
                        basecondition.Type = ConditionType.StartsWith;
                    }
                    if (member.Method.Name == "EndsWith")
                    {
                        basecondition.Type = ConditionType.EndsWith;
                    }
                    if (member.Method.Name == "Contains")
                    {
                        if (member.Method.DeclaringType.Name == "String")
                        {
                            basecondition.Type = ConditionType.Like;
                        }
                        else
                        {
                            basecondition.Type = ConditionType.In;
                        }
                    }
                    if (member.Arguments != null && member.Arguments.Any())
                    {
                        foreach (var arg in member.Arguments)
                        {
                            ParseExpression(arg, basecondition);
                        }
                    }
                    if (member.Object != null)
                    {
                        ParseExpression(member.Object, basecondition);
                    }
                    return new[] { basecondition };
                }
            }
            return new Condition.Condition[0];
        }
        #endregion
    }
}
