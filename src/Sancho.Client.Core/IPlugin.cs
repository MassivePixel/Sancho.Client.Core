// Copyright (c) Massive Pixel.  All Rights Reserved.  Licensed under the MIT License (MIT). See License.txt in the project root for license information.

namespace Sancho.Client.Core
{
    /// <summary>
    /// Represents client side plugin. Each plugin has a name (used by server
    /// to correctly match server/client parts) and can receive arbitrary
    /// messages.
    /// </summary>
    public interface IPlugin
    {
        string Name { get; }

        void Recieve(Message message);
    }
}
