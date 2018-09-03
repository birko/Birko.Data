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
                    if (condition.IsNot)
                    {
                        result.Append(" NOT ");
                    }
                    switch (condition.Type)
                    {
                        case Condition.ConditionType.IsNull:
                            result.Append(" IS NULL");
                            break;
                        case Condition.ConditionType.Less:
                            result.Append(" < "); break;
                        case Condition.ConditionType.Greather:

                            result.Append(" > ");
                            break;
                        case Condition.ConditionType.LessAndEqual:
                            result.Append(" <= ");
                            break;
                        case Condition.ConditionType.GreatherAndEqual:
                            result.Append(" >= ");
                            break;
                        case Condition.ConditionType.Like:
                        case Condition.ConditionType.EndsWith:
                        case Condition.ConditionType.StartsWith:
                            result.Append(" LIKE ");
                            break;
                        case Condition.ConditionType.In:
                            result.Append(" IN ");
                            break;
                        case Condition.ConditionType.Equal:
                        default:
                            result.Append(" = ");
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
                        switch (condition.Type)
                        {
                            case Condition.ConditionType.Like: result.Append("%"); break;
                            case Condition.ConditionType.EndsWith: result.Append("%"); break;
                        }
                        if (!condition.IsField)
                        {
                            if (condition.Type == Condition.ConditionType.In)
                            {
                                int i = 0;
                                foreach (var item in condition.Values)
                                {
                                    var parameter = command.CreateParameter();
                                    parameter.ParameterName = "@WHERE" + condition.Name + i;
                                    parameter.Value = item;
                                    command.Parameters.Add(parameter);
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
                                        var parameter = command.CreateParameter();
                                        parameter.ParameterName = "@WHERE" + condition.Name;
                                        switch (condition.Type)
                                        {
                                            case Condition.ConditionType.StartsWith:
                                                parameter.Value = "%" + (first as string);
                                                break;
                                            case Condition.ConditionType.Like:
                                                parameter.Value = "%" + (first as string) + "%";
                                                break;
                                            case Condition.ConditionType.EndsWith:
                                                parameter.Value = (first as string) + "%";
                                                break;
                                            default:
                                                parameter.Value = first;
                                                break;
                                        }
                                        command.Parameters.Add(parameter);
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
