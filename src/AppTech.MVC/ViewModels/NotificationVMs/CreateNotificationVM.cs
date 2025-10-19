// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using AppTech.Core.Entities.Identity;

namespace AppTech.MVC.ViewModels.NotificationVMs
{
    public class CreateNotificationVM
    {
        public string Title { get; set; }
        public string? Description { get; set; }
        public List<User> Users { get; set; }
        public List<string>? SelectedUserIds { get; set; }
    }
}
