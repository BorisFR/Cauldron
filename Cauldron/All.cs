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

        // combien on affiche de colonne à l'écran
        public static int MAP_SHOW = 38;

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

        public static int RND(int max)
        {
            return rnd.Next(max);
        }

        static All()
        {
            assembly = typeof(All).GetTypeInfo().Assembly;
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
                case 56: // SHIFT
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
                case 56: // SHIFT
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
            int cas;
            if (x1 < x2)
            {
                w = x1 + object1.Width - x2;
                xx1 = object1.Width - w;
                xx2 = 0;
                //System.Diagnostics.Debug.Write(String.Format("Contact X: {0} ({1}) < {2} ({3}) : {4} et {5} => {6}", x1, object1.Width, x2, object2.Width, xx1, xx2, w));
                cas = 0;
            }
            else
            {
                w = x2 + object2.Width - x1;
                xx1 = 0;
                xx2 = object2.Width - w;
                //System.Diagnostics.Debug.Write(String.Format("Contact X: {0} ({1}) > {2} ({3}) : {4} et {5} => {6}", x1, object1.Width, x2, object2.Width, xx1, xx2, w));
                cas = 1;
            }
            if (y1 < y2)
            {
                h = y1 + object1.Height - y2;
                yy1 = object1.Height - h;
                yy2 = 0;
                //System.Diagnostics.Debug.WriteLine(String.Format(" - Y: {0} ({1}) < {2} ({3}) : {4} et {5} => {6}", y1, object1.Height, y2, object2.Height, yy1, yy2, h));

            }
            else
            {
                h = y2 + object2.Height - y1;
                yy1 = 0;
                yy2 = object2.Height - h;
                //System.Diagnostics.Debug.WriteLine(String.Format(" - Y: {0} ({1}) > {2} ({3}) : {4} et {5} => {6}", y1, object1.Height, y2, object2.Height, yy1, yy2, h));
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
                            //System.Diagnostics.Debug.WriteLine(String.Format(" Touch: {0} / {1} : {2}", x1 + xx1 + i, y1 + yy1 + j, color));
                            /*ShowPixelX = x1 + xx1 + i;
                            ShowPixelY = y1 + yy1 + j;
                            ShowPixel = true;*/
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
                            //System.Diagnostics.Debug.WriteLine(String.Format(" Touch: {0} / {1} : {2}", x1 + xx1 + i, y1 + yy1 + j, color));
                            /*ShowPixelX = x1 + xx1 + i;
                            ShowPixelY = y1 + yy1 + j;
                            ShowPixel = true;*/
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
                            //System.Diagnostics.Debug.WriteLine(String.Format(" Touch: {0} / {1} : {2}", x1 + xx1 + i, y1 + yy1 + j, color));
                            /*ShowPixelX = x1 + xx1 + i;
                            ShowPixelY = y1 + yy1 + j;
                            ShowPixel = true;*/
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
                            //System.Diagnostics.Debug.WriteLine(String.Format(" Touch: {0} / {1} : {2}", x1 + xx1 + i, y1 + yy1 + j, color));
                            /*ShowPixelX = x1 + xx1 + i;
                            ShowPixelY = y1 + yy1 + j;
                            ShowPixel = true;*/
                            return true;
                        }
                    }
                    break;
            }

            return false;
        }

        public static SKColor colorTransparent = SKColor.Parse("#00000000");
        /*public static bool ShowPixel;
        public static int ShowPixelX;
        public static int ShowPixelY;*/

    }
}
