using System;
using System.Threading.Tasks;
using Mapp.Application.Interfaces;
using Mapp.Common;
using WindowsInput.Native;

namespace Mapp.BusinessLogic.AutoComplete;

// TODO move into separate assembly, since this part is not applicable to ASP NET proj

public interface IAutoKeyboardInputHelper : IDisposable
{
}

public class AutoKeyboardInputHelper : IAutoKeyboardInputHelper
{
    private readonly ISettingsWrapper _settingsWrapper;
    private IInputSimulator _inputSim;
    private IKeyboardHook _keyboardHook;
    private bool _isCommandPressed;

    public AutoKeyboardInputHelper(ISettingsWrapper settingsWrapper, IKeyboardHook keyboardHook, IInputSimulator inputSimulator)
    {
        _settingsWrapper = settingsWrapper;
        _inputSim = inputSimulator;
        _keyboardHook = keyboardHook; // TODO replace by https://www.nuget.org/packages/MouseKeyHook/

#if RELEASE
        _keyboardHook.KeyDown += keyboardHook_KeyDown;
        _keyboardHook.KeyUp += keyboardHook_KeyUp;

        //Installing the Keyboard Hooks
        _keyboardHook.Install();
#endif
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
            _inputSim.TextEntry($"RR{_settingsWrapper.TrackingCode}CZ");
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
