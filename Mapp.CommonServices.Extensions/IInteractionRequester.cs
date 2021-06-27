using System;

namespace Shmap.CommonServices
{
    public interface IInteractionRequester
    {
        EventHandler<string> UserNotification { get; init; }

        EventHandler<string> UserInteraction { get; init; }
        // UserInputInteraction
    }
}