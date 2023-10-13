using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DPC_Server.Server.Models
{
    internal class MouseMovePacket
    {
        public double x { get; set; }
        public double y { get; set; }

        public double formHeight { get; set; }
        public double formWidth { get; set; }
    }
}
