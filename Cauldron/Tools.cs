// 
// Author: Boris
// Create: 14/06/2018
// 

using System.IO;
using System.Linq;
using System.Reflection;

namespace Cauldron
{
    public static class Tools
    {
        private static readonly Assembly assembly;
        private static readonly string[] resources;

        private static int lastKey;

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


        public static Stream GetStream(string name)
        {
            name = resources.FirstOrDefault(n => n.EndsWith(name));

            Stream stream = null;
            if (name != null)
            {
                stream = assembly.GetManifestResourceStream(name);
            }
            return stream;
        }

        public static void Keydown(int keyCode)
        {
            //System.Diagnostics.Debug.WriteLine(string.Format("KeyDown: {0}", keyCode));
            lastKey = keyCode;
        }

        public static void Keyup(int keyCode)
        {
            //System.Diagnostics.Debug.WriteLine(string.Format("KeyUp: {0}", keyCode));
            lastKey = 0;
        }

        public static int GetKeyCode
        {
            get
            {
                return lastKey;
                /*if (lastKey > 0)
                {
                    int temp = lastKey;
                    lastKey = 0;
                    return temp;
                }
                return 0;*/
            }
        }

    }
}
