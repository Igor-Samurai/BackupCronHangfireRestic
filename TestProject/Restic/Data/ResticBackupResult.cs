using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestProject.Restic.Data
{
    public class ResticBackupResult
    {
        public bool Success { get; set; }
        public string SnapshotId { get; set; }
        public string Output { get; set; }
        public string Error { get; set; }
        public TimeSpan Duration { get; set; }
    }
}
