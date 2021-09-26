namespace Shmap.CommonServices
{
    public interface IDialogService
    {
        void ShowMessage(string message);
        string AskToChangeLongStringIfNeeded(string message, string str, int maxLength);
    }
}