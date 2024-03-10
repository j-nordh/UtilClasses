using System;
using System.Collections.Generic;
using System.Text;

namespace UtilClasses.Interfaces
{
    public interface IConfigurable<in TCfg>
    {
        void Configure(TCfg cfg);
    }
    public interface IConfigurable<in TCfg, in TExtra>
    {
        void Configure(TCfg cfg, TExtra extra);
    }
}
