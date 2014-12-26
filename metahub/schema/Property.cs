using System;
using System.Collections.Generic;
using metahub.schema;

namespace metahub.schema {
/*
struct Dictionary<string, object> {
public string type;
public   Object default_value;
public 	bool allow_null;
public 	string trellis;
  public string other_property;
	public string other_type;
	public bool multiple;
}
*/
class Property {
  public string name;
  public Kind type;
  public Object default_value;
  public bool allow_null;
  public Trellis trellis;
  public int id;
  public Trellis other_trellis;
  public Property other_property;
  public bool multiple = false;

  public Property(string name, Dictionary<string, object> source, Trellis trellis) {

    if (source.ContainsKey("default"))
      this.default_value = source["default"];

    if (source.ContainsKey("allow_null"))
      this.allow_null = (bool) source["allow_null"];

		if (source.ContainsKey("multiple"))
			multiple = (bool)source["multiple"];

    this.name = name;
    this.trellis = trellis;
  }

  //public Void add_dependency (Property_Reference other) {
		//this.dependencies.Add(other);
    ////other.dependents.Add(new Property_Reference(this));
	//}

	public string fullname () {
		return trellis.name + "." + name;
	}

  public Object get_default () {
    if (default_value != null)
      return default_value;

    switch (type) {
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

  public void initialize_link1 (Dictionary<string, object> source) {
		if (source.type != "list" && source.type != "reference")
      return;

    this.other_trellis = this.trellis.schema.get_trellis(source.trellis, trellis.space);
			if (this.other_trellis == null)
				throw new Exception("Could not find other trellis for " + fullname() + ".");
	}

  public void initialize_link2 (Dictionary<string, object> source) {
    if (source.type != "list" && source.type != "reference")
      return;

    if (source.other_property != null) {
      other_property = other_trellis.get_property(source.other_property);
			if (other_property == null) {
				other_property = other_trellis.add_property(source.other_property, {
					type: source.other_type,
					trellis: trellis.name,					
					other_property: name
				});
				other_property.other_trellis = trellis;
				other_property.other_property = this;
				other_trellis.properties.Add(other_property);
			}
		}
    else {

      var other_properties = other_trellis.properties.Filter((p)=>{ return p.other_trellis == this.trellis; });
//        throw new Exception("Could not find other property for " + this.trellis.name + "." + this.name + ".");

      if (other_properties.Count() > 1) {
        throw new Exception("Multiple ambiguous other properties for " + this.trellis.name + "." + this.name + ".");
//        var direct = Lambda.filter(other_properties, (p)=>{ return p.other_property})
      }
      else if (other_properties.Count() == 1) {
        this.other_property = other_properties.first();
        this.other_property.other_trellis = this.trellis;
        this.other_property.other_property = this;
      }
			else {
				if (!other_trellis.is_value) {
					if (other_trellis.space.is_external)
						return;
						
					throw new Exception("Could not find other property for " + fullname());
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

}}