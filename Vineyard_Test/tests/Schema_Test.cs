using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using test.meta.fixtures;

namespace test.meta
{
    [TestFixture]
    public class Schema_Test
    {
        [Test]
        public void test_loading()
        {
            var schema = Vineyard_Fixture.load_schema();
            Assert.Greater(schema.trellises.Count, 1);
            var character = schema.get_trellis("Character");
            var item = schema.get_trellis("Item");
            Assert.Greater(character.all_properties.Count, 1);
            Assert.Greater(character.core_properties.Count, 1);
            var character_property = item.all_properties["character"];
            Assert.AreEqual(character_property, character.all_properties["items"].other_property);


            var weapon = schema.get_trellis("Weapon");
            var weapon_character = weapon.all_properties["character"];
            Assert.AreEqual(weapon, weapon_character.trellis);
        }
    }
}
