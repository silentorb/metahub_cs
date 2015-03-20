using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using imperative;
using imperative.summoner;
using runic.parser;

namespace imp_test.tests
{
    [TestFixture]
    public class Runic_Test
    {
        [Test]
        public void test()
        {
            var code = Utility.load_resource("imp.pizza.imp");
            var runes = Summoner2.read_runes(code);
            Assert.Greater(runes.Count, 5);

            var legend = Summoner2.translate_runes(runes);
            var overlord = new Overlord();
            var summoner = new Summoner2(overlord);
            summoner.summon((Group_Legend)legend);
        }

        [Test]
        public void test_if()
        {
            var code = Utility.load_resource("imp.if.imp");
            var runes = Summoner2.read_runes(code);
            var legend = Summoner2.translate_runes(runes, "if_statement");
            Assert.AreEqual("return_statement", legend.children[1].children[0].rhyme.name);
        }

        [Test]
        public void test_empty_array()
        {
            var code = Utility.load_resource("imp.empty_array.imp");
            var runes = Summoner2.read_runes(code);
            var legend = Summoner2.translate_runes(runes, "statement");
            Assert.AreEqual("return_statement", legend.children[1].children[0].rhyme.name);
        }
    }
}
