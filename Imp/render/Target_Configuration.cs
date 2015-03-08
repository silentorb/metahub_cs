using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace imperative.render
{
    public class Target_Configuration
    {
        public string statement_terminator = "";    // In most cases this will be "" or ";"
        public string namespace_separator = ".";    // Some languages use ::
        public string path_separator = ".";         // Some languages use ->
        public string base_type = "";               // Haven't decided what Imp's base type will be called yet.
        public bool uses_var = true;                // Whether the language requires/supports the "var" keyword for declaring variables.
        public bool supports_namespaces = true;     // True for most of Imp's targets.
    }
}
