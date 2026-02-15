using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.Localization;
using Terraria.Map;
using Terraria.ModLoader;

namespace SpawnIconTP;

internal class ConchMapLayer : ModMapLayer
{
    private static readonly Asset<Texture2D> MagicConchTex = ModContent.Request<Texture2D>("SpawnIconTP/Textures/ocean");
    private static readonly Asset<Texture2D> DemonConchTex = ModContent.Request<Texture2D>("SpawnIconTP/Textures/hell");

    internal static Vector2 oceanPosL;
    internal static Vector2 oceanPosR;
    internal static Vector2 underworldPos;

    private static readonly Player.RandomTeleportationAttemptSettings hellSettings = new()
    {
        mostlySolidFloor = true,
        avoidAnyLiquid = true,
        avoidLava = true,
        avoidHurtTiles = true,
        avoidWalls = true,
        attemptsBeforeGivingUp = 1000,
        maximumFallDistanceFromOrignalPoint = 30
    };

    public override void Draw(ref MapOverlayDrawContext context, ref string text)
    {
        if (!Config.Instance.Enabled || !Config.Instance.ConchEnabled) return;

        bool hasShellphone = SpawnIconTP.HasItems(Config.Instance.ShellphoneItems);

        if (hasShellphone || SpawnIconTP.HasItems(Config.Instance.MagicConchItems))
        {
            SpawnIconTP.DrawIcon(ref context, MagicConchTex.Value, oceanPosL, "Bestiary_Biomes.Ocean", ref text, () => TeleportOcean(false));
            SpawnIconTP.DrawIcon(ref context, MagicConchTex.Value, oceanPosR, "Bestiary_Biomes.Ocean", ref text, () => TeleportOcean(true));
        }

        if (hasShellphone || SpawnIconTP.HasItems(Config.Instance.DemonConchItems))
        {
            SpawnIconTP.DrawIcon(ref context, DemonConchTex.Value, underworldPos, "Bestiary_Biomes.TheUnderworld", ref text, TeleportHell);
        }
    }

    private static void TeleportHell()
    {
        SoundEngine.PlaySound(SoundID.Item6);
        Player player = Main.LocalPlayer;

        bool canSpawn = false;
        Vector2 newPos = player.CheckForGoodTeleportationSpot(ref canSpawn, (Main.maxTilesX / 2) - 50, 100, Main.UnderworldLayer + 20, 80, hellSettings);

        if (!canSpawn)
        {
            Main.NewText(Language.GetTextValue("Mods.SpawnIconTP.TpFailed"), new Color(255, 120, 0));
            return;
        }

        SoundEngine.PlaySound(SoundID.Item6);

        player.Teleport(newPos, 7);
        player.velocity = Vector2.Zero;
        if (Main.netMode == NetmodeID.Server)
        {
            RemoteClient.CheckSection(player.whoAmI, player.position);
            NetMessage.SendData(MessageID.TeleportEntity, -1, -1, null, 0, player.whoAmI, newPos.X, newPos.Y, 7);
        }
    }

    private static void TeleportOcean(bool right = false)
    {
        Player player = Main.LocalPlayer;

        if (!GetOceanPos(out Point landingPoint, right))
        {
            Main.NewText(Language.GetTextValue("Mods.SpawnIconTP.TpFailed"), new Color(255, 120, 0));
            return;
        }

        SoundEngine.PlaySound(SoundID.Item6);

        if (right) oceanPosR = landingPoint.ToVector2();
        else oceanPosL = landingPoint.ToVector2();

        Vector2 newPos = landingPoint.ToWorldCoordinates(8f, 16f) - new Vector2(player.width / 2, player.height); ;
        player.Teleport(newPos, 5);
        player.velocity = Vector2.Zero;
        if (Main.netMode == NetmodeID.Server)
        {
            RemoteClient.CheckSection(player.whoAmI, player.position);
            NetMessage.SendData(MessageID.TeleportEntity, -1, -1, null, 0, player.whoAmI, newPos.X, newPos.Y, 5);
        }
    }

    internal static bool GetOceanPos(out Point landingPoint, bool right = false)
    {
        int crawlOffsetX = right.ToDirectionInt();
        int startX = right ? (Main.maxTilesX - 50) : 50;
        return TeleportHelpers.RequestMagicConchTeleportPosition(Main.LocalPlayer, -crawlOffsetX, startX, out landingPoint);
    }

    private class ConchMapPlayer : ModPlayer
    {
        public override void OnEnterWorld()
        {
            oceanPosL = GetOceanPos(out Point leftPoint, false)
                ? leftPoint.ToVector2()
                : new Vector2(0, (int)Main.worldSurface - 100);

            oceanPosR = GetOceanPos(out Point rightPoint, true)
                ? rightPoint.ToVector2()
                : new Vector2(Main.maxTilesX, (int)Main.worldSurface - 100);

            underworldPos = new(Main.maxTilesX / 2, Main.UnderworldLayer + 30);
        }
    }
}