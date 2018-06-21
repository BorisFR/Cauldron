// /*
// Author: Boris
// Create: 21/06/2018
// */

using System;

namespace Cauldron
{

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
        LeftFly, // 10 11 12 13 14 15
        RightWalk,
        RightMount,
        RightDescend,
        RightFlySlow,
        RightFly,
        EnteringDoor,
        ExitingDoor
    }

    public enum MonsterType
    {
        Bat_1,
        Bat_2,
        Ghost
    }

    public enum MovingDirection : short
    {
        None,
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
        public float[] Pattern { get; set; }
        public int PatternStep { get; set; }
        public int PatternDelay { get; set; }
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
    }


}