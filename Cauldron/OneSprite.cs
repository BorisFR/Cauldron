﻿// /*
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
        bool animStop;
        TimeSpan ts;
        int missedAnim;

        // *********************************************************************

        public OneSprite(int tileNumber, int width, int height, int animCount, int animDelay, bool spriteDoubleWidth = false, bool animBack = false, bool animStop = false, bool withSeparator = true)
        {
            this.Width = width;
            this.Height = height;
            this.spriteDoubleWidth = spriteDoubleWidth;
            this.animBack = animBack;
            this.animStop = animStop;

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
                sources[i] = new Rectangle(sourceX, sourceY, Width, Height);
                if (withSeparator)
                    sourceX += Convert.ToInt32(((width + All.TileWidth * 2 - 1) / All.TileWidth) * All.TileWidth);
                else
                    sourceX += width;
                sourcesScale[i] = new SKRect(sourceScaleX, sourceScaleY, sourceScaleX + WidthScale, sourceScaleY + HeightScale);
                // on se positionne sur l'animation suivante
                // qui est sur la même ligne, séparée par une colonne de la taille d'une tile
                if (withSeparator)
                    sourceScaleX += Convert.ToInt32(((width + All.TileWidth * 2 - 1) / All.TileWidth) * All.TileWidth * All.GAME_SCALE);
                else
                    sourceScaleX += Convert.ToInt32(width * All.GAME_SCALE);
            }
            if (spriteDoubleWidth)
                WidthScale *= 2;
        }

        // *********************************************************************

        public Rectangle Source { get { return sources[StepAnim]; } }

        // *********************************************************************

        public void SetAnimSteps(int from, int to, int delay, bool animStop = false)
        {
            animFrom = from;
            animTo = to;
            if (animFrom < animTo)
                animIncrease = true;
            else
                animIncrease = false;
            animDelay = delay;
            animElaps = TimeSpan.FromMilliseconds(animDelay);
            StepAnim = from;
            this.animStop = animStop;
            startAnim = DateTime.UtcNow;
        }

        // *********************************************************************

        public void DoAnim(DateTime time)
        {
            ts = time - startAnim;
            if (ts < animElaps)
                return;
            startAnim = time;
            missedAnim = (int)(ts.TotalMilliseconds / animElaps.TotalMilliseconds);
            if (animIncrease)
            {
                for (int i = 0; i < missedAnim; i++)
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
                            if (animStop)
                            {
                                StepAnim--;
                            }
                            else
                            {
                                StepAnim = animFrom;
                            }
                        }
                    }
                }
            }
            else
            {
                for (int i = 0; i < missedAnim; i++)
                {
                    StepAnim--;
                    if (StepAnim < animFrom)
                    {
                        StepAnim = animFrom + 1;
                        animIncrease = true;
                        if (animStop)
                        {
                            StepAnim = animFrom;
                        }
                    }
                }
            }
        }

        // *********************************************************************

        SKRect tempSKRect;
        float scaleX, scaleY;
        //SKPaint resize = new SKPaint() { IsAntialias = false, FilterQuality = SKFilterQuality.High };

        public void Draw(SKCanvas canvas, int x, int y, int scrollX = 0, SKPaint paint = null)
        {
            scaleX = x * All.GAME_SCALE;
            scaleY = y * All.GAME_SCALE;
            tempSKRect = new SKRect(All.DECAL_MAP_X + scaleX + scrollX, All.DECAL_MAP_Y + scaleY, All.DECAL_MAP_X + scaleX + WidthScale + scrollX, All.DECAL_MAP_Y + scaleY + HeightScale);
            //canvas.DrawBitmap(All.TilesScale, sourcesScale[StepAnim], tempSKRect, paint);
            canvas.DrawImage(All.TilesScaleImage, sourcesScale[StepAnim], tempSKRect, paint);
        }

        public void DrawNoDecal(SKCanvas canvas, int x, int y)
        {
            scaleX = x * All.GAME_SCALE;
            scaleY = y * All.GAME_SCALE;
            tempSKRect = new SKRect(scaleX, scaleY, scaleX + WidthScale, scaleY + HeightScale);
            //canvas.DrawBitmap(All.TilesScale, sourcesScale[StepAnim], tempSKRect);
            canvas.DrawImage(All.TilesScaleImage, sourcesScale[StepAnim], tempSKRect);
        }

    }
}