// /*
// Author: Boris
// Create: 18/06/2018
// */

using System;
using SkiaSharp;

namespace Cauldron
{
    public class OneSprite
    {
        int decalX;
        int decalY;
        int width;
        int height;
        int widthScale;
        int heightScale;
        bool doubleX;
        int animCount;
        int animDelay;
        int animFrom;
        int animTo;
        TimeSpan animElaps;
        bool animIncrease;
        bool animBack;

        DateTime startAnim;
        int animStep;
        SKRect[] sources;
        SKRect dest;

        public void SetAnimSteps(int from, int to, int delay)
        {
            animFrom = from;
            animTo = to;
            if (animFrom < animTo)
                animIncrease = true;
            else
                animIncrease = false;
            animDelay = delay;
            animStep = from;
        }

        public int StepAnim { get { return animStep; } set { animStep = value; } }


        public OneSprite()
        {

        }

        public OneSprite(int tileNumber, int tileWidth, int tileHeight, int width, int height, int animCount, int animDelay, float scale, int decalX, int decalY, bool doubleX = false, bool animBack = false)
        {
            this.decalX = decalX;
            this.decalY = decalY;
            this.width = width;
            this.height = height;
            this.doubleX = doubleX;
            this.animBack = animBack;
            widthScale = Convert.ToInt32(width * scale);
            heightScale = Convert.ToInt32(height * scale);
            this.animCount = animCount;
            this.animDelay = animDelay;
            animElaps = TimeSpan.FromMilliseconds(animDelay);
            animStep = 0;
            animFrom = 0;
            animTo = animCount - 1;
            if (animFrom < animTo)
                animIncrease = true;
            else
                animIncrease = false;
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
                x += Convert.ToInt32(((width + tileWidth * 2 - 1) / tileWidth) * tileWidth * scale);
            }
            if (doubleX)
                widthScale *= 2;
        }

        public void DoAnim(DateTime time)
        {
            if ((time - startAnim) < animElaps)
                return;
            startAnim = time;//.AddMilliseconds(-animDelay);
            if (animIncrease)
            {
                animStep++;
                if (animStep > animTo)
                {
                    if (animBack)
                    {
                        animStep = animTo - 1;
                        animIncrease = false;
                    }
                    else
                    {
                        animStep = animFrom;
                    }
                }
            }
            else
            {
                animStep--;
                if (animStep < animTo)
                {
                    if (animBack)
                    {
                        animStep = animFrom + 1;
                        animIncrease = true;
                    }
                    else
                    {
                        animStep = animFrom;
                    }
                }
            }
        }

        public void Draw(SKCanvas canvas, int x, int y, SKBitmap tiles, int scrollX = 0)
        {
            dest = new SKRect(decalX + x + scrollX, decalY + y, decalX + x + widthScale + scrollX, decalY + y + heightScale);
            canvas.DrawBitmap(tiles, sources[animStep], dest);
        }

    }
}