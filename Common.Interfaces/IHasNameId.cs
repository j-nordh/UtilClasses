using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Interfaces
{
    public interface IHasNameId :IHasId
    {
        string Name { get; set; }
    }

    public class NameId : IHasNameId
    {
        public string Name { get; set; }

        public long Id {get;set;}
    }


}
