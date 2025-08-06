using Serilog.Core;
using Serilog.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http.Headers;

namespace TestProject.Serilog.Rocketchat
{
    
    public class RocketchatSink:ILogEventSink
    {
        
        public async void Emit(LogEvent logEvent)
        {
            string message = "";
            if (logEvent.Level == LogEventLevel.Information)
            {
                Console.WriteLine($"{logEvent.Timestamp.LocalDateTime} - {logEvent.MessageTemplate}");
                message = $"{logEvent.Timestamp.LocalDateTime} - {logEvent.MessageTemplate}";
            }
            else
            {
                Console.WriteLine($"{logEvent.Timestamp.LocalDateTime} - {logEvent.MessageTemplate} (ошибка: {logEvent.Exception})");
                message = $"{logEvent.Timestamp.LocalDateTime} - {logEvent.MessageTemplate} (ошибка: {logEvent.Exception})";
            }

            HttpClient client = new HttpClient();
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, Setting.WebHook);
            request.Content = new StringContent("{\"alias\":\"logger-bacckup\",\"text\":\"" +
                message +
                "\"}");
            request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            await client.SendAsync(request);

            //Подумать, что делать, если лог не отправился по какой либо причине
            //HttpResponseMessage response = await client.SendAsync(request);
            //response.EnsureSuccessStatusCode();
            //string responseBody = await response.Content.ReadAsStringAsync();
        }

    }
}
