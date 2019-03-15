using System;
using System.Collections.Generic;
using System.Text;

namespace Birko.Data.Helper
{
    public static class ObjectHelper
    {
        public static int Compare(IComparable value, IComparable value2)
        {
            if (value == null && value2 == null)
            {
                return 0;
            }
            else if (value != null && value2 == null)
            {
                return 1;
            }
            else if (value == null && value2 != null)
            {
                return -1;
            }
            else
            {
                return value.CompareTo(value2);
            }
        }
    }
}
