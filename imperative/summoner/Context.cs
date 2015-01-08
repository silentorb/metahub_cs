using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using metahub.imperative.schema;

namespace metahub.imperative.summoner
{
    partial class Summoner
    {
        private class Context
        {
            public Realm realm;
            public Dungeon dungeon;

            public Context(Realm realm, Dungeon dungeon = null)
            {
                this.realm = realm;
                this.dungeon = dungeon;
            }
        }
    }
}
