// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading.Tasks;
using AppTech.Business.DTOs.ExamDTOs;

namespace AppTech.Business.Services.ExternalServices.Interfaces
{
    public interface ITerminalService
    {
        Task HandleWebSocketConnection(WebSocket webSocket, WebSocketDropletDTO dto);
    }
}
