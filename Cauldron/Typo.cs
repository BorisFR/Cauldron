// /*
// Author: Boris
// Create: 24/06/2018
// */
using System;
using System.Text;
using SkiaSharp;

namespace Cauldron
{
    public class Typo
    {

        OneSprite characters;

        public Typo()
        {
            characters = new OneSprite(36 * 100, 8, 8, 37, 0, withSeparator: false);
        }

        public void Write(SKCanvas canvas, string text, int x, int y)
        {
            int pos = x;
            byte[] asciiBytes = Encoding.ASCII.GetBytes(text);
            foreach (byte b in asciiBytes)
            {
                if (b == 37)
                    characters.StepAnim = 26;
                else
                {
                    if (b < 58)
                        characters.StepAnim = b - 48 + 27;
                    else
                        characters.StepAnim = b - 65;
                }
                if (characters.StepAnim >= 0 && characters.StepAnim < 37)
                    characters.Draw(canvas, pos, y);
                pos += 8;
            }
        }
    }
}