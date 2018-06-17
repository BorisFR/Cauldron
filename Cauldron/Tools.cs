// /*
// Author: sfardoux
// Create: 14/06/2018
// */

using System.IO;
using System.Linq;
using System.Reflection;

namespace Cauldron
{
    public static class Tools
    {
        private static readonly Assembly assembly;
        private static readonly string[] resources;

        static Tools() 
        {
            assembly = typeof(Tools).GetTypeInfo().Assembly;
            resources = assembly.GetManifestResourceNames();
        }

        public static Stream GetStreamImage(string name)
        {
            name = $".{name}.png";
            name = resources.FirstOrDefault(n => n.EndsWith(name));

            Stream stream = null;
            if (name != null)
            {
                stream = assembly.GetManifestResourceStream(name);
            }
            return stream;
        }

    }
}
