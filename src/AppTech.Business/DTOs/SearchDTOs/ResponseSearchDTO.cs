// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AppTech.Business.DTOs.SearchDTOs
{
    public class ResponseSearchDTO
    {
        public string Url {  get; set; } 
        public string Type { get; set; }
        public string Title { get; set; }
        public string? SubTitle { get; set; }
        public string? ImageUrl { get; set; }
        public int? LastVersion { get; set; }
    }
}
