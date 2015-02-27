using System;
using System.Collections.Generic;
using System.Linq;
using imperative.code;
using metahub.logic.nodes;
using metahub.render;
using metahub.schema;

namespace metahub.logic.schema
{

    public class Tie
    {
        public Rail rail;
        public Property property;
        public string name;
        public string tie_name;
        public Rail other_rail;
        public Tie other_tie;
        public bool is_value = false;
//        public bool has_getter = false;
//        public bool has_set_post_hook = false;
        public Kind type;
//        public List<IRange> ranges = new List<IRange>();

        public string fullname 
        {
            get { return rail.name + "." + name; }
        }

        public Tie(Rail rail, Property property)
        {
            this.rail = rail;
            this.type = property.type;
            this.property = property;
            tie_name = name = property.name;
        }

        public Tie(string name, Rail rail, Kind type, Rail other_rail = null)
        {
            this.rail = rail;
            this.type = type;
            this.name = tie_name = name;
            this.other_rail = other_rail;
        }

        public void initialize_links()
        {
            if (property.other_trellis != null)
            {
                other_rail = rail.railway.get_rail(property.other_trellis);
                is_value = property.other_trellis.is_value;
                if (other_rail != null && property.other_property != null && other_rail.all_ties.ContainsKey(property.other_property.name))
                {
                    var other_ties = other_rail.all_ties.Values;
                    other_tie = other_ties.FirstOrDefault(t=>t.other_rail == rail)
                        ?? other_rail.all_ties[property.other_property.name];
                    //other_tie.other_rail = rail;
                    //other_tie.other_tie = this;
                }
            }
        }

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
                rail = other_rail,
                is_value = is_value
            };
        }

        public Signature get_other_signature()
        {
            if (other_rail == null)
                throw new Exception("get_other_signature() can only be called on lists or references.");

            var other_type = other_tie != null
            ? other_tie.type
            : type == Kind.list ? Kind.reference : Kind.list;

            return new Signature
            {
                type = type == Kind.list && other_type == Kind.list ? Kind.reference : other_type,
                rail = other_rail,
                is_value = is_value
            };
        }

        public bool is_inherited()
        {
            return rail.parent != null && rail.parent.all_ties.ContainsKey(name);
        }

        public void finalize()
        {
        }

        class Temp
        {
            public Node min;
            public Node max;
            public List<Tie> path;
        }

        public Rail get_abstract_rail()
        {
            return rail;
        }

        public object get_default_value()
        {
            if (other_rail != null && other_rail.default_value != null)
                return other_rail.default_value;

            return property.get_default();
        }
    }
}