// /*
// Author: Boris
// Create: 20/06/2018
// */

using System;
using System.Collections.Generic;
using System.Linq;
using SkiaSharp;

namespace Cauldron
{

    public class Monsters
    {

        // Bat 1
        List<float[]> patternBat1 = new List<float[]>();
        // Bat 2
        List<float[]> patternBat2 = new List<float[]>();
        // Ghost
        List<float[]> patternGhost = new List<float[]>();
        // Shark
        List<float[]> patternShark = new List<float[]>();
        TimeSpan gamePatternSpeed = All.FREQUENCY;
        DateTime lastPatternAnim;


        List<OneMonster> monsters;
        Dictionary<int, int> inhibeGeneratorID;
        int generatorInhibeDelay;

        const int SPRITE_WIDTH = 3 * 8;
        const int SPRITE_HEIGHT = 3 * 8;

        const int BAT_SPRITES_MAX = 4; // nombre d'animations d'une chauve souris
        OneSprite[] spritesBat1 = new OneSprite[BAT_SPRITES_MAX];
        OneSprite[] spritesBat2 = new OneSprite[BAT_SPRITES_MAX];
        const int GHOST_SPRITES_MAX = 8; // nombre d'animations d'un fantôme
        OneSprite[] spritesGhost = new OneSprite[GHOST_SPRITES_MAX];
        OneSprite spriteShark;

        OneSprite spriteExplode;
        Dictionary<int, OneObject> explodes = new Dictionary<int, OneObject>();
        TimeSpan explodeElaps = TimeSpan.FromMilliseconds(60);
        int idExplode = 0;

        int stepX;
        int stepY;

        // *********************************************************************

        public Monsters(int stepX, int stepY)
        {
            monsters = new List<OneMonster>();
            inhibeGeneratorID = new Dictionary<int, int>();
            generatorInhibeDelay = All.TileWidth; // tileWidthScale * 2;
            this.stepX = stepX;
            this.stepY = stepY * 2;

            spriteExplode = new OneSprite(15 * 100 + 100 - 32, SPRITE_WIDTH, SPRITE_HEIGHT, 8, 1000);

            for (int i = 0; i < BAT_SPRITES_MAX; i++)
            {
                spritesBat1[i] = new OneSprite(7 * 100 + 100 - 32, SPRITE_WIDTH, SPRITE_HEIGHT, 4, 120, animBack: true);
                spritesBat1[i].StepAnim = i % 4;
                spritesBat2[i] = new OneSprite(7 * 100 + 100 - 16, SPRITE_WIDTH, SPRITE_HEIGHT, 4, 120, animBack: true);
                spritesBat2[i].StepAnim = i % 4;
            }
            for (int i = 0; i < GHOST_SPRITES_MAX; i++)
            {
                spritesGhost[i] = new OneSprite(11 * 100 + 100 - 32, SPRITE_WIDTH, SPRITE_HEIGHT, 8, 120);
                spritesGhost[i].StepAnim = i % 8;
            }
            spriteShark = new OneSprite(20 * 100 + 100 - 16, SPRITE_WIDTH, SPRITE_HEIGHT, 1, 0);

            // un pattern est constitué d'une suite de couple "mouvement/combien de case(0=infini)"
            // Chauve souris type 1
            float[] pattern = { (short)MovingDirection.DiagUpLeft, 2.3f, (short)MovingDirection.DiagDownLeft, 2.3f, (short)MovingDirection.ToLeft, 0 };
            patternBat1.Add(pattern);
            pattern = new float[] { (short)MovingDirection.DiagUpLeft, 2.3f, (short)MovingDirection.DiagDownLeft, 2.3f, (short)MovingDirection.ToRight, 0 };
            patternBat1.Add(pattern);
            pattern = new float[] { (short)MovingDirection.DiagUpRight, 1.1f, (short)MovingDirection.DiagDownRight, 1.1f, (short)MovingDirection.ToLeft, 0 };
            patternBat1.Add(pattern);
            pattern = new float[] { (short)MovingDirection.DiagUpRight, 1.1f, (short)MovingDirection.DiagDownRight, 1.1f, (short)MovingDirection.ToRight, 0 };
            patternBat1.Add(pattern);

            // Chauve souris type 2
            pattern = new float[] { (short)MovingDirection.ToTop, 0, (short)MovingDirection.ToLeft, 2.3f, (short)MovingDirection.ToDown, 1.6f, (short)MovingDirection.ToRight, 1.9f, (short)MovingDirection.ToUp, 1.3f, (short)MovingDirection.ToWitch, 0 };
            patternBat2.Add(pattern);
            pattern = new float[] { (short)MovingDirection.ToTop, 0, (short)MovingDirection.ToRight, 2.3f, (short)MovingDirection.ToDown, 1.6f, (short)MovingDirection.ToLeft, 1.9f, (short)MovingDirection.ToUp, 1.3f, (short)MovingDirection.ToWitch, 0 };
            patternBat2.Add(pattern);

            // Fantôme
            pattern = new float[] { (short)MovingDirection.ToUpSpeed, 2.0f, (short)MovingDirection.DiagUpLeft, 0.7f, (short)MovingDirection.DiagUpRight, 0.7f, (short)MovingDirection.ToWitch, 0 };
            patternGhost.Add(pattern);
            pattern = new float[] { (short)MovingDirection.ToUpSpeed, 2.0f, (short)MovingDirection.DiagUpRight, 0.7f, (short)MovingDirection.DiagUpLeft, 0.7f, (short)MovingDirection.ToWitch, 0 };
            patternGhost.Add(pattern);
            pattern = new float[] { (short)MovingDirection.ToUp, 3.0f, (short)MovingDirection.DiagUpLeft, 1.0f, (short)MovingDirection.DiagDownRightSpeed, 0.8f, (short)MovingDirection.ToLeft, 2.0f, (short)MovingDirection.ToDownSpeed, 1.8f };
            patternGhost.Add(pattern);


            // Requin
            pattern = new float[] { (short)MovingDirection.DiagUpLeftSlow, 0.2f, (short)MovingDirection.ToLeft, 7.0f, (short)MovingDirection.DiagDownLeftSlow, 2.0f };
            patternShark.Add(pattern);

            lastPatternAnim = DateTime.UtcNow;
        }

        // *********************************************************************

        public void RemoveMonsters()
        {
            monsters.Clear();
        }

        // *********************************************************************

        /// <summary>
        /// Générateur de monstres
        /// </summary>
        /// <param name="category">Category monster</param>
        /// <param name="idGenerator">Identifier generator.</param>
        /// <param name="x">The x coordinate.</param>
        /// <param name="y">The y coordinate.</param>
        public void Generator(MonsterType category, int idGenerator, int x, int y)
        {
            if (inhibeGeneratorID.ContainsKey(idGenerator))
                return;
            // x% de chance de générer un monstre ... qui diminue en fonction des monstres en vie
            if (All.RND(1000) > (16 - monsters.Count))
                return;
            OneMonster monster = new OneMonster();
            monster.Category = category;
            monster.ID = idGenerator;
            monster.X = x;
            monster.Y = y;
            monster.AnimationStep = All.RND(4);
            switch (category)
            {
                case MonsterType.Bat_1:
                    monster.Pattern = patternBat1[All.RND(patternBat1.Count)];
                    break;
                case MonsterType.Bat_2:
                    monster.Pattern = patternBat2[All.RND(patternBat2.Count)];
                    break;
                case MonsterType.Ghost:
                    monster.Pattern = patternGhost[All.RND(patternGhost.Count)];
                    break;
                case MonsterType.Shark:
                    monster.Pattern = patternShark[All.RND(patternShark.Count)];
                    break;
            }
            //monster.UsePattern = true;
            monster.PatternStep = 0;
            monster.PatternDelay = Convert.ToInt32(monster.Pattern[1] * SPRITE_WIDTH);
            if (monster.PatternDelay == 0)
                monster.PatternNonStop = true;
            monsters.Add(monster);
            System.Diagnostics.Debug.WriteLine(String.Format("Monster #{3}: {0} at {1}/{2}", category, x, y, monsters.Count));
            // on rend inactif ce générateur
            inhibeGeneratorID.Add(idGenerator, generatorInhibeDelay);
        }

        // *********************************************************************

        /// <summary>
        /// Le joueur se déplace à gauche, tout scroll vers la droite
        /// </summary>
        public void MapScrollToRight()
        {
            List<OneMonster> toDelete = new List<OneMonster>();
            foreach (OneMonster m in monsters)
            {
                m.X += All.TileWidth;
                if (m.X > (All.MAP_SHOW * All.TileWidth)) // sortie d'écran
                {
                    toDelete.Add(m);
                    System.Diagnostics.Debug.WriteLine(String.Format("Delete {0} at {1}/{2}", m.Category, m.X, m.Y));
                }
            }
            foreach (OneMonster m in toDelete)
                monsters.Remove(m);

            foreach (var kvp in explodes.ToList<KeyValuePair<int, OneObject>>())
            {
                explodes[kvp.Key].X += All.TileWidth;
            }
        }

        // *********************************************************************

        /// <summary>
        /// Le joueur se déplace vers la droite, tout scroll à gauche
        /// </summary>
        public void MapScrollToLeft()
        {
            List<OneMonster> toDelete = new List<OneMonster>();
            foreach (OneMonster m in monsters)
            {
                m.X -= All.TileWidth;
                if ((m.X + SPRITE_WIDTH) < 0) // sortie d'écran
                {
                    toDelete.Add(m);
                    System.Diagnostics.Debug.WriteLine(String.Format("Delete {0} at {1}/{2}", m.Category, m.X, m.Y));
                }
            }
            foreach (OneMonster m in toDelete)
                monsters.Remove(m);

            foreach (var kvp in explodes.ToList<KeyValuePair<int, OneObject>>())
            {
                explodes[kvp.Key].X -= All.TileWidth;
            }
        }

        // *********************************************************************

        public void MapScrollToUp()
        {
            // TODO:
        }

        // *********************************************************************

        public void MapScrollToDown()
        {
            // TODO:
        }

        // *********************************************************************

        private void MoveX(OneMonster monster, int step)
        {
            monster.X += step;
            monster.PatternDelay -= Math.Abs(step);
        }
        private void MoveY(OneMonster monster, int step)
        {
            monster.Y += step;
            monster.PatternDelay -= Math.Abs(step);
        }
        private void MoveXY(OneMonster monster, int x, int y)
        {
            monster.X += x;
            monster.Y += y;
            monster.PatternDelay -= Math.Abs(x);
        }

        /// <summary>
        /// Dos the animation.
        /// </summary>
        /// <param name="time">Time.</param>
        /// <param name="targetX">Target x.</param>
        /// <param name="targetY">Target y.</param>
        public void DoAnim(DateTime time, int targetX, int targetY)
        {
            for (int i = 0; i < BAT_SPRITES_MAX; i++)
            {
                spritesBat1[i].DoAnim(time);
                spritesBat2[i].DoAnim(time);
            }
            for (int i = 0; i < GHOST_SPRITES_MAX; i++)
                spritesGhost[i].DoAnim(time);

            List<int> deleteList = new List<int>();

            if ((time - lastPatternAnim) >= gamePatternSpeed)
            {
                lastPatternAnim = time;

                List<OneMonster> toDelete = new List<OneMonster>();

                foreach (OneMonster monster in monsters)
                {
                    switch ((MovingDirection)monster.Pattern[monster.PatternStep])
                    {
                        case MovingDirection.ToWitch:
                            if (targetX > monster.X)
                            {
                                monster.X += stepX * 3;
                                if (targetX < monster.X)
                                    monster.X = targetX;
                            }
                            else
                            {
                                if (targetX < monster.X)
                                {
                                    monster.X -= stepX * 3;
                                    if (targetX > monster.X)
                                        monster.X = targetX;
                                }
                            }
                            if (targetY > monster.Y)
                            {
                                monster.Y += stepY * 2;
                                if (targetY < monster.Y)
                                    monster.Y = targetY;
                            }
                            else
                            {
                                if (targetY < monster.Y)
                                {
                                    monster.Y -= stepY * 2;
                                    if (targetY > monster.Y)
                                        monster.Y = targetY;
                                }
                            }
                            break;
                        case MovingDirection.ToTop:
                            monster.Y -= stepY * 2;
                            if (monster.Y <= (1 * All.TileHeight))
                            {
                                // pas top
                                monster.Y = 1 * All.TileHeight;
                                monster.PatternNonStop = false;
                            }
                            break;
                        case MovingDirection.ToLeft:
                            MoveX(monster, -stepX * 2);
                            break;
                        case MovingDirection.ToRight:
                            MoveX(monster, stepX * 2);
                            break;
                        case MovingDirection.ToLeftSpeed:
                            MoveX(monster, -stepX * 3);
                            break;
                        case MovingDirection.ToRightSpeed:
                            MoveX(monster, stepX * 3);
                            break;
                        case MovingDirection.ToDown:
                            MoveY(monster, stepY * 2);
                            break;
                        case MovingDirection.ToUp:
                            MoveY(monster, -stepY);
                            break;
                        case MovingDirection.ToDownSpeed:
                            MoveY(monster, 2 * stepY * 2);
                            break;
                        case MovingDirection.ToUpSpeed:
                            MoveY(monster, Convert.ToInt32(-2 * stepY * 1.5f));
                            break;
                        case MovingDirection.DiagUpRight:
                            MoveXY(monster, stepX * 2, -stepY * 2);
                            break;
                        case MovingDirection.DiagUpLeft:
                            MoveXY(monster, -stepX * 2, -stepY * 2);
                            break;
                        case MovingDirection.DiagUpLeftSlow:
                            MoveXY(monster, -stepX, -stepY);
                            break;
                        case MovingDirection.DiagUpRightSpeed:
                            MoveXY(monster, 3 * stepX, -3 * stepY);
                            break;
                        case MovingDirection.DiagUpLeftSpeed:
                            MoveXY(monster, -3 * stepX, -3 * stepY);
                            break;
                        case MovingDirection.DiagDownRight:
                            MoveXY(monster, stepX * 2, stepY * 2);
                            break;
                        case MovingDirection.DiagDownLeft:
                            MoveXY(monster, -stepX * 2, stepY * 2);
                            break;
                        case MovingDirection.DiagDownLeftSlow:
                            MoveXY(monster, -stepX, stepY);
                            break;
                        case MovingDirection.DiagDownRightSpeed:
                            MoveXY(monster, 3 * stepX, 3 * stepY);
                            break;
                        case MovingDirection.DiagDownLeftSpeed:
                            MoveXY(monster, -3 * stepX, 3 * stepY);
                            break;
                        default:
                            System.Diagnostics.Debug.WriteLine(String.Format("Pattern: {0}", (MovingDirection)monster.Pattern[monster.PatternStep]));
                            break;
                    }
                    bool isDelete = false;
                    if (!monster.PatternNonStop && monster.PatternDelay <= 0)
                    {
                        monster.PatternStep += 2;
                        if (monster.PatternStep >= monster.Pattern.Length)
                        {
                            toDelete.Add(monster);
                            isDelete = true;
                        }
                        else
                        {
                            monster.PatternDelay = Convert.ToInt32(monster.Pattern[monster.PatternStep + 1] * SPRITE_WIDTH);
                            if (monster.PatternDelay == 0)
                                monster.PatternNonStop = true;
                        }
                    }
                    else
                    {
                        // pour le pattern sans fin
                        /*if (monster.PatternDelay == -1)
                        {
                            monster.PatternDelay = 0;
                        }*/
                        // sortie de l'écran
                        // TODO: changer les valeurs en dur
                        if (monster.Y < 1 || monster.Y > 21 * All.TileHeight) // TODO: 
                        {
                            toDelete.Add(monster);
                            isDelete = true;
                        }
                        else
                        {
                            if ((monster.X + SPRITE_WIDTH) < 0 || monster.X > All.MAP_SHOW * All.TileWidth)
                            {
                                toDelete.Add(monster);
                                isDelete = true;
                            }
                        }
                    }

                    if (!isDelete)
                    {
                        // tests collision
                        // avec la sorcière
                        //if (!All.ShowPixel)
                        switch (monster.Category)
                        {
                            case MonsterType.Bat_1:
                                if (All.IsCollision(monster.X, monster.Y, spritesBat1[monster.AnimationStep].Source,
                                                    All.Witch.X, All.Witch.Y, All.Witch.Source))
                                {
                                    isDelete = true;
                                    All.AddPoints(PointsType.Kill_Bat_1);
                                    All.LooseMagic(MagicLoose.Kill_Bat_1);
                                    All.LooseLive();
                                }
                                break;
                            case MonsterType.Bat_2:
                                if (All.IsCollision(monster.X, monster.Y, spritesBat2[monster.AnimationStep].Source,
                                                    All.Witch.X, All.Witch.Y, All.Witch.Source))
                                {
                                    isDelete = true;
                                    All.AddPoints(PointsType.Kill_Bat_2);
                                    All.LooseMagic(MagicLoose.Kill_Bat_2);
                                    All.LooseLive();
                                }
                                break;
                            case MonsterType.Ghost:
                                if (All.IsCollision(monster.X, monster.Y, spritesGhost[monster.AnimationStep].Source,
                                                    All.Witch.X, All.Witch.Y, All.Witch.Source))
                                {
                                    isDelete = true;
                                    All.AddPoints(PointsType.Kill_Ghost);
                                    All.LooseMagic(MagicLoose.Kill_Ghost);
                                    All.LooseLive();
                                }
                                break;
                            case MonsterType.Plant1:
                                if (All.IsCollision(monster.X, monster.Y, spritesGhost[monster.AnimationStep].Source,
                                                    All.Witch.X, All.Witch.Y, All.Witch.Source))
                                {
                                    isDelete = true;
                                    All.AddPoints(PointsType.Kill_Plant_1);
                                    All.LooseMagic(MagicLoose.Kill_Plant_1);
                                    All.LooseLive();
                                }
                                break;
                            case MonsterType.Plant2:
                                if (All.IsCollision(monster.X, monster.Y, spritesGhost[monster.AnimationStep].Source,
                                                    All.Witch.X, All.Witch.Y, All.Witch.Source))
                                {
                                    isDelete = true;
                                    All.AddPoints(PointsType.Kill_Plant_2);
                                    All.LooseMagic(MagicLoose.Kill_Plant_2);
                                    All.LooseLive();
                                }
                                break;
                            case MonsterType.Shark:
                                if (All.IsCollision(monster.X, monster.Y, spritesGhost[monster.AnimationStep].Source,
                                                    All.Witch.X, All.Witch.Y, All.Witch.Source))
                                {
                                    isDelete = true;
                                    All.AddPoints(PointsType.Kill_Shark);
                                    All.LooseMagic(MagicLoose.Kill_Shark);
                                    All.LooseLive();
                                }
                                break;
                        }

                        if (isDelete)
                        {
                            // le monstre disparaît
                            toDelete.Add(monster);
                            // et on génère une explosion
                            OneObject temp = new OneObject();
                            temp.ID = idExplode++;
                            temp.Start = DateTime.UtcNow;
                            temp.Step = 0;
                            temp.X = monster.X;
                            temp.Y = monster.Y;
                            temp.Moving = MovingDirection.None;
                            SKColor color = SKColors.BlueViolet;
                            switch (monster.Category)
                            {
                                case MonsterType.Bat_1:
                                    color = SKColor.Parse("#7F7F7F");
                                    break;
                                case MonsterType.Bat_2:
                                    color = SKColor.Parse("#636563");
                                    break;
                                case MonsterType.Ghost:
                                    color = SKColor.Parse("#FFFFFF");
                                    break;
                                case MonsterType.Plant1:
                                    color = SKColor.Parse("#007F00");
                                    break;
                                case MonsterType.Plant2:
                                    color = SKColor.Parse("#FF00FF");
                                    break;
                                case MonsterType.Shark:
                                    color = SKColor.Parse("#7F7F7F");
                                    break;
                            }
                            SKColorFilter filter = SKColorFilter.CreateBlendMode(color, SKBlendMode.SrcIn);
                            temp.paintColor = new SKPaint() { ColorFilter = filter };
                            explodes.Add(temp.ID, temp);
                        }
                    } // !isDelete

                } // foreach monster

                foreach (OneMonster m in toDelete)
                    monsters.Remove(m);


                // traitement de l'inhibition des générateurs
                foreach (var kvp in inhibeGeneratorID.ToList<KeyValuePair<int, int>>())
                {
                    inhibeGeneratorID[kvp.Key]--;
                    if (inhibeGeneratorID[kvp.Key] < 0)
                        deleteList.Add(kvp.Key); // on peut rendre actif ce générateur
                }
                foreach (int k in deleteList)
                    inhibeGeneratorID.Remove(k);

                deleteList = new List<int>();


            } // tempo

            foreach (var kvp in explodes.ToList<KeyValuePair<int, OneObject>>())
            {
                if ((time - kvp.Value.Start) < explodeElaps)
                    continue;
                explodes[kvp.Key].Start = DateTime.UtcNow;
                explodes[kvp.Key].Step++;
                if (explodes[kvp.Key].Step >= 8)
                    deleteList.Add(kvp.Key);
            }
            foreach (int k in deleteList)
                explodes.Remove(k);
        }

        // *********************************************************************

        /// <summary>
        /// Draw the specified canvas and scrollX.
        /// </summary>
        /// <param name="canvas">Canvas.</param>
        /// <param name="scrollX">Scroll x.</param>
        public void Draw(SKCanvas canvas, int scrollX)
        {
            foreach (OneMonster monster in monsters)
            {
                switch (monster.Category)
                {
                    case MonsterType.Bat_1:
                        spritesBat1[monster.AnimationStep].Draw(canvas, monster.X, monster.Y, scrollX);
                        break;
                    case MonsterType.Bat_2:
                        spritesBat2[monster.AnimationStep].Draw(canvas, monster.X, monster.Y, scrollX);
                        break;
                    case MonsterType.Ghost:
                        spritesGhost[monster.AnimationStep].Draw(canvas, monster.X, monster.Y, scrollX);
                        break;
                }
            }

            foreach (var kvp in explodes)
            {
                spriteExplode.StepAnim = kvp.Value.Step;
                spriteExplode.Draw(canvas, kvp.Value.X, kvp.Value.Y, scrollX, kvp.Value.paintColor);
            }
        } // Draw

        public void DrawOnlyShark(SKCanvas canvas, int scrollX)
        {
            foreach (OneMonster monster in monsters)
            {
                if (monster.Category == MonsterType.Shark)
                {
                    spriteShark.Draw(canvas, monster.X, monster.Y, scrollX);
                }
            }
        }
    } // Draw

}