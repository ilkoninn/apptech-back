// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AppTech.Core.Entities.Identity;

namespace AppTech.Core.Entities.Commons
{
    public class Droplet : BaseEntity, IAuditedEntity
    {
        public int? ExamResultId { get; set; }
        public ExamResult? ExamResult { get; set; }
        public string CustomUsername { get; set; }
        public string VpcId { get; set; }
        public string Token { get; set; }
        public string Password { get; set; }
        public string? VolumeId { get; set; }
        public string MachineId { get; set; }
        public string ProjectId { get; set; }
        public string PrivateKeyPath { get; set; }
        public string PublicIpAddress { get; set; }

        // Base Fields
        public string? CreatedBy { get; set; }
        public DateTime? CreatedOn { get; set; }
        public string UpdatedBy { get; set; }
        public DateTime UpdatedOn { get; set; }
        public bool IsDeleted { get; set; }
    }
}
