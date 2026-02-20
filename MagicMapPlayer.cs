using Microsoft.Xna.Framework;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace SpawnIconTP;

public class MagicMapPlayer : ModPlayer
{
    internal static MagicMapPlayer Get(int whoAmI) => Main.player[whoAmI].GetModPlayer<MagicMapPlayer>();
    internal static MagicMapPlayer Get() => Get(Main.myPlayer);

    public override void OnEnterWorld()
    {
        Vector2 oceanPosL = new(0, (int)Main.worldSurface - 100);
        Vector2 oceanPosR = new(Main.maxTilesX, (int)Main.worldSurface - 100);
        Vector2 hellPos = new(Main.maxTilesX / 2f, Main.UnderworldLayer + 20);

        if (Main.netMode == NetmodeID.SinglePlayer)
        {
            if (GetOceanPos(out Vector2 posL, false))
                oceanPosL = posL;
            if (GetOceanPos(out Vector2 posR, true))
                oceanPosR = posR;
        }
        else MagicMap.RequestOceanPos();

        MagicMapLayer.SetOceanPos(oceanPosL, false);
        MagicMapLayer.SetOceanPos(oceanPosR, true);
        MagicMapLayer.HellIcon.SetPosition(hellPos);
    }

    internal void TeleportHome()
    {
        float speedX = Player.velocity.X * 0.5f;
        float speedY = Player.velocity.Y * 0.5f;
        for (int i = 0; i < 70; i++)
            Dust.NewDust(Player.position, Player.width, Player.height, DustID.MagicMirror, speedX, speedY, 150, default, 1.5f);

        Player.RemoveAllGrapplingHooks();
        Player.Spawn(PlayerSpawnContext.RecallFromItem);

        for (int i = 0; i < 70; i++)
            Dust.NewDust(Player.position, Player.width, Player.height, DustID.MagicMirror, 0f, 0f, 150, default, 1.5f);
    }

    internal void TeleportOcean(bool right)
    {
        Vector2 newPos = Player.position;
        if (GetOceanPos(out Vector2 tilePos, right))
        {
            if (Main.netMode == NetmodeID.Server)
                MagicMap.SendOceanPos(Player.whoAmI, tilePos, right);
            else
                MagicMapLayer.SetOceanPos(tilePos, right);

            newPos = tilePos.ToWorldCoordinates(8f, 16f) - new Vector2(Player.width / 2f, Player.height);
        }

        Player.Teleport(newPos, TeleportationStyleID.MagicConch);
        Player.velocity = Vector2.Zero;

        if (Main.netMode == NetmodeID.Server)
        {
            RemoteClient.CheckSection(Player.whoAmI, newPos);
            NetMessage.SendData(MessageID.TeleportEntity, -1, -1, null, 0, Player.whoAmI, newPos.X, newPos.Y, TeleportationStyleID.MagicConch);
        }
    }

    internal bool GetOceanPos(out Vector2 landingPos, bool right)
    {
        int crawlOffsetX = right.ToDirectionInt();
        int startX = right ? (Main.maxTilesX - 50) : 50;
        bool result = TeleportHelpers.RequestMagicConchTeleportPosition(Player, -crawlOffsetX, startX, out Point landingPoint);
        landingPos = landingPoint.ToVector2();
        return result;
    }
}
