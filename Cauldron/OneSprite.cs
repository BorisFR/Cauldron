// /*
// Author: Boris
// Create: 18/06/2018
// */

using System;
using System.Drawing;
using SkiaSharp;

namespace Cauldron
{
    public class OneSprite
    {
        // original
        public int Width { get; private set; }
        public int Height { get; private set; }
        Rectangle[] sources;
        // scale
        public int WidthScale { get; private set; }
        public int HeightScale { get; private set; }
        bool spriteDoubleWidth;
        SKRect[] sourcesScale;


        DateTime startAnim;
        public int StepAnim { get; set; }
        int animFrom;
        int animTo;
        int animCount;
        int animDelay;
        TimeSpan animElaps;
        bool animIncrease;
        bool animBack;


        public OneSprite(int tileNumber, int width, int height, int animCount, int animDelay, bool spriteDoubleWidth = false, bool animBack = false)
        {
            this.Width = width;
            this.Height = height;
            this.spriteDoubleWidth = spriteDoubleWidth;
            this.animBack = animBack;

            WidthScale = Convert.ToInt32(width * All.GAME_SCALE);
            HeightScale = Convert.ToInt32(height * All.GAME_SCALE);

            this.animCount = animCount;
            this.animDelay = animDelay;
            animElaps = TimeSpan.FromMilliseconds(animDelay);
            StepAnim = 0;
            animFrom = 0;
            animTo = animCount - 1;
            if (animFrom < animTo)
                animIncrease = true;
            else
                animIncrease = false;
            startAnim = DateTime.UtcNow;

            // calcul des divers sources
            sources = new Rectangle[animCount];
            int sourceX = Convert.ToInt32((tileNumber % 100) * All.TileWidth);
            int sourceY = Convert.ToInt32((tileNumber / 100) * All.TileHeight);
            sourcesScale = new SKRect[animCount];
            int sourceScaleX = Convert.ToInt32((tileNumber % 100) * All.TileWidth * All.GAME_SCALE);
            int sourceScaleY = Convert.ToInt32((tileNumber / 100) * All.TileHeight * All.GAME_SCALE);
            for (int i = 0; i < animCount; i++)
            {
                sources[i] = new Rectangle(sourceX, sourceY, sourceX + Width, sourceY + Height);
                sourceX += Convert.ToInt32(((width + All.TileWidth * 2 - 1) / All.TileWidth) * All.TileWidth);
                sourcesScale[i] = new SKRect(sourceScaleX, sourceScaleY, sourceScaleX + WidthScale, sourceScaleY + HeightScale);
                // on se positionne sur l'animation suivante
                // qui est sur la même ligne, séparée par une colonne de la taille d'une tile
                sourceScaleX += Convert.ToInt32(((width + All.TileWidth * 2 - 1) / All.TileWidth) * All.TileWidth * All.GAME_SCALE);
            }
            if (spriteDoubleWidth)
                WidthScale *= 2;
        }

        public Rectangle Source { get { return sources[StepAnim]; } }

        public void SetAnimSteps(int from, int to, int delay)
        {
            animFrom = from;
            animTo = to;
            if (animFrom < animTo)
                animIncrease = true;
            else
                animIncrease = false;
            animDelay = delay;
            StepAnim = from;
        }

        public void DoAnim(DateTime time)
        {
            if ((time - startAnim) < animElaps)
                return;
            startAnim = time;
            if (animIncrease)
            {
                StepAnim++;
                if (StepAnim > animTo)
                {
                    if (animBack)
                    {
                        StepAnim = animTo - 1;
                        animIncrease = false;
                    }
                    else
                    {
                        StepAnim = animFrom;
                    }
                }
            }
            else
            {
                StepAnim--;
                if (StepAnim < animTo)
                {
                    if (animBack)
                    {
                        StepAnim = animFrom + 1;
                        animIncrease = true;
                    }
                    else
                    {
                        StepAnim = animFrom;
                    }
                }
            }
        }

        SKRect tempSKRect;
        float scaleX, scaleY;

        public void Draw(SKCanvas canvas, int x, int y, int scrollX = 0)
        {
            scaleX = x * All.GAME_SCALE;
            scaleY = y * All.GAME_SCALE;
            tempSKRect = new SKRect(All.DECAL_MAP_X + scaleX + scrollX, All.DECAL_MAP_Y + scaleY, All.DECAL_MAP_X + scaleX + WidthScale + scrollX, All.DECAL_MAP_Y + scaleY + HeightScale);
            canvas.DrawBitmap(All.TilesScale, sourcesScale[StepAnim], tempSKRect);
        }

    }
}