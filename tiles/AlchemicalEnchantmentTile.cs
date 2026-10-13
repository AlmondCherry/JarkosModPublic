using jarkos.items.usetiles;
using jarkos.core.Data;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ObjectData;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Terraria.GameInput;
using Terraria.ModLoader.IO;
using System.IO;
using jarkos.items.materials;
using Terraria.Enums;
using Jarkos.UI;
using Jarkos.items.usetiles;
using Jarkos.items.materials;
using Terraria.GameContent.ObjectInteractions;
using Jarkos.tiles;
using jarkos.items.potions;
using System;
using jarkos.core.Data;
using System.Linq;
namespace Jarkos.tiles
{
	public class AlchemicalEnchantmentTile : ModTile
	{
    public const int TileWidth = 4;
    public const int TileHeight = 2;
    public override void SetStaticDefaults()
    {
        Main.tileFrameImportant[Type] = true;
        Main.tileNoAttach[Type] = true;
        Main.tileLavaDeath[Type] = true;
        TileID.Sets.HasOutlines[Type] = true;
        TileObjectData.newTile.CopyFrom(TileObjectData.Style2x2);
        TileObjectData.newTile.Width = TileWidth;
        TileObjectData.newTile.Height = TileHeight;
        TileObjectData.newTile.CoordinateHeights = new[] { 16, 16 };
        TileObjectData.newTile.CoordinateWidth = 16;
        TileObjectData.newTile.CoordinatePadding = 2;
        TileObjectData.newTile.Origin = new Point16(1, 1);
        TileObjectData.newTile.AnchorBottom = new AnchorData(AnchorType.SolidTile | AnchorType.SolidWithTop, TileWidth, 0);
        TileObjectData.newTile.HookPostPlaceMyPlayer = new PlacementHook(ModContent.GetInstance<AlchemicalEnchantmentTE>().Hook_AfterPlacement, -1, 0, false);
        TileObjectData.addTile(Type);
        AddMapEntry(new Color(100, 50, 150), CreateMapEntryName());
        DustType = DustID.PurpleTorch;
        AdjTiles = new int[] { TileID.AlchemyTable };
    }
    public override bool HasSmartInteract(int i, int j, SmartInteractScanSettings settings) => true;
    public override void KillMultiTile(int i, int j, int frameX, int frameY)
    {
        Point16 origin = GetTileOrigin(i, j);
        ModContent.GetInstance<AlchemicalEnchantmentTE>().Kill(origin.X, origin.Y);
    }
    public override bool RightClick(int i, int j)
    {
        Point16 origin = GetTileOrigin(i, j);
        if (TileEntity.ByPosition.TryGetValue(origin, out TileEntity te))
        {
            if (te is AlchemicalEnchantmentTE crafterTE)
            {
                AlchemicalEnchantmentUISystem.Instance.ToggleUI(crafterTE);
            }
        }
        return true;
    }
    public override void MouseOver(int i, int j)
    {
        Player player = Main.LocalPlayer;
        player.noThrow = 2;
        player.cursorItemIconEnabled = true;
        player.cursorItemIconID = ModContent.ItemType<AlchemicalEnchantmentTile_item>();
    }
    public static Point16 GetTileOrigin(int i, int j)
    {
        Tile tile = Main.tile[i, j];
        if (tile == null || !tile.HasTile)
            return new Point16(i, j);
        int frameX = tile.TileFrameX;
        int frameY = tile.TileFrameY;
        int frameSizeX = 18;
        int frameSizeY = 18;
        int partX = frameX / frameSizeX;
        int partY = frameY / frameSizeY;
        return new Point16(i - partX, j - partY);
    }
}
	#region TileEntity
	public class AlchemicalEnchantmentTE : ModTileEntity
{
    public Item[] InputSlots = new Item[2];
    public Item OutputSlot = new Item();
    public int SelectedRecipeIndex = 0;
    public int CraftingProgress = 0;
    public bool IsCrafting = false;
    public const int MaxProgress = 120;
    public Dictionary<EssenceType, int> StoredEssences = new Dictionary<EssenceType, int>();
    public bool PullEssenceMode = true;
    private int _essenceTimer = 0;
    public static List<PotionRecipe> Recipes = new List<PotionRecipe>();
    private static bool recipesInitialized = false;
    public int GetTotalEssence()
    {
        int total = 0;
        foreach (var v in StoredEssences.Values) total += v;
        return total;
    }
    public List<(EssenceType type, int count)> GetRequiredEssences(int recipeIndex)
    {
        var list = new List<(EssenceType type, int count)>();
        switch (recipeIndex)
        {
            case 0:
                list.Add((EssenceType.Aer, 5));
                list.Add((EssenceType.Regalis, 2));
                break;
            case 1:
                list.Add((EssenceType.Aer, 5));
                list.Add((EssenceType.Regalis, 4));
                break;
            case 2:
                list.Add((EssenceType.Pyros, 2));
                list.Add((EssenceType.Aqua, 3));
                break;
            case 3:
                list.Add((EssenceType.Pyros, 2));
                list.Add((EssenceType.Aqua, 6));
                break;
            case 4:
                list.Add((EssenceType.Regalis, 4));
                list.Add((EssenceType.Aqua, 6));
                break;
            case 5:
                list.Add((EssenceType.Terra, 5));
                list.Add((EssenceType.Aer, 5));
                break;
            case 6:
                list.Add((EssenceType.Vita, 6));
                list.Add((EssenceType.Aqua, 4));
                break;
            case 7:
                list.Add((EssenceType.Lux, 4));
                list.Add((EssenceType.Aer, 4));
                break;
            case 8:
                list.Add((EssenceType.Venenum, 2));
                list.Add((EssenceType.Lux, 3));
                break;
            case 9:
                list.Add((EssenceType.Arena, 3));
                list.Add((EssenceType.Aer, 3));
                break;
            case 10:
                list.Add((EssenceType.Pelagus, 6));
                list.Add((EssenceType.Imperium, 3));
                break;
        }
        return list;
    }
    public bool HasRequiredEssences(int recipeIndex)
    {
        var reqs = GetRequiredEssences(recipeIndex);
        foreach (var req in reqs)
        {
            if (!StoredEssences.TryGetValue(req.type, out int amount) || amount < req.count)
            {
                return false;
            }
        }
        return true;
    }
    public AlchemicalEnchantmentTE()
    {
        for (int i = 0; i < InputSlots.Length; i++)
        {
            InputSlots[i] = new Item();
        }
        OutputSlot = new Item();
    }
    public override void OnNetPlace()
    {
        NetMessage.SendData(MessageID.TileEntitySharing, -1, -1, null, ID);
    }
    public override bool IsTileValidForEntity(int x, int y)
    {
        Tile tile = Main.tile[x, y];
        if (!tile.HasTile || tile.TileType != ModContent.TileType<AlchemicalEnchantmentTile>())
            return false;
        return tile.TileFrameX == 0 && tile.TileFrameY == 0;
    }
    public override int Hook_AfterPlacement(int i, int j, int type, int style, int direction, int alternate)
    {
        int originX = 1;
        int originY = 1;
        int topLeftX = i - originX;
        int topLeftY = j - originY;
        if (Main.netMode == NetmodeID.MultiplayerClient)
        {
            NetMessage.SendTileSquare(Main.myPlayer, topLeftX, topLeftY, AlchemicalEnchantmentTile.TileWidth, AlchemicalEnchantmentTile.TileHeight);
            NetMessage.SendData(MessageID.TileEntityPlacement, -1, -1, null, topLeftX, topLeftY, Type);
            return -1;
        }
        return Place(topLeftX, topLeftY);
    }
    public static void InitializeRecipes()
    {
        if (recipesInitialized) return;
        recipesInitialized = true;
        Recipes.Clear();
        Recipes.Add(new PotionRecipe(ItemID.BottledWater, 1, ItemID.Daybloom, 2, ModContent.ItemType<Potion_of_e>(), 1, jarkos.systems.LangSystem.RecipeName1, jarkos.systems.LangSystem.RecipeDesc1));
        Recipes.Add(new PotionRecipe(ItemID.BottledWater, 1, ItemID.IronOre, 3, ModContent.ItemType<Strength_Essence>(), 1, jarkos.systems.LangSystem.RecipeName2, jarkos.systems.LangSystem.RecipeDesc2));
        Recipes.Add(new PotionRecipe(ItemID.BottledWater, 1, ItemID.Cactus, 5, ModContent.ItemType<MagicalSoul_Elixir>(), 1, jarkos.systems.LangSystem.RecipeName3, jarkos.systems.LangSystem.RecipeDesc3));
        Recipes.Add(new PotionRecipe(ItemID.BottledWater, 1, ItemID.Blinkroot, 2, ModContent.ItemType<MagicalSoul_Extract>(), 1, jarkos.systems.LangSystem.RecipeName4, jarkos.systems.LangSystem.RecipeDesc4));
        Recipes.Add(new PotionRecipe(ItemID.BottledWater, 1, ItemID.GlowingMushroom, 3, ModContent.ItemType<Potion_of_s>(), 1, jarkos.systems.LangSystem.RecipeName5, jarkos.systems.LangSystem.RecipeDesc5));
        Recipes.Add(new PotionRecipe(ItemID.BottledWater, 1, ItemID.FallenStar, 1, ModContent.ItemType<Potion_of_jump_strength>(), 1, jarkos.systems.LangSystem.RecipeName6, jarkos.systems.LangSystem.RecipeDesc6));
        Recipes.Add(new PotionRecipe(ItemID.BottledWater, 2, ItemID.Moonglow, 1, ModContent.ItemType<Potion_of_regen>(), 1, jarkos.systems.LangSystem.RecipeName7, jarkos.systems.LangSystem.RecipeDesc7));
        Recipes.Add(new PotionRecipe(ItemID.BottledWater, 1, ItemID.SharkFin, 1, ModContent.ItemType<Potion_of_Heaven>(), 1, jarkos.systems.LangSystem.RecipeName8, jarkos.systems.LangSystem.RecipeDesc8));
        Recipes.Add(new PotionRecipe(ItemID.BottledWater, 1, ItemID.GoldOre, 3, ModContent.ItemType<Fallen_star_moonshine>(), 1, jarkos.systems.LangSystem.RecipeName9, jarkos.systems.LangSystem.RecipeDesc9));
        Recipes.Add(new PotionRecipe(ItemID.BottledWater, 1, ItemID.Feather, 2, ModContent.ItemType<Potion01>(), 1, jarkos.systems.LangSystem.RecipeName10, jarkos.systems.LangSystem.RecipeDesc10));
        Recipes.Add(new PotionRecipe(ItemID.BottledWater, 1, ModContent.ItemType<SyntheticBlood>(), 5, ModContent.ItemType<Madness_potion>(), 1, jarkos.systems.LangSystem.RecipeName11, jarkos.systems.LangSystem.RecipeDesc11));
    }
    public static void UnloadRecipes()
    {
        recipesInitialized = false;
        Recipes.Clear();
    }
    public PotionRecipe GetCurrentRecipe()
    {
        if (!recipesInitialized)
        {
            InitializeRecipes();
        }
        if (Recipes.Count == 0) return null;
        SelectedRecipeIndex = Math.Clamp(SelectedRecipeIndex, 0, Recipes.Count - 1);
        return Recipes[SelectedRecipeIndex];
    }
    public void CycleRecipe(int direction)
    {
        SelectedRecipeIndex += direction;
        if (SelectedRecipeIndex < 0) SelectedRecipeIndex = Recipes.Count - 1;
        if (SelectedRecipeIndex >= Recipes.Count) SelectedRecipeIndex = 0;
        if (IsCrafting)
        {
            IsCrafting = false;
            CraftingProgress = 0;
        }
    }
    public bool HasIngredient(int slotIndex, int requiredType, int requiredCount)
    {
        if (slotIndex < 0 || slotIndex >= InputSlots.Length) return false;
        var slot = InputSlots[slotIndex];
        return !slot.IsAir && slot.type == requiredType && slot.stack >= requiredCount;
    }
    public bool IsCorrectIngredientType(int slotIndex)
    {
        var recipe = GetCurrentRecipe();
        if (recipe == null) return false;
        var slot = InputSlots[slotIndex];
        if (slot.IsAir) return false;
        if (slotIndex == 0)
            return slot.type == recipe.Ingredient1;
        else
            return slot.type == recipe.Ingredient2;
    }
    public bool HasEnoughIngredient(int slotIndex)
    {
        var recipe = GetCurrentRecipe();
        if (recipe == null) return false;
        var slot = InputSlots[slotIndex];
        if (slot.IsAir) return false;
        if (slotIndex == 0)
            return slot.type == recipe.Ingredient1 && slot.stack >= recipe.Ingredient1Count;
        else
            return slot.type == recipe.Ingredient2 && slot.stack >= recipe.Ingredient2Count;
    }
    public bool CanCraft()
    {
        var recipe = GetCurrentRecipe();
        if (recipe == null) return false;
        if (!HasRequiredEssences(SelectedRecipeIndex)) return false;
        bool hasIngredient1 = HasIngredient(0, recipe.Ingredient1, recipe.Ingredient1Count);
        bool hasIngredient2 = HasIngredient(1, recipe.Ingredient2, recipe.Ingredient2Count);
        bool outputOk = OutputSlot.IsAir || (OutputSlot.type == recipe.Result && OutputSlot.stack + recipe.ResultCount <= OutputSlot.maxStack);
        return hasIngredient1 && hasIngredient2 && outputOk;
    }
    public void StartCrafting()
    {
        if (CanCraft() && !IsCrafting)
        {
            IsCrafting = true;
            CraftingProgress = 0;
        }
    }
    public override void Update()
    {
        if (IsCrafting)
        {
            if (!CanCraft())
            {
                IsCrafting = false;
                CraftingProgress = 0;
                if (Main.netMode == NetmodeID.Server) SyncData();
                return;
            }
            CraftingProgress++;
            if (CraftingProgress % 20 == 0)
            {
                for (int i = 0; i < 3; i++)
                {
                    int dust = Dust.NewDust(new Vector2(Position.X * 16 + 32, Position.Y * 16 + 16), 64, 32, DustID.MagicMirror, Main.rand.NextFloat(-1f, 1f), Main.rand.NextFloat(-2f, 0f), 100, default, 1.2f);
                    Main.dust[dust].noGravity = true;
                }
            }
            if (CraftingProgress >= MaxProgress)
            {
                CompleteCrafting();
                if (Main.netMode == NetmodeID.Server) SyncData();
            }
        }
        if (PullEssenceMode)
        {
            if (Main.netMode != NetmodeID.MultiplayerClient)
            {
                _essenceTimer++;
                if (_essenceTimer >= 12)
                {
                    _essenceTimer = 0;
                    if (GetTotalEssence() < 1000)
                    {
                        var validFlasks = new List<TileEntity>();
                        foreach (var kvp in TileEntity.ByID)
                        {
                            if (kvp.Value is jarkos.tiles.EssenceFlaskTileEntity flaskTe)
                            {
                                float distSq = Vector2.DistanceSquared(new Vector2(Position.X, Position.Y), new Vector2(flaskTe.Position.X, flaskTe.Position.Y));
                                if (distSq <= 5f * 5f)
                                {
                                    if ((int)flaskTe.storedType != -1 && flaskTe.storedAmount > 0)
                                    {
                                        validFlasks.Add(flaskTe);
                                    }
                                }
                            }
                            else if (kvp.Value is jarkos.tiles.LargeEssenceFlaskTileEntity largeFlaskTe)
                            {
                                float distSq = Vector2.DistanceSquared(new Vector2(Position.X, Position.Y), new Vector2(largeFlaskTe.Position.X, largeFlaskTe.Position.Y));
                                if (distSq <= 7f * 7f)
                                {
                                    if (largeFlaskTe.TotalEssences > 0)
                                    {
                                        validFlasks.Add(largeFlaskTe);
                                    }
                                }
                            }
                        }
                        bool changed = false;
                        foreach (var te in validFlasks)
                        {
                            if (GetTotalEssence() >= 1000) break;
                            if (te is jarkos.tiles.EssenceFlaskTileEntity flaskTe)
                            {
                                flaskTe.storedAmount--;
                                if (!StoredEssences.ContainsKey(flaskTe.storedType)) StoredEssences[flaskTe.storedType] = 0;
                                StoredEssences[flaskTe.storedType]++;
                                changed = true;
                                if (flaskTe.storedAmount <= 0)
                                {
                                    flaskTe.storedType = (EssenceType)(-1);
                                }
                                if (Main.netMode == NetmodeID.Server)
                                {
                                    NetMessage.SendData(MessageID.TileEntitySharing, -1, -1, null, flaskTe.ID, flaskTe.Position.X, flaskTe.Position.Y);
                                }
                                for (int i = 0; i < 3; i++)
                                {
                                    Vector2 startPos = new Vector2(flaskTe.Position.X * 16 + 8, flaskTe.Position.Y * 16 + 8);
                                    Vector2 targetPos = new Vector2(Position.X * 16 + 24, Position.Y * 16 + 16);
                                    int dust = Dust.NewDust(startPos, 16, 16, DustID.MagicMirror, 0f, 0f, 100, default, 1.2f);
                                    Main.dust[dust].noGravity = true;
                                    Vector2 dir = targetPos - startPos;
                                    dir.Normalize();
                                    Main.dust[dust].velocity = dir * Main.rand.NextFloat(2f, 4f);
                                }
                            }
                            else if (te is jarkos.tiles.LargeEssenceFlaskTileEntity largeFlaskTe)
                            {
                                var available = largeFlaskTe.StoredEssences.Where(kv => kv.Value > 0).ToList();
                                if (available.Count > 0)
                                {
                                    EssenceType typeToSuck = available[Main.rand.Next(available.Count)].Key;
                                    largeFlaskTe.StoredEssences[typeToSuck]--;
                                    if (!StoredEssences.ContainsKey(typeToSuck)) StoredEssences[typeToSuck] = 0;
                                    StoredEssences[typeToSuck]++;
                                    changed = true;
                                    if (Main.netMode == NetmodeID.Server)
                                    {
                                        NetMessage.SendData(MessageID.TileEntitySharing, -1, -1, null, largeFlaskTe.ID, largeFlaskTe.Position.X, largeFlaskTe.Position.Y);
                                    }
                                    for (int i = 0; i < 3; i++)
                                    {
                                        Vector2 startPos = new Vector2(largeFlaskTe.Position.X * 16 + 24, largeFlaskTe.Position.Y * 16 + 24);
                                        Vector2 targetPos = new Vector2(Position.X * 16 + 24, Position.Y * 16 + 16);
                                        int dust = Dust.NewDust(startPos, 16, 16, DustID.MagicMirror, 0f, 0f, 100, default, 1.2f);
                                        Main.dust[dust].noGravity = true;
                                        Vector2 dir = targetPos - startPos;
                                        dir.Normalize();
                                        Main.dust[dust].velocity = dir * Main.rand.NextFloat(2f, 4f);
                                    }
                                }
                            }
                        }
                        if (changed && Main.netMode == NetmodeID.Server)
                        {
                            NetMessage.SendData(MessageID.TileEntitySharing, -1, -1, null, ID, Position.X, Position.Y);
                        }
                    }
                }
            }
        }
    }
    private void CompleteCrafting()
    {
        var recipe = GetCurrentRecipe();
        if (recipe == null || !CanCraft())
        {
            IsCrafting = false;
            CraftingProgress = 0;
            return;
        }
        InputSlots[0].stack -= recipe.Ingredient1Count;
        if (InputSlots[0].stack <= 0) InputSlots[0].TurnToAir();
        InputSlots[1].stack -= recipe.Ingredient2Count;
        if (InputSlots[1].stack <= 0) InputSlots[1].TurnToAir();
        var reqs = GetRequiredEssences(SelectedRecipeIndex);
        foreach (var req in reqs)
        {
            if (StoredEssences.ContainsKey(req.type))
            {
                StoredEssences[req.type] = Math.Max(0, StoredEssences[req.type] - req.count);
            }
        }
        if (recipe.Result > 0)
        {
            if (OutputSlot.IsAir)
            {
                OutputSlot = new Item();
                OutputSlot.SetDefaults(recipe.Result);
                OutputSlot.stack = recipe.ResultCount;
            }
            else
            {
                OutputSlot.stack += recipe.ResultCount;
            }
        }
        for (int i = 0; i < 15; i++)
        {
            int dust = Dust.NewDust(new Vector2(Position.X * 16 + 32, Position.Y * 16 + 16), 64, 32, DustID.GoldFlame, Main.rand.NextFloat(-3f, 3f), Main.rand.NextFloat(-3f, 3f), 100, default, 1.5f);
            Main.dust[dust].noGravity = true;
        }
        IsCrafting = false;
        CraftingProgress = 0;
    }
    public override void SaveData(TagCompound tag)
    {
        tag["input0"] = ItemIO.Save(InputSlots[0]);
        tag["input1"] = ItemIO.Save(InputSlots[1]);
        tag["output"] = ItemIO.Save(OutputSlot);
        tag["recipeIndex"] = SelectedRecipeIndex;
        tag["progress"] = CraftingProgress;
        tag["isCrafting"] = IsCrafting;
        tag["pullEssenceMode"] = PullEssenceMode;
        List<TagCompound> essenceList = new List<TagCompound>();
        foreach (var kvp in StoredEssences)
        {
            essenceList.Add(new TagCompound {
                ["type"] = (int)kvp.Key, ["amount"] = kvp.Value
            });
        }
        tag["storedEssences"] = essenceList;
    }
    public override void LoadData(TagCompound tag)
    {
        InputSlots[0] = tag.ContainsKey("input0") ? ItemIO.Load(tag.GetCompound("input0")) : new Item();
        InputSlots[1] = tag.ContainsKey("input1") ? ItemIO.Load(tag.GetCompound("input1")) : new Item();
        OutputSlot = tag.ContainsKey("output") ? ItemIO.Load(tag.GetCompound("output")) : new Item();
        SelectedRecipeIndex = tag.GetInt("recipeIndex");
        CraftingProgress = tag.GetInt("progress");
        IsCrafting = tag.GetBool("isCrafting");
        PullEssenceMode = tag.ContainsKey("pullEssenceMode") ? tag.GetBool("pullEssenceMode") : true;
        StoredEssences.Clear();
        if (tag.ContainsKey("storedEssences"))
        {
            var essenceList = tag.GetList<TagCompound>("storedEssences");
            foreach (var t in essenceList)
            {
                StoredEssences[(EssenceType)t.GetInt("type")] = t.GetInt("amount");
            }
        }
    }
    public void SyncData()
    {
        if (Main.netMode != NetmodeID.SinglePlayer)
        {
            ModPacket packet = ModContent.GetInstance<Jarkos.Jarkos_mod>().GetPacket();
            packet.Write((byte)Jarkos.JarkosPacketType.AlchEnchantStateSync);
            packet.Write(Position.X);
            packet.Write(Position.Y);
            packet.Write(SelectedRecipeIndex);
            packet.Write(CraftingProgress);
            packet.Write(IsCrafting);
            packet.Write(PullEssenceMode);
            packet.Write((byte)StoredEssences.Count);
            foreach (var kvp in StoredEssences)
            {
                packet.Write((byte)kvp.Key);
                packet.Write(kvp.Value);
            }
            bool has0 = InputSlots[0] != null && !InputSlots[0].IsAir;
            packet.Write(has0);
            if (has0) ItemIO.Send(InputSlots[0], packet, true);
            bool has1 = InputSlots[1] != null && !InputSlots[1].IsAir;
            packet.Write(has1);
            if (has1) ItemIO.Send(InputSlots[1], packet, true);
            bool hasOut = OutputSlot != null && !OutputSlot.IsAir;
            packet.Write(hasOut);
            if (hasOut) ItemIO.Send(OutputSlot, packet, true);
            packet.Send();
        }
    }
    public void SendSlotSync(int slotIndex)
    {
        if (Main.netMode == NetmodeID.MultiplayerClient)
        {
            ModPacket packet = ModContent.GetInstance<Jarkos.Jarkos_mod>().GetPacket();
            packet.Write((byte)Jarkos.JarkosPacketType.AlchEnchantSlotSync);
            packet.Write(Position.X);
            packet.Write(Position.Y);
            packet.Write((byte)slotIndex);
            Item item = slotIndex == 2 ? OutputSlot : InputSlots[slotIndex];
            bool sending = item != null && !item.IsAir;
            packet.Write(sending);
            if (sending)
                ItemIO.Send(item, packet, true);
            packet.Send();
        }
    }
    public void SendAction(byte actionType, int arg = 0)
    {
        if (Main.netMode == NetmodeID.MultiplayerClient)
        {
            ModPacket packet = ModContent.GetInstance<Jarkos.Jarkos_mod>().GetPacket();
            packet.Write((byte)Jarkos.JarkosPacketType.AlchEnchantAction);
            packet.Write(Position.X);
            packet.Write(Position.Y);
            packet.Write(actionType);
            packet.Write(arg);
            packet.Send();
        }
    }
    public override void NetSend(BinaryWriter writer)
    {
        InputSlots[0] ??= new Item();
        InputSlots[1] ??= new Item();
        OutputSlot ??= new Item();
        ItemIO.Send(InputSlots[0], writer, true);
        ItemIO.Send(InputSlots[1], writer, true);
        ItemIO.Send(OutputSlot, writer, true);
        writer.Write(SelectedRecipeIndex);
        writer.Write(CraftingProgress);
        writer.Write(IsCrafting);
        writer.Write(PullEssenceMode);
        writer.Write(StoredEssences.Count);
        foreach (var kvp in StoredEssences)
        {
            writer.Write((int)kvp.Key);
            writer.Write(kvp.Value);
        }
    }
    public override void NetReceive(BinaryReader reader)
    {
        InputSlots[0] = ItemIO.Receive(reader, true);
        InputSlots[1] = ItemIO.Receive(reader, true);
        OutputSlot = ItemIO.Receive(reader, true);
        SelectedRecipeIndex = reader.ReadInt32();
        CraftingProgress = reader.ReadInt32();
        IsCrafting = reader.ReadBoolean();
        PullEssenceMode = reader.ReadBoolean();
        StoredEssences.Clear();
        int count = reader.ReadInt32();
        for (int i = 0; i < count; i++)
        {
            EssenceType t = (EssenceType)reader.ReadInt32();
            int amt = reader.ReadInt32();
            StoredEssences[t] = amt;
        }
    }
}
#endregion
public class PotionRecipe
{
    public int Ingredient1;
    public int Ingredient1Count;
    public int Ingredient2;
    public int Ingredient2Count;
    public int Result;
    public int ResultCount;
    public LocalizedText NameText;
    public LocalizedText DescriptionText;
    public string Name => NameText?.Value ?? "";
    public string Description => DescriptionText?.Value ?? "";
    public PotionRecipe(int ing1, int ing1Count, int ing2, int ing2Count, int result, int resultCount, LocalizedText nameText, LocalizedText descriptionText = null)
    {
        Ingredient1 = ing1;
        Ingredient1Count = ing1Count;
        Ingredient2 = ing2;
        Ingredient2Count = ing2Count;
        Result = result;
        ResultCount = resultCount;
        NameText = nameText ?? LocalizedText.Empty;
        DescriptionText = descriptionText ?? LocalizedText.Empty;
    }
    public PotionRecipe(int ing1, int ing2, int result, LocalizedText nameText, LocalizedText descriptionText = null)
        : this(ing1, 1, ing2, 1, result, 1, nameText, descriptionText)
    {
    }
}
public class AlchemyRecipeSystem : ModSystem
{
    public override void PostSetupContent()
    {
        AlchemicalEnchantmentTE.InitializeRecipes();
    }
    public override void Unload()
    {
        AlchemicalEnchantmentTE.UnloadRecipes();
    }
}
}
