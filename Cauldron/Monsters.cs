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
        float DELAY_COEFF;

        // Bat 1
        List<float[]> patternBat1 = new List<float[]>();
        // Bat 2
        List<float[]> patternBat2 = new List<float[]>();
        // Ghost
        List<float[]> patternGhost = new List<float[]>();


        List<OneMonster> monsters;
        Dictionary<int, int> inhibeID;
        int INHIBE_DELAY;

        const int SPRITE_WIDTH = 3 * 8;
        const int SPRITE_HEIGHT = 3 * 8;

        const int BAT_SPRITES_MAX = 4;
        OneSprite[] spritesBat1 = new OneSprite[BAT_SPRITES_MAX];
        OneSprite[] spritesBat2 = new OneSprite[BAT_SPRITES_MAX];
        const int GHOST_SPRITES_MAX = 8;
        OneSprite[] spritesGhost = new OneSprite[GHOST_SPRITES_MAX];
        int stepX;
        int stepY;

        OneSprite spriteExplode;
        Dictionary<int, OneObject> explodes = new Dictionary<int, OneObject>();
        TimeSpan explodeElaps = TimeSpan.FromMilliseconds(60);
        int idExplode = 0;



        public Monsters(int stepX, int stepY)
        {
            DELAY_COEFF = All.TileWidth; // * All.GAME_SCALE / 1.8f;
            monsters = new List<OneMonster>();
            inhibeID = new Dictionary<int, int>();
            INHIBE_DELAY = All.TileWidth; // tileWidthScale * 2;
            this.stepX = stepX;
            this.stepY = stepY;

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
            float[] pattern = { (short)MovingDirection.DiagUpLeftSpeed, 3, (short)MovingDirection.DiagDownLeftSpeed, 3.0f, (short)MovingDirection.ToLeft, 0 };
            patternBat1.Add(pattern);
            pattern = new float[] { (short)MovingDirection.DiagUpLeftSpeed, 3, (short)MovingDirection.DiagDownLeftSpeed, 3.1f, (short)MovingDirection.ToRight, 0 };
            patternBat1.Add(pattern);
            pattern = new float[] { (short)MovingDirection.DiagUpRight, 2, (short)MovingDirection.DiagDownRight, 2.5f, (short)MovingDirection.ToLeft, 0 };
            patternBat1.Add(pattern);
            pattern = new float[] { (short)MovingDirection.DiagUpRight, 2, (short)MovingDirection.DiagDownRight, 2.5f, (short)MovingDirection.ToRight, 0 };
            patternBat1.Add(pattern);

            pattern = new float[] { (short)MovingDirection.ToTop, 0, (short)MovingDirection.ToLeft, 2, (short)MovingDirection.ToDown, 1.5f, (short)MovingDirection.ToRight, 1.5f, (short)MovingDirection.ToUp, 1.3f, (short)MovingDirection.ToWitch, 0 };
            patternBat2.Add(pattern);
            pattern = new float[] { (short)MovingDirection.ToTop, 0, (short)MovingDirection.ToRight, 2, (short)MovingDirection.ToDown, 1.5f, (short)MovingDirection.ToLeft, 1.5f, (short)MovingDirection.ToUp, 1.3f, (short)MovingDirection.ToWitch, 0 };
            patternBat2.Add(pattern);

            pattern = new float[] { (short)MovingDirection.ToUpSpeed, 1.8f, (short)MovingDirection.DiagUpLeft, 2.7f, (short)MovingDirection.DiagUpRight, 2.7f, (short)MovingDirection.ToWitch, 0 };
            patternGhost.Add(pattern);
            pattern = new float[] { (short)MovingDirection.ToUp, 3.5f, (short)MovingDirection.DiagUpRight, 2.7f, (short)MovingDirection.DiagUpLeft, 2.7f, (short)MovingDirection.ToWitch, 0 };
            patternGhost.Add(pattern);
            pattern = new float[] { (short)MovingDirection.ToUp, 3.5f, (short)MovingDirection.DiagUpLeft, 3, (short)MovingDirection.DiagDownRightSpeed, 1.5f, (short)MovingDirection.ToLeft, 2.0f, (short)MovingDirection.ToDownSpeed, 1.8f };
            patternGhost.Add(pattern);
        }

        public void Generator(MonsterType category, int idGenerator, int x, int y)
        {
            if (inhibeID.ContainsKey(idGenerator))
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
                m.X += All.TileWidth;
                if (m.X > (All.MAP_SHOW * All.TileWidth))
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

        public void MapScrollToLeft()
        {
            List<OneMonster> toDelete = new List<OneMonster>();
            foreach (OneMonster m in monsters)
            {
                m.X -= All.TileWidth;
                if (m.X < 0)
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
                        if (monster.Y <= All.TileHeight)
                        {
                            // pas top
                            monster.Y = 1;
                            monster.PatternDelay = 1;
                            //monster.PatternStep += 2;
                        }
                        break;
                    case MovingDirection.ToLeft:
                        monster.X -= stepX;
                        break;
                    case MovingDirection.ToRight:
                        monster.X += stepX;
                        break;
                    case MovingDirection.ToLeftSpeed:
                        monster.X -= stepX * 2;
                        break;
                    case MovingDirection.ToRightSpeed:
                        monster.X += stepX * 2;
                        break;
                    case MovingDirection.ToDown:
                        monster.Y += stepY;
                        break;
                    case MovingDirection.ToUp:
                        monster.Y -= stepY;
                        break;
                    case MovingDirection.ToDownSpeed:
                        monster.Y += stepY * 2;
                        break;
                    case MovingDirection.ToUpSpeed:
                        monster.Y -= stepY * 2;
                        break;
                    case MovingDirection.DiagUpRight:
                        monster.Y -= stepY;
                        monster.X += stepX;
                        break;
                    case MovingDirection.DiagUpLeft:
                        monster.Y -= stepY;
                        monster.X -= stepX;
                        break;
                    case MovingDirection.DiagUpRightSpeed:
                        monster.Y -= stepY * 2;
                        monster.X += stepX * 2;
                        break;
                    case MovingDirection.DiagUpLeftSpeed:
                        monster.Y -= stepY * 2;
                        monster.X -= stepX * 2;
                        break;
                    case MovingDirection.DiagDownRight:
                        monster.Y += stepY;
                        monster.X += stepX;
                        break;
                    case MovingDirection.DiagDownLeft:
                        monster.Y += stepY;
                        monster.X -= stepX;
                        break;
                    case MovingDirection.DiagDownRightSpeed:
                        monster.Y += stepY * 2;
                        monster.X += stepX * 2;
                        break;
                    case MovingDirection.DiagDownLeftSpeed:
                        monster.Y += stepY * 2;
                        monster.X -= stepX * 2;
                        break;
                    default:
                        System.Diagnostics.Debug.WriteLine(String.Format("Pattern: {0}", (MovingDirection)monster.Pattern[monster.PatternStep]));
                        break;
                }
                bool isDelete = false;
                monster.PatternDelay--;
                if (monster.PatternDelay == 0)
                {
                    monster.PatternStep += 2;
                    if (monster.PatternStep >= monster.Pattern.Length)
                    {
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
                    if (monster.Y < 1 || monster.Y > 940) // TODO: 
                    {
                        toDelete.Add(monster);
                        isDelete = true;
                    }
                    else
                    {
                        if (monster.X < 0 || monster.X > 1640) // TODO: 
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
                                isDelete = true;
                            break;
                        case MonsterType.Bat_2:
                            if (All.IsCollision(monster.X, monster.Y, spritesBat2[monster.AnimationStep].Source,
                                                      All.Witch.X, All.Witch.Y, All.Witch.Source))
                                isDelete = true;
                            break;
                        case MonsterType.Ghost:
                            if (All.IsCollision(monster.X, monster.Y, spritesGhost[monster.AnimationStep].Source,
                                                          All.Witch.X, All.Witch.Y, All.Witch.Source))
                                isDelete = true;
                            break;
                    }
                    /*if (targetX >= monster.X && targetX <= (monster.X + widthSprite))
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
                    }*/
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
                        explodes.Add(temp.ID, temp);
                    }
                } // !isDelete

            } // foreach monster

            foreach (OneMonster m in toDelete)
                monsters.Remove(m);

            List<int> deleteList = new List<int>();

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
                spriteExplode.Draw(canvas, kvp.Value.X, kvp.Value.Y, scrollX);
            }
        } // Draw

    }
}