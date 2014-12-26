using System;
using System.Collections.Generic;

namespace metahub.schema
{
    public class ITrellis_Source
    {
        public string name;
        //Dictionary properties<string; Dictionary<string, object>>;
        public object properties;
        public string parent;
        public string primary_key;
        public bool? is_value;
    }

    //struct Identity UInt;
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

        public Trellis(string name, Schema schema, Namespace space)
        {
            this.name = name;
            this.schema = schema;
            this.space = space;
            space.trellises[name] = this;
        }

        public Property add_property(string name, Dictionary<string, object> source)
        {
            Property property = new Property(name, source, this);
            this.property_keys[name] = property;
            property.id = 10000;
            core_properties.Add(property);
            return property;
        }

        public void copy_identity(object source, object target)
        {
            var identity_key = identity_property.name;
            Reflect.setField(target, identity_key, source[identity_key]);
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

        public void get_identity(object seed)
        {
            if (identity_property == null)
                throw new Exception("This trellis does not have an identity property set.");

            return seed[identity_property.name];
        }

        public Property get_property(string name)
        {
            return property_keys[name];
        }

        public Property get_property_or_error(string name)
        {
            var properties = this.get_all_properties();
            if (!properties.ContainsKey(name))
                throw new Exception(this.name + " does not contain a property named " + name + ".");

            return properties[name];
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

        public void load_properties(Dictionary<string, object> source)
        {
            var properties = source["properties"] as Dictionary<string, Dictionary<string, object>>;
            foreach (var name in properties.Keys)
            {
                add_property(name, properties[name]);
            }
        }

        public void initialize1(ITrellis_Source source, Namespace space)
        {
            //var trellises = this.schema.trellises;
            if (source.parent != null)
            {
                var trellis = this.schema.get_trellis((string)source["parent"], space);
                this.set_parent(trellis);
            }
            if (source["primary_key"] != null)
            {
                var primary_key = (string)source["primary_key"];
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

            var tree = this.get_tree();
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

            foreach (var j in Reflect.fields(source["properties"]))
            {
                Property property = this.get_property(j);
                property.initialize_link1(source["properties"][j]);
            }
        }

        public void initialize2(Dictionary<string, object> source)
        {
            if (source.ContainsKey("is_value"))
                is_value = (bool)source["is_value"];
            else if (parent != null)
            {
                is_value = parent.is_value;
            }

            if (source["properties"] != null)
            {
                var properties = (Dictionary<string, Dictionary<string, object>>)source["properties"];
                foreach (var j in properties.Keys)
                {
                    Property property = this.get_property(j);
                    property.initialize_link2(properties[j]);
                }
            }

            //get_all_properties();

            is_numeric = true;
            foreach (var p in properties)
            {
                if (p.type != Kind.Float && p.type != Kind.Int)
                {
                    is_numeric = false;
                    break;
                }
            }
        }

        void set_parent(Trellis parent)
        {
            this.parent = parent;

            //    if (!parent.identity)
            //      throw new Exception(new Error(parent.name + " needs a primary key when being inherited by " + this.name + "."));
            //
            //    this.identity = parent.identity.map((x) => x.clone(this))
        }

        //public General_Port get_port (int index) {
        //return ports[index];
        //}
        //
        //public int get_port_count () {
        //return ports.Count;
        //}

        public string to_string()
        {
            return name;
        }

        //public Context resolve (Context context) {
        //throw new Exception("Not implemented.");
        //}
        //
        //public void on_create_node (Context context) {
        //foreach (var port in on_create_ports) {
        //port.get_node_value(context);
        //}
        //}

    }
}