namespace Mapp.Common
{
    public interface IDialogService
    {
        void ShowMessage(string message);
        string AskToChangeLongStringIfNeeded(string message, string textToChange, int maxLength);
    }
}