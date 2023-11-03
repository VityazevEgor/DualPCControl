using System.Text;
using System.Threading.Tasks;
using DPC_Client.Client.Models;
using TextCopy;

namespace DPC_Client.Client
{
    internal class ClipBoardAPI
    {
        private static string? lastText = null;
        public static async Task<string?> GetTextN()
        {
            var currentTetx = await ClipboardService.GetTextAsync();
            if (currentTetx != lastText)
            {
                lastText = currentTetx;
                return currentTetx;
            }
            else
            {
                return null;
            }
        }

        public static async Task ClearClipboard()
        {
            lastText = "";
            await ClipboardService.SetTextAsync("");
        }

        public static void processClipPacketN(ClipBoardPacket packet)
        {
            var packetText = Encoding.UTF8.GetString(packet.clipBoardData);
            lastText = packetText;
            ClipboardService.SetText(packetText);
        }
    }
}
