using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace metahub.render
{
    public delegate string String_Delegate();

    public static class String_Extensions
    {
        public static string join(this object[] array, string seperator)
        {
            return string.Join(seperator, array);
        }

        public static string join<T>(this IEnumerable<T> array, string seperator)
        {
            return string.Join(seperator, array);
        }
    }

}
