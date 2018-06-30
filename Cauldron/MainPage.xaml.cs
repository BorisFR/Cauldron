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

        // Gestion de la carte de jeu
        Tiled tiled = new Tiled();
        Tiled tiledHouse = new Tiled();
        Map map;
        int startMapX; // colonne de départ d'affichage de la map
        int scrollX; // scroll horizontale
        DateTime lastScroll;
        TimeSpan scrollDelay = All.FREQUENCY;
        int mapMinX;
        int mapMaxX;
        //int currentMapWidth;
        int currentMapHeight;
        Int16[,] currentTerrain;
        public Int16[,] currentItems;
        bool showSky;

        // Jeu en pause, on bloque les animations
        bool blockDisplay;
        bool pausedGame;
        TimeSpan gameSpeed = All.FREQUENCY;
        DateTime lastGameAnim;


        //SKColor[] pixels;
        //SKBitmap bmpPixels;
        //const int MAX_WIDTH = 640;
        //const int MAX_HEIGHT = 300;
        //SKBitmap bmpToShow;

        Typo typo;

        OneSprite spriteMoon;
        OneSprite spriteSheepSkin;
        OneSprite spriteKeyRed;
        OneSprite spriteKeyBlue;
        OneSprite spriteKeyGreen;
        OneSprite spriteKeyPink;

        OneSprite spriteWater1;
        OneSprite spriteWater2;
        TimeSpan tsWater = TimeSpan.FromMilliseconds(2500);
        DateTime lastChangeWater;
        bool isWater1;


        OneSprite spritePlant1;
        OneSprite spritePlant2;

        OneSprite spriteEnergy;
        const int SMOKE_SPRITES_MAX = 30; // visible en même temps
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
        int scrollSpeed;

        // pour debug FPS
        int animPerSecond;
        int framesPerSecond;
        DateTime lastFps;
        String messageFps = " ";

        // *********************************************************************

        public MainPage()
        {
            InitializeComponent();

            theCanvas.SizeChanged += TheCanvas_SizeChanged;
            theCanvas.Touch += TheCanvas_Touch;

            //pixels = new SKColor[MAX_WIDTH * MAX_HEIGHT];
            //bmpPixels = new SKBitmap(MAX_WIDTH, MAX_HEIGHT);

            // on charge la définition des tiles
            tiled.ProcessTileSet(All.GetStream("Cauldron.tsx"));
            // on charge le plan TILED
            tiled.Load(All.GetStream("Cauldron.tmx"));
            // on charge la maison
            tiledHouse.Load(All.GetStream("Cauldron_House.tmx"));

            // on charge l'image de tiles
            SKImageInfo desiredInfo = new SKImageInfo(800, 800, SKImageInfo.PlatformColorType, SKAlphaType.Premul);
            SKManagedStream stream = new SKManagedStream(All.GetStream("tiles.png"));
            All.Tiles = SKBitmap.Decode(stream, desiredInfo);
            All.TilesScaleImage = SKImage.FromBitmap(All.Tiles);
            // on scale cette image
            //SKImageInfo scaleInfo = new SKImageInfo(Convert.ToInt32(All.Tiles.Width * All.GAME_SCALE), Convert.ToInt32(All.Tiles.Height * All.GAME_SCALE));
            //All.TilesScale = All.Tiles.Resize(scaleInfo, SKBitmapResizeMethod.Box); // pas d'amélioration graphique
            // dimension d'un tile
            All.TileWidth = tiled.TileWidth;
            All.TileHeight = tiled.TileHeight;
            All.TileWidthScale = Convert.ToInt32(All.TileWidth * All.GAME_SCALE);
            All.TileHeightScale = Convert.ToInt32(All.TileHeight * All.GAME_SCALE);

            typo = new Typo();

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

            spriteKeyRed = new OneSprite(0 * 100 + 64, 3 * 8, 1 * 8, 1, 0);
            spriteKeyBlue = new OneSprite(1 * 100 + 64, 3 * 8, 1 * 8, 1, 0);
            spriteKeyPink = new OneSprite(2 * 100 + 64, 3 * 8, 1 * 8, 1, 0);
            spriteKeyGreen = new OneSprite(3 * 100 + 64, 3 * 8, 1 * 8, 1, 0);
            spriteMoon = new OneSprite(15 * 100 + 30, 6 * 8, 5 * 8, 1, 0);
            spriteSheepSkin = new OneSprite(15 * 100 + 21, 7 * 8, 4 * 8, 1, 0);
            spriteEnergy = new OneSprite(0 * 100 + 100 - 32, 3 * 8, 3 * 8, 4, 20);
            for (int i = 0; i < SMOKE_SPRITES_MAX; i++)
            {
                spritesSmoke[i] = new OneSprite(3 * 100 + 100 - 32, 3 * 8, 3 * 8, 7, 105);
            }
            spriteWater1 = new OneSprite(44 * 100, 2 * All.TileWidth, 1 * All.TileHeight, 16, 100);
            spriteWater2 = new OneSprite(46 * 100, 2 * All.TileWidth, 1 * All.TileHeight, 16, 100);
            isWater1 = true;

            spritePlant1 = new OneSprite(44 * 100 + 100 - 32, 4 * All.TileWidth, 3 * All.TileHeight, 8, 147, withSeparator: false);
            spritePlant2 = new OneSprite(49 * 100 + 100 - 32, 4 * All.TileWidth, 3 * All.TileHeight, 8, 135, withSeparator: false);
            monsters = new Monsters(stepX, stepY);


            //StartInHouse();
            StartOutside();


            scrollX = 0;
            lastScroll = DateTime.UtcNow;
            lastGameAnim = DateTime.UtcNow;
            lastFps = DateTime.UtcNow;
            lastChangeWater = DateTime.UtcNow;

            GameLoop();
        }

        void TheCanvas_SizeChanged(object sender, EventArgs e)
        {
            //1600 - 40 * 8 - 34 * 8 = 1600 -320 - 272 = 1008
            float tempX = 1.0f * (float)theCanvas.Width / 320.0f;
            // 998 - 23 * 8 - 23 * 8 - 3 = 998 - 184 - 3 = 811
            float tempY = 1.0f * (float)theCanvas.Height / (200.0f + 22.0f);
            if (tempX < tempY)
            {
                theScale = tempX;
                System.Diagnostics.Debug.WriteLine(String.Format("SizeChanged: {0}x{1} => X {2} ({3})", theCanvas.Width, theCanvas.Height, tempX, tempY));
            }
            else
            {
                theScale = tempY;
                // MAP_SHOW standard à 38, avec 1 de chaque côté, soit 40 au total
                System.Diagnostics.Debug.WriteLine(String.Format("SizeChanged: {0}x{1} => Y {3} ({2})", theCanvas.Width, theCanvas.Height, tempX, tempY));
            }
            int deltaTiles = Convert.ToInt32((theCanvas.Width) / (8.0f * theScale));
            All.MAP_SHOW = deltaTiles + 1; // -2 => debord de 1 de chaque côté
        }

        void TheCanvas_Touch(object sender, SKTouchEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine(String.Format("Touched: {0}", e.Location));
        }


        public void StartOutside()
        {
            blockDisplay = true;
            map = Map.Outside;
            showSky = true;
            mapMinX = tiled.MapMinX;
            mapMaxX = tiled.MapMaxX;
            //currentMapWidth = mapMaxX - mapMinX; // tiled.MapWidth;
            currentMapHeight = tiled.MapHeight;
            currentTerrain = tiled.Terrain;
            currentItems = tiled.Items;
            startMapX = tiled.StartHouse - 6 + 1100; // + 900;// + 450;
            All.Witch.X = All.MIDDLE_MAP;
            All.Witch.Y = tiled.StartY * All.TileHeight + 3;
            All.Witch.MinY = 1 * All.TileHeight;
            All.Witch.MaxY = All.Witch.Y;
            All.Witch.CouldFly = true;
            All.Witch.DoAlive();
            All.Witch.DoWalk();
            blockDisplay = false;
        }

        public void StartInHouse()
        {
            blockDisplay = true;
            map = Map.InHouse;
            showSky = false;
            mapMinX = tiledHouse.MapMinX;
            mapMaxX = tiledHouse.MapMaxX;
            //currentMapWidth = tiledHouse.MapWidth;
            currentMapHeight = tiledHouse.MapHeight;
            currentTerrain = tiledHouse.Terrain;
            currentItems = tiledHouse.Items;
            startMapX = mapMinX;
            All.Witch.X = tiledHouse.StartHouse * All.TileWidth - 2;
            All.Witch.Y = tiledHouse.StartY * All.TileHeight + 3;
            All.Witch.MinY = All.Witch.Y;
            All.Witch.MaxY = All.Witch.Y;
            All.Witch.CouldFly = false;
            All.Witch.DoAlive();
            All.Witch.DoPotion();
            blockDisplay = false;
        }

        // *********************************************************************
        // Game logic, proc need to be very optimized!
        // *********************************************************************
        public void GameLoop()
        {
            blockDisplay = false;
            var fps = TimeSpan.FromSeconds(1.0 / 60.0); // mais 1 fois sur 2
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

                if (All.KeyEsc)
                    return false;

                if (All.KeyMap)
                {
                    All.ClearKeyMap();
                    scrollX = 0;
                    scrollSpeed = 0;
                    monsters.RemoveMonsters();
                    switch (map)
                    {
                        case Map.InHouse:
                            StartOutside();
                            return true;
                        case Map.Outside:
                            StartInHouse();
                            return true;
                    }
                }

                if (All.KeyPause)
                {
                    All.ClearKeyPause();
                    pausedGame = !pausedGame;
                }

                if (pausedGame)
                    return true;

                tempo = DateTime.UtcNow; // pour que tout soit bien synchronisé
                spriteEnergy.DoAnim(tempo);
                spriteWater1.DoAnim(tempo);
                spriteWater2.DoAnim(tempo);
                for (int i = 0; i < SMOKE_SPRITES_MAX; i++)
                    spritesSmoke[i].DoAnim(tempo);
                monsters.DoAnim(tempo, All.Witch.X, All.Witch.Y);
                All.Witch.DoAnim(tempo);
                spritePlant1.DoAnim(tempo);
                spritePlant2.DoAnim(tempo);

                if ((tempo - lastChangeWater) > tsWater)
                {
                    lastChangeWater = tempo;
                    isWater1 = !isWater1;
                }

                if ((tempo - lastScroll) >= scrollDelay)
                {
                    if (All.KeyLeft && !All.KeyRight)
                    {
                        lastScroll = tempo;
                        scrollSpeed = Convert.ToInt32(All.Witch.MoveToLeft() * All.GAME_SCALE);
                    }

                    if (All.KeyRight && !All.KeyLeft)
                    {
                        lastScroll = tempo;
                        scrollSpeed = Convert.ToInt32(-All.Witch.MoveToRight() * All.GAME_SCALE);
                    }
                }


                if ((tempo - lastGameAnim) >= gameSpeed)
                {
                    lastGameAnim = tempo;

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


                    scrollX += scrollSpeed;
                    if (All.Witch.IsWalking)
                        scrollSpeed = 0;

                    //System.Diagnostics.Debug.WriteLine(String.Format("ScrollX: {0} + {1} ({2})", startMapX, scrollX, scrollSpeed));
                    while (scrollX >= All.TileWidthScale)
                    {
                        startMapX--;
                        scrollX -= All.TileWidthScale;
                        //System.Diagnostics.Debug.WriteLine(String.Format("* ScrollX: {0} + {1} ({2})", startMapX, scrollX, scrollSpeed));
                        if (startMapX <= mapMinX)// 0)
                        {
                            startMapX = mapMaxX - 1; // currentMapWidth - 1;
                        }
                        monsters.MapScrollToRight();
                        /*if (scrollX != 0)
                        {
                            System.Diagnostics.Debug.WriteLine(String.Format("> *1 ScrollX: {0}", scrollX));
                        }*/
                    }
                    while (scrollX <= -All.TileWidthScale)
                    {
                        //System.Diagnostics.Debug.WriteLine(String.Format("*2 ScrollX: {0}", scrollX));
                        startMapX++;
                        scrollX += All.TileWidthScale;
                        if (startMapX >= mapMaxX) // currentMapWidth)
                        {
                            startMapX = mapMinX + 1; // 0;
                        }
                        monsters.MapScrollToLeft();
                        /*if (scrollX != 0)
                        {
                            System.Diagnostics.Debug.WriteLine(String.Format("> *2 ScrollX: {0}", scrollX));
                        }*/
                    }


                    if (All.KeyUp && !All.KeyDown && !All.KeySpace && !keyFireMustBeRelease)
                    {
                        //if (All.Witch.CouldFly)
                        All.Witch.MoveToUp();
                    }

                    if (All.KeyDown && !All.KeyUp && !All.KeySpace & !keyFireMustBeRelease)
                    {
                        //if (All.Witch.CouldFly)
                        All.Witch.MoveToDown();
                    }


                } // lastGameAnim

                if (!All.KeyLeft && !All.KeyRight)
                {
                    All.Witch.MoveStop();
                }

                if (All.KeySpace)
                {
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
                            All.LooseMagic(MagicLoose.Shoot);
                        }
                    }
                }
                else
                {
                    keyFireMustBeRelease = false;
                }

                if (!blockDisplay)
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
            Color = SKColors.BlueViolet,
            TextSize = 20
        };


        double baseWidth = 0;
        bool isFirst = true;
        float theScale = 1.0f;

        // *********************************************************************
        // Draw the entire screen (each game loop), so need to be optimized too!
        // *********************************************************************
        public void OnPainting(object sender, SKPaintSurfaceEventArgs e)
        {
            if (blockDisplay)
                return;
            canvas = e.Surface.Canvas;
            canvas.Clear(SKColors.Black);

            /*SKSurface aa = SKSurface.Create(320, 200, SKColorType.Rgba8888, SKAlphaType.Premul, new SKSurfaceProps() { PixelGeometry = SKPixelGeometry.RgbHorizontal });
            SKCanvas zz = aa.Canvas;
            // make all the draw on this canvas
            SKImage ee = aa.Snapshot();
            SKBitmap rr = SKBitmap.FromImage(ee);
            // do a resize of the bitmap
            canvas.DrawBitmap of the resize*/


            if (isFirst)
            {
                isFirst = false;
                System.Diagnostics.Debug.WriteLine(String.Format("START WIDTH from {0} to {1}", baseWidth, theCanvas.Width));
                baseWidth = theCanvas.Width;
            }
            if (theCanvas.Width != baseWidth)
            {
                System.Diagnostics.Debug.WriteLine(String.Format("WIDTH from {0} to {1}", baseWidth, theCanvas.Width));
                baseWidth = theCanvas.Width;
            }

            canvas.Scale(theScale);
            /*canvas.DrawRect(All.DECAL_MAP_X,
                            All.DECAL_MAP_Y,
                            All.MAP_SHOW * All.TileWidth * All.GAME_SCALE,
                            21 * All.TileWidth * All.GAME_SCALE,
                            background);*/
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


            // affichage du décor
            if (showSky)
            {
                spriteMoon.Draw(canvas, 5 * All.TileWidth, 6 * All.TileHeight);
            }

            // affichage de la carte
            //Array.Clear(pixels, 0, MAX_WIDTH * MAX_HEIGHT);
            // on commence et termine l'affichage en dehors de l'écran => pour le scroll X que l'on fait de façon intermédiaire et pas par la taille d'une tile
            int currentX = startMapX - 1; // donc on commence une colonne avant
            if (currentX <= mapMinX)
            {
                // la carte boucle sur elle-même
                currentX = mapMaxX - 1; // currentMapWidth - 1;
            }
            monsters.DrawOnlyShark(canvas, scrollX);
            // pour chaque colonne
            for (int i = -1; i < (All.MAP_SHOW + 1); i++) // 1 colonne avant et après
            {
                // et chaque ligne
                for (int j = 0; j < currentMapHeight; j++)
                {
                    // le tile en cours
                    tile = currentTerrain[currentX, j];
                    if (tile > 0)
                    {
                        tile--;
                        //CopyPixels(tile, i, j);

                        int x, y, a, b;
                        bool generator = false;

                        // un attribut particulier sur la tile ?
                        if (tiled.Tiles.ContainsKey(tile))
                        {
                            Tile t = tiled.Tiles[tile];
                            switch (t.Name)
                            {
                                case "generator":
                                    switch (t.Content)
                                    {

                                        case "shark":
                                            monsters.Generator(MonsterType.Shark, currentX, i * All.TileWidth, j * All.TileHeight);
                                            generator = true;
                                            break;
                                    }
                                    break;
                            }
                        }
                        if (!generator)
                        {
                            // position de la source
                            x = (tile % 100) * All.TileWidthScale;
                            y = (tile / 100) * All.TileHeightScale;
                            drawSource = new SKRect(x, y, x + All.TileWidthScale, y + All.TileHeightScale);
                            // position de la cible
                            a = i * All.TileWidthScale + All.DECAL_MAP_X + scrollX;
                            b = j * All.TileHeightScale + All.DECAL_MAP_Y;
                            drawDestination = new SKRect(a, b, a + All.TileWidthScale, b + All.TileHeightScale);
                            // on effectue l'affichage
                            //canvas.DrawBitmap(All.TilesScale, drawSource, drawDestination);
                            canvas.DrawImage(All.TilesScaleImage, drawSource, drawDestination);
                        }
                    }
                }
                // colonne suivante
                currentX++;
                if (currentX >= mapMaxX)
                {
                    // la carte boucle sur elle-même
                    currentX = mapMinX + 1;
                }
            }

            // affichage des items
            smokeIndex = 0;
            currentX = startMapX - 1;
            if (currentX <= mapMinX)
            {
                currentX = mapMaxX - 1;
            }
            // à faire 3x pour éviter le popup des objets qui sont large
            currentX--;
            if (currentX <= mapMinX)
            {
                currentX = mapMaxX - 1;
            }
            currentX--;
            if (currentX <= mapMinX)
            {
                currentX = mapMaxX - 1;
            }
            // pour chaque colonne
            for (int i = -3; i < (All.MAP_SHOW + 2); i++)
            {
                // et chaque ligne
                for (int j = 0; j < currentMapHeight; j++)
                {
                    // le tile en cours
                    tile = currentItems[currentX, j];
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
                                            spriteKeyPink.Draw(canvas, x - All.TileWidth, y, scrollX);
                                            break;
                                        case "key_blue":
                                            spriteKeyBlue.Draw(canvas, x - All.TileWidth, y, scrollX);
                                            break;
                                        case "key_red":
                                            spriteKeyRed.Draw(canvas, x - All.TileWidth, y, scrollX);
                                            break;
                                        case "key_green":
                                            spriteKeyGreen.Draw(canvas, x - All.TileWidth, y, scrollX);
                                            break;
                                        case "energy":
                                            spriteEnergy.Draw(canvas, x - All.TileWidth, y, scrollX);
                                            break;
                                        case "smoke_house":
                                            spritesSmoke[smokeIndex++].Draw(canvas, x, y - 3, scrollX);
                                            break;
                                        case "smoke":
                                            spritesSmoke[smokeIndex++].Draw(canvas, x, y, scrollX);
                                            break;
                                        case "vial":
                                            break;
                                        case "chest":
                                            break;
                                        case "plant1":
                                            spritePlant1.Draw(canvas, x - All.TileWidth, y, scrollX);
                                            break;
                                        case "plant2":
                                            spritePlant2.Draw(canvas, x - All.TileWidth, y, scrollX);
                                            break;
                                        case "water":
                                            if (isWater1)
                                                spriteWater1.Draw(canvas, x, y, scrollX);
                                            else
                                                spriteWater2.Draw(canvas, x, y, scrollX);
                                            break;
                                        case "shark":
                                            monsters.Generator(MonsterType.Shark, currentX, x, y + 2 * All.TileHeight);
                                            break;
                                    }
                                    break;
                            }
                        }
                    }
                }
                // colonne suivante
                currentX++;
                if (currentX >= mapMaxX)
                {
                    currentX = mapMinX + 1;
                }
            }

            // gestion des monstres
            currentX = startMapX - 1;
            if (currentX <= mapMinX)
            {
                currentX = mapMaxX - 1;
            }
            // fait 2x pour les larges monstres comme les plant1 et 2
            currentX--;
            if (currentX <= mapMinX)
            {
                currentX = mapMaxX - 1;
            }
            // pour chaque colonne
            for (int i = -2; i < (All.MAP_SHOW + 2); i++)
            {
                // et chaque ligne
                for (int j = 0; j < currentMapHeight; j++)
                {
                    // le tile en cours
                    tile = currentTerrain[currentX, j];
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
                if (currentX >= mapMaxX)
                {
                    currentX = mapMinX + 1;
                }
            }
            monsters.Draw(canvas, scrollX);


            foreach (var kvp in bullets)
            {
                spriteBullet.StepAnim = kvp.Value.Step;
                spriteBullet.Draw(canvas, kvp.Value.X, kvp.Value.Y);
            }


            All.Witch.Draw(canvas);

            /*canvas.DrawLine(All.SPEED_LEFT_MAX * All.GAME_SCALE + All.DECAL_MAP_X, 0, All.SPEED_LEFT_MAX * All.GAME_SCALE + All.DECAL_MAP_X, 1000, textFPS);
            canvas.DrawLine(All.SPEED_LEFT_MIDDLE * All.GAME_SCALE + All.DECAL_MAP_X, 0, All.SPEED_LEFT_MIDDLE * All.GAME_SCALE + All.DECAL_MAP_X, 1000, paintCyan);
            canvas.DrawLine(All.MIDDLE_MAP * All.GAME_SCALE + All.DECAL_MAP_X, 0, All.MIDDLE_MAP * All.GAME_SCALE + All.DECAL_MAP_X, 1000, paintRed);
            canvas.DrawLine(All.SPEED_RIGHT_MIDDLE * All.GAME_SCALE + All.DECAL_MAP_X, 0, All.SPEED_RIGHT_MIDDLE * All.GAME_SCALE + All.DECAL_MAP_X, 1000, paintCyan);
            canvas.DrawLine(All.SPEED_RIGHT_MAX * All.GAME_SCALE + All.DECAL_MAP_X, 0, All.SPEED_RIGHT_MAX * All.GAME_SCALE + All.DECAL_MAP_X, 1000, textFPS);*/

            // pour cacher le débordement du scroll
            /*canvas.DrawRect(All.DECAL_MAP_X - 2 * All.TileWidthScale, All.DECAL_MAP_Y,
                            3 * All.TileWidthScale, 23 * All.TileHeightScale,
                            paintBlack);
            canvas.DrawRect(All.DECAL_MAP_X + All.MAP_SHOW * All.TileWidthScale, All.DECAL_MAP_Y,
                             4 * All.TileWidthScale, 23 * All.TileHeightScale,
                            paintBlack);*/

            //canvas.DrawLine(0 * All.GAME_SCALE + All.DECAL_MAP_X, All.DECAL_MAP_Y, 0 * All.GAME_SCALE + All.DECAL_MAP_X, 1000, paintRed);
            //canvas.DrawLine(All.MAP_SHOW * All.TileWidth * All.GAME_SCALE + All.DECAL_MAP_X, All.DECAL_MAP_Y, All.MAP_SHOW * All.TileWidth * All.GAME_SCALE + All.DECAL_MAP_X, 1000, paintRed);


            // affichage des infos
            spriteSheepSkin.Draw(canvas, 1 * All.TileWidth, -5 * All.TileHeight - 0);
            spriteSheepSkin.Draw(canvas, (All.MAP_SHOW - 7 + 0) * All.TileWidth, -5 * All.TileHeight - 0);

            typo.Write(canvas, "SCORE", 10 * All.TileWidth, -5 * All.TileHeight);
            typo.Write(canvas, All.ScoreText, 8 * All.TileWidth, -4 * All.TileHeight + 1);
            typo.Write(canvas, "MAGIC", 10 * All.TileWidth, -3 * All.TileHeight + 2);
            typo.Write(canvas, All.MagicText, 10 * All.TileWidth, -2 * All.TileHeight + 3);
            typo.Write(canvas, "HAGS", 28 * All.TileWidth, -5 * All.TileHeight);

            int xx, yy; float aa, bb;
            tile = 8; // tête de sorcière
            // position de la source
            xx = (tile % 100) * All.TileWidthScale;
            yy = (tile / 100) * All.TileHeightScale;
            drawSource = new SKRect(xx, yy, xx + All.TileWidthScale * 2, yy + All.TileHeightScale);
            int count = All.Lives;
            for (int jj = 0; jj < 3; jj++)
            {
                for (int ii = 0; ii < 3; ii++)
                {
                    if (count > 0)
                    {
                        // position de la cible
                        aa = All.DECAL_MAP_X + (27 + ii * 2) * All.TileWidthScale + ii * All.GAME_SCALE;
                        bb = All.DECAL_MAP_Y - (4 - jj) * All.TileHeightScale + jj * All.GAME_SCALE;
                        drawDestination = new SKRect(aa, bb, aa + All.TileWidthScale * 2, bb + All.TileHeightScale);
                        // on effectue l'affichage
                        //canvas.DrawBitmap(All.TilesScale, drawSource, drawDestination);
                        canvas.DrawImage(All.TilesScaleImage, drawSource, drawDestination);
                        count--;
                    }
                }
            }
            if (All.HasKeyRed)
            {
                spriteKeyRed.Draw(canvas, 17 * All.TileWidth, -5 * All.TileHeight);
            }
            if (All.HasKeyPink)
            {
                spriteKeyPink.Draw(canvas, 17 * All.TileWidth, -3 * All.TileHeight);
            }
            if (All.HasKeyBlue)
            {
                spriteKeyBlue.Draw(canvas, 23 * All.TileWidth, -5 * All.TileHeight);
            }
            if (All.HasKeyGreen)
            {
                spriteKeyGreen.Draw(canvas, 23 * All.TileWidth, -3 * All.TileHeight);
            }

            if (All.HasPotion)
            {
                tile = 564;
                xx = (tile % 100) * All.TileWidthScale;
                yy = (tile / 100) * All.TileHeightScale;
                drawSource = new SKRect(xx, yy, xx + All.TileWidthScale * 3, yy + All.TileHeightScale * 2);
                aa = All.DECAL_MAP_X + (21) * All.TileWidthScale - 8 * All.GAME_SCALE;
                bb = All.DECAL_MAP_Y - (5) * All.TileHeightScale;
                drawDestination = new SKRect(aa, bb, aa + All.TileWidthScale * 3, bb + All.TileHeightScale * 2);
                //canvas.DrawBitmap(All.TilesScale, drawSource, drawDestination);
                canvas.DrawImage(All.TilesScaleImage, drawSource, drawDestination);
            }
            if (All.HasChest)
            {
                tile = 864;
                xx = (tile % 100) * All.TileWidthScale;
                yy = (tile / 100) * All.TileHeightScale;
                drawSource = new SKRect(xx, yy, xx + All.TileWidthScale * 3, yy + All.TileHeightScale * 2);
                aa = All.DECAL_MAP_X + (21) * All.TileWidthScale - 8 * All.GAME_SCALE;
                bb = All.DECAL_MAP_Y - (3) * All.TileHeightScale;
                drawDestination = new SKRect(aa, bb, aa + All.TileWidthScale * 3, bb + All.TileHeightScale * 2);
                //canvas.DrawBitmap(All.TilesScale, drawSource, drawDestination);
                canvas.DrawImage(All.TilesScaleImage, drawSource, drawDestination);
            }

            if ((tempo - lastFps) >= All.ONE_SECOND)
            {
                messageFps = String.Format("{0} fps, {1} anim", framesPerSecond, animPerSecond);
                animPerSecond = 0;
                framesPerSecond = 0;
                lastFps = tempo;
            }
            canvas.DrawText(messageFps, 0, 50, textFPS);

            /*bmpPixels.Pixels = pixels;
            bmpToShow = bmpPixels.Resize(scaleInfo, SKBitmapResizeMethod.Box);
            canvas.DrawBitmap(bmpPixels, 0, 0);*/
        }

        SKPaint paintYellow = new SKPaint() { Color = SKColors.Yellow };
        SKPaint paintCyan = new SKPaint() { Color = SKColors.Cyan };
        SKPaint paintRed = new SKPaint() { Color = SKColors.Red };
        SKPaint paintBlack = new SKPaint() { Color = SKColors.Black };

        // *********************************************************************

        /*private void CopyPixels(int tileNumber, int toX, int toY)
        {
            int x, y, a, b;
            a = toX * All.TileWidth;
            if (a >= MAX_WIDTH)
                return;

            b = toY * All.TileHeight;
            if (b >= MAX_HEIGHT)
                return;

            x = (tileNumber % 100) * All.TileWidth;
            y = (tileNumber / 100) * All.TileHeight;
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