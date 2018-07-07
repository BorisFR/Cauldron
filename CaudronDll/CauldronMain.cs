using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using SkiaSharp;
using SkiaSharp.Views;


namespace CaudronDll
{
    public class CauldronMain
    {
        //SKBitmap bitmap;
        SKCanvas canvas;
        SKSurface surface;

        SKSurface surfaceResize;
        SKCanvas canvasResize;
        SKImage snap;
        SKBitmap bitmap;
        SKImageInfo scaleInfo;

        SKPaint paint;

        public CauldronMain()
        {
            surface = SKSurface.Create(320, 200, SkiaSharp.SKColorType.Rgba8888, SkiaSharp.SKAlphaType.Premul);
            canvas = surface.Canvas;

            surfaceResize = SKSurface.Create(4 * 320, 4 * 200, SKColorType.Rgba8888, SKAlphaType.Premul);
            canvasResize = surfaceResize.Canvas;

            scaleInfo = new SKImageInfo(4 * 320, 4 * 200);

        }

        public void Doupdate()
        {

        }

        public void DoDraw()
        {
            canvas.Clear(SKColors.DarkGray);

            var circleFill = new SKPaint
            {
                IsAntialias = true,
                Style = SKPaintStyle.Fill,
                Color = SKColors.Blue
            };
            // draw the circle fill
            canvas.DrawCircle(100, 100, 40, circleFill);
            // create the paint for the circle border
            var circleBorder = new SKPaint
            {
                IsAntialias = true,
                Style = SKPaintStyle.Stroke,
                Color = SKColors.Red,
                StrokeWidth = 5
            };
            // draw the circle border
            canvas.DrawCircle(100, 100, 40, circleBorder);

            // create the paint for the path
            var pathStroke = new SKPaint
            {
                IsAntialias = true,
                Style = SKPaintStyle.Stroke,
                Color = SKColors.Green,
                StrokeWidth = 5
            };
            // create a path
            var path = new SKPath();
            path.MoveTo(160, 60);
            path.LineTo(240, 140);
            path.MoveTo(240, 60);
            path.LineTo(160, 140);
            // draw the path
            canvas.DrawPath(path, pathStroke);

        }

        public Stream Image
        {
            get
            {
                snap = surface.Snapshot();
                bitmap = SKBitmap.FromImage(snap).Resize(scaleInfo, SKBitmapResizeMethod.Mitchell);
                canvasResize.DrawBitmap(bitmap, 0, 0);


                return surfaceResize.Snapshot().Encode().AsStream();
            }
        }

    }
}