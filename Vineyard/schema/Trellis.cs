using System;
using System.Collections.Generic;
using System.Linq;

namespace metahub.schema
{
    public class ITrellis_Source
    {
        public string name;
        public Dictionary<string, IProperty_Source> properties;
        public string parent;
        public string primary_key;
        public bool? is_value;
        public bool? is_abstract;
        public bool? is_interface = false;
        public List<string> interfaces = new List<string>(); 
    }

    public class Trellis
    {
        public string name;
        public Dictionary<string, Property> core_properties = new Dictionary<string, Property>();
        public Dictionary<string, Property> all_properties = new Dictionary<string, Property>();
        public Trellis parent;
        public uint id;
        public Property identity_property;
        public Schema space;
//        public List<Property> properties = new List<Property>();
        public bool is_value = false;
        public List<string> events;
        public bool is_numeric = false;
        List<Trellis> _tree = null;
        public bool is_abstract = false;
        protected Trellis implementation;
        public bool is_interface = false;
        public List<Trellis> interfaces = new List<Trellis>();
        public object default_value = null;
        public bool needs_tick = false;

        public Trellis(string name, Schema space)
        {
            this.name = name;
            this.space = space;
            space.trellises[name] = this;
        }

        public Property add_property(string property_name, IProperty_Source source)
        {
            Property property = new Property(property_name, source, this);
            this.all_properties[property_name] = property;
            property.id = 10000;
            core_properties[property_name] = property;
            return property;
        }
        
//        public Dictionary<string, Property> get_all_properties()
//        {
//            return property_keys;
//            //Dictionary<string, Property> result = new Dictionary<string, Property>();
//            //var tree = this.get_tree();
//            //int index = 0;
//            //foreach (var trellis in tree) {
//            //foreach (var property in trellis.core_properties) {
//            //result[property.name] = property;
//            //property.id = index++;
//            //}
//            //}
//            //return result;
//        }

        public Property get_property(string name)
        {
            var props = all_properties;
            return !props.ContainsKey(name) ? null : props[name];
        }

        public Property get_property_or_error(string name)
        {
            var props = all_properties;
            if (!props.ContainsKey(name))
                throw new Exception(this.name + " does not contain a property named " + name + ".");

            return props[name];
        }

        public Property get_property_or_null(string name)
        {
            var properties = this.all_properties;
            if (!properties.ContainsKey(name))
                return null;

            return properties[name];
        }

        public Property get_reference(Trellis rail)
        {
            return all_properties.Values.FirstOrDefault(t => t.other_trellis == rail);
        }

        public List<Trellis> get_tree()
        {
            if (_tree == null)
            {
                var trellis = this;
                _tree = new List<Trellis>();

                do
                {
                    _tree.Insert(0, trellis);
                    trellis = trellis.parent;
                }
                while (trellis != null);
            }

            return _tree;
        }

        public bool is_a(Trellis trellis)
        {
            var current = this;

            do
            {
                if (current == trellis)
                    return true;

                current = current.parent;
            }
            while (current != null);

            return false;
        }

        public Trellis get_real()
        {
            return implementation ?? this;
        }

        public void load_properties(ITrellis_Source source)
        {
            if (source.properties == null)
                return;

            foreach (var item in source.properties)
            {
                add_property(item.Key, item.Value);
            }
        }

        public void initialize1(ITrellis_Source source, Schema space)
        {
            if (source.is_abstract.HasValue)
                is_abstract = source.is_abstract.Value;

            if (source.is_interface.HasValue)
                is_interface = source.is_interface.Value;

            if (source.is_value.HasValue)
                is_value = source.is_value.Value;

            if (source.parent != null)
            {
                var trellis = this.space.get_trellis(source.parent, space);
                set_parent(trellis);
            }
            if (source.primary_key != null)
            {
                var primary_key = source.primary_key;
                if (core_properties.ContainsKey(primary_key))
                {
                    identity_property = core_properties[primary_key];
                }
            }
            else
            {
                if (parent != null)
                {
                    identity_property = parent.identity_property;
                }
                else if (core_properties.ContainsKey("id"))
                {
                    identity_property = core_properties["id"];
                }
            }

            var tree = get_tree();
            int index = 0;
            foreach (var trellis in tree)
            {
                foreach (var property in trellis.core_properties)
                {
//                    property.id = index++;
//                    properties.Add(property);
//                    all_properties[property.name] = property;
                }
            }

            if (source.properties != null)
            {
                foreach (var j in source.properties.Keys)
                {
                    Property property = this.get_property(j);
                    property.initialize_link1(source.properties[j]);
                }
            }
        }

        public void initialize2(ITrellis_Source source)
        {
            if (!source.is_value.HasValue && parent != null)
                is_value = parent.is_value;

            if (source.properties != null)
            {
                foreach (var j in source.properties.Keys)
                {
                    Property property = get_property(j);
                    property.initialize_link2(source.properties[j]);
                }
            }

            is_numeric = core_properties.Values.Any(p => p.type != Kind.Float && p.type != Kind.Int);

            if (source.interfaces != null)
            {
                interfaces = source.interfaces.Select(i => space.get_trellis(i, space, true)).ToList();
            }
        }

        void set_parent(Trellis parent)
        {
            this.parent = parent;
            if (parent.is_abstract && !is_abstract && is_value)
            {
                parent.implementation = this;
                //foreach (var child_space in schema.root_namespace.children)
                //{
                //    for
                //}

                foreach (var property in parent.core_properties.Values)
                {
                    if (property.other_property != null)
                    {
                        property.other_property.other_trellis = this;
                        property.other_property.other_property = get_property_or_null(property.name);
                    }
                }
            }

            //    if (!parent.identity)
            //      throw new Exception(new Error(parent.name + " needs a primary key when being inherited by " + this.name + "."));
            //
            //    this.identity = parent.identity.map((x) => x.clone(this))
        }

        public string to_string()
        {
            return name;
        }
    }
}