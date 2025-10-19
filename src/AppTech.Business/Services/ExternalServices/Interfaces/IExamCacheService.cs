// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AppTech.Business.DTOs.ExamDTOs;

namespace AppTech.Business.Services.ExternalServices.Interfaces
{
    public interface IExamCacheService
    {
        void RemoveExam(string examToken);
        void RemoveDropletMetaData(string examToken);
        RandomExamDTO GetStoredExam(string examToken);
        DropletMetaDataDTO GetDropletMetaData(string examToken);
        void StoreExam(string examToken, RandomExamDTO examData);
        void StoreDropletMetaData(string examToken, DropletMetaDataDTO metaData);
    }
}
