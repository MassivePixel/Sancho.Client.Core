// Copyright (c) Massive Pixel.  All Rights Reserved.  Licensed under the MIT License (MIT). See License.txt in the project root for license information.

namespace Sancho.Client.Core
{
    public class MessageMetadata
    {
        public string pluginId { get; set; }
        public string origin { get; set; }
        public string senderId { get; set; }
    }

    /// <summary>
    /// Represents a message exchanged between server/client parts of a plugin.
    /// </summary>
    public class Message
    {
        public string command { get; set; }
        public object data { get; set; }

        public MessageMetadata metadata { get; set; }
    }
}
