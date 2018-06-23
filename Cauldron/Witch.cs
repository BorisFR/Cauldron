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
        WitchState state;
        int currentStep;
        DateTime startAnim;
        TimeSpan animElaps;
        bool moving;

        public int X { get; set; }
        public int Y { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public int MinY { get; set; }
        public int MaxY { get; set; }
        int stepX;
        int stepY;
        int middleY;
        int rightX;

        const int ANIM_DELAY_WALK = 70;
        const int ANIM_DELAY_MOUNT = 80;
        const int ANIM_DELAY_FLY_SLOW = 285;

        public Witch(int stepX, int stepY)
        {
            Width = 2 * All.TileWidth;
            Height = 21;
            toLeft = new OneSprite(20 * 100, 16, 21, 18, 100, true);
            toRight = new OneSprite(24 * 100, 16, 21, 18, 100, true);
            currentStep = 0;
            toRight.SetAnimSteps(currentStep, currentStep, 0);
            ChangeToState(WitchState.RightWalk);
            moving = false;
            this.stepX = stepX;
            this.stepY = stepY;
            middleY = 21 / 2 - 1;// au niveau du balai quand on vole
            rightX = 12 * 2;
        }

        private void PrintState()
        {
            //System.Diagnostics.Debug.WriteLine(String.Format("State: {0}, moving: {1}, current: {2}", state, moving, currentStep));
        }

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
                    case WitchState.LeftFly:
                        return toLeft.Source;
                    case WitchState.RightWalk:
                    case WitchState.RightMount:
                    case WitchState.RightDescend:
                    case WitchState.RightFlySlow:
                    case WitchState.RightFly:
                        return toRight.Source;
                }
                return toLeft.Source; // TODO: entering / Exit door
            }
        }

        public bool IsFlying
        {
            get
            {
                switch (state)
                {
                    case WitchState.LeftFly:
                    case WitchState.LeftFlySlow:
                    case WitchState.RightFly:
                    case WitchState.RightFlySlow:
                        return true;
                }
                return false;
            }
        }

        public MovingDirection Direction
        {
            get
            {
                switch (state)
                {
                    case WitchState.LeftDescend:
                    case WitchState.LeftFly:
                    case WitchState.LeftWalk:
                    case WitchState.LeftMount:
                    case WitchState.LeftFlySlow:
                        return MovingDirection.ToLeft;
                    case WitchState.RightDescend:
                    case WitchState.RightFly:
                    case WitchState.RightFlySlow:
                    case WitchState.RightMount:
                    case WitchState.RightWalk:
                        return MovingDirection.ToRight;
                    default:
                        return MovingDirection.None;
                }
            }
        }

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

        public int BulletY
        {
            get
            {
                return Y + middleY;
            }
        }

        private void ChangeToState(WitchState newState)
        {
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
                case WitchState.LeftFly:
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
                case WitchState.RightFly:
                    break;
                case WitchState.EnteringDoor:
                    break;
                case WitchState.ExitingDoor:
                    break;
            }
            state = newState;
            startAnim = DateTime.UtcNow;
            PrintState();
        }

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
                    toLeft.DoAnim(time);
                    currentStep = toLeft.StepAnim;
                    break;
                case WitchState.RightFlySlow:
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
            }
        }

        public void Draw(SKCanvas canvas)
        {
            switch (state)
            {
                case WitchState.LeftWalk:
                case WitchState.LeftMount:
                case WitchState.LeftDescend:
                case WitchState.LeftFlySlow:
                case WitchState.LeftFly:
                    toLeft.Draw(canvas, X, Y);
                    break;
                case WitchState.RightWalk:
                case WitchState.RightMount:
                case WitchState.RightDescend:
                case WitchState.RightFlySlow:
                case WitchState.RightFly:
                    toRight.Draw(canvas, X, Y);
                    break;
            }
        }

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
                case WitchState.LeftFly:
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
                case WitchState.RightFly:
                    break;
                case WitchState.EnteringDoor:
                    break;
                case WitchState.ExitingDoor:
                    break;
            }
        } // MoveStop

        public float MoveToRight()
        {
            moving = true;
            switch (state)
            {
                case WitchState.LeftWalk:
                    currentStep = toLeft.StepAnim;
                    toRight.SetAnimSteps(currentStep, currentStep, 0);
                    state = WitchState.RightWalk;
                    return 0;
                case WitchState.LeftMount:
                    return 0;
                case WitchState.LeftDescend:
                    return 0;
                case WitchState.LeftFlySlow:
                    ChangeToState(WitchState.RightFlySlow);
                    return 0;
                case WitchState.LeftFly:
                    return 0;
                case WitchState.RightWalk:
                    return 1.6f;
                case WitchState.RightMount:
                    return 0;
                case WitchState.RightDescend:
                    return 0;
                case WitchState.RightFlySlow:
                    return 1.6f;
                case WitchState.RightFly:
                    return 2;
                case WitchState.EnteringDoor:
                    return 0;
                case WitchState.ExitingDoor:
                    return 0;
            }
            return 0;
        } // MoveToRight

        public float MoveToLeft()
        {
            moving = true;
            switch (state)
            {
                case WitchState.LeftWalk:
                    return 1.6f;
                case WitchState.LeftMount:
                    return 0;
                case WitchState.LeftDescend:
                    return 0;
                case WitchState.LeftFlySlow:
                    return 1.6f;
                case WitchState.LeftFly:
                    return 2;
                case WitchState.RightWalk:
                    currentStep = toRight.StepAnim;
                    toLeft.SetAnimSteps(currentStep, currentStep, 0);
                    state = WitchState.LeftWalk;
                    return 0;
                case WitchState.RightMount:
                    return 0;
                case WitchState.RightDescend:
                    return 0;
                case WitchState.RightFlySlow:
                    ChangeToState(WitchState.LeftFlySlow);
                    return 0;
                case WitchState.RightFly:
                    return 0;
                case WitchState.EnteringDoor:
                    return 0;
                case WitchState.ExitingDoor:
                    return 0;
            }
            return 0;
        } // MoveToLeft

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
                    if (Y > MinY)
                    {
                        Y -= stepY;
                        if (Y < MinY)
                            Y = MinY;
                    }
                    break;
                case WitchState.LeftFly:
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
                    if (Y > MinY)
                    {
                        Y -= stepY;
                        if (Y < MinY)
                            Y = MinY;
                    }
                    break;
                case WitchState.RightFly:
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
            }
            return 0;
        } // MoveToUp

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
                case WitchState.LeftFly:
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
                case WitchState.RightFly:
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
            }
            return 0;
        } // MoveToDown

    }
}