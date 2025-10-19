// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AppTech.Core.Entities.Commons;
using AppTech.Core.Entities.Identity;

namespace AppTech.Core.Entities
{
    public class Subscription : BaseEntity, IAuditedEntity
    {
        public decimal Price { get; set; }
        public string ImageUrl { get; set; }
        public int CertificationId { get; set; }
        public string? SpecificDomain { get; set; }
        public Certification Certification { get; set; }
        public ICollection<Transaction> Transactions { get; set; }
        public ICollection<SubscriptionUser> SubscriptionUsers { get; set; }

        // Base Fields
        public string? CreatedBy { get; set; }
        public DateTime? CreatedOn { get; set; }
        public string UpdatedBy { get; set; }
        public DateTime UpdatedOn { get; set; }
        public bool IsDeleted { get; set; }
    }
}
