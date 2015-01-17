using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using metahub.imperative.schema;

namespace metahub.imperative.summoner
{
    partial class Summoner
    {
        public class Context
        {
            public Realm realm;
            public Dungeon dungeon;
            public Scope scope;
            public Context parent;
            public Dictionary<string, string> inserts = new Dictionary<string, string>();

            public Context(Realm realm, Dungeon dungeon = null)
            {
                this.realm = realm;
                this.dungeon = dungeon;
            }

            public Context(Context parent)
            {
                this.parent = parent;
                realm = parent.realm;
                dungeon = parent.dungeon;
                scope = parent.scope;
            }
        }
    }
}
