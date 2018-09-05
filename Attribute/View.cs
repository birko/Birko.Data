using System;
using System.Collections.Generic;
using System.Text;

namespace Birko.Data.Attribute
{
    [System.AttributeUsage(System.AttributeTargets.Class | System.AttributeTargets.Struct, AllowMultiple = true)]
    public class View: System.Attribute
    {
        public Type ModelLeft { get; private set; }
        public Type ModelRight { get; private set; }
        public string ModelProperyLeft { get; private set; }
        public string ModelProperyRight { get; private set; }
        public string Name { get; internal set; }

        public View(Type modelLeft, Type modelRight, string modelProperyLeft, string modelProperyRight, string name = null)
        {
            ModelLeft = modelLeft;
            ModelRight = modelRight;
            ModelProperyLeft = modelProperyLeft;
            ModelProperyRight = modelProperyRight;
            Name = name;
        }
    }
}
