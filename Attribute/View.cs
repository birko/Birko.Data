using System;
using System.Collections.Generic;
using System.Text;

namespace Birko.Data.Attribute
{
    [System.AttributeUsage(System.AttributeTargets.Class | System.AttributeTargets.Struct)]
    public class View: System.Attribute
    {
        public string[] Names { get; private set; }

        public View(string[] names)
        {
            Names = names;
        }
    }
}
