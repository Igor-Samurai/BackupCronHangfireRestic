using Hangfire;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace TestProject
{
    public static class Setting
    {
        public static string Cron { get; set; }
        public static string ServerS3 { get; set; }
        public static string PasswordRep { get; set; }
        public static string AwsAccessKeyId { get; set; }
        public static string AwsSecretAccessKey { get; set; }
        public static string Folder { get; set; }
        public static List<string> Files { get; set; } = [];
        public static bool InitRep { get; set; }
        public static string WebHook { get; set; }
    }
}
