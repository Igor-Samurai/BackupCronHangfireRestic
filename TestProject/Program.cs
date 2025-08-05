using Hangfire;
using Hangfire.SqlServer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Configuration;
using Serilog.Events;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net.NetworkInformation;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using TestProject.Restic;
using TestProject.Serilog;

namespace TestProject
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Log.Logger = new LoggerConfiguration()
               .MinimumLevel.Verbose()
               .WriteTo.RocketchatSink().CreateLogger(); // Добавляем наш приемник


            GetCron(); //Получаем от пользователя расписание

            Console.WriteLine("Укажите адрес сервера S3:");
            Setting.ServerS3 = $"s3:{Console.ReadLine()}";

            Console.WriteLine("Укажите пароль репозитория:");
            Setting.PasswordRep = Console.ReadLine();

            Console.WriteLine("Укажите AwsAccessKeyId");
            Setting.AwsAccessKeyId = Console.ReadLine();

            Console.WriteLine("Укажите AwsSecretAccessKey");
            Setting.AwsSecretAccessKey = Console.ReadLine();

            Console.WriteLine("Укажите путь к папке бэкапа");
            Setting.Folder = Console.ReadLine();

            GetFiles(); //Получаем от пользователя пути к файлам

            GetInitRepository();//Получаем от ответ по инициализации репозитория (да или нет)



            IHost host = CreateHostBuilder(args).Build();
            var lifetime = host.Services.GetRequiredService<IHostApplicationLifetime>();
            lifetime.ApplicationStarted.Register(() =>
            {
                Log.Information("Сервер запущен!");
                ResticServise RS = new ResticServise();
                if (Setting.InitRep)
                {
                    RS.InitNewRepository(); //Если нужно инициализировать репозиторий, то инициализируем его
                }
                using (var server = new BackgroundJobServer())
                {
                    RecurringJob.AddOrUpdate("backup_job",
                        () => RS.BackupData(),
                        Setting.Cron);
                }
            });
            lifetime.ApplicationStopped.Register(() =>
            {
                Log.Information("Сервер остановлен!");
            });
            host.Run();

        }

        public static void GetInitRepository()
        {
            bool success = false;
            while (!success)
            {
                success = true;
                Console.WriteLine("Нужно ли инициализировать новый репозиторий? \n" +
                 "1 - Да \n" +
                 "2 - Нет");
                string Answer = Console.ReadLine();
                switch (Answer)
                {
                    case "1":
                        Setting.InitRep = true;
                        break;
                    case "2":
                        Setting.InitRep = false;
                        break;
                    default:
                        Console.WriteLine("Укажите один из перечисленных вариантов");
                        success = false;
                        break;
                }
            }
        }

        public static void GetCron()
        {
            bool success = false;

            while (!success)
            {
                success = true;
                Console.WriteLine("Укажите цифрой периодичность формирования бэкапов: \n" +
                    "1 - Раз в минуту \n" +
                    "2 - Раз в Час \n" +
                    "3 - Раз в сутки \n" +
                    "4 - Раз в неделю \n");
                string Answer = Console.ReadLine();
                switch (Answer)
                {
                    case "1":
                        Setting.Cron = Cron.Minutely();
                        break;
                    case "2":
                        Setting.Cron = Cron.Hourly();
                        break;
                    case "3":
                        Setting.Cron = Cron.Daily();
                        break;
                    case "4":
                        Setting.Cron = Cron.Weekly();
                        break;
                    default:
                        Console.WriteLine("Укажите один из перечисленных вариантов");
                        success = false;
                        break;
                }
            }
        }

        public static void GetFiles()
        {
            bool success = false;
            while (!success)
            {
                success = true;
                Console.WriteLine("Хотите добавить файл к папке? \n" +
                 "1 - Да \n" +
                 "2 - Нет");

                string Answer = Console.ReadLine();
                switch (Answer)
                {
                    case "1":
                        Console.WriteLine("Укажите путь к файлу");
                        Setting.Files.Add(Console.ReadLine());
                        GetFiles();
                        break;
                    case "2":
                        break;
                    default:
                        Console.WriteLine("Укажите один из перечисленных вариантов");
                        success = false;
                        break;
                }
            }
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
        Host.CreateDefaultBuilder(args)
            .ConfigureWebHostDefaults(builder =>
            {
                builder.Configure(app =>
                {
                    app.UseRouting();

                    app.UseHangfireDashboard("/dashboard");
                    app.UseEndpoints(endpoints =>
                    {
                        endpoints.MapHangfireDashboard();
                    });
                });
            })
            .ConfigureServices((hostContext, services) =>
            {
                services.AddHangfire(conf => conf.UseSqlServerStorage("Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog=TaskBackupDB;Integrated Security=True;Connect Timeout=30;Encrypt=False;Trust Server Certificate=False;Application Intent=ReadWrite;Multi Subnet Failover=False"));
                services.AddHangfireServer();
                services.AddLogging(loggingBuilder =>
                {
                    loggingBuilder.ClearProviders(); // Удаляем провайдеры по умолчанию
                    loggingBuilder.AddConsole(); // Добавляем провайдер консоли
                    loggingBuilder.AddFilter("Microsoft", LogLevel.None); // Устанавливаем уровень логирования для Microsoft
                    loggingBuilder.AddFilter("System", LogLevel.None); // Устанавливаем уровень логирования для System
                    loggingBuilder.AddFilter("Hangfire", LogLevel.None);
                });
            });
    }

}