// Copyright (c) Massive Pixel.  All Rights Reserved.  Licensed under the MIT License (MIT). See License.txt in the project root for license information.

namespace Sancho.Client.Core
{
    public interface IPlugin
    {
        string Name { get; }

        void Recieve(Message message);
    }
}
