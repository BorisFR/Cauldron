// 
// Author: Boris
// Create: 14/06/2018
// 

using System;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Cauldron
{
    public static class Tools
    {
        private static readonly Assembly assembly;
        private static readonly string[] resources;


        public static bool KeyUp { get; private set; }
        public static bool KeyDown { get; private set; }
        public static bool KeyLeft { get; private set; }
        public static bool KeyRight { get; private set; }
        public static bool KeySpace { get; private set; }

        private static int lastKey;
        private static Random rnd;

        public static int RND(int max)
        {
            return rnd.Next(max);
        }

        static Tools()
        {
            assembly = typeof(Tools).GetTypeInfo().Assembly;
            resources = assembly.GetManifestResourceNames();
            rnd = new Random(DateTime.UtcNow.Millisecond);
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
            switch (keyCode)
            {
                case 123: // LEFT
                    KeyLeft = true;
                    //KeyRight = false;
                    break;
                case 124: // RIGHT
                    KeyRight = true;
                    //KeyLeft = false;
                    break;
                case 125: // DOWN
                    KeyDown = true;
                    //KeyUp = false;
                    break;
                case 126: // UP
                    KeyUp = true;
                    //KeyDown = false;
                    break;
                case 49: // SPACE
                    KeySpace = true;
                    break;
            }
        }

        public static void Keyup(int keyCode)
        {
            //System.Diagnostics.Debug.WriteLine(string.Format("KeyUp: {0}", keyCode));
            lastKey = 0;
            switch (keyCode)
            {
                case 123: // LEFT
                    KeyLeft = false;
                    break;
                case 124: // RIGHT
                    KeyRight = false;
                    break;
                case 125: // DOWN
                    KeyDown = false;
                    break;
                case 126: // UP
                    KeyUp = false;
                    break;
                case 49: // SPACE
                    KeySpace = false;
                    break;
            }
        }

        public static int GetKeyCode
        {
            get
            {
                return lastKey;
            }
        }

    }
}
