using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using metahub.logic;
using metahub.logic.nodes;
using vineyard.transform;
using vineyard_test.fixtures;

namespace vineyard_test.tests
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

            var transform = new Transform(strength).center_on(strength);
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
            var strength = pumpkin.aggregate(Dir.In).OfType<Property_Node>()
                .First(n => n.property.name == "strength");

            var transform = new Transform(pumpkin);
            transform.change_context(strength);
            pumpkin = (Function_Node)transform.new_origin;
            var strength2 = (Property_Node)pumpkin.inputs[1].inputs[0];
            var character = (Property_Node)pumpkin.inputs[0].inputs[0].inputs[0];
            Assert.AreEqual("strength", strength2.property.name);
            Assert.AreEqual("character", character.property.name);
            Assert.AreEqual("Race", ((Scope_Node)character.inputs[0]).trellis.name);
        }

        [Test]
        public void test_dual_transform()
        {
            var logician = Vineyard_Fixture.simple_equation();
            var pumpkin = logician.functions[0];
            var property_nodes = pumpkin.aggregate(Dir.In)
                .OfType<Property_Node>().ToArray();
            var race = property_nodes
                .First(n => n.property.name == "race");
            var strength = property_nodes
                .First(n => n.property.name == "strength");

            var transform = new Transform(pumpkin).change_context(race);
            transform.center_on(strength);
            pumpkin = (Function_Node)transform.get_transformed(pumpkin);

            var strength2 = (Property_Node) pumpkin.inputs[0];
            var race2 = (Property_Node)pumpkin.inputs[0].inputs[0];
            var damage = (Property_Node)pumpkin.inputs[1].inputs[0];
            var weapon = (Property_Node)pumpkin.inputs[1].inputs[0].inputs[0];
            var character1 = (Scope_Node)pumpkin.inputs[0].inputs[0].inputs[0];
            var character2 = (Scope_Node)pumpkin.inputs[1].inputs[0].inputs[0].inputs[0];

            Assert.AreEqual("strength", strength2.property.name);
            Assert.AreEqual("race", race2.property.name);
            Assert.AreEqual("damage", damage.property.name);
            Assert.AreEqual("weapon", weapon.property.name);
            Assert.AreSame(character1, character2);
            Assert.AreEqual("Character", character1.trellis.name);
            var division = (Function_Node)pumpkin.inputs[1];
            Assert.AreEqual("/", division.name);

        }
    }
}
