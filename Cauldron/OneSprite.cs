// /*
// Author: Boris
// Create: 18/06/2018
// */

using System;
using SkiaSharp;
using SkiaSharp.Views.Forms;

namespace Cauldron
{
    public class OneSprite
    {
        int width;
        int height;
        int widthScale;
        int heightScale;
        int animCount;
        int animDelay;
        TimeSpan animElaps;

        DateTime startAnim;
        int animStep;
        SKRect[] sources;
        SKRect dest;



        public OneSprite(int tileNumber, int tileWidth, int tileHeight, int width, int height, int animCount, int animDelay, float scale)
        {
            this.width = width;
            this.height = height;
            widthScale = Convert.ToInt32(width * scale);
            heightScale = Convert.ToInt32(height * scale);
            this.animCount = animCount;
            this.animDelay = animDelay;
            animElaps = TimeSpan.FromMilliseconds(animDelay);
            animStep = 0;
            startAnim = DateTime.UtcNow;
            // calcul des divers sources
            sources = new SKRect[animCount];
            int x = Convert.ToInt32((tileNumber % 100) * tileWidth * scale);
            int y = Convert.ToInt32((tileNumber / 100) * tileHeight * scale);
            for (int i = 0; i < animCount; i++)
            {
                sources[i] = new SKRect(x, y, x + widthScale, y + heightScale);
                // on se positionne sur l'animation suivante
                // qui est sur la même ligne, séparée par une colonne de la taille d'une tile
                x += Convert.ToInt32(width * scale + ((tileWidth * 2 - 1) / tileWidth) * scale);
            }
        }

        public void DoAnim(DateTime time)
        {
            if ((time - startAnim) < animElaps)
                return;
            startAnim.AddMilliseconds(-animDelay);
            animStep++;
            if (animStep >= animCount)
            {
                animStep = 0;
            }
        }

        public void Draw(SKCanvas canvas, int x, int y, SKBitmap tiles)
        {
            dest = new SKRect(x, y, x + widthScale, y + heightScale);
            canvas.DrawBitmap(tiles, sources[animStep], dest);
        }

    }
}