using WindowsInput.Native;

namespace Mapp.Application.Interfaces;

public interface IInputSimulator
{
    void TextEntry(string text);
    void KeyPress(VirtualKeyCode button);
    void ModifiedKeyStroke(VirtualKeyCode[] modifiers, VirtualKeyCode key);
}