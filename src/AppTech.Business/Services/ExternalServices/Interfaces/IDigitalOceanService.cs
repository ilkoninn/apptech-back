// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AppTech.Business.DTOs.ExamDTOs;
using AppTech.Business.Services.ExternalServices.Abstractions;
using AppTech.Core.Entities.Commons;

namespace AppTech.Business.Services.ExternalServices.Interfaces
{
    public interface IDigitalOceanService
    {
        Task<List<Droplet>> CreateDropletAndAttachVolumeAsync(CreateDropletDTO dto);
        Task<string> GetDropletPublicIpAsync(long dropletId);
        Task<bool> DeleteProjectAsync(string projectId);
        Task<string> DeleteDropletAsync(long dropletId);
        Task<bool> DeleteVolumeAsync(string volumeId);
        Task<bool> DeleteVpcAsync(string vpcId);
    }
}
