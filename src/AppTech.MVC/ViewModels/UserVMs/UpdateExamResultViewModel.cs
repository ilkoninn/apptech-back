// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using AppTech.Business.DTOs.ExamDTOs;
using AppTech.Core.Entities;
using AppTech.Core.Entities.Commons;
using AppTech.Core.Entities.Identity;
using Microsoft.AspNetCore.Identity;

namespace AppTech.MVC.ViewModels.UserVMs
{
    public class UpdateExamResultViewModel
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public int ExamId { get; set; }
        public string ProjectId { get; set; }
        public string VpcId { get; set; }
        public double UserScore { get; set; }
        public bool IsPassed { get; set; }
        public IQueryable<ExamDTO> Exams { get; set; }
        public ICollection<User> Users { get; set; }
        public ICollection<Droplet> Droplets { get; set; }
    }
}
