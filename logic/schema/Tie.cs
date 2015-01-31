using System;
using System.Collections.Generic;
using System.Linq;
using metahub.imperative.code;
using metahub.logic.nodes;
using metahub.render;
using metahub.schema;

namespace metahub.logic.schema
{
    public class Tie_Hooks
    {
        public object set_post;
    }

    public class Tie_Addition
    {
        public Tie_Hooks hooks;
    }

    public class Tie
    {

        public Rail rail;
        public Property property;
        public string name;
        public string tie_name;
        public Rail other_rail;
        public Tie other_tie;
        public bool is_value = false;
        public bool has_getter = false;
        public bool has_set_post_hook = false;
        public Kind type;
        public List<IRange> ranges = new List<IRange>();

        public List<Constraint> constraints = new List<Constraint>();

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

            if (rail.property_additional.ContainsKey(name))
            {
                var additional = rail.property_additional[name];
                if (additional.hooks != null)
                {
                    has_set_post_hook = additional.hooks.set_post != null;
                }
            }
        }

        public bool has_setter()
        {
            return (property.type != Kind.list && constraints.Count > 0)
            || has_set_post_hook || (property.type == Kind.reference && !is_inherited());
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
            determine_range();
        }



        class Temp
        {
            public Node min;
            public Node max;
            public List<Tie> path;
        }


        void determine_range()
        {
            //if (type != Kind.Float)
            //return;
            if (type == Kind.list)
                return;

            var pairs = new Dictionary<string, Temp>();
            //Constraint min = null;
            //Constraint max = null;

            foreach (var constraint in constraints)
            {
                var path = constraint.first.get_path().Where(t=>t as Property_Reference != null)
                    .Select(t => ((Property_Reference)t).tie).ToList();
                path.RemoveAt(0);
                var path_name = path.Select(t => t.name).join(".");
                if (!pairs.ContainsKey(path_name))
                {
                    pairs[path_name] = new Temp
                    {
                        min = null,
                        max = null,
                        path = path
                    };
                }

                if (constraint.op == ">=<")
                {
                    var args = (Array_Expression)constraint.second;
                    pairs[path_name].min = args.children[0];
                    pairs[path_name].max = args.children[1];
                }
                else if (constraint.op == ">" || constraint.op == ">=")
                {
                    pairs[path_name].min = constraint.second;
                }
                else if (constraint.op == "<" || constraint.op == "<=")
                {
                    pairs[path_name].max = constraint.second;
                }
            }

            foreach (var pair in pairs.Values)
            {
                if (pair.min != null && pair.max != null)
                {
                    //trace("range", fullname());
                    ranges.Add(new Range_Float(
                        get_expression_float(pair.min.get_path().Last()),
                        get_expression_float(pair.max.get_path().Last()), pair.path));
                }
            }
        }

        static float get_expression_float(Node expression)
        {
            return ((Literal_Value)expression).get_float();
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