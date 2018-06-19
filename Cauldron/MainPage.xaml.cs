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

        Tiled tiled = new Tiled();
        SKBitmap tiles;
        SKBitmap tilesScale;
        SKRect source = new SKRect();
        SKRect dest = new SKRect();
        SKCanvas canvas;
        const int DECAL_SCREEN_X = 10;
        const int DECAL_SCREEN_Y = 10;
        const int DECAL_MAP_X = 0;
        const int DECAL_MAP_Y = 100;
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

        Witch witch;
        OneSprite spriteMoon;
        OneSprite spriteSheepSkin;
        OneSprite spriteEnergy;
        OneSprite[] spritesSmoke = new OneSprite[3];
        int smokeIndex;
        DateTime tempo;


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

            startMapX = tiled.StartHouse - 10;
            //pixels = new SKColor[MAX_WIDTH * MAX_HEIGHT];
            //bmpPixels = new SKBitmap(MAX_WIDTH, MAX_HEIGHT);

            witch = new Witch(tiled.TileWidth, tiled.TileHeight, SCALE, DECAL_MAP_X, DECAL_MAP_Y);
            witch.X = Convert.ToInt32(17 * 8 * SCALE);
            witch.Y = 480;
            spriteMoon = new OneSprite(15 * 100 + 30, tiled.TileWidth, tiled.TileHeight, 6 * 8, 5 * 8, 1, 0, SCALE, DECAL_SCREEN_X, DECAL_SCREEN_Y);
            spriteSheepSkin = new OneSprite(15 * 100 + 21, tiled.TileWidth, tiled.TileHeight, 7 * 8, 4 * 8, 1, 0, SCALE, DECAL_SCREEN_X, DECAL_SCREEN_Y);
            spriteEnergy = new OneSprite(0 * 100 + 100 - 32, tiled.TileWidth, tiled.TileHeight, 3 * 8, 3 * 8, 4, 30, SCALE, DECAL_SCREEN_X, DECAL_SCREEN_Y);
            spritesSmoke[0] = new OneSprite(3 * 100 + 100 - 32, tiled.TileWidth, tiled.TileHeight, 3 * 8, 3 * 8, 7, 110, SCALE, DECAL_SCREEN_X, DECAL_SCREEN_Y);

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

                tempo = DateTime.UtcNow;
                spriteEnergy.DoAnim(tempo);
                spritesSmoke[0].DoAnim(tempo);
                witch.DoAnim(tempo);

                switch (Tools.GetKeyCode)
                {
                    case 123: // LEFT
                        startMapX--;
                        if (startMapX < 0)
                        {
                            startMapX = tiled.MapWidth - 1;
                        }
                        witch.MoveToLeft();
                        break;
                    case 124: // RIGHT
                        startMapX++;
                        if (startMapX >= tiled.MapWidth)
                        {
                            startMapX = 0;
                        }
                        witch.MoveToRight();
                        break;
                    case 125: // DOWN
                        witch.MoveToDown();
                        break;
                    case 126: // UP
                        witch.MoveToUp();
                        break;
                    case 49: // SPACE
                        break;
                    default:
                        witch.MoveStop();
                        break;
                }

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


            // affichage de la carte
            //Array.Clear(pixels, 0, MAX_WIDTH * MAX_HEIGHT);
            int currentX = startMapX;
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
                        a = i * tileWidth + DECAL_MAP_X;
                        b = j * tileHeight + DECAL_MAP_Y;
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

            // affichage des items
            smokeIndex = 0;
            currentX = startMapX;
            // pour chaque colonne
            for (int i = 0; i < MAP_SHOW; i++)
            {
                // et chaque ligne
                for (int j = 0; j < tiled.MapHeight; j++)
                {
                    // le tile en cours
                    tile = tiled.Items[currentX, j];
                    if (tile > 0)
                    {
                        tile--;
                        int x = i * tileWidth + DECAL_MAP_X - 8;
                        int y = j * tileHeight + DECAL_MAP_Y - 8;
                        switch (tile)
                        {
                            case 68: // energy
                                spriteEnergy.Draw(canvas, x, y, tilesScale);
                                break;
                            case 368: // smoke
                                spritesSmoke[smokeIndex++].Draw(canvas, x, y, tilesScale);
                                break;
                            default:
                                //System.Diagnostics.Debug.WriteLine(String.Format("Item: {0}", tile));
                                break;
                        }

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



            // affichage des infos
            spriteSheepSkin.Draw(canvas, 1, 1, tilesScale);
            spriteSheepSkin.Draw(canvas, 1000, 1, tilesScale);
            spriteMoon.Draw(canvas, 150, 200, tilesScale);


            witch.Draw(canvas, tilesScale);
            /*bmpPixels.Pixels = pixels;
            bmpToShow = bmpPixels.Resize(scaleInfo, SKBitmapResizeMethod.Box);
            canvas.DrawBitmap(bmpPixels, 0, 0);*/

            DateTime timeStop = DateTime.UtcNow;
            TimeSpan timeElaps = timeStop - timeStart;
            if ((DateTime.UtcNow - each) > TimeSpan.FromMilliseconds(1000))
            {
                //System.Diagnostics.Debug.WriteLine(timeElaps.TotalMilliseconds);
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