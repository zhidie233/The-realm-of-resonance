﻿using System;
namespace Game.InputManager
{
    [Flags]
    public enum InputType : short
    {
        Keyboard = 0,
        Xbox = 1,
        Playstation = 2,
        Other = 3,
    }
}