using System;

namespace Shmap.CommonServices
{
    public interface IInteractionRequester
    {
        public EventHandler<string> UserNotification { get; init; }

        public EventHandler<string> UserInteraction { get; init; }
        // UserInputInteraction
    }
}