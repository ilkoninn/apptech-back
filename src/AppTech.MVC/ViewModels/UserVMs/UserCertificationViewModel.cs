// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using AppTech.Business.DTOs.CertificationDTOs;
using AppTech.Core.Entities;
using AppTech.Core.Entities.Identity;

namespace AppTech.MVC.ViewModels.UserVMs
{
    public class UserCertificationViewModel
    {
        public string UsernameOrEmail { get; set; }
        public int CertificationId { get; set; }
        public ICollection<Certification> Certifications { get; set; }
        public ICollection<User> Users { get; set; }
    }
}
