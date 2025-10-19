// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using AppTech.Business.DTOs.ExamDTOs;
using AppTech.Business.Services.ExternalServices.Interfaces;
using AppTech.Core.Entities.Commons;
using AppTech.DAL.Repositories.Interfaces;
using Microsoft.IdentityModel.Tokens;
using MySqlX.XDevAPI.Common;
using Newtonsoft.Json;
using RestSharp;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace AppTech.Business.Services.ExternalServices.Abstractions
{
    public class DigitalOceanService : IDigitalOceanService
    {
        private readonly string _apiToken;
        private readonly RestClient _client;

        public DigitalOceanService()
        {
            _apiToken = "hello";
            _client = new RestClient("https://api.digitalocean.com/v2/");
        }

        private RestRequest CreateRequest(string resource, RestSharp.Method method)
        {
            var request = new RestRequest(resource, method);
            request.AddHeader("Authorization", $"Bearer {_apiToken}");
            request.AddHeader("Content-Type", "application/json");
            return request;
        }

        public async Task<string> GetDropletPublicIpAsync(long dropletId)
        {
            var request = CreateRequest($"droplets/{dropletId}", RestSharp.Method.Get);
            string publicIpAddress = null;

            for (int retryCount = 0; retryCount < 10; retryCount++)
            {
                var response = await _client.ExecuteAsync(request);

                if (response.IsSuccessful)
                {
                    dynamic result = JsonConvert.DeserializeObject(response.Content);

                    var networks = result.droplet.networks.v4;
                    if (networks != null && networks.Count > 0)
                    {
                        publicIpAddress = networks[0].ip_address;
                        break;
                    }
                }

                await Task.Delay(3000);
            }

            return publicIpAddress;
        }

        public async Task<List<Droplet>> CreateDropletAndAttachVolumeAsync(CreateDropletDTO dto)
        {
            string region = dto.Region;
            string size = dto.Size;
            string image = dto.Image;
            string username = dto.Username;
            string password = dto.Password;

            var dropletCount = await GetTotalDropletCountAsync();

            if (dto.CertificationCode.Contains("200"))
            {
                var droplets = new List<Droplet>();

                if (dropletCount + 2 > 25)
                {
                    return new List<Droplet>();
                }

                string vpcId = await CreateVpcAsync(username, region, dto.CertificationCode);

                if (string.IsNullOrEmpty(vpcId))
                {
                    return new List<Droplet>();
                }

                string projectName = $"{username}-{new Random().Next(1, 99999)}-{dto.CertificationCode}";
                string projectId = await CreateProjectAsync(projectName, "Web Application", "Development");

                if (string.IsNullOrEmpty(projectId))
                {
                    return new List<Droplet>();
                }

                await Task.Delay(5000);

                string volumeTitle = $"{username.ToLower()}-volume-{new Random().Next(1, 99999)}";
                string volumeId = await CreateVolumeAsync(volumeTitle, region);

                if (string.IsNullOrEmpty(volumeId))
                {
                    return new List<Droplet>();
                }

                await Task.Delay(5000);

                string dropletNameS1 = $"{username}-{new Random().Next(1, 99999)}";
                string userDataEx200S1 = CreateEx200Script1(username, password);
                var dropletIdS1 = await CreateDropletWithGeneratedKeyAsync(dropletNameS1, region,
                    size, image, username, userDataEx200S1, vpcId);

                if (string.IsNullOrEmpty(dropletIdS1.Item1))
                {
                    return new List<Droplet>();
                }

                var tokenS1 = Guid.NewGuid().ToString();

                droplets.Add(new Droplet
                {
                    MachineId = dropletIdS1.Item1,
                    PrivateKeyPath = dropletIdS1.Item2,
                    ProjectId = projectId,
                    VpcId = vpcId,
                    PublicIpAddress = await GetDropletPublicIpAsync(Convert.ToInt64(dropletIdS1)),
                    Password = password,
                    Token = tokenS1,
                    CustomUsername = username,
                });

                string dropletNameS2 = $"{username}-{new Random().Next(1, 99999)}";
                string userDataEx200S2 = CreateEx200Script1(username, password);
                var dropletIdS2 = await CreateDropletWithGeneratedKeyAsync(dropletNameS2, region,
                    size, image, username, userDataEx200S2, vpcId);

                if (string.IsNullOrEmpty(dropletIdS2.Item1))
                {
                    return new List<Droplet>();
                }

                var tokenS2 = Guid.NewGuid().ToString();

                droplets.Add(new Droplet
                {
                    MachineId = dropletIdS2.Item1,
                    PrivateKeyPath = dropletIdS2.Item2,
                    ProjectId = projectId,
                    VpcId = vpcId,
                    PublicIpAddress = await GetDropletPublicIpAsync(Convert.ToInt64(dropletIdS2)),
                    Password = password,
                    Token = tokenS2,
                    CustomUsername = username,
                    VolumeId = volumeId,
                });

                await Task.Delay(7000);

                bool volumeAttached = await AttachVolumeToDropletAsync(dropletIdS2.Item1, volumeId, region);

                if (!volumeAttached)
                {
                    return new List<Droplet>();
                }

                await Task.Delay(5000);

                bool updateProjectResources = await AddDropletsToProjectAsync(projectId, new List<string> { dropletIdS1.Item1,
                    dropletIdS2.Item1 });

                if (!updateProjectResources)
                {
                    return new List<Droplet>();
                }

                return droplets;
            }
            //else if (dto.CertificationCode.Contains("294"))
            //{
            //    if (dropletCount + 6 > 25)
            //    {
            //        return new List<Droplet>();
            //    }

            //    string vpcId = await CreateVpcAsync(username, region, dto.CertificationCode);

            //    if (string.IsNullOrEmpty(vpcId))
            //    {
            //        return new List<Droplet>();
            //    }

            //    string vpcIpRange = await GetVpcIpRangeAsync(vpcId);

            //    if (string.IsNullOrEmpty(vpcIpRange))
            //    {
            //        return new List<Droplet>();
            //    }

            //    var vpcLimitedAddress = string.Join(".", vpcIpRange.Split('.').Take(3));

            //    string projectName = $"{username}-{new Random().Next(1, 99999)}-{dto.CertificationCode}";
            //    string projectId = await CreateProjectAsync(projectName, "Web Application", "Development");

            //    if (string.IsNullOrEmpty(projectId))
            //    {
            //        return new List<Droplet>();
            //    }

            //    var servers = new Dictionary<string, string>
            //    {
            //        { "Controller.apptech.edu.az", $"{vpcLimitedAddress}.10" },
            //        { "Node1.apptech.edu.az", $"{vpcLimitedAddress}.11" },
            //        { "Node2.apptech.edu.az", $"{vpcLimitedAddress}.12" },
            //        { "Node3.apptech.edu.az", $"{vpcLimitedAddress}.13" },
            //        { "Node4.apptech.edu.az", $"{vpcLimitedAddress}.14" },
            //        { "Node5.apptech.edu.az", $"{vpcLimitedAddress}.15" }
            //    };

            //    var droplets = new List<Droplet>();
            //    var dropletIds = new List<string>();

            //    foreach (var server in servers)
            //    {
            //        string dropletName = server.Key;
            //        string ip = server.Value;

            //        string userDataEx294 = CreateEx294Script(username, password, servers);

            //        var dropletId = await CreateDropletAsync(dropletName, region, size, image, userDataEx294, vpcId);

            //        if (string.IsNullOrEmpty(dropletId))
            //        {
            //            return new List<Droplet>();
            //        }

            //        dropletIds.Add(dropletId);

            //        var token = Guid.NewGuid().ToString();

            //        droplets.Add(new Droplet
            //        {
            //            MachineId = dropletId,
            //            ProjectId = projectId,
            //            VpcId = vpcId,
            //            PublicIpAddress = await GetDropletPublicIpAsync(Convert.ToInt64(dropletId)),
            //            Password = password,
            //            Token = token,
            //            CustomUsername = username,
            //        });
            //    }

            //    await Task.Delay(25000);

            //    bool updateProjectResources = await AddDropletsToProjectAsync(projectId, dropletIds);

            //    if (!updateProjectResources)
            //    {
            //        return new List<Droplet>();
            //    }

            //    return droplets;
            //}

            return new List<Droplet>();
        }

        public async Task<(string, string)> CreateDropletWithGeneratedKeyAsync(string dropletName, string region, string size,
            string image, string username, string userData,
            string vpcId)
        {
            var keyPair = GenerateSshKeyPair(username);

            var sshKeyId = await AddSshKeyToDigitalOceanAsync($"{username}_key", keyPair.PublicKey);

            SavePublicKeyToWindowsPath(username, keyPair.PublicKey);
            var privateKeyPath = SavePrivateKeyToWindowsPath(username, keyPair.PrivateKey);

            var dropletId = await CreateDropletWithSshKeyAsync(dropletName, region, size, image, sshKeyId, userData, vpcId);

            return (dropletId, privateKeyPath);
        }

        private async Task<string> CreateDropletWithSshKeyAsync(string dropletName, string region, string size,
            string image, string userData, string vpcId, string sshKeyId)
        {
            var request = CreateRequest("droplets", RestSharp.Method.Post);
            var body = new
            {
                name = dropletName,
                region = region,
                size = size,
                image = image,
                vpc_uuid = vpcId,
                ssh_keys = new[] { sshKeyId },
                backups = false,
                ipv6 = false,
                user_data = userData,
                monitoring = true
            };
            request.AddJsonBody(body);

            var response = await _client.ExecuteAsync(request);

            if (response.IsSuccessful)
            {
                dynamic result = JsonConvert.DeserializeObject(response.Content);
                return result.droplet.id.ToString();
            }

            return null;
        }

        private (string PrivateKey, string PublicKey) GenerateSshKeyPair(string username)
        {
            using (var rsa = System.Security.Cryptography.RSA.Create(4096)) // 4096-bit RSA key
            {
                var privateKey = ExportPrivateKey(rsa);

                var publicKey = ExportPublicKey(rsa, username);

                return (privateKey, publicKey);
            }
        }

        private string ExportPrivateKey(System.Security.Cryptography.RSA rsa)
        {
            var privateKeyBytes = rsa.ExportPkcs8PrivateKey();
            var base64Key = Convert.ToBase64String(privateKeyBytes);

            // Format as PEM
            var pem = "-----BEGIN PRIVATE KEY-----\n" +
                      string.Join("\n", Enumerable.Range(0, base64Key.Length / 64)
                                                  .Select(i => base64Key.Substring(i * 64, Math.Min(64, base64Key.Length - i * 64)))) +
                      "\n-----END PRIVATE KEY-----";
            return pem;
        }

        private string ExportPublicKey(System.Security.Cryptography.RSA rsa, string username)
        {
            var publicKeyParameters = rsa.ExportParameters(false);
            using (var memoryStream = new MemoryStream())
            using (var writer = new BinaryWriter(memoryStream))
            {
                void WriteString(string value) => writer.Write(BitConverter.GetBytes(IPAddress.HostToNetworkOrder(value.Length)).Reverse().ToArray().Concat(Encoding.ASCII.GetBytes(value)).ToArray());
                void WriteBytes(byte[] value) => writer.Write(BitConverter.GetBytes(IPAddress.HostToNetworkOrder(value.Length)).Reverse().ToArray().Concat(value).ToArray());

                WriteString("ssh-rsa");

                WriteBytes(publicKeyParameters.Exponent);

                WriteBytes(publicKeyParameters.Modulus);

                var base64Key = Convert.ToBase64String(memoryStream.ToArray());
                return $"ssh-rsa {base64Key} {username}@apptech";
            }
        }

        private string SavePrivateKeyToWindowsPath(string username, string privateKey)
        {
            var sshDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".ssh");
            Directory.CreateDirectory(sshDirectory);

            var privateKeyPath = Path.Combine(sshDirectory, $"{username}_id_rsa");
            File.WriteAllText(privateKeyPath, privateKey);

            // Secure the private key file (read-only for the owner)
            var fileInfo = new FileInfo(privateKeyPath);
            fileInfo.Attributes = FileAttributes.Hidden | FileAttributes.ReadOnly;

            return privateKeyPath;
        }

        private string SavePublicKeyToWindowsPath(string username, string publicKey)
        {
            var sshDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".ssh");
            Directory.CreateDirectory(sshDirectory);

            var publicKeyPath = Path.Combine(sshDirectory, $"{username}_id_rsa.pub");
            File.WriteAllText(publicKeyPath, publicKey);

            return publicKeyPath;
        }

        public async Task<string> AddSshKeyToDigitalOceanAsync(string keyName, string publicKey)
        {
            var request = CreateRequest("account/keys", RestSharp.Method.Post);

            var body = new
            {
                name = keyName,
                public_key = publicKey
            };
            request.AddJsonBody(body);

            var response = await _client.ExecuteAsync(request);

            if (!response.IsSuccessful)
            {
                throw new Exception($"Failed to add SSH key: {response.StatusCode} - {response.Content}");
            }

            var result = JsonConvert.DeserializeObject<dynamic>(response.Content);
            return result.ssh_key.id;
        }
 
        private async Task<string> GetVpcIpRangeAsync(string vpcId)
        {
            var request = CreateRequest($"vpcs/{vpcId}", RestSharp.Method.Get);
            var response = await _client.ExecuteAsync(request);

            if (response.IsSuccessful)
            {
                dynamic result = JsonConvert.DeserializeObject(response.Content);
                return result.vpc.ip_range.ToString(); 
            }

            return null;
        }

        private async Task<bool> AddDropletsToProjectAsync(string projectId, List<string> dropletIds)
        {
            var request = CreateRequest($"projects/{projectId}/resources", RestSharp.Method.Post);

            var resources = dropletIds.Select(id => $"do:droplet:{id}").ToList();

            var body = new
            {
                resources = resources
            };
            request.AddJsonBody(body);

            var response = await _client.ExecuteAsync(request);

            if (!response.IsSuccessful)
            {
                Console.WriteLine($"Failed to add droplets to project. Status Code: {response.StatusCode}, Response: {response.Content}");
                return false;
            }

            return true;
        }

        private async Task<List<string>> GetExistingVpcRangesAsync()
        {
            var request = CreateRequest("vpcs", RestSharp.Method.Get);
            var response = await _client.ExecuteAsync(request);

            if (response.IsSuccessful)
            {
                dynamic result = JsonConvert.DeserializeObject(response.Content);
                var vpcRanges = new List<string>();

                foreach (var vpc in result.vpcs)
                {
                    vpcRanges.Add(vpc.ip_range.ToString());
                }

                return vpcRanges;
            }

            return new List<string>();
        }

        public async Task<string> CreateVpcAsync(string username, string region, string examCode)
        {
            var existingRanges = await GetExistingVpcRangesAsync();

            string uniqueIpRange = GenerateUniqueIpRange(existingRanges);

            var request = CreateRequest("vpcs", RestSharp.Method.Post);

            var body = new
            {
                name = $"{username.ToLower()}-vpc-{new Random().Next(1, 99999)}-{examCode}",
                region = region,
                ip_range = uniqueIpRange
            };
            request.AddJsonBody(body);

            var response = await _client.ExecuteAsync(request);

            if (response.IsSuccessful)
            {
                dynamic result = JsonConvert.DeserializeObject(response.Content);
                return result.vpc.id.ToString();
            }

            return null;
        }

        private string GenerateUniqueIpRange(List<string> existingRanges)
        {
            string baseIpRange = "172.25.250.0/24";
            var baseIp = new System.Net.IPAddress(new byte[] { 172, 25, 250, 0 });

            for (int i = 0; i < 255; i++)
            {
                string candidateRange = $"{baseIp}/24";
                if (!existingRanges.Contains(candidateRange))
                {
                    return candidateRange;
                }

                var nextIp = baseIp.GetAddressBytes();

                if (nextIp[2] == 255)
                {
                    nextIp[1] = (byte)(nextIp[1] + 1);
                    nextIp[2] = 0;
                }
                else
                {
                    nextIp[2] = (byte)(nextIp[2] + 1);
                }

                baseIp = new System.Net.IPAddress(nextIp);
            }

            throw new Exception("No unique IP range available.");
        }

        private async Task<string> CreateProjectAsync(string projectName, string purpose, string environment)
        {
            var request = CreateRequest("projects", RestSharp.Method.Post);
            var body = new
            {
                name = projectName,
                purpose = purpose,
                environment = environment,
                is_default = false
            };
            request.AddJsonBody(body);

            var response = await _client.ExecuteAsync(request);

            if (response.IsSuccessful)
            {
                dynamic result = JsonConvert.DeserializeObject(response.Content);
                return result.project.id.ToString();
            }

            return null;
        }

        private async Task<string> CreateVolumeAsync(string volumeName, string region)
        {
            var request = CreateRequest("volumes", RestSharp.Method.Post);

            var body = new
            {
                name = volumeName,
                region = region,
                size_gigabytes = 20,
                description = $"Exam volume {new Random().Next(1, 99999)}"
            };
            request.AddJsonBody(body);

            var response = await _client.ExecuteAsync(request);

            if (response.IsSuccessful)
            {
                dynamic result = JsonConvert.DeserializeObject(response.Content);
                return result.volume.id.ToString();
            }
            else
            {
                return response?.ErrorMessage;
            }
        }

        private async Task<bool> AttachVolumeToDropletAsync(string dropletId, string volumeId, string region)
        {
            var request = CreateRequest($"volumes/{volumeId}/actions", RestSharp.Method.Post);
            var body = new
            {
                type = "attach",
                droplet_id = dropletId,
                region = region
            };
            request.AddJsonBody(body);

            var response = await _client.ExecuteAsync(request);
            return response.IsSuccessful;
        }

        private string CreateEx294Script(string username, string userPassword, Dictionary<string, string> servers)
        {
            string hostsContent = string.Join("\n", servers.Select(s => $"{s.Value} {s.Key} {s.Key.Split('.')[0]}"));

            return $@"
            #cloud-config
            chpasswd:
              list: |
                root:{userPassword}
                {username}:{userPassword}
              expire: False

            ssh_pwauth: True

            users:
              - name: {username}
                gecos: {username} User
                sudo: ALL=(ALL) NOPASSWD:ALL
                shell: /bin/bash
                groups: wheel
                ssh_import_id: None
                lock_passwd: False
                passwd: {HashPassword(userPassword)}
            ";
        }

        private string CreateEx200Script1(string username, string password)
        {
            string hashedPassword = HashPassword(password);

            return $@"
            #cloud-config
            users:
              - name: {username}
                gecos: '{username} User'
                sudo: ALL=(ALL) NOPASSWD:ALL
                shell: /bin/bash
                groups: wheel
                lock_passwd: False
            chpasswd:
              list: |
                root:{password}
                {username}:{password}
              expire: False
            ssh_pwauth: False

            runcmd:
              - yum install -y httpd wget curl vim vi unzip podman chrony
              - systemctl start httpd
              - systemctl enable httpd
              - rm -rf /var/www/html/*
              - echo '<html><head><title>Welcome Apptech</title></head><body>Welcome to Apptech!</body></html>' > /var/www/html/index.html
              - touch /usr/share/dict/words
              - grep -o '\\b[[:alpha:]]*ich[[:alpha:]]*\\b' /usr/share/dict/words | head -n 200 > /usr/share/dict/ich_words.txt
            ";
        }

        private string CreateEx200Script2(string username, string password)
        {
            string hashedPassword = HashPassword(password);

            return $@"
            #cloud-config
            users:
              - name: {username}
                gecos: '{username} User'
                sudo: ALL=(ALL) NOPASSWD:ALL
                shell: /bin/bash
                groups: wheel
                lock_passwd: False
            chpasswd:
              list: |
                root:{password}
                {username}:{password}
              expire: False
            ssh_pwauth: False

            runcmd:
              # General setup
              - yum install -y wget curl vim vi unzip autofs tuned lvm2
            ";
        }

        private string HashPassword(string password)
        {
            using (var sha512 = new System.Security.Cryptography.SHA512Managed())
            {
                var bytes = Encoding.UTF8.GetBytes(password);
                var hash = sha512.ComputeHash(bytes);
                return Convert.ToBase64String(hash);
            }
        }

        private async Task<int> GetTotalDropletCountAsync()
        {
            var request = CreateRequest("droplets", RestSharp.Method.Get);

            var response = await _client.ExecuteAsync(request);
            if (response.IsSuccessful)
            {
                dynamic result = JsonConvert.DeserializeObject(response.Content);
                return result.meta.total;
            }

            return 0;
        }

        public async Task<bool> DeleteProjectAsync(string projectId)
        {
            var request = CreateRequest($"projects/{projectId}", RestSharp.Method.Delete);

            var response = await _client.ExecuteAsync(request);

            if (!response.IsSuccessful)
            {
                return false;
            }

            return true;
        }

        public async Task<bool> DeleteVpcAsync(string vpcId)
        {
            var request = CreateRequest($"vpcs/{vpcId}", RestSharp.Method.Delete);
            var response = await _client.ExecuteAsync(request);

            if (!response.IsSuccessful)
            {
                return false;
            }

            return true;
        }

        public async Task<bool> DeleteVolumeAsync(string volumeId)
        {
            var request = CreateRequest($"volumes/{volumeId}", RestSharp.Method.Delete);

            request.AddQueryParameter("destroy_volumes", "true");

            var response = await _client.ExecuteAsync(request);

            if (!response.IsSuccessful)
            {
                return false; 
            }

            return true; 
        }

        public async Task<string> DeleteDropletAsync(long dropletId)
        {
            var request = CreateRequest($"droplets/{dropletId}", RestSharp.Method.Delete);
            var response = await _client.ExecuteAsync(request);

            return response.Content;
        }

    }
}

