﻿// /*
// Author: Boris
// Create: 20/06/2018
// */

using System;
using System.Collections.Generic;
using SkiaSharp;
using System.Linq;

namespace Cauldron
{
    public enum MonsterType
    {
        Bat_1,
        Bat_2,
        Ghost
    }

    public enum MonsterDirection : short
    {
        DiagUpLeft,
        DiagUpLeftSpeed,
        DiagDownLeft,
        DiagDownLeftSpeed,
        DiagUpRight,
        DiagUpRightSpeed,
        DiagDownRight,
        DiagDownRightSpeed,
        ToLeft,
        ToRight,
        ToTop,
        ToDown,
        ToUp,
        ToWitch,
    }

    public class OneMonster
    {
        public MonsterType Category { get; set; }
        public int ID { get; set; }
        public int AnimationStep { get; set; }
        public int X { get; set; }
        public int Y { get; set; }
        public float[] Pattern { get; set; }
        public int PatternStep { get; set; }
        public int PatternDelay { get; set; }
    }

    public class OneExplode
    {
        public int ID { get; set; }
        public int X { get; set; }
        public int Y { get; set; }
        public DateTime Start { get; set; }
        public int Step { get; set; }
    }

    public class Monsters
    {
        float DELAY_COEFF;

        // Bat 1
        List<float[]> patternBat1 = new List<float[]>(); /*{
            { (short) MonsterDirection.DiagUpLeftSpeed, 6, (short) MonsterDirection.DiagDownLeftSpeed, 6, (short) MonsterDirection.ToLeft, 0 },
            { (short) MonsterDirection.DiagUpLeftSpeed, 6, (short) MonsterDirection.DiagDownLeftSpeed, 6, (short) MonsterDirection.ToRight, 0 },
            { (short)MonsterDirection.DiagUpRight, 3, (short)MonsterDirection.DiagDownRight, 3, (short)MonsterDirection.ToLeft, 0 },
            { (short)MonsterDirection.DiagUpRight, 3, (short)MonsterDirection.DiagDownRight, 3, (short)MonsterDirection.ToRight, 0 },
        };*/
        // Bat 2
        List<float[]> patternBat2 = new List<float[]>();
        /*short[,] patternBat2 = {
            { (short)MonsterDirection.ToTop, 0, (short)MonsterDirection.ToLeft, 6, (short)MonsterDirection.ToDown, 4, (short)MonsterDirection.ToRight, 4, (short)MonsterDirection.ToUp, 3, (short)MonsterDirection.ToWitch, 0 },
            { (short)MonsterDirection.ToTop, 0, (short)MonsterDirection.ToRight, 6, (short)MonsterDirection.ToDown, 4, (short)MonsterDirection.ToLeft, 4, (short)MonsterDirection.ToUp, 3, (short)MonsterDirection.ToWitch, 0 }
        };*/

        int tileWidthScale;
        List<OneMonster> monsters;
        Dictionary<int, int> inhibeID;
        int INHIBE_DELAY;
        int widthSprite;
        int heightSprite;

        const int BAT_SPRITES_MAX = 4;
        OneSprite[] spritesBat1 = new OneSprite[BAT_SPRITES_MAX];
        OneSprite[] spritesBat2 = new OneSprite[BAT_SPRITES_MAX];
        //int batIndex;
        const int GHOST_SPRITES_MAX = 8;
        OneSprite[] spritesGhost = new OneSprite[GHOST_SPRITES_MAX];
        //int ghostIndex;
        int stepX;
        int stepY;

        OneSprite spriteExplode;
        Dictionary<int, OneExplode> explodes = new Dictionary<int, OneExplode>();
        TimeSpan explodeElaps = TimeSpan.FromMilliseconds(80);
        int idExplode = 0;



        public Monsters(int tileWidth, int tileHeight, float scale, int decalX, int decalY, int stepX, int stepY)
        {
            DELAY_COEFF = tileWidth * scale / 1.8f;
            tileWidthScale = Convert.ToInt32(tileWidth * scale);
            widthSprite = Convert.ToInt32(tileWidth * 3 * scale);
            heightSprite = Convert.ToInt32(tileHeight * 3 * scale);
            monsters = new List<OneMonster>();
            inhibeID = new Dictionary<int, int>();
            INHIBE_DELAY = tileWidthScale * 2;
            this.stepX = stepX;
            this.stepY = stepY;

            spriteExplode = new OneSprite(15 * 100 + 100 - 32, tileWidth, tileHeight, 3 * 8, 3 * 8, 8, 1000, scale, decalX, decalY);


            for (int i = 0; i < BAT_SPRITES_MAX; i++)
            {
                spritesBat1[i] = new OneSprite(7 * 100 + 100 - 32, tileWidth, tileHeight, 3 * 8, 3 * 8, 4, 120, scale, decalX, decalY, animBack: true);
                spritesBat1[i].StepAnim = i % 4;
                spritesBat2[i] = new OneSprite(7 * 100 + 100 - 16, tileWidth, tileHeight, 3 * 8, 3 * 8, 4, 120, scale, decalX, decalY, animBack: true);
                spritesBat2[i].StepAnim = i % 4;
            }
            for (int i = 0; i < GHOST_SPRITES_MAX; i++)
            {
                spritesGhost[i] = new OneSprite(11 * 100 + 100 - 32, tileWidth, tileHeight, 3 * 8, 3 * 8, 8, 120, scale, decalX, decalY);
                spritesGhost[i].StepAnim = i % 8;
            }
            float[] pattern = { (short)MonsterDirection.DiagUpLeftSpeed, 3, (short)MonsterDirection.DiagDownLeftSpeed, 3, (short)MonsterDirection.ToLeft, 0 };
            patternBat1.Add(pattern);
            pattern = new float[] { (short)MonsterDirection.DiagUpLeftSpeed, 3, (short)MonsterDirection.DiagDownLeftSpeed, 3, (short)MonsterDirection.ToRight, 0 };
            patternBat1.Add(pattern);
            pattern = new float[] { (short)MonsterDirection.DiagUpRight, 2, (short)MonsterDirection.DiagDownRight, 2, (short)MonsterDirection.ToLeft, 0 };
            patternBat1.Add(pattern);
            pattern = new float[] { (short)MonsterDirection.DiagUpRight, 2, (short)MonsterDirection.DiagDownRight, 2, (short)MonsterDirection.ToRight, 0 };
            patternBat1.Add(pattern);

            pattern = new float[] { (short)MonsterDirection.ToTop, 0, (short)MonsterDirection.ToLeft, 2, (short)MonsterDirection.ToDown, 1.5f, (short)MonsterDirection.ToRight, 1.5f, (short)MonsterDirection.ToUp, 1.3f, (short)MonsterDirection.ToWitch, 0 };
            patternBat2.Add(pattern);
            pattern = new float[] { (short)MonsterDirection.ToTop, 0, (short)MonsterDirection.ToRight, 2, (short)MonsterDirection.ToDown, 1.5f, (short)MonsterDirection.ToLeft, 1.5f, (short)MonsterDirection.ToUp, 1.3f, (short)MonsterDirection.ToWitch, 0 };
            patternBat2.Add(pattern);
        }

        public void Generator(MonsterType category, int idGenerator, int x, int y)
        {
            if (inhibeID.ContainsKey(idGenerator))
                return;
            /*foreach (OneMonster m in monsters)
            {
                if (m.ID == idGenerator)
                    if (Tools.RND(100) > 10)
                        return;
                //if (m.X == x && m.Y == y)
                //  return;
            }*/
            // x% de chance de générer un monstre
            if (Tools.RND(10000) > (110 - monsters.Count * 10))
                return;
            OneMonster monster = new OneMonster();
            monster.Category = category;
            monster.ID = idGenerator;
            monster.X = x;
            monster.Y = y;
            monster.AnimationStep = Tools.RND(4);
            switch (category)
            {
                case MonsterType.Bat_1:
                    monster.Pattern = patternBat1[Tools.RND(patternBat1.Count)];
                    break;
                case MonsterType.Bat_2:
                    monster.Pattern = patternBat2[Tools.RND(patternBat2.Count)];
                    break;
                case MonsterType.Ghost:
                    monster.Pattern = patternBat1[Tools.RND(patternBat1.Count)];
                    break;
            }
            monster.PatternStep = 0;
            monster.PatternDelay = Convert.ToInt32(monster.Pattern[1] * DELAY_COEFF);
            monsters.Add(monster);
            System.Diagnostics.Debug.WriteLine(String.Format("Monster #{3}: {0} at {1}/{2}", category, x, y, monsters.Count));
            // on rend inactif ce générateur
            inhibeID.Add(idGenerator, INHIBE_DELAY);
        }

        public void MapScrollToRight()
        {
            List<OneMonster> toDelete = new List<OneMonster>();
            foreach (OneMonster m in monsters)
            {
                m.X += tileWidthScale;
                if (m.X > 40 * tileWidthScale)
                {
                    toDelete.Add(m);
                    System.Diagnostics.Debug.WriteLine(String.Format("Delete {0} at {1}/{2}", m.Category, m.X, m.Y));
                }
            }
            foreach (OneMonster m in toDelete)
                monsters.Remove(m);

            foreach (var kvp in explodes.ToList<KeyValuePair<int, OneExplode>>())
            {
                explodes[kvp.Key].X += tileWidthScale;
            }
        }

        public void MapScrollToLeft()
        {
            List<OneMonster> toDelete = new List<OneMonster>();
            foreach (OneMonster m in monsters)
            {
                m.X -= tileWidthScale;
                if (m.X < 0)
                {
                    toDelete.Add(m);
                    System.Diagnostics.Debug.WriteLine(String.Format("Delete {0} at {1}/{2}", m.Category, m.X, m.Y));
                }
            }
            foreach (OneMonster m in toDelete)
                monsters.Remove(m);

            foreach (var kvp in explodes.ToList<KeyValuePair<int, OneExplode>>())
            {
                explodes[kvp.Key].X -= tileWidthScale;
            }
        }

        public void MapScrollToUp()
        {
            // TODO:
        }

        public void MapScrollToDown()
        {
            // TODO:
        }

        public void DoAnim(DateTime time, int targetX, int targetY)
        {
            for (int i = 0; i < BAT_SPRITES_MAX; i++)
            {
                spritesBat1[i].DoAnim(time);
                spritesBat2[i].DoAnim(time);
            }
            for (int i = 0; i < GHOST_SPRITES_MAX; i++)
                spritesGhost[i].DoAnim(time);

            List<OneMonster> toDelete = new List<OneMonster>();

            foreach (OneMonster monster in monsters)
            {
                switch ((MonsterDirection)monster.Pattern[monster.PatternStep])
                {
                    case MonsterDirection.ToWitch:
                        if (targetX > monster.X)
                        {
                            monster.X += stepX * 2;
                        }
                        else
                        {
                            if (targetX < monster.X)
                            {
                                monster.X -= stepX * 2;
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
                    case MonsterDirection.ToTop:
                        monster.Y -= stepY * 2;
                        if (monster.Y <= 0)
                        {
                            // pas top
                            monster.Y = 0;
                            monster.PatternDelay = 1;
                            //monster.PatternStep += 2;
                        }
                        break;
                    case MonsterDirection.ToLeft:
                        monster.X -= stepX * 2;
                        break;
                    case MonsterDirection.ToRight:
                        monster.X += stepX * 2;
                        break;
                    case MonsterDirection.ToDown:
                        monster.Y += stepY * 2;
                        break;
                    case MonsterDirection.ToUp:
                        monster.Y -= stepY * 2;
                        break;
                    case MonsterDirection.DiagUpRight:
                        monster.Y -= stepY;
                        monster.X += stepX;
                        break;
                    case MonsterDirection.DiagUpLeft:
                        monster.Y -= stepY;
                        monster.X -= stepX;
                        break;
                    case MonsterDirection.DiagUpRightSpeed:
                        monster.Y -= stepY * 2;
                        monster.X += stepX * 2;
                        break;
                    case MonsterDirection.DiagUpLeftSpeed:
                        monster.Y -= stepY * 2;
                        monster.X -= stepX * 2;
                        break;
                    case MonsterDirection.DiagDownRight:
                        monster.Y += stepY;
                        monster.X += stepX;
                        break;
                    case MonsterDirection.DiagDownLeft:
                        monster.Y += stepY;
                        monster.X -= stepX;
                        break;
                    case MonsterDirection.DiagDownRightSpeed:
                        monster.Y += stepY * 2;
                        monster.X += stepX * 2;
                        break;
                    case MonsterDirection.DiagDownLeftSpeed:
                        monster.Y += stepY * 2;
                        monster.X -= stepX * 2;
                        break;
                    default:
                        System.Diagnostics.Debug.WriteLine(String.Format("Pattern: {0}", (MonsterDirection)monster.Pattern[monster.PatternStep]));
                        break;
                }
                bool isDelete = false;
                monster.PatternDelay--;
                if (monster.PatternDelay == 0)
                {
                    monster.PatternStep += 2;
                    if (monster.PatternStep > monster.Pattern.Length)
                    {
                        // normallement pas possible, sauf si erreur dans la définition du pattern
                        toDelete.Add(monster);
                        isDelete = true;
                    }
                    else
                    {
                        monster.PatternDelay = Convert.ToInt32(monster.Pattern[monster.PatternStep + 1] * DELAY_COEFF);
                    }
                }
                else
                {
                    // pour le pattern sans fin
                    if (monster.PatternDelay == -1)
                    {
                        monster.PatternDelay = 0;
                    }
                    // sortie de l'écran
                    // TODO: changer les valeurs en dur
                    if (monster.Y < 0 || monster.Y > 940)
                    {
                        toDelete.Add(monster);
                        isDelete = true;
                    }
                    else
                    {
                        if (monster.X < 0 || monster.X > 1640)
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
                    if (targetX >= monster.X && targetX <= (monster.X + widthSprite))
                    {
                        if (targetY >= monster.Y && targetY <= (monster.Y + heightSprite))
                        {
                            isDelete = true;
                        }
                        else if (targetY <= monster.Y && (targetY + heightSprite) >= monster.Y)
                        {
                            isDelete = true;
                        }
                    }
                    else if (targetX <= monster.X && (targetX + widthSprite) >= monster.X)
                    {
                        if (targetY >= monster.Y && targetY <= (monster.Y + heightSprite))
                        {
                            isDelete = true;
                        }
                        else if (targetY <= monster.Y && (targetY + heightSprite) >= monster.Y)
                        {
                            isDelete = true;
                        }
                    }
                    if (isDelete)
                    {
                        // TODO: générer une explosion
                        toDelete.Add(monster);
                        OneExplode temp = new OneExplode();
                        temp.ID = idExplode++;
                        temp.Start = DateTime.UtcNow;
                        temp.Step = 0;
                        temp.X = monster.X;
                        temp.Y = monster.Y;
                        explodes.Add(temp.ID, temp);
                    }
                } // !isDelete

            } // foreach monster

            foreach (OneMonster m in toDelete)
                monsters.Remove(m);

            List<int> deleteList = new List<int>();

            foreach (var kvp in explodes.ToList<KeyValuePair<int, OneExplode>>())
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

            deleteList = new List<int>();

            // traitement de l'inhibition des générateurs
            foreach (var kvp in inhibeID.ToList<KeyValuePair<int, int>>())
            {
                inhibeID[kvp.Key]--;
                if (inhibeID[kvp.Key] < 0)
                    deleteList.Add(kvp.Key); // on peut rendre actif ce générateur
            }
            foreach (int k in deleteList)
                inhibeID.Remove(k);
        }

        public void Draw(SKCanvas canvas, SKBitmap tiles, int scrollX)
        {
            foreach (OneMonster monster in monsters)
            {
                switch (monster.Category)
                {
                    case MonsterType.Bat_1:
                        spritesBat1[monster.AnimationStep].Draw(canvas, monster.X, monster.Y, tiles, scrollX);
                        break;
                    case MonsterType.Bat_2:
                        spritesBat2[monster.AnimationStep].Draw(canvas, monster.X, monster.Y, tiles, scrollX);
                        break;
                    case MonsterType.Ghost:
                        spritesGhost[monster.AnimationStep].Draw(canvas, monster.X, monster.Y, tiles, scrollX);
                        break;
                }
            }
            foreach (var kvp in explodes)
            {
                spriteExplode.StepAnim = kvp.Value.Step;
                spriteExplode.Draw(canvas, kvp.Value.X, kvp.Value.Y, tiles, scrollX);
            }
        } // Draw

    }
}