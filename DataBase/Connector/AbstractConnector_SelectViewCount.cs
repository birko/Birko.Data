﻿using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace Birko.Data.DataBase.Connector
{
    public abstract partial class AbstractConnector
    {
        public long SelectCountView(Type type, LambdaExpression expr)
        {
            return SelectCountView(type, DataBase.ParseExpression(expr));
        }

        public long SelectCountView(Type type, IEnumerable<Condition.Condition> conditions = null)
        {
            return SelectCount(DataBase.LoadView(type), conditions);
        }

        public long SelectCount(Table.View view, LambdaExpression expr)
        {
            return SelectCount(view, DataBase.ParseExpression(expr));
        }

        public long SelectCount(Table.View view, IEnumerable<Condition.Condition> conditions = null)
        {
            return SelectCount(view.Tables.Select(x => x.Name), view.Join, conditions);
        }
    }
}
