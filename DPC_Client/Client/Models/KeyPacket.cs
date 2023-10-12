using System.Windows.Input;

namespace DPC_Client.Client.Models
{
    internal class KeyPacket
    {
        public Key key { get; set; }
        public int type { get; set; } // 1-down; 2-up
    }
}
