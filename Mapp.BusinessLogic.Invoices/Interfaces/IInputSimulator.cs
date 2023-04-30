using WindowsInput.Native;

namespace Shmap.Application.Interfaces;

public interface IInputSimulator
{
    void TextEntry(string text);
    void KeyPress(VirtualKeyCode button);
    void ModifiedKeyStroke(VirtualKeyCode[] modifiers, VirtualKeyCode key);
}