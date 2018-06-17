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

        /*SKBitmap bmSource;
        SKBitmap bm1;
        SKBitmap bm2;
        SKBitmap bm3;
        SKBitmap bm4;
        int posX;
        */

        Tiled tiled = new Tiled();
        SKBitmap tiles;
        SKBitmap tilesScale;
        SKRect source = new SKRect();
        SKRect dest = new SKRect();
        int tile;
        int startMapX;
        const int MAP_SHOW = 100;

        //int destSize = 8;
        const int SCALE = 4;
        int tileWidth;
        int tileHeight;


        public MainPage()
        {
            InitializeComponent();

            // on charge le plan TILED
            tiled.Load(Tools.GetStream("Cauldron.tmx"));
            // on charge l'image de tiles
            SKImageInfo desiredInfo = new SKImageInfo(800, 800, SKImageInfo.PlatformColorType, SKAlphaType.Premul);
            SKManagedStream stream = new SKManagedStream(Tools.GetStream("tiles.png"));
            tiles = SKBitmap.Decode(stream, desiredInfo);
            // on scale cette image
            SKImageInfo scaleInfo = new SKImageInfo(tiles.Width * SCALE, tiles.Height * SCALE);
            tilesScale = tiles.Resize(scaleInfo, SKBitmapResizeMethod.Hamming);
            // dimension d'un tile
            tileWidth = tiled.TileWidth * SCALE;
            tileHeight = tiled.TileHeight * SCALE;

            startMapX = 0;//tiled.MapWidth / 2;


            /*
            desiredInfo = new SKImageInfo(12, 21, SKImageInfo.PlatformColorType, SKAlphaType.Premul);
            //SKManagedStream stream = new SKManagedStream(Tools.GetStreamImage("entering_door_1"));
            stream = new SKManagedStream(Tools.GetStream("entering_door_1.png"));
            bmSource = SKBitmap.Decode(stream, desiredInfo);
            var x = new SKImageInfo(24, 42);
            bm1 = bmSource.Resize(x, SKBitmapResizeMethod.Hamming);
            bm2 = bmSource.Resize(x, SKBitmapResizeMethod.Lanczos3);
            bm3 = bmSource.Resize(x, SKBitmapResizeMethod.Mitchell);
            bm4 = bmSource.Resize(x, SKBitmapResizeMethod.Triangle);

            posX = 0;
            */

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

                switch (Tools.GetKeyCode)
                {
                    case 123:
                        startMapX--;
                        if (startMapX < 0)
                        {
                            startMapX = tiled.MapWidth - 1;
                        }
                        break;
                    case 124:
                        startMapX++;
                        if (startMapX >= tiled.MapWidth)
                        {
                            startMapX = 0;
                        }
                        break;

                }

                /*posX++;
                if (posX > 400)
                    posX = 0;*/
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

            /*
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
            */

            /*canvas.DrawBitmap(bmSource, 50, 300);
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
            */

            // affichage de la carte
            int currentX = startMapX;
            // pour chaque colonne
            for (int i = 0; i < MAP_SHOW; i++)
            {
                // et chaque ligne
                for (int j = 0; j < tiled.MapHeight; j++)
                {
                    // le tile en cours
                    tile = tiled.Terrain[currentX, j];
                    if (tile > 0)
                    {
                        tile--;
                        int x, y, a, b;
                        // position de la source
                        x = (tile % 100) * tileWidth;
                        y = (tile / 100) * tileHeight;
                        source = new SKRect(x, y, x + tileWidth, y + tileHeight);
                        // position de la cible
                        a = i * tileWidth;
                        b = j * tileHeight;
                        dest = new SKRect(a, b, a + tileWidth, b + tileHeight);
                        // on effectue l'affichage
                        canvas.DrawBitmap(tilesScale, source, dest);
                    }
                }
                // colonne suivante
                currentX++;
                if (currentX >= tiled.MapWidth)
                {
                    // la carte boucle sur elle-même
                    currentX = 0;
                }
            }
        }

    }
}