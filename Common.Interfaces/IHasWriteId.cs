using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Interfaces
{

    public interface IHasWriteId:IHasId
    {
        new long Id { get;  set; }
    }
}
