// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AppTech.Core.Entities.Commons;
using AppTech.Core.Entities.Identity;

namespace AppTech.Core.Entities
{
    public class SubscriptionUser : BaseEntity, IAuditedEntity
    {
        public User User { get; set; }
        public string UserId { get; set; }
        public int SubscriptionId { get; set; }
        public Subscription Subscription { get; set; }
        public DateTime ExpiredOn { get; set; }

        // Base Fields
        public string? CreatedBy { get; set; }
        public DateTime? CreatedOn { get; set; }
        public string UpdatedBy { get; set; }
        public DateTime UpdatedOn { get; set; }
        public bool IsDeleted { get; set; }
    }
}
