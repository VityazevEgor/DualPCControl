using System.Windows.Input;
namespace DPC_Server.Server.Models
{
    internal class MouseButtonPacket
    {
        public required MouseButton mButton { get; set; }
        public required int state { get; set; } // 1-down, 2-up
    }
}
