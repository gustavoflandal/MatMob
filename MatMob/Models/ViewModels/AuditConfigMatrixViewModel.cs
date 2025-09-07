using System.Collections.Generic;

namespace MatMob.Models.ViewModels
{
    public class AuditConfigMatrixViewModel
    {
        public List<string> Modules { get; set; } = new();
        public List<string> Processes { get; set; } = new();
        public HashSet<string> EnabledPairs { get; set; } = new(); // key: MODULE|PROCESS
    }
}
