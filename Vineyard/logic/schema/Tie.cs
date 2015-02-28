using System;
using System.Collections.Generic;
using System.Linq;
using metahub.logic.nodes;
using metahub.logic.schema;
/*
namespace metahub.schema
{

    public class Property2
    {
        public Trellis trellis;
        public Property property;
        public string name;
        public string tie_name;
        public Trellis other_trellis;
        public Property other_property;
        public bool is_value = false;
//        public bool has_getter = false;
//        public bool has_set_post_hook = false;
        public Kind type;
//        public List<IRange> ranges = new List<IRange>();

        public string fullname 
        {
            get { return trellis.name + "." + name; }
        }

        public Property2(Trellis trellis, Property property)
        {
            this.trellis = trellis;
            this.type = property.type;
            this.property = property;
            tie_name = name = property.name;
        }

        public Property2(string name, Trellis trellis, Kind type, Trellis other_trellis = null)
        {
            this.trellis = trellis;
            this.type = type;
            this.name = tie_name = name;
            this.other_trellis = other_trellis;
        }

//        public void initialize_links()
//        {
//            if (property.other_trellis != null)
//            {
//                other_trellis = trellis.railway.get_rail(property.other_trellis);
//                is_value = property.other_trellis.is_value;
//                if (other_trellis != null && property.other_property != null && other_trellis.all_properties.ContainsKey(property.other_property.name))
//                {
//                    var other_ties = other_trellis.all_properties.Values;
//                    other_property = other_ties.FirstOrDefault(t=>t.other_trellis == trellis)
//                        ?? other_trellis.all_properties[property.other_property.name];
//                    //other_tie.other_rail = rail;
//                    //other_tie.other_tie = this;
//                }
//            }
//        }

        public bool has_setter()
        {
            return (property.type != Kind.list)
            || (property.type == Kind.reference && !is_inherited());
        }

        public string get_setter_post_name()
        {
            return "set_" + name + "_post";
        }

        public Signature get_signature()
        {
            return new Signature
            {
                type = property.type,
                rail = other_trellis,
                is_value = is_value
            };
        }

        public Signature get_other_signature()
        {
            if (other_trellis == null)
                throw new Exception("get_other_signature() can only be called on lists or references.");

            var other_type = other_property != null
            ? other_property.type
            : type == Kind.list ? Kind.reference : Kind.list;

            return new Signature
            {
                type = type == Kind.list && other_type == Kind.list ? Kind.reference : other_type,
                rail = other_trellis,
                is_value = is_value
            };
        }

        public bool is_inherited()
        {
            return trellis.parent != null && trellis.parent.all_properties.ContainsKey(name);
        }

        public void finalize()
        {
        }

        class Temp
        {
            public Node min;
            public Node max;
            public List<Property> path;
        }

        public Trellis get_abstract_rail()
        {
            return trellis;
        }

        public object get_default_value()
        {
            if (other_trellis != null && other_trellis.default_value != null)
                return other_trellis.default_value;

            return property.get_default();
        }
    }
 
}*/