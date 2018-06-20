﻿// /*
// Author: Boris
// Create: 18/06/2018
// */

using System;
using SkiaSharp;

namespace Cauldron
{

    enum WitchState
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

        const int ANIM_DELAY_WALK = 70;
        const int ANIM_DELAY_MOUNT = 80;
        const int ANIM_DELAY_FLY_SLOW = 350;

        public Witch(int tileWidth, int tileHeight, float scale, int decalX, int decalY)
        {
            toLeft = new OneSprite(20 * 100, tileWidth, tileHeight, 16, 21, 18, 100, scale, decalX, decalY, true);
            toRight = new OneSprite(24 * 100, tileWidth, tileHeight, 16, 21, 18, 100, scale, decalX, decalY, true);
            currentStep = 0;
            toRight.SetAnimSteps(currentStep, currentStep, 0);
            ChangeToState(WitchState.RightWalk);
            moving = false;
        }

        private void PrintState()
        {
            //System.Diagnostics.Debug.WriteLine(String.Format("State: {0}, moving: {1}, current: {2}", state, moving, currentStep));
        }

        private void ChangeToState(WitchState newState)
        {
            switch (newState)
            {
                case WitchState.LeftWalk:
                    animElaps = TimeSpan.FromMilliseconds(ANIM_DELAY_WALK);
                    break;
                case WitchState.LeftMount:
                    toLeft.SetAnimSteps(3, 8, ANIM_DELAY_MOUNT);
                    animElaps = TimeSpan.FromMilliseconds(ANIM_DELAY_MOUNT);
                    break;
                case WitchState.LeftDescend:
                    toLeft.SetAnimSteps(8, 3, ANIM_DELAY_MOUNT);
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
                    toRight.SetAnimSteps(3, 8, ANIM_DELAY_MOUNT);
                    animElaps = TimeSpan.FromMilliseconds(ANIM_DELAY_MOUNT);
                    break;
                case WitchState.RightDescend:
                    toRight.SetAnimSteps(8, 3, ANIM_DELAY_MOUNT);
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

        public void Draw(SKCanvas canvas, SKBitmap tiles)
        {
            switch (state)
            {
                case WitchState.LeftWalk:
                case WitchState.LeftMount:
                case WitchState.LeftDescend:
                case WitchState.LeftFlySlow:
                    toLeft.Draw(canvas, X, Y, tiles);
                    break;
                case WitchState.RightWalk:
                case WitchState.RightMount:
                case WitchState.RightDescend:
                case WitchState.RightFlySlow:
                    toRight.Draw(canvas, X, Y, tiles);
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
        }

        public void MoveToRight()
        {
            moving = true;
            switch (state)
            {
                case WitchState.LeftWalk:
                    currentStep = toLeft.StepAnim;
                    toRight.SetAnimSteps(currentStep, currentStep, 0);
                    state = WitchState.RightWalk;
                    break;
                case WitchState.LeftMount:
                    break;
                case WitchState.LeftDescend:
                    break;
                case WitchState.LeftFlySlow:
                    ChangeToState(WitchState.RightFlySlow);
                    break;
                case WitchState.LeftFly:
                    break;
                case WitchState.RightWalk:
                    break;
                case WitchState.RightMount:
                    break;
                case WitchState.RightDescend:
                    break;
                case WitchState.RightFlySlow:
                    break;
                case WitchState.RightFly:
                    break;
                case WitchState.EnteringDoor:
                    break;
                case WitchState.ExitingDoor:
                    break;
            }
        }

        public void MoveToLeft()
        {
            moving = true;
            switch (state)
            {
                case WitchState.LeftWalk:
                    break;
                case WitchState.LeftMount:
                    break;
                case WitchState.LeftDescend:
                    break;
                case WitchState.LeftFlySlow:
                    break;
                case WitchState.LeftFly:
                    break;
                case WitchState.RightWalk:
                    currentStep = toRight.StepAnim;
                    toLeft.SetAnimSteps(currentStep, currentStep, 0);
                    state = WitchState.LeftWalk;
                    break;
                case WitchState.RightMount:
                    break;
                case WitchState.RightDescend:
                    break;
                case WitchState.RightFlySlow:
                    ChangeToState(WitchState.LeftFlySlow);
                    break;
                case WitchState.RightFly:
                    break;
                case WitchState.EnteringDoor:
                    break;
                case WitchState.ExitingDoor:
                    break;
            }
        }

        public void MoveToUp()
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
                    break;
                case WitchState.LeftFly:
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
                    break;
                case WitchState.RightFly:
                    break;
                case WitchState.EnteringDoor:
                    break;
                case WitchState.ExitingDoor:
                    break;
            }
        }

        public void MoveToDown()
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
                    if (!moving)
                        ChangeToState(WitchState.LeftDescend);
                    break;
                case WitchState.LeftFly:
                    break;
                case WitchState.RightWalk:
                    break;
                case WitchState.RightMount:
                    break;
                case WitchState.RightDescend:
                    break;
                case WitchState.RightFlySlow:
                    if (!moving)
                        ChangeToState(WitchState.RightDescend);
                    break;
                case WitchState.RightFly:
                    break;
                case WitchState.EnteringDoor:
                    break;
                case WitchState.ExitingDoor:
                    break;
            }
        }
    }
}