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
            var summoner = new Rune_Summoner();
            Assert.Greater(summoner.lexer.whispers.Length, 5);
        }
    }
}
