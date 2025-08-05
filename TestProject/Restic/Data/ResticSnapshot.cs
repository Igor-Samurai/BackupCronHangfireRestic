using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace TestProject.Restic.Data
{
    // Модели данных для JSON ответов restic
    public class ResticSnapshot
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonPropertyName("time")]
        public DateTime Time { get; set; }

        [JsonPropertyName("hostname")]
        public string Hostname { get; set; }

        [JsonPropertyName("paths")]
        public List<string> Paths { get; set; }

        [JsonPropertyName("tags")]
        public List<string> Tags { get; set; }
    }
}
