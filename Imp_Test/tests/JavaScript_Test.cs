using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using imp_test.fixtures;
using metahub.render.targets.js;

namespace imp_test.tests
{
    [TestFixture]
    public class JavaScript_Test
    {
        [Test]
        public void test_simple()
        {
            var target = new Js_Target();
            var overlord = Imp_Fixture.create_overlord(target, "simple.imp");
            var output = target.generate();
            var goal = Utility.load_resource("js.simple.js");
//            var diff = get_first_diff(goal, output);
//            if (diff == -1)
//                Assert.AreEqual(-1, diff);
            
            Assert.AreEqual(goal, output);
        }

//        public int get_first_diff(string a, string b)
//        {
//            const int equalsReturnCode = -1;
//            if (String.IsNullOrEmpty(a) || String.IsNullOrEmpty(b))
//            {
//                return equalsReturnCode;
//            }
//
//            string longest = b.Length > a.Length ? b : a;
//            string shorten = b.Length > a.Length ? a : b;
//            for (int i = 0; i < shorten.Length; i++)
//            {
//                if (shorten[i] != longest[i])
//                {
//                    return i;
//                }
//            }
//
//            if (a.Length != b.Length)
//            {
//                return shorten.Length;
//            }
//
//            return equalsReturnCode;
//        }
    }
}
