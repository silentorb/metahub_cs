using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using metahub.imperative.schema;

namespace metahub.jackolantern.code
{
    public class Pickaxe
    {
        public Dwarf dwarf;
        public Minion minion;
        public Dungeon dungeon { get { return dwarf.dungeon; } }
        public List<Ration> rations = new List<Ration>();

        public Pickaxe(Dwarf dwarf, Minion minion)
        {
            this.dwarf = dwarf;
            this.minion = minion;
        }
    }
}
