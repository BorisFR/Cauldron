// /*
// Author: Boris
// Create: 21/06/2018
// */

using System;
using SkiaSharp;

namespace Cauldron
{

    public enum Map
    {
        Outside,
        InHouse,
        GreenCave,
        RedCave
    }

    public class Tile
    {
        public string Name { get; set; }
        public string Content { get; set; }
    }

    public enum WitchState : byte
    {
        LeftWalk, // 0 1 2 3
        LeftMount, // 4 5 6 7
        LeftDescend,
        LeftFlySlow, // 8 9
        LeftFlyMiddle, // 14 15 16 17
        LeftFlyMax, // 10 11 12 13
        RightWalk,
        RightMount,
        RightDescend,
        RightFlySlow,
        RightFlyMiddle,
        RightFlyMax,
        EnterDoor,
        ExitingDoor,
        MakePotion,
        Dying
    }

    public enum GameLocation : byte
    {
        AtHouse,
        AtGreenDoor,
        AtBlueDoor,
        AtRedDoor,
        AtPurpleDoor
    }

    public enum ScriptState : byte
    {
        None,

        // In House
        WalkingToPotion,
        MakingPotion,
        ExitingHouse,

        OpeningDoor,
        EnteringDoor,
        ExitDoor,
        WalkingToRight,
        WalkingToDoor
    }

    public enum PointsType : byte
    {
        Kill_Bat_1 = 20,
        Kill_Bat_2 = 40,
        Kill_Ghost = 50,
        Kill_Seagull = 75,
        Kill_Shark = 25,
        Kill_Plant_1 = 100,
        Kill_Plant_2 = 100,
        Kill_Firebal = 75,
    }

    public enum MagicLoose : byte
    {
        Kill_Bat_1 = 10,
        Kill_Bat_2 = 15,
        Kill_Ghost = 15,
        Kill_Seagull = 15,
        Kill_Shark = 50,
        Kill_Plant_1 = 15,
        Kill_Plant_2 = 15,
        Kill_Firebal = 15,
        Flying = 1,
        Shoot = 1,
    }

    public enum MonsterType : byte
    {
        Bat_1,
        Bat_2,
        Ghost,
        Plant1,
        Plant2,
        Shark
    }

    public enum MovingDirection : short
    {
        None,
        DiagUpLeftSlow,
        DiagUpLeft,
        DiagUpLeftSpeed,
        DiagDownLeftSlow,
        DiagDownLeft,
        DiagDownLeftSpeed,
        DiagUpRight,
        DiagUpRightSpeed,
        DiagDownRight,
        DiagDownRightSpeed,
        ToLeft,
        ToRight,
        ToLeftSpeed,
        ToRightSpeed,
        ToTop,
        ToDown,
        ToUp,
        ToDownSpeed,
        ToUpSpeed,
        ToWitch
    }

    public class OneMonster
    {
        public MonsterType Category { get; set; }
        public int ID { get; set; }
        public int AnimationStep { get; set; }
        public int X { get; set; }
        public int Y { get; set; }
        //public bool UsePattern { get; set; }
        public float[] Pattern { get; set; }
        public int PatternStep { get; set; }
        public int PatternDelay { get; set; }
        public bool PatternNonStop { get; set; }
    }

    public class OneObject
    {
        public int ID { get; set; }
        public int X { get; set; }
        public int Y { get; set; }
        public DateTime Start { get; set; }
        public int Step { get; set; }
        public MovingDirection Moving { get; set; }
        public int TimeToLive { get; set; }
        public SKPaint paintColor { get; set; }
    }


}