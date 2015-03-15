using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using imperative.summoner;
using interpreter.runic;

namespace imp_test.tests
{
    [TestFixture]
    public class Lexer_Test
    {
        [Test]
        public void test()
        {
            var code = Utility.load_resource("imp.pizza.imp");
            var runes = Summoner.read_runes(code);
            Assert.Greater(runes.Count, 5);
        }
    }
}
