using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Common.Interfaces
{
    public interface IPausable
    {
        Task Pause(CancellationToken ct);
        bool Paused { get; }
        void Resume();
    }
}
