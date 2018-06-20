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

    public class OneMonster
    {
        public MonsterType Category { get; set; }
        public int ID { get; set; }
        public int AnimationStep { get; set; }
        public int X { get; set; }
        public int Y { get; set; }
    }

    public class Monsters
    {
        int tileWidthScale;
        List<OneMonster> monsters;
        Dictionary<int, int> inhibeID;
        int INHIBE_DELAY;

        const int BAT_SPRITES_MAX = 4;
        OneSprite[] spritesBat = new OneSprite[BAT_SPRITES_MAX];
        //int batIndex;
        const int GHOST_SPRITES_MAX = 8;
        OneSprite[] spritesGhost = new OneSprite[GHOST_SPRITES_MAX];
        //int ghostIndex;


        public Monsters(int tileWidth, int tileHeight, float scale, int decalX, int decalY)
        {
            tileWidthScale = Convert.ToInt32(tileWidth * scale);
            monsters = new List<OneMonster>();
            inhibeID = new Dictionary<int, int>();
            INHIBE_DELAY = tileWidthScale * 2;

            for (int i = 0; i < BAT_SPRITES_MAX; i++)
            {
                spritesBat[i] = new OneSprite(7 * 100 + 100 - 32, tileWidth, tileHeight, 3 * 8, 3 * 8, 4, 100, scale, decalX, decalY);
                spritesBat[i].StepAnim = i % 4;
            }
            for (int i = 0; i < GHOST_SPRITES_MAX; i++)
            {
                spritesGhost[i] = new OneSprite(11 * 100 + 100 - 32, tileWidth, tileHeight, 3 * 8, 3 * 8, 8, 100, scale, decalX, decalY);
                spritesGhost[i].StepAnim = i % 8;
            }
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
                spritesBat[i].DoAnim(time);
            for (int i = 0; i < GHOST_SPRITES_MAX; i++)
                spritesGhost[i].DoAnim(time);
            List<OneMonster> toDelete = new List<OneMonster>();
            foreach (OneMonster monster in monsters)
            {
                switch (monster.Category)
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
                }
                if (monster.Y < 3 * tileWidthScale)
                {
                    toDelete.Add(monster);
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
                    case MonsterType.Bat_2:
                        spritesBat[monster.AnimationStep].Draw(canvas, monster.X, monster.Y, tiles, scrollX);
                        break;
                    case MonsterType.Ghost:
                        spritesGhost[monster.AnimationStep].Draw(canvas, monster.X, monster.Y, tiles, scrollX);
                        break;
                }
            }
        }
    }
}