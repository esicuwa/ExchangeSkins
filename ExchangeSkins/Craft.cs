using SteamKit2.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExchangeSkins
{
    public class MsgGCCraft : IGCSerializableMessage
    {
        public short Blueprint { get; set; } = 0;
        public ushort ItemCount { get; set; }
        public List<ulong> ItemIds { get; set; } = [];

        public uint GetEMsg() => (uint)SteamKit2.GC.CSGO.Internal.EGCItemMsg.k_EMsgGCCraft;

        public void Serialize(Stream stream)
        {
            var logger = Logger.Instance;

            using var memoryStream = new MemoryStream();
            using var writer = new BinaryWriter(memoryStream, Encoding.UTF8, leaveOpen: true);

            writer.Write(Blueprint);
            writer.Write(ItemCount);
            foreach (var id in ItemIds)
                writer.Write(id);

            writer.Flush();
            byte[] bytes = memoryStream.ToArray();

            logger.Trace($"[MsgGCCraft] Serialize: Blueprint={Blueprint}, ItemCount={ItemCount}, PayloadSize={bytes.Length}");
            logger.Trace($"[MsgGCCraft] Items: [{string.Join(", ", ItemIds)}]");

            stream.Write(bytes, 0, bytes.Length);
        }

        public void Deserialize(Stream stream)
        {
            var logger = Logger.Instance;

            using var reader = new BinaryReader(stream, Encoding.UTF8, leaveOpen: true);
            Blueprint = reader.ReadInt16();
            ItemCount = reader.ReadUInt16();
            ItemIds.Clear();
            for (int i = 0; i < ItemCount; i++)
                ItemIds.Add(reader.ReadUInt64());

            logger.Trace($"[MsgGCCraft] Deserialize: Blueprint={Blueprint}, ItemCount={ItemCount}");
            logger.Trace($"[MsgGCCraft] Items: [{string.Join(", ", ItemIds)}]");
        }
    }
}
