using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace test
{
    public class Pizza
    {
        public List<string> toppings = new List<string>();

        public void add(string topping)
        {
            if (topping == null)
                return;

            toppings.Add(topping);
        }
    }
}
