// 
// Author: Boris
// Create: 14/06/2018
// 

using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using SkiaSharp;

namespace Cauldron
{
    public static class All
    {
        private static readonly Assembly assembly;
        private static readonly string[] resources;

        public static TimeSpan FREQUENCY = TimeSpan.FromSeconds(1.0 / 30.0);// 60x par seconde = 17ms, 30x = 33ms
        public static TimeSpan ONE_SECOND = TimeSpan.FromSeconds(1);

        // combien on affiche de colonne à l'écran
        public static int MAP_SHOW = 38;
        public static int MIDDLE_MAP;
        public static int SPEED_LEFT_1;
        public static int SPEED_LEFT_2;
        public static int SPEED_LEFT_3;
        public static int SPEED_LEFT_MAX;
        public static int SPEED_RIGHT_1;
        public static int SPEED_RIGHT_2;
        public static int SPEED_RIGHT_3;
        public static int SPEED_RIGHT_MAX;

        public const int DECAL_MAP_X = 200;
        public const int DECAL_MAP_Y = 5 * 8 * 3; // ce dernier doit être égal à GAME_SCALE...
        public const float GAME_SCALE = 3.0f;

        public static SKBitmap Tiles;
        public static SKBitmap TilesScale;
        public static int TileWidth;
        public static int TileHeight;
        public static int TileWidthScale;
        public static int TileHeightScale;

        public static Witch Witch;


        public static bool KeyUp { get; private set; }
        public static bool KeyDown { get; private set; }
        public static bool KeyLeft { get; private set; }
        public static bool KeyRight { get; private set; }
        public static bool KeySpace { get; private set; }

        private static int lastKey;
        private static Random rnd;

        // *********************************************************************

        static All()
        {
            assembly = typeof(All).GetTypeInfo().Assembly;
            resources = assembly.GetManifestResourceNames();
            rnd = new Random(DateTime.UtcNow.Millisecond);
            TileWidth = 8;
            double part = MAP_SHOW * TileWidth / 10;
            SPEED_LEFT_MAX = Convert.ToInt32(part - 16);
            SPEED_LEFT_3 = Convert.ToInt32(part * 2 - 16);
            SPEED_LEFT_2 = Convert.ToInt32(part * 3 - 16);
            SPEED_LEFT_1 = Convert.ToInt32(part * 4 - 16);
            MIDDLE_MAP = Convert.ToInt32(part * 5 - 16);
            SPEED_RIGHT_1 = Convert.ToInt32(part * 6);
            SPEED_RIGHT_2 = Convert.ToInt32(part * 7);
            SPEED_RIGHT_3 = Convert.ToInt32(part * 8);
            SPEED_RIGHT_MAX = Convert.ToInt32(part * 9);
        }

        // *********************************************************************

        public static int RND(int max) { return rnd.Next(max); }

        // *********************************************************************

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

        // *********************************************************************

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

        // *********************************************************************

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
                case 56: // SHIFT
                    KeySpace = true;
                    break;
            }
        }

        // *********************************************************************

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
                case 56: // SHIFT
                    KeySpace = false;
                    break;
            }
        }

        // *********************************************************************

        public static int GetKeyCode
        {
            get
            {
                return lastKey;
            }
        }

        // *********************************************************************

        public static SKColor colorTransparent = SKColor.Parse("#00000000");
        public static int cas;

        public static bool IsCollision(int x1, int y1, Rectangle object1, int x2, int y2, Rectangle object2)
        {
            // on checke les 4 coins pour voir si il y a recouvrement
            // taille écran donc avec SCALE
            if ((x1 + object1.Width) <= x2)
                return false;
            if (x1 >= (x2 + object2.Width))
                return false;
            if ((y1 + object1.Height) <= y2)
                return false;
            if (y1 >= (y2 + object2.Width))
                return false;
            // on check plus précisement
            int xx1, yy1, w, h, xx2, yy2; // la surface de recouvrement
            if (x1 < x2)
            {
                w = x1 + object1.Width - x2;
                xx1 = object1.Width - w;
                xx2 = 0;
                cas = 0;
            }
            else
            {
                w = x2 + object2.Width - x1;
                xx1 = 0;
                xx2 = object2.Width - w;
                cas = 1;
            }
            if (y1 < y2)
            {
                h = y1 + object1.Height - y2;
                yy1 = object1.Height - h;
                yy2 = 0;
            }
            else
            {
                h = y2 + object2.Height - y1;
                yy1 = 0;
                yy2 = object2.Height - h;
                cas += 2;
            }
            SKColor color;
            switch (cas)
            {
                case 0:
                    for (int i = 0; i < w; i++)
                    {
                        for (int j = 0; j < h; j++)
                        {
                            color = Tiles.GetPixel(object1.X + xx1 + i, object1.Y + yy1 + j);
                            if (color == colorTransparent)
                                continue;
                            color = Tiles.GetPixel(object2.X + xx2 + i, object2.Y + yy2 + j);
                            if (color == colorTransparent)
                                continue;
                            return true;
                        }
                    }
                    break;
                case 1:
                    for (int i = (w - 1); i >= 0; i--)
                    {
                        for (int j = 0; j < h; j++)
                        {
                            color = Tiles.GetPixel(object1.X + xx1 + i, object1.Y + yy1 + j);
                            if (color == colorTransparent)
                                continue;
                            color = Tiles.GetPixel(object2.X + xx2 + i, object2.Y + yy2 + j);
                            if (color == colorTransparent)
                                continue;
                            return true;
                        }
                    }
                    break;
                case 2:
                    for (int i = 0; i < w; i++)
                    {
                        for (int j = (h - 1); j >= 0; j--)
                        {
                            color = Tiles.GetPixel(object1.X + xx1 + i, object1.Y + yy1 + j);
                            if (color == colorTransparent)
                                continue;
                            color = Tiles.GetPixel(object2.X + xx2 + i, object2.Y + yy2 + j);
                            if (color == colorTransparent)
                                continue;
                            return true;
                        }
                    }
                    break;
                case 3:
                    for (int i = (w - 1); i >= 0; i--)
                    {
                        for (int j = (h - 1); j >= 0; j--)
                        {
                            color = Tiles.GetPixel(object1.X + xx1 + i, object1.Y + yy1 + j);
                            if (color == colorTransparent)
                                continue;
                            color = Tiles.GetPixel(object2.X + xx2 + i, object2.Y + yy2 + j);
                            if (color == colorTransparent)
                                continue;
                            return true;
                        }
                    }
                    break;
            }

            return false;
        }

        // *********************************************************************


    }
}
