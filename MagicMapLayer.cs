using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.GameInput;
using Terraria.ID;
using Terraria.Localization;
using Terraria.Map;
using Terraria.ModLoader;
using Terraria.ModLoader.Config;
using Terraria.UI;

namespace SpawnIconTP;

public class MagicMapLayer : ModMapLayer
{
    public override Position GetDefaultPosition() => new Before(IMapLayer.Spawn);

    internal static Icon HomeIcon = new(TextureAssets.SpawnBed, "UI.SpawnBed", ClickHome);
    internal static Icon SpawnIcon = new(TextureAssets.SpawnPoint, "UI.SpawnPoint", ClickSpawn);
    internal static Icon OceanIconL = new("SpawnIconTP/Textures/ocean", "Bestiary_Biomes.Ocean", () => ClickOcean(false));
    internal static Icon OceanIconR = new("SpawnIconTP/Textures/ocean", "Bestiary_Biomes.Ocean", () => ClickOcean(true));
    internal static Icon HellIcon = new("SpawnIconTP/Textures/hell", "Bestiary_Biomes.TheUnderworld", ClickHell);

    public static void SetOceanPos(Vector2 pos, bool right)
    {
        if (right) OceanIconR.SetPosition(pos);
        else OceanIconL.SetPosition(pos);
    }

    public override void Draw(ref MapOverlayDrawContext context, ref string text)
    {
        if (!MagicMapConfig.Instance.Enabled) return;

        bool hasShellphone = !MagicMapConfig.Instance.RequireMirror || HasItems(MagicMapConfig.Instance.ShellphoneItems);

        if (hasShellphone || HasItems(MagicMapConfig.Instance.MirrorItems))
        {
            IMapLayer.Spawn.Hide();

            SpawnIcon.Draw(ref context, ref text, new Vector2(Main.spawnTileX, Main.spawnTileY));
            if (Main.LocalPlayer.SpawnX != -1)
                HomeIcon.Draw(ref context, ref text, new Vector2(Main.LocalPlayer.SpawnX, Main.LocalPlayer.SpawnY));
        }

        if (!MagicMapConfig.Instance.ConchEnabled) return;

        if (hasShellphone || HasItems(MagicMapConfig.Instance.MagicConchItems))
        {
            OceanIconL.Draw(ref context, ref text);
            OceanIconR.Draw(ref context, ref text);
        }

        if (hasShellphone || HasItems(MagicMapConfig.Instance.DemonConchItems))
            HellIcon.Draw(ref context, ref text);
    }

    private static bool HasItems(List<ItemDefinition> list)
    {
        return MagicMapConfig.Instance.AllowVoidBag
            ? list.Exists(item => Main.LocalPlayer.HasItemInInventoryOrOpenVoidBag(item.Type))
            : list.Exists(item => Main.LocalPlayer.HasItem(item.Type));
    }

    private static void ClickSpawn()
    {
        if (Main.netMode == NetmodeID.SinglePlayer)
            Main.LocalPlayer.Shellphone_Spawn();
        else
            NetMessage.SendData(MessageID.RequestTeleportationByServer, -1, -1, null, 3);
    }

    private static void ClickHome()
    {
        Player player = Main.LocalPlayer;
        
        float speedX = player.velocity.X * 0.5f;
        float speedY = player.velocity.Y * 0.5f;
        for (int i = 0; i < 70; i++)
            Dust.NewDust(player.position, player.width, player.height, DustID.MagicMirror, speedX, speedY, 150, default, 1.5f);

        player.RemoveAllGrapplingHooks();
        player.Spawn(PlayerSpawnContext.RecallFromItem);

        for (int i = 0; i < 70; i++)
            Dust.NewDust(player.position, player.width, player.height, DustID.MagicMirror, 0f, 0f, 150, default, 1.5f);
    }

    private static void ClickOcean(bool right)
    {
        if (Main.netMode == NetmodeID.SinglePlayer)
            Main.LocalPlayer.GetModPlayer<MagicMapPlayer>().TeleportOcean(right);
        else
            MagicMap.RequestOceanTeleport(right);
    }

    private static void ClickHell()
    {
        if (Main.netMode == NetmodeID.SinglePlayer)
            Main.LocalPlayer.DemonConch();
        else
            NetMessage.SendData(MessageID.RequestTeleportationByServer, -1, -1, null, 2);
    }

    public class Icon(Asset<Texture2D> texture, LocalizedText hoverText, Action clickAction)
    {
        public Icon(string textureName, string hoverKey, Action clickAction)
            : this(ModContent.Request<Texture2D>(textureName), hoverKey, clickAction) { }
        
        public Icon(Asset<Texture2D> texture, string hoverKey, Action clickAction)
            : this(texture, Language.GetText(hoverKey), clickAction) { }

        private static readonly SpriteFrame Frame = new(1, 1, 0, 0);
        private Vector2 _position;

        public void SetPosition(Vector2 position) => _position = position;

        public void Draw(ref MapOverlayDrawContext context, ref string text)
            => Draw(ref context, ref text, _position);

        public void Draw(ref MapOverlayDrawContext context, ref string text, Vector2 position)
        {
            var result = context.Draw(texture.Value, position, Color.White, Frame, 1f, 2f, Alignment.Center);
            if (!result.IsMouseOver) return;

            Main.cancelWormHole = true;
            text = hoverText.Value;

            if (!Main.mouseLeft || !Main.mouseLeftRelease)
                return;

            Main.mouseLeftRelease = false;
            Main.mapFullscreen = false;
            PlayerInput.LockGamepadButtons("MouseLeft");
            SoundEngine.PlaySound(SoundID.MenuClose);
            SoundEngine.PlaySound(SoundID.Item6);

            clickAction();
        }
    }
}
