using System;
using System.Collections.Generic;
using System.Text;

namespace Birko.Data.Model
{
    public interface ICopyable<T>
    {
        T CopyTo(T clone);
    }
}
