using System;

namespace Shmap.Common
{
    public interface IInteractionRequester
    {
        public EventHandler<string> UserNotification { get; init; }

        public EventHandler<string> UserInteraction { get; init; }
        // UserInputInteraction
    }
}