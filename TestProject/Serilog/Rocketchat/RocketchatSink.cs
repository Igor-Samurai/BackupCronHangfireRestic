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
        private string _urlChanel = "Какой то адрес канала";
        public async void Emit(LogEvent logEvent)
        {
            if (logEvent.Level == LogEventLevel.Information)
            {
                Console.WriteLine($"{logEvent.Timestamp.LocalDateTime} - {logEvent.MessageTemplate}");
            }
            else
            {
                Console.WriteLine($"{logEvent.Timestamp.LocalDateTime} - {logEvent.MessageTemplate} (ошибка: {logEvent.Exception})");
            }
            
            
            //string message = "Какое-то сообщение";
            
            //string requestText = "{\"text\" :\"" + message + "\"}";

           
            //HttpClient client = new HttpClient();
            //HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, _urlChanel);
            //request.Content = new StringContent(requestText);
            //request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

            //    //Можно закомментировать эти три строчки, чтобы не отправлялись уведомления
            //HttpResponseMessage response = await client.SendAsync(request);
            //response.EnsureSuccessStatusCode();
            //string responseBody = await response.Content.ReadAsStringAsync();

            }

    }
}
