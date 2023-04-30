using WindowsInput.Native;
using IInputSimulator = Shmap.Application.Interfaces.IInputSimulator;

namespace Shmap.Infrastructure.Input;

public class InputSimulator : IInputSimulator
{
    private readonly WindowsInput.InputSimulator _inputSimulator;

    public InputSimulator()
    {
        _inputSimulator = new WindowsInput.InputSimulator();
    }

    public void TextEntry(string text)
    {
        _inputSimulator.Keyboard.TextEntry(text);
    }

    public void KeyPress(VirtualKeyCode button)
    {
        _inputSimulator.Keyboard.KeyPress(button);
    }

    public void ModifiedKeyStroke(VirtualKeyCode[] modifiers, VirtualKeyCode key)
    {
        _inputSimulator.Keyboard.ModifiedKeyStroke(modifiers, key);
    }
}

