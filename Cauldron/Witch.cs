// /*
// Author: Boris
// Create: 18/06/2018
// */

using System;
using System.Drawing;
using SkiaSharp;

namespace Cauldron
{

    public class Witch
    {
        OneSprite toLeft;
        OneSprite toRight;
        OneSprite spriteMakePotion;
        OneSprite spriteDying;
        WitchState state;
        int currentStep;
        DateTime startAnim;
        TimeSpan animElaps;
        bool moving;

        const float SPEED_SLOW = 1.6f;
        const float SPEED_MAX = 8.0f;
        const float SPEED_DELTA = SPEED_MAX - SPEED_SLOW;

        public int X { get; set; }
        public int Y { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public int MinY { get; set; }
        public int MaxY { get; set; }
        int stepX;
        int stepY;
        int rightX;
        public bool CouldFly { get; set; }

        const int ANIM_DELAY_WALK = 70;
        const int ANIM_DELAY_MOUNT = 80;
        const int ANIM_DELAY_FLY_SLOW = 285;
        const int ANIM_DELAY_FLY_MIDDLE = 135;
        const int ANIM_DELAY_FLY_MAX = 125;
        const int ANIM_DELAY_MAKE_POTION = 150;
        const int ANIM_DELAY_DYING = 150;

        // *********************************************************************

        public Witch(int stepX, int stepY)
        {
            Width = 2 * All.TileWidth;
            Height = 21;
            toLeft = new OneSprite(20 * 100, 16, 21, 18, 100, true);
            toRight = new OneSprite(24 * 100, 16, 21, 18, 100, true);
            spriteMakePotion = new OneSprite(32 * 100, 16, 21, 3, 80, true);
            spriteDying = new OneSprite(38 * 100, 16, 21, 8, 80, true);
            currentStep = 0;
            toRight.SetAnimSteps(currentStep, currentStep, 0);
            ChangeToState(WitchState.RightWalk);
            moving = false;
            this.stepX = stepX;
            this.stepY = stepY;
            rightX = 12 * 2;
        }

        // *********************************************************************

        private void PrintState()
        {
            //System.Diagnostics.Debug.WriteLine(String.Format("State: {0}, moving: {1}, current: {2}", state, moving, currentStep));
        }

        // *********************************************************************

        public Rectangle Source
        {
            get
            {
                switch (state)
                {
                    case WitchState.LeftWalk:
                    case WitchState.LeftMount:
                    case WitchState.LeftDescend:
                    case WitchState.LeftFlySlow:
                    case WitchState.LeftFlyMiddle:
                    case WitchState.LeftFlyMax:
                        return toLeft.Source;
                    case WitchState.RightWalk:
                    case WitchState.RightMount:
                    case WitchState.RightDescend:
                    case WitchState.RightFlySlow:
                    case WitchState.RightFlyMiddle:
                    case WitchState.RightFlyMax:
                        return toRight.Source;
                }
                return toLeft.Source; // TODO: entering / Exit door
            }
        }

        // *********************************************************************

        public bool IsWalking
        {
            get
            {
                switch (state)
                {
                    case WitchState.LeftWalk:
                    case WitchState.RightWalk:
                        return true;
                }
                return false;
            }
        }

        // *********************************************************************

        public bool IsFlying
        {
            get
            {
                switch (state)
                {
                    case WitchState.LeftFlyMiddle:
                    case WitchState.LeftFlyMax:
                    case WitchState.LeftFlySlow:
                    case WitchState.RightFlyMiddle:
                    case WitchState.RightFlyMax:
                    case WitchState.RightFlySlow:
                        return true;
                }
                return false;
            }
        }

        // *********************************************************************

        public MovingDirection Direction
        {
            get
            {
                switch (state)
                {
                    case WitchState.LeftDescend:
                    case WitchState.LeftFlyMiddle:
                    case WitchState.LeftFlyMax:
                    case WitchState.LeftWalk:
                    case WitchState.LeftMount:
                    case WitchState.LeftFlySlow:
                        return MovingDirection.ToLeft;
                    case WitchState.RightDescend:
                    case WitchState.RightFlyMiddle:
                    case WitchState.RightFlyMax:
                    case WitchState.RightFlySlow:
                    case WitchState.RightMount:
                    case WitchState.RightWalk:
                        return MovingDirection.ToRight;
                    default:
                        return MovingDirection.None;
                }
            }
        }

        // *********************************************************************

        public int BulletX
        {
            get
            {
                if (Direction == MovingDirection.ToLeft)
                {
                    return X;
                }
                return X + rightX;
            }
        }

        // *********************************************************************

        public void DoDying()
        {
            ChangeToState(WitchState.Dying);
            PrintState();
        }

        // *********************************************************************

        public void DoAlive()
        {
            state = WitchState.MakePotion;
            PrintState();
        }

        // *********************************************************************

        public void DoPotion()
        {
            ChangeToState(WitchState.MakePotion);
            PrintState();
        }

        // *********************************************************************

        public void DoWalk()
        {
            ChangeToState(WitchState.RightWalk);
            currentStep = 0;
            toRight.SetAnimSteps(currentStep, currentStep, 0);
            PrintState();
        }

        // *********************************************************************

        private void ChangeToState(WitchState newState)
        {
            if (state == WitchState.Dying)
                return;
            switch (newState)
            {
                case WitchState.LeftWalk:
                    animElaps = TimeSpan.FromMilliseconds(ANIM_DELAY_WALK);
                    break;
                case WitchState.LeftMount:
                    toLeft.SetAnimSteps(3, 8, ANIM_DELAY_MOUNT, true);
                    animElaps = TimeSpan.FromMilliseconds(ANIM_DELAY_MOUNT);
                    break;
                case WitchState.LeftDescend:
                    toLeft.SetAnimSteps(8, 3, ANIM_DELAY_MOUNT, true);
                    animElaps = TimeSpan.FromMilliseconds(ANIM_DELAY_MOUNT);
                    break;
                case WitchState.LeftFlySlow:
                    toLeft.SetAnimSteps(8, 9, ANIM_DELAY_FLY_SLOW);
                    animElaps = TimeSpan.FromMilliseconds(ANIM_DELAY_FLY_SLOW);
                    break;
                case WitchState.LeftFlyMiddle:
                    toLeft.SetAnimSteps(14, 17, ANIM_DELAY_FLY_MIDDLE);
                    animElaps = TimeSpan.FromMilliseconds(ANIM_DELAY_FLY_MIDDLE);
                    break;
                case WitchState.LeftFlyMax:
                    toLeft.SetAnimSteps(10, 13, ANIM_DELAY_FLY_MAX);
                    animElaps = TimeSpan.FromMilliseconds(ANIM_DELAY_FLY_MAX);
                    break;
                case WitchState.RightWalk:
                    animElaps = TimeSpan.FromMilliseconds(ANIM_DELAY_WALK);
                    break;
                case WitchState.RightMount:
                    toRight.SetAnimSteps(3, 8, ANIM_DELAY_MOUNT, true);
                    animElaps = TimeSpan.FromMilliseconds(ANIM_DELAY_MOUNT);
                    break;
                case WitchState.RightDescend:
                    toRight.SetAnimSteps(8, 3, ANIM_DELAY_MOUNT, true);
                    animElaps = TimeSpan.FromMilliseconds(ANIM_DELAY_MOUNT);
                    break;
                case WitchState.RightFlySlow:
                    toRight.SetAnimSteps(8, 9, ANIM_DELAY_FLY_SLOW);
                    animElaps = TimeSpan.FromMilliseconds(ANIM_DELAY_FLY_SLOW);
                    break;
                case WitchState.RightFlyMiddle:
                    toRight.SetAnimSteps(14, 17, ANIM_DELAY_FLY_MIDDLE);
                    animElaps = TimeSpan.FromMilliseconds(ANIM_DELAY_FLY_MIDDLE);
                    break;
                case WitchState.RightFlyMax:
                    toRight.SetAnimSteps(10, 13, ANIM_DELAY_FLY_MAX);
                    animElaps = TimeSpan.FromMilliseconds(ANIM_DELAY_FLY_MAX);
                    break;
                case WitchState.EnteringDoor:
                    break;
                case WitchState.ExitingDoor:
                    break;
                case WitchState.MakePotion:
                    animElaps = TimeSpan.FromMilliseconds(ANIM_DELAY_MAKE_POTION);
                    break;
                case WitchState.Dying:
                    animElaps = TimeSpan.FromMilliseconds(ANIM_DELAY_DYING);
                    break;
            }
            state = newState;
            startAnim = DateTime.UtcNow;
            PrintState();
        }

        // *********************************************************************

        public void DoAnim(DateTime time)
        {
            if ((time - startAnim) < animElaps)
                return;
            startAnim = time;
            switch (state)
            {
                case WitchState.LeftWalk:
                    if (!moving) return;
                    currentStep = toLeft.StepAnim;
                    currentStep++;
                    if (currentStep == 4)
                        currentStep = 0;
                    toLeft.SetAnimSteps(currentStep, currentStep, 0);
                    PrintState();
                    break;
                case WitchState.RightWalk:
                    if (!moving) return;
                    currentStep = toRight.StepAnim;
                    currentStep++;
                    if (currentStep == 4)
                        currentStep = 0;
                    toRight.SetAnimSteps(currentStep, currentStep, 0);
                    PrintState();
                    break;
                case WitchState.LeftMount:
                    toLeft.DoAnim(time);
                    currentStep = toLeft.StepAnim;
                    PrintState();
                    if (currentStep == 8)
                        ChangeToState(WitchState.LeftFlySlow);
                    break;
                case WitchState.RightMount:
                    toRight.DoAnim(time);
                    currentStep = toRight.StepAnim;
                    PrintState();
                    if (currentStep == 8)
                        ChangeToState(WitchState.RightFlySlow);
                    break;
                case WitchState.LeftFlySlow:
                case WitchState.LeftFlyMiddle:
                case WitchState.LeftFlyMax:
                    toLeft.DoAnim(time);
                    currentStep = toLeft.StepAnim;
                    break;
                case WitchState.RightFlySlow:
                case WitchState.RightFlyMiddle:
                case WitchState.RightFlyMax:
                    toRight.DoAnim(time);
                    currentStep = toRight.StepAnim;
                    break;
                case WitchState.LeftDescend:
                    toLeft.DoAnim(time);
                    currentStep = toLeft.StepAnim;
                    PrintState();
                    if (currentStep == 3)
                    {
                        toLeft.SetAnimSteps(currentStep, currentStep, 0);
                        ChangeToState(WitchState.LeftWalk);
                    }
                    break;
                case WitchState.RightDescend:
                    toRight.DoAnim(time);
                    currentStep = toRight.StepAnim;
                    PrintState();
                    if (currentStep == 3)
                    {
                        toRight.SetAnimSteps(currentStep, currentStep, 0);
                        ChangeToState(WitchState.RightWalk);
                    }
                    break;
                case WitchState.MakePotion:
                    spriteMakePotion.DoAnim(time);
                    break;
                case WitchState.Dying:
                    spriteDying.DoAnim(time);
                    break;
            }
        }

        // *********************************************************************

        public void Draw(SKCanvas canvas)
        {
            switch (state)
            {
                case WitchState.LeftWalk:
                case WitchState.LeftMount:
                case WitchState.LeftDescend:
                case WitchState.LeftFlySlow:
                case WitchState.LeftFlyMiddle:
                case WitchState.LeftFlyMax:
                    toLeft.Draw(canvas, X, Y);
                    break;
                case WitchState.RightWalk:
                case WitchState.RightMount:
                case WitchState.RightDescend:
                case WitchState.RightFlySlow:
                case WitchState.RightFlyMiddle:
                case WitchState.RightFlyMax:
                    toRight.Draw(canvas, X, Y);
                    break;
                case WitchState.MakePotion:
                    spriteMakePotion.Draw(canvas, X, Y);
                    break;
                case WitchState.Dying:
                    spriteDying.Draw(canvas, X, Y);
                    break;
            }
        }

        // *********************************************************************

        public void MoveStop()
        {
            switch (state)
            {
                case WitchState.LeftWalk:
                    moving = false;
                    break;
                case WitchState.LeftMount:
                    break;
                case WitchState.LeftDescend:
                    break;
                case WitchState.LeftFlySlow:
                    moving = false;
                    break;
                case WitchState.LeftFlyMiddle:
                case WitchState.LeftFlyMax:
                    break;
                case WitchState.RightWalk:
                    moving = false;
                    break;
                case WitchState.RightMount:
                    break;
                case WitchState.RightDescend:
                    break;
                case WitchState.RightFlySlow:
                    moving = false;
                    break;
                case WitchState.RightFlyMiddle:
                case WitchState.RightFlyMax:
                    break;
                case WitchState.EnteringDoor:
                    break;
                case WitchState.ExitingDoor:
                    break;
                case WitchState.MakePotion:
                    break;
                case WitchState.Dying:
                    break;
            }
        } // MoveStop

        // *********************************************************************

        int delta;

        private float CalcSpeedLeft()
        {
            delta = All.SPEED_LEFT_MIDDLE - X;
            return SPEED_SLOW + SPEED_DELTA * (delta * 100.0f / All.SPEED_DELTA_X) / 100.0f;
        }

        private float CalcSpeedRight()
        {
            delta = X - All.SPEED_RIGHT_MIDDLE;
            return SPEED_SLOW + SPEED_DELTA * (delta * 100.0f / All.SPEED_DELTA_X) / 100.0f;
        }

        public float MoveToRight()
        {
            moving = true;
            switch (state)
            {
                case WitchState.LeftWalk: // demi-tour
                    currentStep = toLeft.StepAnim;
                    toRight.SetAnimSteps(currentStep, currentStep, 0);
                    state = WitchState.RightWalk;
                    return 0;
                case WitchState.LeftMount:
                    return 0;
                case WitchState.LeftDescend:
                    return 0;
                case WitchState.LeftFlySlow:
                    X -= 3;
                    if (X < All.MIDDLE_MAP)
                    {
                        ChangeToState(WitchState.RightFlySlow);
                    }
                    return 0.0f;
                case WitchState.LeftFlyMiddle:
                    X -= 3;
                    if (X < All.SPEED_RIGHT_MIDDLE)
                    {
                        ChangeToState(WitchState.LeftFlySlow);
                        return 0.0f;
                    }
                    return -CalcSpeedRight();
                case WitchState.LeftFlyMax:
                    X -= 3;
                    if (X < All.SPEED_RIGHT_MAX)
                    {
                        ChangeToState(WitchState.LeftFlyMiddle);
                    }
                    return -CalcSpeedRight();
                case WitchState.RightWalk:
                    if (X < All.MIDDLE_MAP)
                    {
                        X += 1;
                        return 0.0f;
                    }
                    // TODO: changer ça si on veut bloquer le scroll de la carte (sous terrain)
                    // faire plutôt X+=1 et retourner 0.0f
                    return SPEED_SLOW;
                case WitchState.RightMount:
                    return 0;
                case WitchState.RightDescend:
                    return 0;
                case WitchState.RightFlySlow:
                    X -= 3;
                    if (X < All.SPEED_LEFT_MIDDLE)
                    {
                        ChangeToState(WitchState.RightFlyMiddle);
                        return CalcSpeedLeft();
                    }
                    return 0.0f;
                case WitchState.RightFlyMiddle:
                    X -= 3;
                    if (X < All.SPEED_LEFT_MAX)
                    {
                        ChangeToState(WitchState.RightFlyMax);
                    }
                    return CalcSpeedLeft();
                case WitchState.RightFlyMax:
                    return CalcSpeedLeft();
                case WitchState.EnteringDoor:
                    return 0;
                case WitchState.ExitingDoor:
                    return 0;
                case WitchState.MakePotion:
                    return 0;
                case WitchState.Dying:
                    return 0;
            }
            return 0;
        } // MoveToRight

        // *********************************************************************

        public float MoveToLeft()
        {
            moving = true;
            switch (state)
            {
                case WitchState.LeftWalk:
                    if (X > All.MIDDLE_MAP)
                    {
                        X -= 1;
                        return 0.0f;
                    }
                    // TODO: changer ça si on veut bloquer le scroll de la carte (sous terrain)
                    // faire plutôt X+=1 et retourner 0.0f
                    return SPEED_SLOW;
                case WitchState.LeftMount:
                    return 0;
                case WitchState.LeftDescend:
                    return 0;
                case WitchState.LeftFlySlow:
                    X += 3;
                    if (X >= All.SPEED_RIGHT_MIDDLE)
                    {
                        ChangeToState(WitchState.LeftFlyMiddle);
                        return CalcSpeedRight();
                    }
                    return 0.0f;
                case WitchState.LeftFlyMiddle:
                    X += 3;
                    if (X >= All.SPEED_RIGHT_MAX)
                    {
                        ChangeToState(WitchState.LeftFlyMax);
                    }
                    return CalcSpeedRight();
                case WitchState.LeftFlyMax:
                    return CalcSpeedRight();
                case WitchState.RightWalk: // demi-tour
                    currentStep = toRight.StepAnim;
                    toLeft.SetAnimSteps(currentStep, currentStep, 0);
                    state = WitchState.LeftWalk;
                    return 0;
                case WitchState.RightMount:
                    return 0;
                case WitchState.RightDescend:
                    return 0;
                case WitchState.RightFlySlow:
                    X += 3;
                    if (X > All.MIDDLE_MAP)
                    {
                        ChangeToState(WitchState.LeftFlySlow);
                    }
                    return 0.0f;
                case WitchState.RightFlyMiddle:
                    X += 3;
                    if (X > All.SPEED_LEFT_MIDDLE)
                    {
                        ChangeToState(WitchState.RightFlySlow);
                        return 0.0f;
                    }
                    return -CalcSpeedLeft();
                case WitchState.RightFlyMax:
                    X += 3;
                    if (X > All.SPEED_LEFT_MAX)
                    {
                        ChangeToState(WitchState.RightFlyMiddle);
                    }
                    return -CalcSpeedLeft();
                case WitchState.EnteringDoor:
                    return 0;
                case WitchState.ExitingDoor:
                    return 0;
                case WitchState.MakePotion:
                    return 0;
                case WitchState.Dying:
                    return 0;
            }
            return 0;
        } // MoveToLeft

        // *********************************************************************

        public int MoveToUp()
        {
            switch (state)
            {
                case WitchState.LeftWalk:
                    if (!moving)
                        ChangeToState(WitchState.LeftMount);
                    break;
                case WitchState.LeftMount:
                    break;
                case WitchState.LeftDescend:
                    break;
                case WitchState.LeftFlySlow:
                case WitchState.LeftFlyMiddle:
                case WitchState.LeftFlyMax:
                    if (Y > MinY)
                    {
                        Y -= stepY;
                        if (Y < MinY)
                            Y = MinY;
                    }
                    break;
                case WitchState.RightWalk:
                    if (!moving)
                        ChangeToState(WitchState.RightMount);
                    break;
                case WitchState.RightMount:
                    break;
                case WitchState.RightDescend:
                    break;
                case WitchState.RightFlySlow:
                case WitchState.RightFlyMiddle:
                case WitchState.RightFlyMax:
                    if (Y > MinY)
                    {
                        Y -= stepY;
                        if (Y < MinY)
                            Y = MinY;
                    }
                    break;
                case WitchState.EnteringDoor:
                    break;
                case WitchState.ExitingDoor:
                    break;
                case WitchState.MakePotion:
                    break;
                case WitchState.Dying:
                    break;
            }
            return 0;
        } // MoveToUp

        // *********************************************************************

        public int MoveToDown()
        {
            switch (state)
            {
                case WitchState.LeftWalk:
                    break;
                case WitchState.LeftMount:
                    break;
                case WitchState.LeftDescend:
                    break;
                case WitchState.LeftFlySlow:
                    //if (!moving)
                    if (Y == MaxY)
                        ChangeToState(WitchState.LeftDescend);
                    else
                    {
                        if (Y < MaxY)
                        {
                            Y += stepY;
                            if (Y > MaxY)
                                Y = MaxY;
                        }
                    }
                    break;
                case WitchState.LeftFlyMiddle:
                case WitchState.LeftFlyMax:
                    if (Y < MaxY)
                    {
                        Y += stepY;
                        if (Y > MaxY)
                            Y = MaxY;
                    }
                    break;
                case WitchState.RightWalk:
                    break;
                case WitchState.RightMount:
                    break;
                case WitchState.RightDescend:
                    break;
                case WitchState.RightFlySlow:
                    //if (!moving)
                    if (Y == MaxY)
                        ChangeToState(WitchState.RightDescend);
                    else
                    {
                        if (Y < MaxY)
                        {
                            Y += stepY;
                            if (Y > MaxY)
                                Y = MaxY;
                        }
                    }
                    break;
                case WitchState.RightFlyMiddle:
                case WitchState.RightFlyMax:
                    if (Y < MaxY)
                    {
                        Y += stepY;
                        if (Y > MaxY)
                            Y = MaxY;
                    }
                    break;
                case WitchState.EnteringDoor:
                    break;
                case WitchState.ExitingDoor:
                    break;
                case WitchState.MakePotion:
                    break;
                case WitchState.Dying:
                    break;
            }
            return 0;
        } // MoveToDown

    }
}