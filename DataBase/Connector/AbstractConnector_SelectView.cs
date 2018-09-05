using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq.Expressions;
using System.Text;

namespace Birko.Data.DataBase.Connector
{
    public abstract partial class AbstractConnector
    {
        public void SelectView(Type type, Action<object> readAction, LambdaExpression expr)
        {
            SelectView(type, readAction, DataBase.ParseExpression(expr));
        }

        public void SelectView(Type type, Action<object> readAction, IEnumerable<Condition.Condition> conditions = null)
        {
            Select(DataBase.LoadView(type), (fields, reader) => {
                if (readAction != null)
                {
                    var data = Activator.CreateInstance(type, new object[0]);
                    DataBase.ReadView(reader, data);
                    readAction(data);
                }
            }, conditions);
        }

        public void Select(Table.View view, Action<IDictionary<int, string>, DbDataReader> readAction, LambdaExpression expr)
        {
            Select(view, readAction, DataBase.ParseExpression(expr));
        }

        public void Select(Table.View view, Action<IDictionary<int, string>, DbDataReader> readAction = null, IEnumerable<Condition.Condition> conditions = null)
        {
            if (view != null)
            {
                using (var db = CreateConnection(_settings))
                {
                    db.Open();
                    try
                    {
                        using (var command = CreateSelectCommand(db, view, conditions))
                        {
                            var reader = command.ExecuteReader();
                            if (reader.HasRows)
                            {
                                bool isNext = reader.Read();
                                while (isNext)
                                {
                                    readAction?.Invoke(view.GetSelectFields(), reader);
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
