// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AppTech.Business.DTOs.Commons;
using Microsoft.AspNetCore.Http;

namespace AppTech.Business.DTOs.SubscriptionDTOs
{
    public class CreateSubscriptionDTO : IAuditedEntityDTO
    {
        public decimal Price { get; set; }
        public IFormFile Image { get; set; }
        public int CertificationId { get; set; }
        public string? SpecificDomain { get; set; }
    }
}
