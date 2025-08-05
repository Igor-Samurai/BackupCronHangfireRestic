using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestProject.Restic.Data
{
    public class ResticConfiguration
    {
        public string Repository { get; set; } = "s3:http://storage.yandexcloud.net/bucket1996";
        public string Password { get; set; } = "";
        public string AwsAccessKeyId { get; set; } = "";
        public string AwsSecretAccessKey { get; set; } = "";
        public string ResticExecutablePath { get; set; } = "restic";
        public int TimeoutMinutes { get; set; } = 60;
    }
}
