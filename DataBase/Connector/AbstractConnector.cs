using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace Birko.Data.DataBase.Connector
{
    public delegate void InitConnector(AbstractConnector connector);
    public delegate void OnException(Exception ex);
    public abstract partial class AbstractConnector
    {
        protected readonly Store.PasswordSettings _settings = null;
        public event InitConnector OnInit;
        public event OnException OnException;

        public bool IsInit { get; private set; } = false;

        public AbstractConnector(Store.PasswordSettings settings)
        {
            _settings = settings;
        }

        public abstract DbConnection CreateConnection(Store.PasswordSettings settings);

        public abstract string ConvertType(DbType type);

        public abstract string FieldDefinition(Field.AbstractField field);

        public virtual void InitException(Exception ex)
        {
            if (OnException != null)
            {
                OnException?.Invoke(ex);
            }
            else
            {
                throw ex;
            }
        }

        public void DoInit()
        {
            if (!IsInit)
            {
                IsInit = true;
                OnInit?.Invoke(this);
                IsInit = false;
            }
        }

        private object EscapeValue(object item)
        {
            if(item is string)
            {
                return (item as string).Replace("'", "''");
            }
            return item;
        }

        public virtual DbCommand AddParameter(DbCommand command, string name, object value)
        {
            var parameter = command.CreateParameter();
            parameter.ParameterName = name;
            parameter.Value = value;
            command.Parameters.Add(parameter);
            return command;
        }

        public virtual string ConditionDefinition(Condition.Condition condition, DbCommand command)
        {
            var result = new StringBuilder();
            if (condition != null)
            {
                if (condition.SubConditions != null && condition.SubConditions.Any())
                {
                    result.Append("(");
                    result.Append(ConditionDefinition(condition.SubConditions, command));
                    result.Append(")");
                }
                else if (!string.IsNullOrEmpty(condition.Name))
                {
                    result.Append(condition.Name);
                    switch (condition.Type)
                    {
                        case Condition.ConditionType.IsNull:
                            if (condition.IsNot)
                            {
                                result.Append(" NOT ");
                            }
                            result.Append(" IS NULL");
                            break;
                        case Condition.ConditionType.Less:
                            if (condition.IsNot)
                            {
                                result.Append(" >= ");
                            }
                            else
                            {
                                result.Append(" < ");
                            }
                            break;
                        case Condition.ConditionType.Greather:
                            if (condition.IsNot)
                            {
                                result.Append(" <= ");
                            }
                            else
                            {
                                result.Append(" > ");
                            }
                            break;
                        case Condition.ConditionType.LessAndEqual:
                            if (condition.IsNot)
                            {
                                result.Append(" > ");
                            }
                            else
                            {
                                result.Append(" <= ");
                            }
                            break;
                        case Condition.ConditionType.GreatherAndEqual:
                            if (condition.IsNot)
                            {
                                result.Append(" < ");
                            }
                            else
                            {
                                result.Append(" >= ");
                            }
                            break;
                        case Condition.ConditionType.Like:
                        case Condition.ConditionType.EndsWith:
                        case Condition.ConditionType.StartsWith:
                            if (condition.IsNot)
                            {
                                result.Append(" NOT ");
                            }
                            result.Append(" LIKE ");
                            break;
                        case Condition.ConditionType.In:
                            if (condition.IsNot)
                            {
                                result.Append(" NOT ");
                            }
                            result.Append(" IN ");
                            break;
                        case Condition.ConditionType.Equal:
                        default:
                            if (condition.IsNot)
                            {
                                result.Append(" <> ");
                            }
                            else
                            {
                                result.Append(" = ");
                            }
                            break;
                    }
                    if (condition.Type != Condition.ConditionType.IsNull)
                    {
                        if (condition.Type == Condition.ConditionType.In)
                        {
                            int i = 0;
                            result.Append("(");
                            foreach (var item in condition.Values)
                            {
                                if (i > 0)
                                {
                                    result.Append(", ");
                                }
                                result.Append("@WHERE" + condition.Name + i);
                                i++;
                            }
                            result.Append(")");
                        }
                        else if (condition.Values != null)
                        {
                            var enumerator = condition.Values.GetEnumerator();
                            if (enumerator.MoveNext())
                            {
                                var first = enumerator.Current;
                                if (first != null)
                                {
                                    result.Append((condition.IsField) ? first : "@WHERE" + condition.Name);
                                }
                            }
                        }
                        if (!condition.IsField)
                        {
                            if (condition.Type == Condition.ConditionType.In)
                            {
                                int i = 0;
                                foreach (var item in condition.Values)
                                {
                                    var value  = EscapeValue(item);
                                    AddParameter(command, "@WHERE" + condition.Name + i, value);
                                    i++;
                                }
                            }
                            else if (condition.Values != null)
                            {
                                var enumerator = condition.Values.GetEnumerator();
                                if (enumerator.MoveNext())
                                {
                                    var first = enumerator.Current;
                                    {
                                        object value = null;
                                        switch (condition.Type)
                                        {
                                            case Condition.ConditionType.StartsWith:
                                                value = "%" + (first as string);
                                                break;
                                            case Condition.ConditionType.Like:
                                                value = "%" + (first as string) + "%";
                                                break;
                                            case Condition.ConditionType.EndsWith:
                                                value = (first as string) + "%";
                                                break;
                                            default:
                                                value = first;
                                                break;
                                        }
                                        value = EscapeValue(value);
                                        AddParameter(command, "@WHERE" + condition.Name, value);
                                    }
                                }
                            }
                        }
                    }
                }
            }
            return result.ToString();
        }

        public virtual string ConditionDefinition(IEnumerable<Condition.Condition> conditions, DbCommand command)
        {
            var result = new StringBuilder();
            if (conditions != null && conditions.Any())
            {
                int i = 0;
                foreach (var condition in conditions)
                {
                    if (i > 0)
                    {
                        if (condition.IsOr)
                        {
                            result.Append(" OR ");
                        }
                        else
                        {
                            result.Append(" AND ");
                        }
                    }
                    result.Append(ConditionDefinition(condition, command));
                    i++;
                }
            }
            return result.ToString();
        }

        public virtual DbCommand AddWhere(IEnumerable<Condition.Condition> conditions, DbCommand command)
        {
            if (command != null && conditions != null && conditions.Any())
            {
                command.CommandText += " WHERE ";
                command.CommandText += ConditionDefinition(conditions, command);
            }
            return command;
        }
    }
}
