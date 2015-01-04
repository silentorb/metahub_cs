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
    }

    public class Trellis
    {
        public string name;
        public Schema schema;
        public List<Property> core_properties = new List<Property>();
        List<Property> all_properties;
        public Trellis parent;
        public uint id;
        public Property identity_property;
        public Namespace space;
        Dictionary<string, Property> property_keys = new Dictionary<string, Property>();
        public List<Property> properties = new List<Property>();
        public bool is_value = false;
        public List<string> events;
        public bool is_numeric = false;
        List<Trellis> _tree = null;
        public bool is_abstract = false;
        protected Trellis implementation;

        public Trellis(string name, Schema schema, Namespace space)
        {
            this.name = name;
            this.schema = schema;
            this.space = space;
            space.trellises[name] = this;
        }

        public Property add_property(string property_name, IProperty_Source source)
        {
            Property property = new Property(property_name, source, this);
            this.property_keys[property_name] = property;
            property.id = 10000;
            core_properties.Add(property);
            return property;
        }
        
        public Dictionary<string, Property> get_all_properties()
        {
            return property_keys;
            //Dictionary<string, Property> result = new Dictionary<string, Property>();
            //var tree = this.get_tree();
            //int index = 0;
            //foreach (var trellis in tree) {
            //foreach (var property in trellis.core_properties) {
            //result[property.name] = property;
            //property.id = index++;
            //}
            //}
            //return result;
        }

        public Property get_property(string name)
        {
            var props = get_all_properties();
            return !props.ContainsKey(name) ? null : props[name];
        }

        public Property get_property_or_error(string name)
        {
            var props = get_all_properties();
            if (!props.ContainsKey(name))
                throw new Exception(this.name + " does not contain a property named " + name + ".");

            return props[name];
        }

        public Property get_property_or_null(string name)
        {
            var properties = this.get_all_properties();
            if (!properties.ContainsKey(name))
                return null;

            return properties[name];
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

        public void initialize1(ITrellis_Source source, Namespace space)
        {
            if (source.is_abstract.HasValue)
                is_abstract = source.is_abstract.Value;

            if (source.is_value.HasValue)
                is_value = source.is_value.Value;

            if (source.parent != null)
            {
                var trellis = this.schema.get_trellis(source.parent, space);
                set_parent(trellis);
            }
            if (source.primary_key != null)
            {
                var primary_key = source.primary_key;
                if (property_keys.ContainsKey(primary_key))
                {
                    identity_property = property_keys[primary_key];
                }
            }
            else
            {
                if (parent != null)
                {
                    identity_property = parent.identity_property;
                }
                else if (property_keys.ContainsKey("id"))
                {
                    identity_property = property_keys["id"];
                }
            }

            var tree = get_tree();
            int index = 0;
            foreach (var trellis in tree)
            {
                foreach (var property in trellis.core_properties)
                {
                    property.id = index++;
                    properties.Add(property);
                    property_keys[property.name] = property;
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

            is_numeric = properties.Any(p => p.type != Kind.Float && p.type != Kind.Int);
        }

        void set_parent(Trellis parent)
        {
            this.parent = parent;
            if (parent.is_abstract && !is_abstract)
            {
                parent.implementation = this;
                //foreach (var child_space in schema.root_namespace.children)
                //{
                //    for
                //}

                foreach (var property in parent.properties)
                {
                    if (property.other_property != null)
                    {
                        property.other_property.other_trellis = this;
                        property.other_property.other_property = get_property_or_error(property.name);
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