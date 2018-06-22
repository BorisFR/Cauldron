using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
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
        SKRect source = new SKRect();
        SKRect dest = new SKRect();
        SKCanvas canvas;
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


        //SKColor[] pixels;
        //SKBitmap bmpPixels;
        //const int MAX_WIDTH = 640;
        //const int MAX_HEIGHT = 300;
        //SKBitmap bmpToShow;

        OneSprite spriteMoon;
        OneSprite spriteSheepSkin;
        OneSprite spriteEnergy;
        const int SMOKE_SPRITES_MAX = 1;
        OneSprite[] spritesSmoke = new OneSprite[SMOKE_SPRITES_MAX];
        int smokeIndex;
        Monsters monsters;
        DateTime tempo;

        OneSprite spriteBullet;
        Dictionary<int, OneObject> bullets = new Dictionary<int, OneObject>();
        TimeSpan bulletElaps = TimeSpan.FromMilliseconds(30);
        int idBullet = 0;
        bool keyFireRelease;
        int bulletDecalXLeft;
        int bulletDecalXRight;
        int bulletDecalY;

        int stepX;
        int stepY;

        public MainPage()
        {
            InitializeComponent();

            // on charge la définition des tiles
            tiled.ProcessTileSet(All.GetStream("Cauldron.tsx"));
            // on charge le plan TILED
            tiled.Load(All.GetStream("Cauldron.tmx"));
            // on charge l'image de tiles
            SKImageInfo desiredInfo = new SKImageInfo(800, 800, SKImageInfo.PlatformColorType, SKAlphaType.Premul);
            SKManagedStream stream = new SKManagedStream(All.GetStream("tiles.png"));
            All.Tiles = SKBitmap.Decode(stream, desiredInfo);
            // on scale cette image
            SKImageInfo scaleInfo = new SKImageInfo(Convert.ToInt32(All.Tiles.Width * All.GAME_SCALE), Convert.ToInt32(All.Tiles.Height * All.GAME_SCALE));
            All.TilesScale = All.Tiles.Resize(scaleInfo, SKBitmapResizeMethod.Box); // pas d'amélioration graphique
            // dimension d'un tile
            All.TileWidth = tiled.TileWidth;
            All.TileHeight = tiled.TileHeight;
            All.TileWidthScale = Convert.ToInt32(All.TileWidth * All.GAME_SCALE);
            All.TileHeightScale = Convert.ToInt32(All.TileHeight * All.GAME_SCALE);

            startMapX = tiled.StartHouse - 10; // + 450;
            //pixels = new SKColor[MAX_WIDTH * MAX_HEIGHT];
            //bmpPixels = new SKBitmap(MAX_WIDTH, MAX_HEIGHT);

            stepX = Convert.ToInt32((24 / 12)); // Convert.ToInt32((24 / 12) * All.GAME_SCALE / 3); // on monte/descend d'1/6 de la taille du sprite qui vaut 17 (mode vole)
            stepY = Convert.ToInt32((17 / 6) / 2); // Convert.ToInt32((17 / 6) * All.GAME_SCALE / 2); // on monte/descend d'1/6 de la taille du sprite qui vaut 17 (mode vole)


            All.Witch = new Witch(stepX, stepY * 2);
            All.Witch.X = 17 * 8;
            All.Witch.Y = 19 * 8 + 3;
            All.Witch.MinY = 0;
            All.Witch.MaxY = All.Witch.Y; // All.Witch.ScaleY;
            keyFireRelease = true;

            spriteBullet = new OneSprite(0 * 100 + 100 - 16, 3 * 8, 3 * 8, 4, 40);
            bulletDecalXLeft = Convert.ToInt32(3 * 8 / 2) + Convert.ToInt32(0.5f * 8 / 2);
            bulletDecalXRight = Convert.ToInt32(3 * 8 / 2) - Convert.ToInt32(0.5f * 8 / 2);
            bulletDecalY = Convert.ToInt32(3 * 8 / 2); ;

            spriteMoon = new OneSprite(15 * 100 + 30, 6 * 8, 5 * 8, 1, 0);
            spriteSheepSkin = new OneSprite(15 * 100 + 21, 7 * 8, 4 * 8, 1, 0);
            spriteEnergy = new OneSprite(0 * 100 + 100 - 32, 3 * 8, 3 * 8, 4, 40);
            for (int i = 0; i < SMOKE_SPRITES_MAX; i++)
            {
                spritesSmoke[i] = new OneSprite(3 * 100 + 100 - 32, 3 * 8, 3 * 8, 7, 110);
            }
            monsters = new Monsters(stepX, stepY);

            scrollStep = Convert.ToInt32(tiled.TileWidth / SCROLL_MAP_DELAY); // Convert.ToInt32(tiled.TileWidth * All.GAME_SCALE / SCROLL_MAP_DELAY);
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
                monsters.DoAnim(tempo, All.Witch.X, All.Witch.Y);
                All.Witch.DoAnim(tempo);


                List<int> deleteList = new List<int>();

                foreach (var kvp in bullets.ToList<KeyValuePair<int, OneObject>>())
                {
                    bullets[kvp.Key].TimeToLive--;
                    if (bullets[kvp.Key].TimeToLive == 0)
                    {
                        deleteList.Add(kvp.Key);
                        continue;
                    }
                    switch (kvp.Value.Moving)
                    {
                        case MovingDirection.ToLeft:
                            bullets[kvp.Key].X -= stepX * 3;
                            break;
                        case MovingDirection.ToRight:
                            bullets[kvp.Key].X += stepX * 3;
                            break;
                        case MovingDirection.DiagUpLeft:
                            bullets[kvp.Key].X -= stepX * 3;
                            bullets[kvp.Key].Y -= stepX * 2;
                            break;
                        case MovingDirection.DiagDownLeft:
                            bullets[kvp.Key].X -= stepX * 3;
                            bullets[kvp.Key].Y += stepX * 2;
                            break;
                        case MovingDirection.DiagUpRight:
                            bullets[kvp.Key].X += stepX * 3;
                            bullets[kvp.Key].Y -= stepX * 2;
                            break;
                        case MovingDirection.DiagDownRight:
                            bullets[kvp.Key].X += stepX * 3;
                            bullets[kvp.Key].Y += stepX * 2;
                            break;
                    }
                    if ((tempo - kvp.Value.Start) < bulletElaps)
                        continue;
                    bullets[kvp.Key].Start = DateTime.UtcNow;
                    bullets[kvp.Key].Step++;
                    if (bullets[kvp.Key].Step >= 4)
                        bullets[kvp.Key].Step = 0;
                }

                foreach (int k in deleteList)
                    bullets.Remove(k);


                if (All.KeyLeft && !All.KeyRight)
                {
                    scrollSpeed = All.Witch.MoveToLeft();
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

                if (All.KeyRight && !All.KeyLeft)
                {
                    scrollSpeed = All.Witch.MoveToRight();
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

                if (!All.KeyLeft && !All.KeyRight)
                {
                    All.Witch.MoveStop();
                }

                if (All.KeyUp && !All.KeyDown && !All.KeySpace && keyFireRelease)
                {
                    All.Witch.MoveToUp();
                }

                if (All.KeyDown && !All.KeyUp && !All.KeySpace & keyFireRelease)
                {
                    All.Witch.MoveToDown();
                }

                if (All.KeySpace)
                {
                    stopRedraw = false;
                    All.ShowPixel = false;
                    if (All.Witch.IsFlying)
                    {
                        if (keyFireRelease)
                        {
                            // génération d'un tir de la sorcière
                            OneObject temp = new OneObject();
                            temp.ID = idBullet++;
                            temp.TimeToLive = 60;
                            temp.Start = DateTime.UtcNow;
                            temp.Step = 0;
                            temp.Y = All.Witch.BulletY - bulletDecalY;
                            temp.Moving = All.Witch.Direction;
                            if (All.KeyUp)
                            {
                                if (temp.Moving == MovingDirection.ToLeft)
                                    temp.Moving = MovingDirection.DiagUpLeft;
                                else
                                    temp.Moving = MovingDirection.DiagUpRight;
                            }
                            if (All.KeyDown)
                            {
                                if (temp.Moving == MovingDirection.ToLeft)
                                    temp.Moving = MovingDirection.DiagDownLeft;
                                else
                                    temp.Moving = MovingDirection.DiagDownRight;
                            }
                            if (temp.Moving == MovingDirection.ToLeft)
                                temp.X = All.Witch.BulletX - bulletDecalXLeft;
                            else
                                temp.X = All.Witch.BulletX - bulletDecalXRight;
                            if (temp.Moving != MovingDirection.None)
                                bullets.Add(temp.ID, temp);
                            keyFireRelease = false;
                        }
                    }
                }
                else
                {
                    keyFireRelease = true;
                }

                theCanvas.InvalidateSurface();
                return true;
            });
        }

        DateTime each = DateTime.UtcNow;
        bool stopRedraw;
        SKPaint paint = new SKPaint { Style = SKPaintStyle.Fill, Color = SKColors.DarkRed };

        public void OnPainting(object sender, SKPaintSurfaceEventArgs e)
        {
            if (stopRedraw)
                return;
            // we get the current surface from the event args
            //var surface = e.Surface;
            // then we get the canvas that we can draw on
            canvas = e.Surface.Canvas;
            // clear the canvas / view
            canvas.Clear(SKColors.Black);

            canvas.DrawRect(All.DECAL_MAP_X, All.DECAL_MAP_Y, All.MAP_SHOW * All.TileWidth * All.GAME_SCALE, 21 * All.TileWidth * All.GAME_SCALE, paint);
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
            spriteSheepSkin.Draw(canvas, -1 * All.TileWidth, -2 * All.TileHeight - 0);
            spriteSheepSkin.Draw(canvas, (All.MAP_SHOW - 7 + 1) * All.TileWidth, -2 * All.TileHeight - 0);
            spriteMoon.Draw(canvas, 5 * All.TileWidth, 6 * All.TileHeight);


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
            for (int i = -1; i < (All.MAP_SHOW + 1); i++) // 1 colonne avant et après
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
                        x = (tile % 100) * All.TileWidthScale;
                        y = (tile / 100) * All.TileHeightScale;
                        source = new SKRect(x, y, x + All.TileWidthScale, y + All.TileHeightScale);
                        // position de la cible
                        a = i * All.TileWidthScale + All.DECAL_MAP_X + scrollX;
                        b = j * All.TileHeightScale + All.DECAL_MAP_Y;
                        dest = new SKRect(a, b, a + All.TileWidthScale, b + All.TileHeightScale);
                        // on effectue l'affichage
                        canvas.DrawBitmap(All.TilesScale, source, dest);
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
            for (int i = -1; i < (All.MAP_SHOW + 1); i++)
            {
                // et chaque ligne
                for (int j = 0; j < tiled.MapHeight; j++)
                {
                    // le tile en cours
                    tile = tiled.Items[currentX, j];
                    if (tile > 0)
                    {
                        tile--;
                        int x = i * All.TileWidth;
                        int y = j * All.TileHeight;

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
                                            spriteEnergy.Draw(canvas, x, y, scrollX);
                                            break;
                                        case "smoke":
                                            spritesSmoke[smokeIndex++].Draw(canvas, x, y, scrollX);
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
            for (int i = -1; i < (All.MAP_SHOW + 1); i++)
            {
                // et chaque ligne
                for (int j = 0; j < tiled.MapHeight; j++)
                {
                    // le tile en cours
                    tile = tiled.Terrain[currentX, j];
                    if (tile > 0)
                    {
                        tile--;
                        int x = i * All.TileWidth;
                        int y = j * All.TileHeight;
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
                                            break;
                                        case "bat_2":
                                            monsters.Generator(MonsterType.Bat_2, currentX, x, y);
                                            break;
                                        case "ghost":
                                            monsters.Generator(MonsterType.Ghost, currentX, x, y);
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
            monsters.Draw(canvas, scrollX);


            foreach (var kvp in bullets)
            {
                spriteBullet.StepAnim = kvp.Value.Step;
                spriteBullet.Draw(canvas, kvp.Value.X, kvp.Value.Y);
            }


            All.Witch.Draw(canvas);

            //canvas.DrawPoint(All.Witch.ScaleX + All.DECAL_MAP_X, All.Witch.ScaleY + All.DECAL_MAP_Y, colorPink);
            //canvas.DrawPoint(All.Witch.BulletX + All.DECAL_MAP_X, All.Witch.BulletY + All.DECAL_MAP_Y, colorRed);

            if (All.ShowPixel)
            {
                canvas.DrawPoint(All.ShowPixelX + All.DECAL_MAP_X, All.ShowPixelY + All.DECAL_MAP_Y, coloCyan);
                stopRedraw = true;
            }

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

        SKColor colorPink = SKColor.Parse("#FF69B4");
        SKColor colorRed = SKColor.Parse("#FF0000");
        SKColor coloCyan = SKColor.Parse("#00FFFF");

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