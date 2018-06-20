// /*
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
        public short[] Pattern { get; set; }
        public int PatternStep { get; set; }
        public int PatternDelay { get; set; }
    }

    public class Monsters
    {
        int DELAY_COEFF = 2;

        // Bat 1
        List<short[]> patternBat1 = new List<short[]>(); /*{
            { (short) MonsterDirection.DiagUpLeftSpeed, 6, (short) MonsterDirection.DiagDownLeftSpeed, 6, (short) MonsterDirection.ToLeft, 0 },
            { (short) MonsterDirection.DiagUpLeftSpeed, 6, (short) MonsterDirection.DiagDownLeftSpeed, 6, (short) MonsterDirection.ToRight, 0 },
            { (short)MonsterDirection.DiagUpRight, 3, (short)MonsterDirection.DiagDownRight, 3, (short)MonsterDirection.ToLeft, 0 },
            { (short)MonsterDirection.DiagUpRight, 3, (short)MonsterDirection.DiagDownRight, 3, (short)MonsterDirection.ToRight, 0 },
        };*/
        // Bat 2
        List<short[]> patternBat2 = new List<short[]>();
        /*short[,] patternBat2 = {
            { (short)MonsterDirection.ToTop, 0, (short)MonsterDirection.ToLeft, 6, (short)MonsterDirection.ToDown, 4, (short)MonsterDirection.ToRight, 4, (short)MonsterDirection.ToUp, 3, (short)MonsterDirection.ToWitch, 0 },
            { (short)MonsterDirection.ToTop, 0, (short)MonsterDirection.ToRight, 6, (short)MonsterDirection.ToDown, 4, (short)MonsterDirection.ToLeft, 4, (short)MonsterDirection.ToUp, 3, (short)MonsterDirection.ToWitch, 0 }
        };*/

        int tileWidthScale;
        List<OneMonster> monsters;
        Dictionary<int, int> inhibeID;
        int INHIBE_DELAY;

        const int BAT_SPRITES_MAX = 4;
        OneSprite[] spritesBat1 = new OneSprite[BAT_SPRITES_MAX];
        OneSprite[] spritesBat2 = new OneSprite[BAT_SPRITES_MAX];
        //int batIndex;
        const int GHOST_SPRITES_MAX = 8;
        OneSprite[] spritesGhost = new OneSprite[GHOST_SPRITES_MAX];
        //int ghostIndex;


        public Monsters(int tileWidth, int tileHeight, float scale, int decalX, int decalY)
        {
            DELAY_COEFF = Convert.ToInt32(tileWidth * scale);
            tileWidthScale = Convert.ToInt32(tileWidth * scale);
            monsters = new List<OneMonster>();
            inhibeID = new Dictionary<int, int>();
            INHIBE_DELAY = tileWidthScale * 2;

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
            short[] pattern = { (short)MonsterDirection.DiagUpLeftSpeed, 4, (short)MonsterDirection.DiagDownLeftSpeed, 4, (short)MonsterDirection.ToLeft, 0 };
            patternBat1.Add(pattern);
            pattern = new short[] { (short)MonsterDirection.DiagUpLeftSpeed, 4, (short)MonsterDirection.DiagDownLeftSpeed, 4, (short)MonsterDirection.ToRight, 0 };
            patternBat1.Add(pattern);
            pattern = new short[] { (short)MonsterDirection.DiagUpRight, 4, (short)MonsterDirection.DiagDownRight, 4, (short)MonsterDirection.ToLeft, 0 };
            patternBat1.Add(pattern);
            pattern = new short[] { (short)MonsterDirection.DiagUpRight, 4, (short)MonsterDirection.DiagDownRight, 4, (short)MonsterDirection.ToRight, 0 };
            patternBat1.Add(pattern);

            pattern = new short[] { (short)MonsterDirection.ToTop, 0, (short)MonsterDirection.ToLeft, 4, (short)MonsterDirection.ToDown, 3, (short)MonsterDirection.ToRight, 3, (short)MonsterDirection.ToUp, 2, (short)MonsterDirection.ToWitch, 0 };
            patternBat2.Add(pattern);
            pattern = new short[] { (short)MonsterDirection.ToTop, 0, (short)MonsterDirection.ToRight, 4, (short)MonsterDirection.ToDown, 3, (short)MonsterDirection.ToLeft, 3, (short)MonsterDirection.ToUp, 2, (short)MonsterDirection.ToWitch, 0 };
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
            if (Tools.RND(10000) > (90 - monsters.Count * 10))
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
            monster.PatternDelay = monster.Pattern[1] * DELAY_COEFF;
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
        }

        public void MapScrollToUp()
        {

        }

        public void MapScrollToDown()
        {

        }

        public void DoAnim(DateTime time)
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
                        if (Tools.RND(3) == 1)
                            monster.Pattern[monster.PatternStep] = (short)MonsterDirection.DiagDownRightSpeed;
                        else
                            monster.Pattern[monster.PatternStep] = (short)MonsterDirection.DiagDownLeftSpeed;
                        break;
                    case MonsterDirection.ToTop:
                        monster.Y -= 4;
                        if (monster.Y <= 0)
                        {
                            // pas top
                            monster.Y = 0;
                            monster.PatternDelay = 1;
                            //monster.PatternStep += 2;
                        }
                        break;
                    case MonsterDirection.ToLeft:
                        monster.X -= 4;
                        break;
                    case MonsterDirection.ToRight:
                        monster.X += 4;
                        break;
                    case MonsterDirection.ToDown:
                        monster.Y += 4;
                        break;
                    case MonsterDirection.ToUp:
                        monster.Y -= 4;
                        break;
                    case MonsterDirection.DiagUpRight:
                        monster.Y -= 2;
                        monster.X += 2;
                        break;
                    case MonsterDirection.DiagUpLeft:
                        monster.Y -= 2;
                        monster.X -= 2;
                        break;
                    case MonsterDirection.DiagUpRightSpeed:
                        monster.Y -= 4;
                        monster.X += 4;
                        break;
                    case MonsterDirection.DiagUpLeftSpeed:
                        monster.Y -= 4;
                        monster.X -= 4;
                        break;
                    case MonsterDirection.DiagDownRight:
                        monster.Y += 2;
                        monster.X += 2;
                        break;
                    case MonsterDirection.DiagDownLeft:
                        monster.Y += 2;
                        monster.X -= 2;
                        break;
                    case MonsterDirection.DiagDownRightSpeed:
                        monster.Y += 4;
                        monster.X += 4;
                        break;
                    case MonsterDirection.DiagDownLeftSpeed:
                        monster.Y += 4;
                        monster.X -= 4;
                        break;
                    default:
                        System.Diagnostics.Debug.WriteLine(String.Format("Pattern: {0}", (MonsterDirection)monster.Pattern[monster.PatternStep]));
                        break;
                }
                monster.PatternDelay--;
                if (monster.PatternDelay == 0)
                {
                    monster.PatternStep += 2;
                    if (monster.PatternStep > monster.Pattern.Length)
                    {
                        toDelete.Add(monster);
                    }
                    else
                    {
                        monster.PatternDelay = monster.Pattern[monster.PatternStep + 1] * DELAY_COEFF;
                    }
                }
                else
                {
                    if (monster.PatternDelay == -1)
                    {
                        monster.PatternDelay = 0;
                    }

                    /*switch (monster.Category)
                    {
                        case MonsterType.Bat_1:
                            monster.Y--;
                            break;
                        case MonsterType.Bat_2:
                            monster.Y--;
                            break;
                        case MonsterType.Ghost:
                            monster.Y--;
                            break;
                    }*/
                    if (monster.Y < 0 || monster.Y > 940)//3 * tileWidthScale)
                    {
                        toDelete.Add(monster);
                    }
                    else
                    {
                        if (monster.X < 0 || monster.X > 1640)
                        {
                            toDelete.Add(monster);
                        }
                    }
                }
            }
            foreach (OneMonster m in toDelete)
                monsters.Remove(m);

            // traitement de l'inhibition des générateurs
            List<int> deleteInhibe = new List<int>();
            foreach (var kvp in inhibeID.ToList<KeyValuePair<int, int>>())
            {
                inhibeID[kvp.Key]--;
                if (inhibeID[kvp.Key] < 0)
                    deleteInhibe.Add(kvp.Key); // on peut rendre actif ce générateur
            }
            foreach (int k in deleteInhibe)
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
        }
    }
}