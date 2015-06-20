using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using imperative.expressions;
using metahub.logic.nodes;
using metahub_test.fixtures;

namespace metahub_test.tests
{
    [TestFixture]
    public class Swamp_Test
    {
        [Test]
        public void test_pair()
        {
            var swamp = Jack_Fixture.create_simple_swamp();

            var property_nodes = swamp.end.aggregate(Dir.In)
                .OfType<Property_Node>().ToArray();

            var race2 = property_nodes
                .First(n => n.property.name == "race");

            var strength2 = property_nodes
                .First(n => n.property.name == "strength");

            var jack = swamp.jack;
            var pair = swamp.get_expression_pair2(race2, strength2);

            // Should be:
            // race.strength = weapon.damage / 4
            var race = (Portal_Expression)pair[0];
            var strength = (Portal_Expression)race.next;
            var division = (Operation)pair[1];
            var weapon = (Portal_Expression)division.expressions[0];
            var damage = (Portal_Expression)weapon.next;
            var literal = (Literal)division.expressions[1];

            Assert.AreEqual("race", race.portal.name);
            Assert.AreEqual("strength", strength.portal.name);
            Assert.AreEqual("weapon", weapon.portal.name);
            Assert.AreEqual("damage", damage.portal.name);
            Assert.AreEqual(4, literal.value);
            Assert.AreEqual("/", division.op);
        }
    }
}
