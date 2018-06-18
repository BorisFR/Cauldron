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
        SKCanvas canvas;
        int tile;
        int startMapX;
        const int MAP_SHOW = 100;
        //SKColor[] pixels;
        //SKBitmap bmpPixels;
        SKImageInfo scaleInfo;
        //const int MAX_WIDTH = 640;
        //const int MAX_HEIGHT = 300;
        //SKBitmap bmpToShow;

        //int destSize = 8;
        const float SCALE = 3.0f;
        int tileWidth;
        int tileHeight;

        OneSprite test;


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
            scaleInfo = new SKImageInfo(Convert.ToInt32(tiles.Width * SCALE), Convert.ToInt32(tiles.Height * SCALE));
            tilesScale = tiles.Resize(scaleInfo, SKBitmapResizeMethod.Box);
            // dimension d'un tile
            tileWidth = Convert.ToInt32(tiled.TileWidth * SCALE);
            tileHeight = Convert.ToInt32(tiled.TileHeight * SCALE);

            startMapX = 0;//tiled.MapWidth / 2;
            //pixels = new SKColor[MAX_WIDTH * MAX_HEIGHT];
            //bmpPixels = new SKBitmap(MAX_WIDTH, MAX_HEIGHT);

            test = new OneSprite(20 * 100, tiled.TileWidth, tiled.TileHeight, 24, 21, 3, 400, SCALE);
            //test = new OneSprite(0, tiled.TileWidth, tiled.TileHeight, 24, 21, 3, 100, SCALE);

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

                test.DoAnim(DateTime.UtcNow);

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

        DateTime each = DateTime.UtcNow;

        public void OnPainting(object sender, SKPaintSurfaceEventArgs e)
        {
            // we get the current surface from the event args
            //var surface = e.Surface;
            // then we get the canvas that we can draw on
            canvas = e.Surface.Canvas;
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
            //Array.Clear(pixels, 0, MAX_WIDTH * MAX_HEIGHT);
            DateTime timeStart = DateTime.UtcNow;
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
                        //CopyPixels(tile, i, j);

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
            test.Draw(canvas, 100, 450, tilesScale);
            /*bmpPixels.Pixels = pixels;
            bmpToShow = bmpPixels.Resize(scaleInfo, SKBitmapResizeMethod.Box);
            canvas.DrawBitmap(bmpPixels, 0, 0);*/

            DateTime timeStop = DateTime.UtcNow;
            TimeSpan timeElaps = timeStop - timeStart;
            if ((DateTime.UtcNow - each) > TimeSpan.FromMilliseconds(1000))
            {
                System.Diagnostics.Debug.WriteLine(timeElaps.TotalMilliseconds);
                each = DateTime.UtcNow;
            }

        }

        /*private void CopyPixels(int tileNumber, int toX, int toY)
        {
            int x, y, a, b;
            a = toX * tiled.TileWidth;
            if (a >= MAX_WIDTH)
                return;

            b = toY * tiled.TileHeight;
            if (b >= MAX_HEIGHT)
                return;

            x = (tileNumber % 100) * tiled.TileWidth;
            y = (tileNumber / 100) * tiled.TileHeight;
            for (int j = 0; j < 8; j++)
            {
                if ((b + j) >= MAX_HEIGHT)
                    return;
                pixels[a + (b + j) * MAX_WIDTH] = tiles.GetPixel(x, y + j);
                if ((a + 1) < MAX_WIDTH)
                    pixels[a + 1 + (b + j) * MAX_WIDTH] = tiles.GetPixel(x + 1, y + j);
                if ((a + 2) < MAX_WIDTH)
                    pixels[a + 2 + (b + j) * MAX_WIDTH] = tiles.GetPixel(x + 2, y + j);
                if ((a + 3) < MAX_WIDTH)
                    pixels[a + 3 + (b + j) * MAX_WIDTH] = tiles.GetPixel(x + 3, y + j);
                if ((a + 4) < MAX_WIDTH)
                    pixels[a + 4 + (b + j) * MAX_WIDTH] = tiles.GetPixel(x + 4, y + j);
                if ((a + 5) < MAX_WIDTH)
                    pixels[a + 5 + (b + j) * MAX_WIDTH] = tiles.GetPixel(x + 5, y + j);
                if ((a + 6) < MAX_WIDTH)
                    pixels[a + 6 + (b + j) * MAX_WIDTH] = tiles.GetPixel(x + 6, y + j);
                if ((a + 7) < MAX_WIDTH)
                    pixels[a + 7 + (b + j) * MAX_WIDTH] = tiles.GetPixel(x + 7, y + j);
            }
        }*/

    }
}