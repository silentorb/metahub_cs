using System;
using System.Collections.Generic;

namespace metahub.parser
{
public class Bootstrap : Context {

//  public Bootstrap() { }

    public class Boot
    {
        public string type;
        public string text;
        public string name;
        public Boot[] patterns;
        public Boot pattern;
        public Boot divider;
        public object value;
        public int? min;
        public int? max;
    }

    public Bootstrap(Definition definition)
        : base(definition)
    {
    }


    public override object perform_action (string name, object data, Match match) {
    if (name == null)
      return data;

    switch(name) {

      case "group":
        return @group((object[])data);
      case "and_group":
        return and_group(data);
      case "or":
        return or_group(data);
      case "literal":
        return literal((object[])data);
      case "pattern":
        return pattern((object[])data, match);
      case "start":
        return start((Boot[])data);
      case "repetition":
        return repetition((object[])data);
      case "reference":
        return reference(data);
      case "regex":
        return regex((object[])data);
      case "rule":
        return rule((object[])data);

      default:
        throw new Exception("Invalid parser method: " + name + ".");
    }
  }

    static object literal (object[] data) {
//    trace("data", data);
    return data[1];
  }

    static object regex (object[] data) {
//    trace("data", data);
    return new Boot {
    type = "regex",
    text = (string)data[1]
    };
  }

    static object reference (object data) {
    return new Boot {
    type = "reference",
    name = (string)data
    };
  }

    static object and_group (object data) {
    return new Boot {
    type = "and",
    patterns = (Boot[])data
    };
  }

    static object group (object[] data) {
//  trace("group", data);
    return data[2];
  }

    static object or_group (object data) {
      return new Boot
      {
    type = "or",
    patterns = (Boot[])data
    };
  }

    static object pattern (object[] data, Match match) {
//    trace("pattern:", data);

    if (data.Length == 0)
      return null;
    else if (data.Length == 1)
      return data[0];
    else
        return new Boot
        {
      type = "and",
      patterns = (Boot[])data
      };
  }

    static object repetition (object[] data) {
//    trace("rule", data);
    var settings = (string[])data[1];
    var result = new Boot {
			type = "repetition",
			pattern= new Boot {
				type = "reference",
				name = settings[0]
			},
			divider = new Boot {
				type = "reference",
				name = settings[1]
			}
    };

    if (settings.Length > 2) {
      result.min = int.Parse(settings[2]);
      if (settings.Length > 3) {
        result.max = int.Parse(settings[3]);
      }
    }
    return result;
  }

    static object rule (object[] data) {
//    trace("rule", data);
    var value = (object[])data[4];
    return new Boot {
    name = (string)data[0],
    value = value != null && value.Length == 1 ? value[0] : value
    };
  }

  object start (IEnumerable<Boot> data)
  {
      var map = new Dictionary<string, object>();
//    trace("start", data);

      foreach (var item in data)
      {
      map[item.name] = item.value;
//      map.setField(item.name, item.value);
    }
//    trace("data", data);
    return map; //haxe.Json.parse(haxe.Json.stringify(map);
  }
}
}