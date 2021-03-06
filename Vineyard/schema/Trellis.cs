using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace metahub.schema
{
    public delegate void Trellis_Property_Event(Trellis trellis, Property property);

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

    [DebuggerDisplay("Trellis({name})")]
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
        public event Trellis_Property_Event on_add_property;

        public Trellis(string name, Schema space)
        {
            this.name = name;
            this.space = space;
            space.trellises[name] = this;
        }

        public Property add_property(string property_name, IProperty_Source source)
        {
            Property property = new Property(property_name, source, this);
            add_property(property);
            return property;
        }

        public Property add_property(Property property)
        {
            all_properties[property.name] = property;
            core_properties[property.name] = property;
            property.trellis = this;

            if (on_add_property != null)
                on_add_property(this, property);

            return property;
        }

        public Property get_property(string name)
        {
            return !all_properties.ContainsKey(name) ? null : all_properties[name];
        }

        public Property get_property_or_error(string name)
        {
            if (!all_properties.ContainsKey(name))
                throw new Exception(this.name + " does not contain a property named " + name + ".");

            return all_properties[name];
        }

        public Property get_property_or_null(string name)
        {
            if (!all_properties.ContainsKey(name))
                return null;

            return all_properties[name];
        }

        public Property get_reference(Trellis rail)
        {
            return all_properties.Values.First(t => t.other_trellis == rail);
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
                var trellis = this.space.get_trellis(source.parent);
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

//            var tree = get_tree();
//            int index = 0;
//            foreach (var trellis in tree)
//            {
//                foreach (var property in trellis.core_properties)
//                {
////                    property.id = index++;
////                    properties.Add(property);
////                    all_properties[property.name] = property;
//                }
//            }

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
                interfaces = source.interfaces.Select(i => space.get_trellis(i, true)).ToList();
            }
        }

        void set_parent(Trellis new_parent)
        {
            this.parent = new_parent;
            if (new_parent.is_abstract && !is_abstract && is_value)
            {
                new_parent.implementation = this;
                //foreach (var child_space in schema.root_namespace.children)
                //{
                //    for
                //}

                foreach (var property in new_parent.core_properties.Values)
                {
                    if (property.other_property != null)
                    {
                        property.other_property.other_trellis = this;
                        property.other_property.other_property = get_property_or_null(property.name);
                    }
                }
            }
            else
            {
                foreach (var property in new_parent.all_properties.Values)
                {
                    add_property(property.clone());
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

        public string get_available_name(string key, int start = 0)
        {
            var result = "";
            do
            {
                result = key;
                if (start != 0)
                    result += start;

                ++start;
            } while (all_properties.ContainsKey(result));

            return result;
        }
    }
}