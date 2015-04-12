using System;
using System.Collections.Generic;

using System.Text;

namespace UnityFramework.Misc.Pool
{
    public interface IPoolable : IDisposable
    {
        void Reset();
    }
}
