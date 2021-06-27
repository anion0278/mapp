using System;

namespace Mapp.CommonServices
{
    public interface IInteractionRequester
    {
        EventHandler<string> UserNotification { get; init; }

        EventHandler<string> UserInteraction { get; init; }
        // UserInputInteraction
    }
}