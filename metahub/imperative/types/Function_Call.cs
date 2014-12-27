using System.Collections.Generic;
using metahub.meta.types;

namespace metahub.imperative.types
{

    public class Function_Call : Expression
    {
        public string name;
        public List<object> args;
        public bool is_platform_specific;

        public Function_Call(string name, List<object> args = null, bool is_platform_specific = false)
            : base(Expression_Type.function_call) {
		this.name = name;
		this.is_platform_specific = is_platform_specific;
		this.args = args ?? new List<object>();
	}
    }

}