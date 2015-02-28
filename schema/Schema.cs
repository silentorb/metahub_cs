using System;
using System.Collections.Generic;
using System.Linq;
using metahub.logic.schema;
using imperative.schema;

namespace metahub.schema
{
    public class Schema_Source
    {
        public bool? is_external;
        public Dictionary<string, ITrellis_Source> trellises;
        public Dictionary<string, Region_Additional> targets;
    }

    public class Schema
    {
        //public List<Trellis> trellises = new List<Trellis>();
        public Namespace root = new Namespace("root", "root");
        private uint trellis_counter = 1;

        public Schema()
        {

        }

        public Namespace add_namespace(string name)
        {
            if (root.children.ContainsKey(name))
                return root.children[name];

            var space = new Namespace(name, name);
            root.children[name] = space;
            space.parent = root;
            return space;
        }

        Trellis add_trellis(string name, Trellis trellis)
        {
            trellis.id = trellis_counter++;
            //trellises.Add(trellis);
            return trellis;
        }

        public void load_trellises(Dictionary<string, ITrellis_Source> trellises, Load_Settings settings)
        {
            if (settings.space == null)
                settings.space = root;

            var space = settings.space;
            // Due to cross referencing, loading trellises needs to be done in passes
            // First load the core trellises
            Trellis trellis;
            foreach (var name in trellises.Keys)
            {

                var source = trellises[name];
                if (space.trellises.ContainsKey(name))
                {
                    trellis = space.trellises[name];
                }
                else
                {
                    trellis = space.trellises[name] = add_trellis(name, new Trellis(name, this, space));
                }

                trellis.load_properties(source);

                if (settings.auto_identity && source.primary_key == null && source.parent == null)
                {
                    var identity_property = trellis.get_property_or_null("id");
                    if (identity_property == null)
                    {
                        identity_property = trellis.add_property("id", new IProperty_Source { type = "int" });
                    }

                    trellis.identity_property = identity_property;
                }

            }

            // Initialize parents
            foreach (var name in trellises.Keys)
            {
                var source = trellises[name];
                trellis = space.trellises[name];
                trellis.initialize1(source, space);
            }

            // Connect everything together
            foreach (var name in trellises.Keys)
            {
                var source = trellises[name];
                trellis = space.trellises[name];
                trellis.initialize2(source);
            }
        }

        public Trellis get_trellis(string name, Namespace space, bool throw_exception_on_missing = false)
        {
            if (name.Contains("."))
            {
                var path = name.Split('.').ToList();
                name = path.Last();
                path.RemoveAt(path.Count - 1);
                space = space.get_namespace(path);
            }

            if (space == null)
                throw new Exception("Could not find space for trellis: " + name + ".");

            if (!space.trellises.ContainsKey(name))
            {
                if (!throw_exception_on_missing)
                    return null;

                throw new Exception("Could not find trellis named: " + name + ".");
            }
            return space.trellises[name];
        }


        public Trellis resolve_rail_path(IEnumerable<string> path)
        {
            var tokens = path.Take(path.Count() - 1);
            var rail_name = path.Last();
            var region = root;
            foreach (var token in tokens)
            {
                if (!region.children.ContainsKey(token))
                    throw new Exception("Namespace " + region.name + " does not have space: " + token + ".");

                region = region.children[token];
            }

            if (!region.trellises.ContainsKey(rail_name))
                throw new Exception("Namespace " + region.name + " does not have a rail named " + rail_name + ".");

            return region.trellises[rail_name];
        }
    }
}