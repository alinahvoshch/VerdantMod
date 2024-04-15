﻿using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Verdant.Items.Verdant.Misc;

[Sacrifice(1)]
class CorruptEffigy : ModItem
{
    public override void SetStaticDefaults() => ItemID.Sets.ItemNoGravity[Type] = true;

    public override void SetDefaults()
    {
        Item.rare = ItemRarityID.Quest;
        Item.value = 0;
        Item.consumable = false;
        Item.width = 40;
        Item.height = 42;
    }
}

[Sacrifice(1)]
class CrimsonEffigy : ModItem
{
    public override void SetStaticDefaults() => ItemID.Sets.ItemNoGravity[Type] = true;

    public override void SetDefaults()
    {
        Item.rare = ItemRarityID.Quest;
        Item.value = 0;
        Item.consumable = false;
        Item.width = 40;
        Item.height = 38;
    }
}