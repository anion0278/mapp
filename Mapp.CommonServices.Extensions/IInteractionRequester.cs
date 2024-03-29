﻿using System;

namespace Mapp.Common
{
    public interface IInteractionRequester
    {
        public EventHandler<string> UserNotification { get; init; }

        public EventHandler<string> UserInteraction { get; init; }
        // UserInputInteraction
    }
}