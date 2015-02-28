using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using metahub.logic.schema;

namespace metahub.schema
{

    public class IProperty_Source
    {
        public string type;
        public string trellis;
        public string other_property;
        public string other_type;
        public bool? multiple;
        public bool? allow_null;

        [JsonProperty("default")]
        public object default_value;
    }

    public class Property
    {
        public string name;
        public Kind type;
        public object default_value;
        public bool allow_null;
        public Trellis trellis;
        public int id;
        public Trellis other_trellis;
        public Property other_property;
        public bool multiple = false;
        public bool is_value = false;

        public Property(string name, IProperty_Source source, Trellis trellis)
        {
            type = (Kind)Enum.Parse(typeof(Kind), source.type, true);
            if (source.default_value != null)
                default_value = source.default_value;

            if (source.allow_null.HasValue)
                allow_null = source.allow_null.Value;

            if (source.multiple.HasValue)
                multiple = source.multiple.Value;

            this.name = name;
            this.trellis = trellis;
        }

        
        public Property(string name, Trellis trellis, Kind type, Trellis other_trellis = null)
        {
            this.trellis = trellis;
            this.type = type;
            this.name = name;
            this.other_trellis = other_trellis;
        }


        //public Void add_dependency (Property_Reference other) {
        //this.dependencies.Add(other);
        ////other.dependents.Add(new Property_Reference(this));
        //}

        public string fullname
        {
            get { return trellis.name + "." + name; }
        }

        public object get_default()
        {
            if (default_value != null)
                return default_value;

            switch (type)
            {
                case Kind.Int:
                    return 0;

                case Kind.Float:
                    return 0;

                case Kind.String:
                    return "";

                case Kind.Bool:
                    return false;

                default:
                    return null;

            }
        }

        public Signature get_signature()
        {
            return new Signature
            {
                type = type,
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
        public void initialize_link1(IProperty_Source source)
        {
            if (source.type != "list" && source.type != "reference")
                return;

            other_trellis = trellis.space.get_trellis(source.trellis, trellis.space);
            if (other_trellis == null)
                throw new Exception("Could not find other trellis for " + fullname + ".");
        }

        public void initialize_link2(IProperty_Source source)
        {
            if (source.type != "list" && source.type != "reference")
                return;

            if (source.other_property != null)
            {
                other_property = other_trellis.get_property(source.other_property);
                if (other_property == null)
                {
                    other_property = other_trellis.add_property(source.other_property, new IProperty_Source
                    {
                        type = source.other_type ?? "reference",
                        trellis = trellis.name,
                        other_property = name
                    });
                    other_property.other_trellis = trellis.get_real();
                    other_property.other_property = this;
                    other_trellis.all_properties[other_property.name] = other_property;
                }
            }
            else
            {
                var other_properties = other_trellis.core_properties.Values.Where(p => p.other_trellis == trellis).ToArray();
                //        throw new Exception("Could not find other property for " + this.trellis.name + "." + this.name + ".");

                if (other_properties.Count() > 1)
                {
                    throw new Exception("Multiple ambiguous other properties for " + trellis.name + "." + name + ".");
                    //        var direct = Lambda.filter(other_properties, (p)=>{ return p.other_property})
                }
                else if (other_properties.Count() == 1)
                {
                    other_property = other_properties.First();
                    other_property.other_trellis = trellis.get_real();
                    other_property.other_property = this;
                }
                else
                {
                    if (!other_trellis.is_value)
                    {
                        if (other_trellis.space.is_external)
                            return;

                        throw new Exception("Could not find other property for " + fullname);
                    }
                }
            }
        }

        //public void get_signature () {
        //Type_Signature result = new Type_Signature(type, other_trellis);
        //
        ////if (other_trellis != null && other_trellis.is_value)
        ////result.is_numeric = other_trellis.is_numeric;
        //
        //return result;
        //}

    }
}