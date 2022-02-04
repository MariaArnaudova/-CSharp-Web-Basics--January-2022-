using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BasicWebServer.Server.Common
{
    public static class Guard
    {
        // Accept a value and optionally a name. If the value is null, an exception will be thrown.
        public static void AgainstNull(object value, string name = null)
        {

            if (value == null)
            {
                name ??= "value";
                throw new ArgumentException($"Value {value} cannot be null.");
            };

        }
    }
}
