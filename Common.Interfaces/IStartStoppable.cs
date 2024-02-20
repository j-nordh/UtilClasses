using System;
using System.Collections.Generic;
using System.Text;

namespace Common.Interfaces
{
    public interface IStartStoppable
    {
        void Start();
        void Stop(bool wait);
    }
}
