using System;
using System.Diagnostics;
using SkiaSharp;
using SkiaSharp.Views.Forms;
using Xamarin.Forms;

namespace Cauldron
{
    public partial class MainPage : ContentPage
    {
        Stopwatch watch;
        float start;

        SKBitmap bmSource;
        SKBitmap bm1;
        SKBitmap bm2;
        SKBitmap bm3;
        SKBitmap bm4;

        int posX;

        public MainPage()
        {
            InitializeComponent();


            SKImageInfo desiredInfo = new SKImageInfo(12, 21, SKImageInfo.PlatformColorType, SKAlphaType.Premul);
            SKManagedStream stream = new SKManagedStream(Tools.GetStreamImage("entering_door_1"));
            bmSource = SKBitmap.Decode(stream, desiredInfo);
            var x = new SKImageInfo(24, 42);
            bm1 = bmSource.Resize(x, SKBitmapResizeMethod.Hamming);
            bm2 = bmSource.Resize(x, SKBitmapResizeMethod.Lanczos3);
            bm3 = bmSource.Resize(x, SKBitmapResizeMethod.Mitchell);
            bm4 = bmSource.Resize(x, SKBitmapResizeMethod.Triangle);

            posX = 0;

            watch = new Stopwatch();
            start = 0;
            var fps = TimeSpan.FromSeconds(1.0 / 30.0);
            watch.Start();

            Device.StartTimer(fps, () =>
            {
                // get elapsed time
                var time = (float)watch.Elapsed.TotalMinutes;
                watch.Restart();
                // get the elapsed rotation
                start += (360 * time) % 360;

                posX++;
                if (posX > 400)
                    posX = 0;
                theCanvas.InvalidateSurface();
                return true;
            });
        }

        public void OnPainting(object sender, SKPaintSurfaceEventArgs e)
        {
            // we get the current surface from the event args
            var surface = e.Surface;
            // then we get the canvas that we can draw on
            var canvas = surface.Canvas;
            // clear the canvas / view
            canvas.Clear(SKColors.Black);


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


            // create the paint for the text
            var textPaint = new SKPaint
            {
                IsAntialias = true,
                Style = SKPaintStyle.Fill,
                Color = SKColors.Orange,
                TextSize = 80
            };

            // draw the text (from the baseline)
            canvas.DrawText("Bonjour ;)", 60, 160 + 80, textPaint);


            canvas.DrawBitmap(bmSource, 50, 300);
            canvas.DrawBitmap(bm1, 100, 300);
            canvas.DrawBitmap(bm2, 150, 300);
            canvas.DrawBitmap(bm3, 200, 300);
            canvas.DrawBitmap(bm4, 250, 300);

            var bitmapPaint = new SKPaint
            {
                FilterQuality = SKFilterQuality.High,
                IsAntialias = true
            };
            canvas.DrawBitmap(bmSource, posX, 350, bitmapPaint);
            canvas.DrawBitmap(bm1, 100, 350, bitmapPaint);
            canvas.DrawBitmap(bm2, 150, 350, bitmapPaint);
            canvas.DrawBitmap(bm3, 200, 350, bitmapPaint);
            canvas.DrawBitmap(bm4, 250, 350, bitmapPaint);
        }

    }
}