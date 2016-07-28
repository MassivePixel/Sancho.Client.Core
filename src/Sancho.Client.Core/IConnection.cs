// Copyright (c) Massive Pixel.  All Rights Reserved.  Licensed under the MIT License (MIT). See License.txt in the project root for license information.

using System.Threading.Tasks;

namespace Sancho.Client.Core
{
    public interface IConnection
    {
        Task SendAsync(string pluginId, string command, object data = null, string prevMessageId = null);
    }
}
