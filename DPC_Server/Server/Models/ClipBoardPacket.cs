using System.Linq;
namespace DPC_Server.Server.Models
{
    internal class ClipBoardPacket
    {
        public required uint type {  get; set; } // 1 - text, 2-img
        public required byte[] clipBoardData { get; set; }


        public override bool Equals(object? obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;

            var other = (ClipBoardPacket)obj;

            return type == other.type && Enumerable.SequenceEqual(clipBoardData, other.clipBoardData);
        }
    }
}
