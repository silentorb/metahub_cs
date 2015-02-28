using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace metahub.logic.schema
{
    public class Constraint_Group
    {
        //public bool is_animated = false;
        //public bool is_active = true;
        public string name;
        public List<Constraint> constraints = new List<Constraint>();

        public Constraint_Group(string name)
        {
            this.name = name;
        }
    }
}
