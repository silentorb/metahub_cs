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
            protected Dictionary<string, string> string_inserts = new Dictionary<string, string>();
            protected Dictionary<string, Profession> profession_inserts = new Dictionary<string, Profession>();

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

            public Profession add_pattern(string name, Profession profession)
            {
                profession_inserts[name] = profession;
                return profession;
            }

            public string add_pattern(string name, string text)
            {
                string_inserts[name] = text;
                return text;
            }

            public Profession get_profession_pattern(string name)
            {
                if (profession_inserts.ContainsKey(name))
                    return profession_inserts[name];

                if (parent != null)
                    return parent.get_profession_pattern(name);

                return null;
            }

            public string get_string_pattern(string name)
            {
                if (string_inserts.ContainsKey(name))
                    return string_inserts[name];

                if (parent != null)
                    return parent.get_string_pattern(name);

                return null;
            }
        }
    }
}
