using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UtilClasses.Interfaces
{
    public interface IHasNameId : IHasId, IHasName
    {
    }

    public class NameId : IHasNameId
    {
        public string Name { get; set; }

        public long Id { get; set; }
    }


}
