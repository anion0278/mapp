﻿using System;
using WindowsInput;
using WindowsInput.Native;

namespace Shmap.BusinessLogic.AutocompletionHelper
{
    public interface IAutoKeyboardInputHelper: IDisposable
    {
        string TrackingCode { get; set; }
    }

    public class AutoKeyboardInputHelper: IAutoKeyboardInputHelper
    {
        private InputSimulator _keyboardSim = new InputSimulator();
        private KeyboardHook _keyboardHook;
        private bool _isCommandPressed = false;
        //public DateTime _lastAutoinputTime;

        public string TrackingCode { get; set; }// TODO should be somehow connected with VM

        public AutoKeyboardInputHelper() 
        {
            _keyboardHook = new KeyboardHook(); // TODO replace by https://www.nuget.org/packages/MouseKeyHook/
            _keyboardHook.KeyDown += keyboardHook_KeyDown;
            _keyboardHook.KeyUp += keyboardHook_KeyUp;

            //Installing the Keyboard Hooks
            _keyboardHook.Install();
        }

        public void Dispose()
        {
            _keyboardHook.KeyDown -= keyboardHook_KeyDown;
            _keyboardHook.KeyUp -= keyboardHook_KeyUp;
            _keyboardHook.Uninstall();
        }

        private void keyboardHook_KeyDown(KeyboardHook.VKeys key)
        {
            if (key == KeyboardHook.VKeys.F2)
            {
                _isCommandPressed = true;
            }

            //var elapsedTime = _lastAutoinputTime - DateTime.Now;
            if (key == KeyboardHook.VKeys.F4 && _isCommandPressed ) /*&& elapsedTime.Seconds > 2*/
            {
                //_lastAutoinputTime = DateTime.Now;
                _keyboardSim.Keyboard.TextEntry($"RR{TrackingCode}CZ");
                _keyboardSim.Keyboard.KeyPress(VirtualKeyCode.TAB);
                _keyboardSim.Keyboard.Sleep(50);
                _keyboardSim.Keyboard.TextEntry(DateTime.Now.ToString("dd.MM.yyyy"));
                _keyboardSim.Keyboard.Sleep(50);
                _keyboardSim.Keyboard.ModifiedKeyStroke(new[] { VirtualKeyCode.SHIFT }, VirtualKeyCode.TAB);
                _keyboardSim.Keyboard.KeyPress(VirtualKeyCode.HOME);
                for (int i = 0; i < 8; i++)
                {
                    _keyboardSim.Keyboard.KeyPress(VirtualKeyCode.RIGHT);
                }
            }
        }

        private void keyboardHook_KeyUp(KeyboardHook.VKeys key)
        {
            if (key == KeyboardHook.VKeys.F2)
            {
                _isCommandPressed = false;
            }
        }
    }
}