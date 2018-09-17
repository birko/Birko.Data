using Birko.Data.DataBase.Condition;
using Birko.Data.DataBase.Field;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;

namespace Birko.Data.DataBase
{
    public static partial class DataBase
    {
        private static Dictionary<Type, Table.View> _viewCache = null;

        public static IEnumerable<Table.View> LoadViews(IEnumerable<Type> types)
        {
            if (types != null && types.Any())
            {
                List<Table.View> tables = new List<Table.View>();
                foreach (Type type in types)
                {
                    var table = LoadView(type);
                    if (table != null && table.Tables != null && table.Tables.Any() && table.Tables.Any(x => x.Fields != null && x.Fields.Any()))
                    {
                        tables.Add(table);
                    }
                }
                return tables.ToArray();
            }
            else
            {
                throw new Exceptions.TableAttributeException("Types enumerable is empty ot null");
            }
        }

        public static Table.View LoadView(Type type)
        {
            if (_viewCache == null)
            {
                _viewCache = new Dictionary<Type, Table.View>();
            }
            if (!_viewCache.ContainsKey(type))
            {
                object[] attrs = type.GetCustomAttributes(typeof(Attribute.View), true).ToArray();
                if (attrs != null)
                {
                    Table.View view = new Table.View();
                    foreach (Attribute.View attr in attrs)
                    {
                        var tableLeft = attr.ModelLeft != null ? LoadTable(attr.ModelLeft) : null;
                        var tableRight = attr.ModelRight != null ? LoadTable(attr.ModelRight) : null;
                        if (tableLeft != null && tableRight != null)
                        {
                            var fieldLeft = (attr.ModelProperyLeft is string) ? tableLeft.GetFieldByPropertyName((string)attr.ModelProperyLeft) : null;
                            var fieldRight = (attr.ModelProperyRight is string) ? tableRight?.GetFieldByPropertyName((string)attr.ModelProperyRight) ?? null : null;
                            if (fieldLeft != null || fieldRight != null)
                            {
                                var joinType = JoinType.Cross;
                                switch (attr.Connect)
                                {
                                    case Attribute.ViewConnect.Check: joinType = JoinType.LeftOuter;  break;
                                    case Attribute.ViewConnect.CheckExisting: joinType = JoinType.Inner; break;
                                }

                                Condition.Condition cond = null;
                                if (fieldLeft != null && fieldRight != null)
                                {
                                    cond = Condition.Condition.AndField(tableLeft.Name + "." + fieldLeft.Name, tableRight.Name + "." + fieldRight.Name);
                                }
                                else if (fieldLeft != null && fieldRight == null)
                                {
                                    cond = Condition.Condition.AndValue(tableLeft.Name + "." + fieldLeft.Name, attr.ModelProperyRight);
                                }
                                else if (fieldLeft == null && fieldRight != null)
                                {
                                    cond = Condition.Condition.AndValue(tableRight.Name + "." + fieldRight.Name, attr.ModelProperyLeft);
                                }
                                if (cond != null)
                                {
                                    view.AddJoin(Join.Create(tableLeft.Name, tableRight.Name, cond, joinType));
                                }
                            }
                        }
                        foreach (var field in type.GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance))
                        {
                            object[] fieldAttrs = field.GetCustomAttributes(typeof(Attribute.ViewField), true);
                            if (fieldAttrs != null && fieldAttrs.Any())
                            {
                                foreach (Attribute.ViewField fieldAttr in fieldAttrs)
                                {
                                    var table = LoadTable(fieldAttr.ModelType);
                                    if (table != null)
                                    {
                                        var tablefield = table.GetFieldByPropertyName(fieldAttr.ModelProperyName);
                                        if (tablefield != null)
                                        {
                                            if (fieldAttr is Attribute.AggregateField)
                                            {
                                                var functionField = FunctionField.CreateFunctionAggregateField(field, (Attribute.AggregateField)fieldAttr, tablefield);
                                                if (functionField != null)
                                                {
                                                    view.AddField(table.Name, functionField, tablefield.Name + functionField.Name);
                                                }
                                            }
                                            else
                                            {
                                                view.AddField(table.Name, tablefield);
                                            }
                                        }
                                    }
                                }
                            }
                            else
                            {
                                throw new Exceptions.TableAttributeException("No field attributes in type");
                            }
                        }
                    }
                    _viewCache.Add(type, view);
                }
                else
                {
                    throw new Exceptions.TableAttributeException("No view attributes in type");
                }
            }
            return _viewCache[type];
        }

        public static int ReadView(DbDataReader reader, object data, int index = 0)
        {
            var type = data.GetType();
            List<Field.AbstractField> tableFields = new List<Field.AbstractField>();
            foreach (var field in type.GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance))
            {
                object[] fieldAttrs = field.GetCustomAttributes(typeof(Attribute.ViewField), true);
                if (fieldAttrs != null)
                {
                    foreach (Attribute.ViewField fieldAttr in fieldAttrs)
                    {
                        string name = !string.IsNullOrEmpty(fieldAttr.ModelProperyName) ? fieldAttr.ModelProperyName : field.Name;
                        if (_fieldsCache.ContainsKey(fieldAttr.ModelType) && _fieldsCache[type].Any(x => x.Property?.Name == name))
                        {
                            tableFields.Add(_fieldsCache[type].FirstOrDefault(x => x.Property?.Name == name));
                        }
                        else
                        {
                            tableFields.Add(new StringField(field, fieldAttr.Name));
                        }
                    }
                }
            }
            return Read(tableFields, reader, data, index);
        }
    }
}
