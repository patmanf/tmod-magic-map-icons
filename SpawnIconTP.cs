using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.GameInput;
using Terraria.ID;
using Terraria.Localization;
using Terraria.Map;
using Terraria.ModLoader;
using Terraria.UI;

namespace SpawnIconTP;

public class SpawnIconTP : Mod
{
    public override void Load()
    {
        On_SpawnMapLayer.Draw += On_SpawnMapLayer_Draw;
    }

    private void On_SpawnMapLayer_Draw(On_SpawnMapLayer.orig_Draw orig, SpawnMapLayer self, ref MapOverlayDrawContext context, ref string text)
    {
        if (!Config.Instance.Enabled)
        {
            orig(self, ref context, ref text);
            return;
        }

        Vector2 spawnPos = new(Main.spawnTileX, Main.spawnTileY);
        Vector2 bedPos = new(Main.LocalPlayer.SpawnX, Main.LocalPlayer.SpawnY);

        DrawIcon(context, TextureAssets.SpawnPoint.Value, spawnPos, "UI.SpawnPoint", ref text, TeleportSpawn);

        if (Main.LocalPlayer.SpawnX != -1)
        {
            DrawIcon(context, TextureAssets.SpawnBed.Value, bedPos, "UI.SpawnBed", ref text, TeleportBed);
        }
    }

    private static void DrawIcon(MapOverlayDrawContext context, Texture2D tex, Vector2 pos, string hoverTextKey, ref string text, Action teleport)
    {
        var result = context.Draw(tex, pos, Color.White, new(1, 1, 0, 0), 1f, 2f, Alignment.Center);
        if (!result.IsMouseOver)
            return;

        Main.cancelWormHole = true;
        text = Language.GetTextValue(hoverTextKey);

        if (!Main.mouseLeft || !Main.mouseLeftRelease)
            return;

        Main.mouseLeftRelease = false;
        Main.mapFullscreen = false;
        PlayerInput.LockGamepadButtons("MouseLeft");
        SoundEngine.PlaySound(SoundID.MenuClose);

        if (!HasMirror())
        {
            Main.NewText(Language.GetTextValue("Mods.SpawnIconTP.NoMirror"), new Color(255, 240, 20));
            return;
        }

        SoundEngine.PlaySound(SoundID.Item6);

        teleport?.Invoke();
    }

    private static void TeleportSpawn()
    {
        if (Main.netMode == NetmodeID.SinglePlayer)
            Main.LocalPlayer.Shellphone_Spawn();
        else if (Main.netMode == NetmodeID.MultiplayerClient)
            NetMessage.SendData(MessageID.RequestTeleportationByServer, -1, -1, null, 3);
    }

    private static void TeleportBed()
    {
        Player player = Main.LocalPlayer;
        float speedX = player.velocity.X * 0.5f;
        float speedY = player.velocity.Y * 0.5f;

        for (int i = 0; i < 70; i++)
        {
            Dust.NewDust(player.position, player.width, player.height, DustID.MagicMirror, speedX, speedY, 150, default, 1.5f);
        }

        player.RemoveAllGrapplingHooks();
        player.Spawn(PlayerSpawnContext.RecallFromItem);

        for (int i = 0; i < 70; i++)
        {
            Dust.NewDust(player.position, player.width, player.height, DustID.MagicMirror, 0f, 0f, 150, default, 1.5f);
        }
    }

    private static bool HasMirror()
    {
        if (!Config.Instance.RequireMirror) return true;

        Func<int, bool> hasItem = Config.Instance.AllowVoidBag
            ? Main.LocalPlayer.HasItemInInventoryOrOpenVoidBag
            : Main.LocalPlayer.HasItem;

        return Config.Instance.MirrorItems.Exists(item => hasItem(item.Type));
    }
}