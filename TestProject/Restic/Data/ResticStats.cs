using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace TestProject.Restic.Data
{
    public class ResticStats
    {
        [JsonPropertyName("total_size")]
        public long TotalSize { get; set; }

        [JsonPropertyName("total_file_count")]
        public long TotalFileCount { get; set; }
    }
}
