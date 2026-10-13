using jarkos;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System.Collections.Generic;
using Terraria;
using Terraria.GameContent;
using Terraria.ModLoader;
namespace Jarkos.UI.ResourceOverlay
{
	public class ResourceOverlayLife : ModResourceOverlay
	{
        private Dictionary<string, Asset<Texture2D>> vanillaAssetCache = new();
        public string baseFolder = "jarkos/UI/ResourceOverlay/";
        public string LifeTexturePath()
        {
            string folder = $"{baseFolder}HP";
            Player player = Main.LocalPlayer;
			var modPlayer = player.GetModPlayer<Jarkos_player_statboost>();
            if (modPlayer.Steak_used)
				return folder + "SteakFood";
            return string.Empty;
        }
        public string ManaTexturePath()
        {
            string folder = $"{baseFolder}MP";
            Player player = Main.LocalPlayer;
			var modPlayer = player.GetModPlayer<Jarkos_player_statboost>();
            if (modPlayer.DFF_used)
                return folder + "Diffusion";
            return string.Empty;
        }
        public override void PostDrawResource(ResourceOverlayDrawContext context)
        {
            Asset<Texture2D> asset = context.texture;
            string fancyFolder = "Images/UI/PlayerResourceSets/FancyClassic/";
            string barsFolder = "Images/UI/PlayerResourceSets/HorizontalBars/";
            if (ManaTexturePath() != string.Empty)
            {
                if (asset == TextureAssets.Mana || CompareAssets(asset, fancyFolder + "Star_Fill"))
                {
                    context.texture = ModContent.Request<Texture2D>(ManaTexturePath() + "Star");
                    context.Draw();
                }
                else if (CompareAssets(asset, barsFolder + "MP_Fill"))
                {
                    context.texture = ModContent.Request<Texture2D>(ManaTexturePath() + "Bar");
                    context.Draw();
                }
            }
            if (LifeTexturePath() == string.Empty)
                return;
            if (asset == TextureAssets.Heart || asset == TextureAssets.Heart2 || CompareAssets(asset, fancyFolder + "Heart_Fill") || CompareAssets(asset, fancyFolder + "Heart_Fill_B"))
            {
                context.texture = ModContent.Request<Texture2D>(LifeTexturePath() + "Heart");
                context.Draw();
            }
            else if (CompareAssets(asset, barsFolder + "HP_Fill") || CompareAssets(asset, barsFolder + "HP_Fill_Honey"))
            {
                context.texture = ModContent.Request<Texture2D>(LifeTexturePath() + "Bar");
                context.Draw();
            }
        }
        private bool CompareAssets(Asset<Texture2D> currentAsset, string compareAssetPath)
        {
            if (!vanillaAssetCache.TryGetValue(compareAssetPath, out var asset))
                asset = vanillaAssetCache[compareAssetPath] = Main.Assets.Request<Texture2D>(compareAssetPath);
            return currentAsset == asset;
        }
	}
}
