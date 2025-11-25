using System;

namespace DamasChinas_Client.UI.Utilities
{
    public sealed class ClientValidationException : Exception
    {
        public string ResourceKey { get; }

        public ClientValidationException(string resourceKey)
            : base(resourceKey)
        {
            ResourceKey = resourceKey;
        }
    }
}
