using System.Collections.Generic;
using System.ComponentModel;
using Terraria.ID;
using Terraria.ModLoader.Config;

namespace SpawnIconTP;

internal class MagicMapConfig : ModConfig
{
    public override ConfigScope Mode => ConfigScope.ClientSide;

    public static MagicMapConfig Instance;

    [DefaultValue(true)]
    [BackgroundColor(161, 219, 255)]
    public bool Enabled;

    [DefaultValue(true)]
    [BackgroundColor(104, 159, 255)]
    public bool ConchEnabled;

    [DefaultValue(true)]
    public bool RequireMirror;

    [DefaultValue(true)]
    [BackgroundColor(123, 95, 212)]
    public bool AllowVoidBag;


    [Header("ItemLists")]

    [BackgroundColor(115, 230, 238)]
    public List<ItemDefinition> MirrorItems;

    [BackgroundColor(211, 168, 127)]
    public List<ItemDefinition> MagicConchItems;

    [BackgroundColor(157, 70, 70)]
    public List<ItemDefinition> DemonConchItems;

    [BackgroundColor(90, 146, 90)]
    public List<ItemDefinition> ShellphoneItems;

    public MagicMapConfig()
    {
        MirrorItems =
        [
            new ItemDefinition(ItemID.MagicMirror),
            new ItemDefinition(ItemID.IceMirror),
            new ItemDefinition(ItemID.CellPhone)
        ];

        MagicConchItems =
        [
            new ItemDefinition(ItemID.MagicConch)
        ];

        DemonConchItems =
        [
            new ItemDefinition(ItemID.DemonConch)
        ];

        ShellphoneItems =
        [
            new ItemDefinition(ItemID.Shellphone),
            new ItemDefinition(ItemID.ShellphoneSpawn),
            new ItemDefinition(ItemID.ShellphoneOcean),
            new ItemDefinition(ItemID.ShellphoneHell)
        ];
    }
}
