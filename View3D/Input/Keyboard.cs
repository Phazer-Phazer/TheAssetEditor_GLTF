﻿using Microsoft.Xna.Framework.Input;
using MonoGame.Framework.WpfInterop.Input;
using System;
using System.Collections.Generic;
using System.Text;

namespace View3D.Input
{
    public delegate void KeybordButtonReleasedDelegate(Keys key);

    public class Keyboard
    {
        public event KeybordButtonReleasedDelegate KeybordButtonReleased;

        KeyboardState _currentKeyboardState;
        KeyboardState _lastKeyboardState;

        WpfKeyboard _wpfKeyboard;

        public Keyboard(WpfKeyboard wpfKeyboard)
        {
            _wpfKeyboard = wpfKeyboard;
        }

        public void Update()
        {
            var keyboardState = _wpfKeyboard.GetState();

            _lastKeyboardState = _currentKeyboardState;
            _currentKeyboardState = keyboardState;

            if (_lastKeyboardState == null)
                _lastKeyboardState = keyboardState;

            foreach (var key in _lastKeyboardState.GetPressedKeys())
            {
                if (IsKeyUp(key))
                    KeybordButtonReleased?.Invoke(key);
            }
        }

        public bool IsKeyReleased(Keys key)
        {
            var currentUp = _currentKeyboardState.IsKeyUp(key);
            var lastDown = _lastKeyboardState.IsKeyDown(key);
            return currentUp && lastDown;
        }

        public bool IsKeyDown(Keys key)
        {
            return _currentKeyboardState.IsKeyDown(key);
        }

        public bool IsKeyUp(Keys key)
        {
            return _currentKeyboardState.IsKeyUp(key);
        }
    }
}