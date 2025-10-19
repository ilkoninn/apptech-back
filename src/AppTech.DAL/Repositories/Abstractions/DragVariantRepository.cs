// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AppTech.Core.Entities;
using AppTech.DAL.Persistence;
using AppTech.DAL.Repositories.Interfaces;

namespace AppTech.DAL.Repositories.Abstractions
{
    public class DragVariantRepository : Repository<DragVariant>, IDragVariantRepository
    {
        public DragVariantRepository(AppDbContext context) : base(context) { }
    }
}
