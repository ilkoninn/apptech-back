// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AppTech.Business.DTOs.ExamDTOs;
using AppTech.Business.Services.ExternalServices.Interfaces;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json;

namespace AppTech.Business.Services.ExternalServices.Abstractions
{
    public class ExamCacheService : IExamCacheService
    {
        private readonly string _cacheDirectory;

        public ExamCacheService(IWebHostEnvironment env)
        {
            _cacheDirectory = "C:\\inetpub\\Apptech-Backend\\exams";

            if (!Directory.Exists(_cacheDirectory))
            {
                Directory.CreateDirectory(_cacheDirectory);
            }
        }

        public void StoreExam(string examToken, RandomExamDTO examData)
        {
            var filePath = GetFilePath(examToken);

            var jsonData = JsonConvert.SerializeObject(examData, Formatting.Indented);
            File.WriteAllText(filePath, jsonData);
        }

        public RandomExamDTO GetStoredExam(string examToken)
        {
            var filePath = GetFilePath(examToken);

            if (File.Exists(filePath))
            {
                var jsonData = File.ReadAllText(filePath);
                var examData = JsonConvert.DeserializeObject<RandomExamDTO>(jsonData);
                return examData;
            }

            return null;
        }

        public void RemoveExam(string examToken)
        {
            var filePath = GetFilePath(examToken);

            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
        }

        private string GetFilePath(string examToken)
        {
            return Path.Combine(_cacheDirectory, $"{examToken}.json");
        }

        public void StoreDropletMetaData(string examToken, DropletMetaDataDTO metaData)
        {
            var filePath = GetMetaDataFilePath(examToken);

            var jsonData = JsonConvert.SerializeObject(metaData, Formatting.Indented);
            File.WriteAllText(filePath, jsonData);
        }

        public DropletMetaDataDTO GetDropletMetaData(string examToken)
        {
            var filePath = GetMetaDataFilePath(examToken);

            if (File.Exists(filePath))
            {
                var jsonData = File.ReadAllText(filePath);
                var metaData = JsonConvert.DeserializeObject<DropletMetaDataDTO>(jsonData);
                return metaData;
            }

            return null;
        }

        public void RemoveDropletMetaData(string examToken)
        {
            var filePath = GetMetaDataFilePath(examToken);

            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
        }

        private string GetMetaDataFilePath(string examToken)
        {
            return Path.Combine(_cacheDirectory, $"{examToken}_metadata.json");
        }
    }
}
