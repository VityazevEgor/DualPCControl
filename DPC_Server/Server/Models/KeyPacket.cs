using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace DPC_Server.Server.Models
{
    internal class KeyPacket
    {
        public Key key { get; set; }
        public int type { get; set; } // 1-down; 2-up
    }
}
