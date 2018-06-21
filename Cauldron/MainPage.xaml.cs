using System;
using System.Collections.Generic;
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

        int startMapX; // colonne de départ d'affichage de la map
        // pour "ralentir" le scroll horizontale
        int scrollMapCount;
        const int SCROLL_MAP_DELAY = 3;
        // mais de façon fluide avec un scroll intermédiaire
        // on ne scrolle pas de la taille d'une tile
        int scrollX;
        int scrollStep;
        int scrollSpeed;

        // combien on affiche de colonne à l'écran
        const int MAP_SHOW = 40;

        //SKColor[] pixels;
        //SKBitmap bmpPixels;
        //const int MAX_WIDTH = 640;
        //const int MAX_HEIGHT = 300;
        //SKBitmap bmpToShow;

        //int destSize = 8;
        const float SCALE = 4.0f;
        SKImageInfo scaleInfo;
        int tileWidth;
        int tileHeight;

        Witch witch;
        OneSprite spriteMoon;
        OneSprite spriteSheepSkin;
        OneSprite spriteEnergy;
        const int SMOKE_SPRITES_MAX = 1;
        OneSprite[] spritesSmoke = new OneSprite[SMOKE_SPRITES_MAX];
        int smokeIndex;
        Monsters monsters;
        DateTime tempo;

        OneSprite spriteBullet;
        Dictionary<int, OneExplode> bullets = new Dictionary<int, OneExplode>();
        TimeSpan bulletElaps = TimeSpan.FromMilliseconds(40);
        int idBullet = 0;


        public MainPage()
        {
            InitializeComponent();

            // on charge la définition des tiles
            tiled.ProcessTileSet(Tools.GetStream("Cauldron.tsx"));
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

            int stepX = Convert.ToInt32((24 / 12) * SCALE / 3); // on monte/descend d'1/6 de la taille du sprite qui vaut 17 (mode vole)
            int stepY = Convert.ToInt32((17 / 6) * SCALE / 2); // on monte/descend d'1/6 de la taille du sprite qui vaut 17 (mode vole)


            witch = new Witch(tiled.TileWidth, tiled.TileHeight, SCALE, DECAL_MAP_X, DECAL_MAP_Y, stepX, stepY * 2);
            witch.X = Convert.ToInt32(17 * 8 * SCALE);
            witch.Y = Convert.ToInt32((19 * 8 + 3) * SCALE);
            witch.MinY = 0;
            witch.MaxY = witch.Y;

            spriteBullet = new OneSprite(0 * 100 + 100 - 16, tiled.TileWidth, tiled.TileHeight, 3 * 8, 3 * 8, 4, 40, SCALE, DECAL_SCREEN_X, DECAL_SCREEN_Y);

            spriteMoon = new OneSprite(15 * 100 + 30, tiled.TileWidth, tiled.TileHeight, 6 * 8, 5 * 8, 1, 0, SCALE, DECAL_SCREEN_X, DECAL_SCREEN_Y);
            spriteSheepSkin = new OneSprite(15 * 100 + 21, tiled.TileWidth, tiled.TileHeight, 7 * 8, 4 * 8, 1, 0, SCALE, DECAL_SCREEN_X, DECAL_SCREEN_Y);
            spriteEnergy = new OneSprite(0 * 100 + 100 - 32, tiled.TileWidth, tiled.TileHeight, 3 * 8, 3 * 8, 4, 40, SCALE, DECAL_SCREEN_X, DECAL_SCREEN_Y);
            for (int i = 0; i < SMOKE_SPRITES_MAX; i++)
            {
                spritesSmoke[i] = new OneSprite(3 * 100 + 100 - 32, tiled.TileWidth, tiled.TileHeight, 3 * 8, 3 * 8, 7, 110, SCALE, DECAL_SCREEN_X, DECAL_SCREEN_Y);
            }
            monsters = new Monsters(tiled.TileWidth, tiled.TileHeight, SCALE, DECAL_MAP_X, DECAL_MAP_Y, stepX, stepY);

            scrollStep = Convert.ToInt32(tiled.TileWidth * SCALE / SCROLL_MAP_DELAY);
            scrollX = 0;

            watch = new Stopwatch();
            start = 0;
            var fps = TimeSpan.FromSeconds(1.0 / 60.0);
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
                for (int i = 0; i < SMOKE_SPRITES_MAX; i++)
                    spritesSmoke[i].DoAnim(tempo);
                monsters.DoAnim(tempo, witch.X, witch.Y);
                witch.DoAnim(tempo);

                switch (Tools.GetKeyCode)
                {
                    case 123: // LEFT
                        break;
                    case 124: // RIGHT
                        break;
                    case 125: // DOWN
                        break;
                    case 126: // UP
                        break;
                    case 49: // SPACE
                        OneExplode temp = new OneExplode();
                        temp.ID = idBullet++;
                        temp.Start = DateTime.UtcNow;
                        temp.Step = 0;
                        temp.X = witch.X;
                        temp.Y = witch.Y;
                        bullets.Add(temp.ID, temp);

                        break;
                    default:
                        break;
                }

                if (Tools.KeyLeft && !Tools.KeyRight)
                {
                    scrollSpeed = witch.MoveToLeft();
                    if (scrollSpeed > 0)
                    {
                        scrollMapCount++;
                        scrollX += scrollStep;
                        if (scrollMapCount > SCROLL_MAP_DELAY)
                        {
                            scrollMapCount = 0;
                            startMapX--;
                            scrollX = 0;
                            if (startMapX < 0)
                            {
                                startMapX = tiled.MapWidth - 1;
                            }
                            monsters.MapScrollToRight();
                        }
                    }
                }

                if (Tools.KeyRight && !Tools.KeyLeft)
                {
                    scrollSpeed = witch.MoveToRight();
                    if (scrollSpeed > 0)
                    {
                        scrollMapCount++;
                        scrollX -= scrollStep;
                        if (scrollMapCount > SCROLL_MAP_DELAY)
                        {
                            scrollMapCount = 0;
                            startMapX++;
                            scrollX = 0;
                            if (startMapX >= tiled.MapWidth)
                            {
                                startMapX = 0;
                            }
                            monsters.MapScrollToLeft();
                        }
                    }
                }

                if (!Tools.KeyLeft && !Tools.KeyRight)
                {
                    witch.MoveStop();
                }

                if (Tools.KeyUp && !Tools.KeyDown)
                {
                    witch.MoveToUp();
                }

                if (Tools.KeyDown && !Tools.KeyUp)
                {
                    witch.MoveToDown();
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


            // affichage des infos
            spriteSheepSkin.Draw(canvas, Convert.ToInt32(1 * SCALE), Convert.ToInt32(1 * SCALE), tilesScale);
            spriteSheepSkin.Draw(canvas, Convert.ToInt32(255 * SCALE), Convert.ToInt32(1 * SCALE), tilesScale);
            spriteMoon.Draw(canvas, Convert.ToInt32(50 * SCALE), Convert.ToInt32(69 * SCALE), tilesScale);


            // affichage de la carte
            //Array.Clear(pixels, 0, MAX_WIDTH * MAX_HEIGHT);
            // on commence et termine l'affichage en dehors de l'écran => pour le scroll X que l'on fait de façon intermédiaire et pas par la taille d'une tile
            int currentX = startMapX - 1; // donc on commence une colonne avant
            if (currentX < 0)
            {
                // la carte boucle sur elle-même
                currentX = tiled.MapWidth - 1;
            }
            //DateTime timeStart = DateTime.UtcNow;
            // pour chaque colonne
            for (int i = -1; i < (MAP_SHOW + 1); i++) // 1 colonne avant et après
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
                        a = i * tileWidth + DECAL_MAP_X + scrollX;
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
            currentX = startMapX - 1;
            if (currentX < 0)
            {
                // la carte boucle sur elle-même
                currentX = tiled.MapWidth - 1;
            }
            // pour chaque colonne
            for (int i = -1; i < (MAP_SHOW + 1); i++)
            {
                // et chaque ligne
                for (int j = 0; j < tiled.MapHeight; j++)
                {
                    // le tile en cours
                    tile = tiled.Items[currentX, j];
                    if (tile > 0)
                    {
                        tile--;
                        int x = i * tileWidth + DECAL_MAP_X;
                        int y = j * tileHeight + DECAL_MAP_Y;
                        /*switch (tile)
                        {
                            case 68: // energy
                                //spriteEnergy.Draw(canvas, x, y, tilesScale);
                                break;
                            case 368: // smoke
                                //spritesSmoke[smokeIndex++].Draw(canvas, x - Convert.ToInt32(10 * 1), y - Convert.ToInt32(10 * 1), tilesScale);
                                break;
                            default:
                                //System.Diagnostics.Debug.WriteLine(String.Format("Item: {0}", tile));
                                break;
                        }*/
                        // un attribut particulier sur la tile ?
                        if (tiled.Tiles.ContainsKey(tile))
                        {
                            Tile t = tiled.Tiles[tile];
                            switch (t.Name)
                            {
                                case "item":
                                    switch (t.Content)
                                    {
                                        case "key_purple":
                                            break;
                                        case "key_blue":
                                            break;
                                        case "key_red":
                                            break;
                                        case "key_green":
                                            break;
                                        case "energy":
                                            spriteEnergy.Draw(canvas, x, y, tilesScale, scrollX);
                                            break;
                                        case "smoke":
                                            spritesSmoke[smokeIndex++].Draw(canvas, x - Convert.ToInt32(10 * 1), y - Convert.ToInt32(10 * 1), tilesScale, scrollX);
                                            break;
                                        case "vial":
                                            break;
                                        case "chest":
                                            break;
                                    }
                                    break;
                            }
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

            // gestion des monstres
            currentX = startMapX - 1;
            if (currentX < 0)
            {
                // la carte boucle sur elle-même
                currentX = tiled.MapWidth - 1;
            }
            // pour chaque colonne
            for (int i = -1; i < (MAP_SHOW + 1); i++)
            {
                // et chaque ligne
                for (int j = 0; j < tiled.MapHeight; j++)
                {
                    // le tile en cours
                    tile = tiled.Terrain[currentX, j];
                    if (tile > 0)
                    {
                        tile--;
                        int x = i * tileWidth + DECAL_MAP_X;
                        int y = j * tileHeight + DECAL_MAP_Y;
                        // un attribut particulier sur la tile ?
                        if (tiled.Tiles.ContainsKey(tile))
                        {
                            Tile t = tiled.Tiles[tile];
                            switch (t.Name)
                            {
                                case "generator":
                                    switch (t.Content)
                                    {
                                        case "bat_1":
                                            monsters.Generator(MonsterType.Bat_1, currentX, x, y);
                                            //spritesBat[batIndex++].Draw(canvas, x, y, tilesScale);
                                            break;
                                        case "bat_2":
                                            monsters.Generator(MonsterType.Bat_2, currentX, x, y);
                                            //spritesBat[batIndex++].Draw(canvas, x, y, tilesScale);
                                            break;
                                        case "ghost":
                                            monsters.Generator(MonsterType.Ghost, currentX, x, y);
                                            //spritesGhost[ghostIndex++].Draw(canvas, x, y, tilesScale);
                                            break;
                                    }
                                    break;
                            }
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
            monsters.Draw(canvas, tilesScale, scrollX);



            witch.Draw(canvas, tilesScale);

            /*bmpPixels.Pixels = pixels;
            bmpToShow = bmpPixels.Resize(scaleInfo, SKBitmapResizeMethod.Box);
            canvas.DrawBitmap(bmpPixels, 0, 0);*/

            /*DateTime timeStop = DateTime.UtcNow;
            TimeSpan timeElaps = timeStop - timeStart;
            if ((DateTime.UtcNow - each) > TimeSpan.FromMilliseconds(1000))
            {
                //System.Diagnostics.Debug.WriteLine(timeElaps.TotalMilliseconds);
                each = DateTime.UtcNow;
            }*/

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