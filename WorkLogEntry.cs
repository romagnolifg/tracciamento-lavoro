using System;
using System.Collections.Generic;
using System.Text;

namespace Tracciamento_lavoro
{
    public class WorkLogEntry
    {
        public string Code { get; set; }
        public string Start { get; set; }
        public string End { get; set; }
        public string Elapsed { get; set; }
    }
}
