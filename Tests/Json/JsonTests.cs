using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UtilClasses.Json;
using Xunit;
using Xunit.Abstractions;

namespace UtilClasses.Tests.Json
{
    public class JsonTests
    {
        ITestOutputHelper _output;
        public JsonTests(ITestOutputHelper output)
        {
            _output = output;
        }
        
        [Fact]
        public void Bcc()
        {
            JsonUtil.Settings.Default()
                .SetBcc<IPrimate, PrimateType>(x => x
                    .With<Chimp>(PrimateType.Chimp)
                    .With<Gorilla>(PrimateType.Gorilla));

            var json = JsonUtil.Serialize(new Chimp());
            var o = JsonUtil.Get<IPrimate>(json);
            Assert.IsType<Chimp>(o);
            Assert.IsNotType<Gorilla>(o);
        }
        [Fact]
        public void BccDouble()
        {
            JsonUtil
                .SetBcc<IPrimate, PrimateType>(x => x // this is wrong
                    .With<Gorilla>(PrimateType.Chimp)
                    .With<Chimp>(PrimateType.Gorilla))
                .SetBcc<IPrimate, PrimateType>(x => x //that's better
                    .With<Chimp>(PrimateType.Chimp)
                    .With<Gorilla>(PrimateType.Gorilla));

            var json = JsonUtil.Serialize(new Chimp());
            var o = JsonUtil.Get<IPrimate>(json);
            Assert.IsType<Chimp>(o);
            Assert.IsNotType<Gorilla>(o);
        }
    }
}
