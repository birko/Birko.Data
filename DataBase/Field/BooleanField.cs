﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Text;

namespace Birko.Data.DataBase.Field
{
    public class BooleanField : AbstractField
    {
        public BooleanField(System.Reflection.PropertyInfo property, string name, bool primary = false, bool unique = false)
            : base(property, name, DbType.Boolean, primary, true, unique)
        {
        }

        public override void Read(object value, DbDataReader reader, int index)
        {
            Property.SetValue(value, reader.GetBoolean(index), null);
        }
    }

    public class NullableBooleanField : BooleanField
    {
        public NullableBooleanField(System.Reflection.PropertyInfo property, string name, bool primary = false, bool unique = false)
            : base(property, name, primary, unique)
        {
            IsNotNull = false;
        }

        public override void Read(object value, DbDataReader reader, int index)
        {
            if (reader.IsDBNull(index))
            {
                Property.SetValue(value, null, null);
            }
            else
            {
                base.Read(value, reader, index);
            }
        }
    }

    public class BooleanFunction : FunctionField
    {
        public BooleanFunction(System.Reflection.PropertyInfo property, string name, object[] parameters)
            : base(property, name, parameters, DbType.Boolean, true)
        {

        }

        public override void Read(object value, DbDataReader reader, int index)
        {
            Property.SetValue(value, reader.GetBoolean(index), null);
        }
    }

    public class NullableBooleanFunction : BooleanFunction
    {
        public NullableBooleanFunction(System.Reflection.PropertyInfo property, string name, object[] parameters)
            : base(property, name, parameters)
        {
            IsNotNull = false;
        }

        public override void Read(object value, DbDataReader reader, int index)
        {
            if (reader.IsDBNull(index))
            {
                Property.SetValue(value, null, null);
            }
            else
            {
                base.Read(value, reader, index);
            }
        }
    }
}
