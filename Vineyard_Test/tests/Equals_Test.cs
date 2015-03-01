using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using metahub.logic;
using metahub.logic.nodes;
using test.meta.fixtures;
using vineyard.transform;

namespace test.meta
{
    [TestFixture]
    public class Equals_Test
    {
        [Test]
        public void test_equals()
        {
            var logician = Vineyard_Fixture.simple_equation();
            var pumpkin = logician.functions[0];
            var damage = (Property_Node) pumpkin.inputs[0];
            var multiply = (Function_Node)pumpkin.inputs[1];
            var strength = (Property_Node)pumpkin.inputs[1].inputs[0];

            Assert.AreEqual("=", pumpkin.name);
            Assert.AreEqual(2, pumpkin.inputs.Count);
            Assert.AreEqual("*", multiply.name);
            Assert.AreEqual("damage", damage.property.name);
            Assert.AreEqual("strength", strength.property.name);
        }

        [Test]
        public void test_transform()
        {
            var logician = Vineyard_Fixture.simple_equation();
            var pumpkin = logician.functions[0];
            var strength = (Property_Node)pumpkin.inputs[1].inputs[0];

            var transform = Transform.center_on(strength);
            var pumpkin2 = (Function_Node) transform.get_transformed(pumpkin);
            var division = (Function_Node)pumpkin2.inputs[1];
            var strength2 = (Property_Node)pumpkin2.inputs[0];

            Assert.AreEqual("=", pumpkin2.name);
            Assert.AreEqual("/", division.name);
            Assert.AreEqual("strength", strength2.property.name);

        }

        [Test]
        public void test_context_transform()
        {
            var logician = Vineyard_Fixture.simple_equation();
            var pumpkin = logician.functions[0];
            var strength = (Property_Node)pumpkin.inputs[1].inputs[0];
            var race = (Property_Node)pumpkin.aggregate(Dir.In).OfType<Property_Node>()
                .First(n => n.property.name == "race");

            Transform.change_context(pumpkin, race);
//            var pathfinder = new Pathfinder(logician.schema.get_trellis("Character"));

//            pathfinder.get_pair()
        }
    }
}
