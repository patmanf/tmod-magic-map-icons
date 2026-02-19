using Microsoft.Xna.Framework;
using System.IO;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace SpawnIconTP;

public class MagicMap : Mod
{
    internal static MagicMap Instance => ModContent.GetInstance<MagicMap>();

    public enum MessageId : byte
    {
        RequestOceanTeleport,
        RequestOceanPos,
        ReceiveOceanLeft,
        ReceiveOceanRight
    }

    public override void HandlePacket(BinaryReader reader, int whoAmI)
    {
        var msgType = (MessageId)reader.ReadByte();
        switch (msgType)
        {
            case MessageId.RequestOceanTeleport:
                if (Main.netMode != NetmodeID.Server) break;

                bool right = reader.ReadBoolean();
                MagicMapPlayer.Get(whoAmI).TeleportOcean(right);
                break;

            case MessageId.RequestOceanPos:
                if (Main.netMode != NetmodeID.Server) break;

                var player = MagicMapPlayer.Get(whoAmI);
                if (player.GetOceanPos(out Vector2 leftPos, false))
                    SendOceanPos(whoAmI, leftPos, false);
                if (player.GetOceanPos(out Vector2 rightPos, true))
                    SendOceanPos(whoAmI, rightPos, true);

                break;

            case MessageId.ReceiveOceanLeft:
            case MessageId.ReceiveOceanRight:
                if (Main.netMode != NetmodeID.MultiplayerClient || whoAmI != 256) break;

                MagicMapLayer.SetOceanPos(reader.ReadVector2(), msgType == MessageId.ReceiveOceanRight);
                break;
        }
    }

    public static void RequestOceanTeleport(bool right)
    {
        if (Main.netMode != NetmodeID.MultiplayerClient) return;
        
        ModPacket packet = Instance.GetPacket();
        packet.Write((byte)MessageId.RequestOceanTeleport);
        packet.Write(right);
        packet.Send();
    }

    public static void RequestOceanPos()
    {
        if (Main.netMode != NetmodeID.MultiplayerClient) return;
        
        ModPacket packet = Instance.GetPacket();
        packet.Write((byte)MessageId.RequestOceanPos);
        packet.Send();
    }

    public static void SendOceanPos(int to, Vector2 pos, bool right)
    {
        if (Main.netMode != NetmodeID.Server) return;
        
        ModPacket packet = Instance.GetPacket();
        packet.Write((byte)(right ? MessageId.ReceiveOceanRight : MessageId.ReceiveOceanLeft));
        packet.WriteVector2(pos);
        packet.Send(to);
    }
}
