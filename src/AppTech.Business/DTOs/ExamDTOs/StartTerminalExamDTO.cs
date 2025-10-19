// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AppTech.Business.DTOs.ExamDTOs
{
    public class StartTerminalExamDTO
    {
        public string userId { get; set; }
        public int  examId { get; set; }
    }

    public class ResponseTerminalDTO
    {
        public ICollection<string> WebSocketUrls { get; set; }
    }

    public class CreateDropletDTO
    {
        public string Region { get; set; }
        public string Size { get; set; }
        public string Image { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string CertificationCode { get; set; }
    }

    public class WebSocketDropletDTO
    {
        public string Username { get; set; }
        public string Password { get; set; }
        public string PrivateKeyPath { get; set; }
        public string DropletPublicIp { get; set; }
    }
}
