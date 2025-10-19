// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading.Tasks;
using AppTech.Business.DTOs.ExamDTOs;
using AppTech.Business.Services.ExternalServices.Interfaces;
using AppTech.Core.Entities.Commons;
using AppTech.DAL.Repositories.Interfaces;
using iText.Forms.Form.Element;
using Microsoft.AspNetCore.Hosting;
using Renci.SshNet;

namespace AppTech.Business.Services.ExternalServices.Abstractions
{
    public class TerminalService : ITerminalService
    {
        public async Task HandleWebSocketConnection(WebSocket webSocket, WebSocketDropletDTO dto)
        {
            var dropletIp = dto.DropletPublicIp;
            var username = dto.Username;
            string password = dto.Password;
            string privateKeyPath = dto.PrivateKeyPath;


            while (webSocket.State == WebSocketState.Open)
            {
                using (var client = new SshClient(new ConnectionInfo(
                    dropletIp,
                    username,
                    new PrivateKeyAuthenticationMethod(username, new PrivateKeyFile(privateKeyPath))
                )))
                {
                    client.Connect();

                    var shellStream = client.CreateShellStream("xterm", 160, 48, 1024, 768, 4096);

                    shellStream.Write("clear\n");
                    shellStream.Flush();

                    var buffer = new byte[8 * 1024];
                    WebSocketReceiveResult result;

                    var sshOutputTask = Task.Run(async () =>
                    {
                        var sshBuffer = new byte[8 * 1024];
                        while (webSocket.State == WebSocketState.Open && client.IsConnected)
                        {
                            if (shellStream.DataAvailable)
                            {
                                var bytesRead = shellStream.Read(sshBuffer, 0, sshBuffer.Length);

                                if (bytesRead > 0)
                                {
                                    var sshOutput = sshBuffer[..bytesRead];
                                    await webSocket.SendAsync(new ArraySegment<byte>(sshOutput), WebSocketMessageType.Text, true, CancellationToken.None);
                                }
                            }

                            await Task.Delay(10);
                        }
                    });

                    var cmdBuilder = new StringBuilder();

                    while (webSocket.State == WebSocketState.Open && client.IsConnected)
                    {
                        result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

                        if (result.Count > 0)
                        {
                            var cmd = Encoding.UTF8.GetString(buffer, 0, result.Count);

                            cmdBuilder.Append(cmd);

                            if (cmdBuilder.ToString().Trim().ToLower() == "exit")
                            {
                                shellStream.Write("prohibited!\n");
                                shellStream.Flush();

                                cmdBuilder.Clear();
                                continue;
                            }

                            shellStream.Write(cmd);
                            shellStream.Flush();
                        }
                    }

                    await sshOutputTask;

                    client.Disconnect();
                }

                await Task.Delay(5000);
            }

            await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Connection closed", CancellationToken.None);
        }
    }
}

