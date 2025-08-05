using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using TestProject.Restic.Data;

namespace TestProject.Restic
{
    public class ResticWrapper
    {
        private readonly ResticConfiguration _config;
        private readonly Dictionary<string, string> _environmentVariables;

        public ResticWrapper(ResticConfiguration config)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));

            _environmentVariables = new Dictionary<string, string>
            {
                ["RESTIC_REPOSITORY"] = _config.Repository,
                ["RESTIC_PASSWORD"] = _config.Password
            };

            if (!string.IsNullOrEmpty(_config.AwsAccessKeyId))
                _environmentVariables["AWS_ACCESS_KEY_ID"] = _config.AwsAccessKeyId;

            if (!string.IsNullOrEmpty(_config.AwsSecretAccessKey))
                _environmentVariables["AWS_SECRET_ACCESS_KEY"] = _config.AwsSecretAccessKey;
        }

        /// <summary>
        /// Инициализирует новый репозиторий restic
        /// </summary>
        public async Task<bool> InitializeRepositoryAsync()
        {
            var result = await ExecuteResticCommandAsync("init");
            return result.Success;
        }

        /// <summary>
        /// Создает резервную копию указанного пути
        /// </summary>
        public async Task<ResticBackupResult> BackupAsync(
            string path,
            List<string> tags = null,
            string hostname = null,
            string excludeFile = null,
            CancellationToken cancellationToken = default)
        {
            var startTime = DateTime.UtcNow;
            var args = new List<string> { "backup", path };

            if (tags != null && tags.Count > 0)
            {
                foreach (var tag in tags)
                {
                    args.AddRange(new[] { "--tag", tag });
                }
            }

            if (!string.IsNullOrEmpty(hostname))
            {
                args.AddRange(new[] { "--host", hostname });
            }

            if (!string.IsNullOrEmpty(excludeFile) && File.Exists(excludeFile))
            {
                args.AddRange(new[] { "--exclude-file", excludeFile });
            }

            args.Add("--json");

            var result = await ExecuteResticCommandAsync(string.Join(" ", args), cancellationToken);

            var backupResult = new ResticBackupResult
            {
                Success = result.Success,
                Output = result.Output,
                Error = result.Error,
                Duration = DateTime.UtcNow - startTime
            };

            // Попытка извлечь ID снимка из JSON вывода
            if (result.Success && !string.IsNullOrEmpty(result.Output))
            {
                try
                {
                    var lines = result.Output.Split('\n');
                    foreach (var line in lines)
                    {
                        if (line.Trim().StartsWith("{") && line.Contains("snapshot_id"))
                        {
                            var jsonDoc = JsonDocument.Parse(line);
                            if (jsonDoc.RootElement.TryGetProperty("snapshot_id", out var snapshotId))
                            {
                                backupResult.SnapshotId = snapshotId.GetString();
                                break;
                            }
                        }
                    }
                }
                catch (JsonException)
                {
                    // Игнорируем ошибки парсинга JSON
                }
            }

            return backupResult;
        }

        /// <summary>
        /// Получает список снимков
        /// </summary>
        public async Task<List<ResticSnapshot>> GetSnapshotsAsync(CancellationToken cancellationToken = default)
        {
            var result = await ExecuteResticCommandAsync("snapshots --json", cancellationToken);

            if (!result.Success)
                throw new InvalidOperationException($"Ошибка получения снимков: {result.Error}");

            try
            {
                var snapshots = JsonSerializer.Deserialize<List<ResticSnapshot>>(result.Output);
                return snapshots ?? new List<ResticSnapshot>();
            }
            catch (JsonException ex)
            {
                throw new InvalidOperationException($"Ошибка парсинга JSON: {ex.Message}");
            }
        }

        /// <summary>
        /// Получает статистику репозитория
        /// </summary>
        public async Task<ResticStats> GetStatsAsync(string snapshotId = "latest", CancellationToken cancellationToken = default)
        {
            var result = await ExecuteResticCommandAsync($"stats --json --mode restore-size {snapshotId}", cancellationToken);

            if (!result.Success)
                throw new InvalidOperationException($"Ошибка получения статистики: {result.Error}");

            try
            {
                return JsonSerializer.Deserialize<ResticStats>(result.Output);
            }
            catch (JsonException ex)
            {
                throw new InvalidOperationException($"Ошибка парсинга JSON: {ex.Message}");
            }
        }

        /// <summary>
        /// Проверяет целостность репозитория
        /// </summary>
        public async Task<bool> CheckIntegrityAsync(double dataSubsetPercent = 5.0, CancellationToken cancellationToken = default)
        {
            var result = await ExecuteResticCommandAsync($"check --read-data-subset={dataSubsetPercent:F1}%", cancellationToken);
            return result.Success;
        }

        /// <summary>
        /// Удаляет старые снимки по политике хранения
        /// </summary>
        public async Task<bool> ForgetSnapshotsAsync(
            int keepDaily = 7,
            int keepWeekly = 4,
            int keepMonthly = 6,
            bool prune = true,
            CancellationToken cancellationToken = default)
        {
            var args = $"forget --keep-daily {keepDaily} --keep-weekly {keepWeekly} --keep-monthly {keepMonthly}";
            if (prune)
                args += " --prune";

            var result = await ExecuteResticCommandAsync(args, cancellationToken);
            return result.Success;
        }

        /// <summary>
        /// Восстанавливает файлы из снимка
        /// </summary>
        public async Task<bool> RestoreAsync(
            string snapshotId,
            string targetPath,
            List<string> includePaths = null,
            CancellationToken cancellationToken = default)
        {
            var args = $"restore {snapshotId} --target {targetPath}";

            if (includePaths != null && includePaths.Count > 0)
            {
                foreach (var path in includePaths)
                {
                    args += $" --include {path}";
                }
            }

            var result = await ExecuteResticCommandAsync(args, cancellationToken);
            return result.Success;
        }

        /// <summary>
        /// Проверяет доступность репозитория
        /// </summary>
        public async Task<bool> IsRepositoryAccessibleAsync(CancellationToken cancellationToken = default)
        {
            var result = await ExecuteResticCommandAsync("snapshots --last 1", cancellationToken);
            return result.Success;
        }

        private async Task<(bool Success, string Output, string Error)> ExecuteResticCommandAsync(
            string arguments,
            CancellationToken cancellationToken = default)
        {
            using var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = _config.ResticExecutablePath,
                    Arguments = arguments,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true
                }
            };

            // Устанавливаем переменные окружения
            foreach (var env in _environmentVariables)
            {
                process.StartInfo.EnvironmentVariables[env.Key] = env.Value;
            }

            var outputBuilder = new System.Text.StringBuilder();
            var errorBuilder = new System.Text.StringBuilder();

            process.OutputDataReceived += (sender, e) =>
            {
                if (e.Data != null)
                    outputBuilder.AppendLine(e.Data);
            };

            process.ErrorDataReceived += (sender, e) =>
            {
                if (e.Data != null)
                    errorBuilder.AppendLine(e.Data);
            };

            process.Start();
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();

            try
            {
                await process.WaitForExitAsync(cancellationToken);

                // Дополнительная проверка таймаута
                if (!process.HasExited)
                {
                    var timeout = TimeSpan.FromMinutes(_config.TimeoutMinutes);
                    if (!process.WaitForExit((int)timeout.TotalMilliseconds))
                    {
                        process.Kill();
                        return (false, "", "Операция прервана по таймауту");
                    }
                }
            }
            catch (OperationCanceledException)
            {
                if (!process.HasExited)
                    process.Kill();
                throw;
            }

            return (process.ExitCode == 0, outputBuilder.ToString(), errorBuilder.ToString());
        }
    }
}
