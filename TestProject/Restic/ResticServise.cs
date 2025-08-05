using Hangfire;
using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TestProject.Restic.Data;

namespace TestProject.Restic
{
    
    public class ResticServise
    {
        private ResticConfiguration _resticConfiguration;
        private ResticWrapper _resticWrapper;
        public ResticServise()
        {
            _resticConfiguration = new ResticConfiguration() { Repository = Setting.ServerS3, 
                Password = Setting.PasswordRep, AwsAccessKeyId = Setting.AwsAccessKeyId, AwsSecretAccessKey = Setting.AwsSecretAccessKey
            };
            _resticWrapper = new ResticWrapper(_resticConfiguration);
        }

        public void InitNewRepository()
        {
            Log.Information("Начало инициализации репозитория");
            bool result = _resticWrapper.InitializeRepositoryAsync().Result;
            Log.Information("Репозиторий инициализирован");

        }

        public void BackupData()
        {
            if (_resticWrapper.IsRepositoryAccessibleAsync().Result)
            {
                Log.Information($"Начало выполнения бэкапа папки {Setting.Folder}");
                try
                {
                    foreach (var File in Setting.Files)
                    {
                        if (File.Trim() != "")
                        {
                            FileInfo fileInf = new FileInfo(File);
                            if (fileInf.Exists)
                            {
                                fileInf.CopyTo($"{Setting.Folder}\\{fileInf.Name}", true);
                            }
                        }
                    }

                    ResticBackupResult RBR = _resticWrapper.BackupAsync(Setting.Folder).Result;

                    Log.Information($"Бэкап папки {Setting.Folder} успешно выполнен! ID снапшота: {RBR.SnapshotId}");
                }
                catch (Exception e)
                {
                    Log.Error(e, "Не удалось выполнить бэкап!");
                }
            }   
        }
    }
}
