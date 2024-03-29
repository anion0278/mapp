﻿using System;
using System.Threading.Tasks;
using Mapp.Application.Interfaces;
using WindowsInput.Native;

namespace Mapp.BusinessLogic.AutoComplete;

// TODO move into separate assembly, since this part is not applicable to ASP NET proj

public interface IAutoKeyboardInputHelper : IDisposable
{
}

public class AutoKeyboardInputHelper : IAutoKeyboardInputHelper
{
    private readonly IAutocompleteConfiguration _autocompleteConfiguration;
    private IInputSimulator _inputSim;
    private IKeyboardHook _keyboardHook;
    private bool _isCommandPressed;

    public AutoKeyboardInputHelper(IAutocompleteConfiguration autocompleteConfiguration, IKeyboardHook keyboardHook, IInputSimulator inputSimulator)
    {
        _autocompleteConfiguration = autocompleteConfiguration;
        _inputSim = inputSimulator;
        _keyboardHook = keyboardHook; // TODO replace by https://www.nuget.org/packages/MouseKeyHook/
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

    private void keyboardHook_KeyDown(object sender, VKeys key)
    {
        if (key == VKeys.F2)
        {
            _isCommandPressed = true;
        }

        //var elapsedTime = _lastAutoinputTime - DateTime.Now;
        if (key == VKeys.F4 && _isCommandPressed) /*&& elapsedTime.Seconds > 2*/
        {
            //_lastAutoinputTime = DateTime.Now;
            _inputSim.TextEntry($"RR{_autocompleteConfiguration.TrackingCode}CZ");
            _inputSim.KeyPress(VirtualKeyCode.TAB);
            Task.Delay(TimeSpan.FromMilliseconds(50));
            _inputSim.TextEntry(DateTime.Now.ToString("dd.MM.yyyy"));
            Task.Delay(TimeSpan.FromMilliseconds(50));
            _inputSim.ModifiedKeyStroke(new[] { VirtualKeyCode.SHIFT }, VirtualKeyCode.TAB);
            _inputSim.KeyPress(VirtualKeyCode.HOME);
            for (int i = 0; i < 8; i++)
            {
                _inputSim.KeyPress(VirtualKeyCode.RIGHT);
            }
        }
    }

    private void keyboardHook_KeyUp(object sender, VKeys key)
    {
        if (key == VKeys.F2)
        {
            _isCommandPressed = false;
        }
    }
}

public interface IAutocompleteConfiguration
{
    public string TrackingCode { get; set; }
}

public class AutocompleteConfiguration : IAutocompleteConfiguration
{
    public string TrackingCode { get; set; }
}