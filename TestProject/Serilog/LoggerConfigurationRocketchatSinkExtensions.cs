using Serilog;
using Serilog.Configuration;
using Serilog.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TestProject.Serilog.Rocketchat;

namespace TestProject.Serilog
{
    public static class LoggerConfigurationRocketchatSinkExtensions
    {
        public static LoggerConfiguration RocketchatSink(
           this LoggerSinkConfiguration sinkConfiguration,
           string output = "text" // Добавляем параметр для выбора формата вывода: text или json
           )
        {
            if (sinkConfiguration == null) throw new ArgumentNullException(nameof(sinkConfiguration));

            return sinkConfiguration.Sink(new RocketchatSink());
        }
    }
}
