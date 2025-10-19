// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AppTech.Business.DTOs.Commons;

namespace AppTech.Business.DTOs.NotificationDTOs
{
    public class NotificationResponseDTO : BaseEntityDTO
    {
        public int Count { get; set; }
        public string Title { get; set; }
        public string ImageUrl { get; set; }
        public DateTime SendedOn { get; set; }
        public string Description { get; set; }
        public string UserOrFullName { get; set; }
    }
}
