using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent.UI.Elements;
using Terraria.UI;
using static Terraria.ModLoader.ModContent;
using Terraria.ModLoader;
using System.Collections.Generic;

namespace Jarkos.UI
{
	internal class Termal_bar : UIState
	{
		private UIText text;
		private UIElement area;
		private UIImage barFrame;
		private Color gradientA;
		private Color gradientB;
		public override void OnInitialize() {
			area = new UIElement();
			area.Left.Set(-area.Width.Pixels - 600, 1f);
			area.Top.Set(30, 0f);
			area.Width.Set(182, 0f);
			area.Height.Set(60, 0f);

			barFrame = new UIImage(Request<Texture2D>("Jarkos/UI/Termal_bar"));
			barFrame.Left.Set(-430, 0f);
			barFrame.Top.Set(500, 0f);
			barFrame.Width.Set(138, 0f);
			barFrame.Height.Set(34, 0f);

			text = new UIText("0/0", 0.8f);
			text.Width.Set(138, 0f);
			text.Height.Set(34, 0f);
			text.Top.Set(540, 0f);
			text.Left.Set(-430, 0f);

			gradientA = new Color(255, 110, 110);
			gradientB = new Color(232, 28, 28);

			area.Append(text);
			area.Append(barFrame);
			Append(area);
		}
		public override void Draw(SpriteBatch spriteBatch) {
			return;
		}
		protected override void DrawSelf(SpriteBatch spriteBatch) {
			return;
		}
		public override void Update(GameTime gameTime) {
			return;
		}
		class ThermalUISystem : ModSystem
		{
		private UserInterface thermal_barUserInterface;
		internal Termal_bar thermal_bar;
		public override void Load() {
			if (!Main.dedServ) {
				thermal_bar = new();
				thermal_barUserInterface = new();
				thermal_barUserInterface.SetState(thermal_bar);
			}
		}
		public override void UpdateUI(GameTime gameTime)
		{
			thermal_barUserInterface?.Update(gameTime);
		}
		public override void ModifyInterfaceLayers(List<GameInterfaceLayer> layers)
		{
			int resourceBarIndex = layers.FindIndex(layer => layer.Name.Equals("Vanilla: Resource Bars"));
			if (resourceBarIndex != -1)
			{
				layers.Insert(resourceBarIndex, new LegacyGameInterfaceLayer("Jarkos: Mechanoid Resource Bar", delegate
				{
					thermal_barUserInterface.Draw(Main.spriteBatch, new GameTime());
					return true;
				}, InterfaceScaleType.UI));
			}
		}
	}
}
}
