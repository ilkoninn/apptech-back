// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using AppTech.Business.DTOs.ExamDTOs;
using AppTech.Core.Entities.Identity;

namespace AppTech.MVC.ViewModels.UserVMs
{

    public class ExamResultViewModel
    {
        public string UserId { get; set; }
        public int ExamId { get; set; }
        public List<User> Users { get; set; }
        public IQueryable<ExamDTO> Exams { get; set; }

        public List<string> SelectedUserIds { get; set; }
    }
}
