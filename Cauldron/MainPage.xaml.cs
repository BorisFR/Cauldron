﻿using System;
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

        //Stopwatch watch;
        //float start;

        // Gestion de la carte de jeu
        Tiled tiled = new Tiled();
        int startMapX; // colonne de départ d'affichage de la map
        int scrollX; // scroll horizontale
        DateTime lastScroll;
        TimeSpan scrollDelay = All.FREQUENCY;

        // Jeu en pause, on bloque les animations
        bool pausedGame;


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

        // Getion des tirs de la sorcière
        OneSprite spriteBullet;
        Dictionary<int, OneObject> bullets = new Dictionary<int, OneObject>();
        TimeSpan bulletElaps = All.FREQUENCY;
        int idBullet = 0;
        int bulletDecalLeftFromWitch;
        int bulletDecalRightFromWitch;
        bool keyFireMustBeRelease;

        // déplacement des objets dans le jeu
        int stepX;
        int stepY;

        // variables génériques pour l'ensemble du programme
        int tile;
        DateTime tempo;
        float scrollSpeed;

        // pour debug FPS
        int animPerSecond;
        int framesPerSecond;
        DateTime lastFps;
        String messageFps = " ";

        // *********************************************************************

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

            stepX = Convert.ToInt32((24 / 12 / 2)); // Convert.ToInt32((24 / 12) * All.GAME_SCALE / 3); // on monte/descend d'1/6 de la taille du sprite qui vaut 17 (mode vole)
            stepY = Convert.ToInt32((17 / 6) / 2); // Convert.ToInt32((17 / 6) * All.GAME_SCALE / 2); // on monte/descend d'1/6 de la taille du sprite qui vaut 17 (mode vole)


            All.Witch = new Witch(stepX, stepY * 2);
            All.Witch.X = All.MIDDLE_MAP; //All.MAP_SHOW * All.TileWidth / 2;
            All.Witch.Y = 19 * All.TileHeight + 3;
            All.Witch.MinY = 1 * All.TileHeight;
            All.Witch.MaxY = All.Witch.Y;
            keyFireMustBeRelease = false;

            spriteBullet = new OneSprite(0 * 100 + 100 - 16, 3 * 8, 3 * 8, 4, 40);
            bulletDecalLeftFromWitch = Convert.ToInt32(3 * 8 / 2) + Convert.ToInt32(0.5f * 8 / 2);
            bulletDecalRightFromWitch = Convert.ToInt32(3 * 8 / 2) - Convert.ToInt32(0.5f * 8 / 2);

            spriteMoon = new OneSprite(15 * 100 + 30, 6 * 8, 5 * 8, 1, 0);
            spriteSheepSkin = new OneSprite(15 * 100 + 21, 7 * 8, 4 * 8, 1, 0);
            spriteEnergy = new OneSprite(0 * 100 + 100 - 32, 3 * 8, 3 * 8, 4, 20);
            for (int i = 0; i < SMOKE_SPRITES_MAX; i++)
            {
                spritesSmoke[i] = new OneSprite(3 * 100 + 100 - 32, 3 * 8, 3 * 8, 7, 105);
            }
            monsters = new Monsters(stepX, stepY);

            scrollX = 0;
            lastScroll = DateTime.UtcNow;

            //watch = new Stopwatch();
            //start = 0;
            lastFps = DateTime.UtcNow;
            //watch.Start();

            GameLoop();
        }

        // *********************************************************************
        // Game logic, proc need to be very optimized!
        // *********************************************************************
        public void GameLoop()
        {
            var fps = TimeSpan.FromSeconds(1.0 / 60.0);
            bool even = false;
            Device.StartTimer(fps, () =>
            {
                if (even && (All.GetKeyCode != 3))
                {
                    even = false;
                    return true;
                }
                even = true;
                animPerSecond++;
                // get elapsed time
                //var time = (float)watch.Elapsed.TotalMinutes;
                //watch.Restart();
                // get the elapsed rotation
                //start += (360 * time) % 360;

                if (pausedGame)
                    return true;

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
                    bullets[kvp.Key].Start = tempo;
                    bullets[kvp.Key].Step++;
                    if (bullets[kvp.Key].Step >= 4)
                        bullets[kvp.Key].Step = 0;
                }

                foreach (int k in deleteList)
                    bullets.Remove(k);


                if (All.KeyLeft && !All.KeyRight)
                {
                    if ((tempo - lastScroll) >= scrollDelay)
                    {
                        lastScroll = tempo;
                        scrollSpeed = All.Witch.MoveToLeft();
                        if (scrollSpeed > 0)
                        {
                            scrollX += Convert.ToInt32(scrollSpeed * All.GAME_SCALE);
                        }
                        while (scrollX >= All.TileWidthScale)
                        {
                            startMapX--;
                            scrollX -= All.TileWidthScale; // = 0;
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
                    if ((tempo - lastScroll) >= scrollDelay)
                    {
                        //System.Diagnostics.Debug.WriteLine(String.Format("Scroll: {0}", tempo - lastScroll));
                        lastScroll = tempo;
                        scrollSpeed = All.Witch.MoveToRight();
                        if (scrollSpeed > 0)
                        {
                            scrollX -= Convert.ToInt32(scrollSpeed * All.GAME_SCALE);
                        }
                        while (scrollX <= All.TileWidthScale)
                        {
                            startMapX++;
                            scrollX += All.TileWidthScale; // = 0;
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

                if (All.KeyUp && !All.KeyDown && !All.KeySpace && !keyFireMustBeRelease)
                {
                    All.Witch.MoveToUp();
                }

                if (All.KeyDown && !All.KeyUp && !All.KeySpace & !keyFireMustBeRelease)
                {
                    All.Witch.MoveToDown();
                }

                if (All.KeySpace)
                {
                    //stopRedraw = false;
                    //All.ShowPixel = false;
                    if (All.Witch.IsFlying)
                    {
                        if (!keyFireMustBeRelease)
                        {
                            // génération d'un tir de la sorcière
                            OneObject temp = new OneObject();
                            temp.ID = idBullet++;
                            temp.TimeToLive = 60;
                            temp.Start = tempo;
                            temp.Step = 0;
                            temp.Y = All.Witch.Y;
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
                                temp.X = All.Witch.BulletX - bulletDecalLeftFromWitch;
                            else
                                temp.X = All.Witch.BulletX - bulletDecalRightFromWitch;
                            if (temp.Moving != MovingDirection.None)
                                bullets.Add(temp.ID, temp);
                            keyFireMustBeRelease = true;
                        }
                    }
                }
                else
                {
                    keyFireMustBeRelease = false;
                }

                theCanvas.InvalidateSurface();
                return true;
            });
        }

        // *********************************************************************

        SKRect drawSource = new SKRect();
        SKRect drawDestination = new SKRect();
        SKCanvas canvas;
        SKPaint background = new SKPaint { Style = SKPaintStyle.Fill, Color = SKColor.Parse("#333333") };
        SKPaint textFPS = new SKPaint
        {
            IsAntialias = true,
            Style = SKPaintStyle.Fill,
            Color = SKColors.White,
            TextSize = 20
        };

        // *********************************************************************
        // Draw the entire screen (each game loop), so need to be optimized too!
        // *********************************************************************
        public void OnPainting(object sender, SKPaintSurfaceEventArgs e)
        {
            canvas = e.Surface.Canvas;
            canvas.Clear(SKColors.Black);

            canvas.DrawRect(All.DECAL_MAP_X,
                            All.DECAL_MAP_Y,
                            All.MAP_SHOW * All.TileWidth * All.GAME_SCALE,
                            21 * All.TileWidth * All.GAME_SCALE,
                            background);
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
            */

            tempo = DateTime.UtcNow;
            framesPerSecond++;
            if ((tempo - lastFps) >= All.ONE_SECOND)
            {
                messageFps = String.Format("{0} fps, {1} anim", framesPerSecond, animPerSecond);
                animPerSecond = 0;
                framesPerSecond = 0;
                lastFps = tempo;
            }
            canvas.DrawText(messageFps, 0, 50, textFPS);


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
                        drawSource = new SKRect(x, y, x + All.TileWidthScale, y + All.TileHeightScale);
                        // position de la cible
                        a = i * All.TileWidthScale + All.DECAL_MAP_X + scrollX;
                        b = j * All.TileHeightScale + All.DECAL_MAP_Y;
                        drawDestination = new SKRect(a, b, a + All.TileWidthScale, b + All.TileHeightScale);
                        // on effectue l'affichage
                        canvas.DrawBitmap(All.TilesScale, drawSource, drawDestination);
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

            canvas.DrawLine(0 * All.GAME_SCALE + All.DECAL_MAP_X, 0, 0 * All.GAME_SCALE + All.DECAL_MAP_X, 1000, paintRed);
            canvas.DrawLine(All.SPEED_LEFT_MAX * All.GAME_SCALE + All.DECAL_MAP_X, 0, All.SPEED_LEFT_MAX * All.GAME_SCALE + All.DECAL_MAP_X, 1000, textFPS);
            canvas.DrawLine(All.SPEED_LEFT_3 * All.GAME_SCALE + All.DECAL_MAP_X, 0, All.SPEED_LEFT_3 * All.GAME_SCALE + All.DECAL_MAP_X, 1000, textFPS);
            canvas.DrawLine(All.SPEED_LEFT_2 * All.GAME_SCALE + All.DECAL_MAP_X, 0, All.SPEED_LEFT_2 * All.GAME_SCALE + All.DECAL_MAP_X, 1000, paintYellow);
            canvas.DrawLine(All.SPEED_LEFT_1 * All.GAME_SCALE + All.DECAL_MAP_X, 0, All.SPEED_LEFT_1 * All.GAME_SCALE + All.DECAL_MAP_X, 1000, paintCyan);
            canvas.DrawLine(All.MIDDLE_MAP * All.GAME_SCALE + All.DECAL_MAP_X, 0, All.MIDDLE_MAP * All.GAME_SCALE + All.DECAL_MAP_X, 1000, paintRed);
            canvas.DrawLine(All.SPEED_RIGHT_1 * All.GAME_SCALE + All.DECAL_MAP_X, 0, All.SPEED_RIGHT_1 * All.GAME_SCALE + All.DECAL_MAP_X, 1000, paintCyan);
            canvas.DrawLine(All.SPEED_RIGHT_2 * All.GAME_SCALE + All.DECAL_MAP_X, 0, All.SPEED_RIGHT_2 * All.GAME_SCALE + All.DECAL_MAP_X, 1000, paintYellow);
            canvas.DrawLine(All.SPEED_RIGHT_3 * All.GAME_SCALE + All.DECAL_MAP_X, 0, All.SPEED_RIGHT_3 * All.GAME_SCALE + All.DECAL_MAP_X, 1000, textFPS);
            canvas.DrawLine(All.SPEED_RIGHT_MAX * All.GAME_SCALE + All.DECAL_MAP_X, 0, All.SPEED_RIGHT_MAX * All.GAME_SCALE + All.DECAL_MAP_X, 1000, textFPS);
            canvas.DrawLine(All.MAP_SHOW * All.TileWidth * All.GAME_SCALE + All.DECAL_MAP_X, 0, All.MAP_SHOW * All.TileWidth * All.GAME_SCALE + All.DECAL_MAP_X, 1000, paintRed);

            /*bmpPixels.Pixels = pixels;
            bmpToShow = bmpPixels.Resize(scaleInfo, SKBitmapResizeMethod.Box);
            canvas.DrawBitmap(bmpPixels, 0, 0);*/
        }

        SKPaint paintYellow = new SKPaint() { Color = SKColors.Yellow };
        SKPaint paintCyan = new SKPaint() { Color = SKColors.Cyan };
        SKPaint paintRed = new SKPaint() { Color = SKColors.Red };

        // *********************************************************************

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