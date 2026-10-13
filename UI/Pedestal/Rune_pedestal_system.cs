using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ModLoader;
using Terraria.UI;
namespace jarkos.UI.Pedestal
{
  internal class Rune_pedestal_system : ModSystem
  {
    internal UserInterface pedestal_Interface;
    internal Pedestal_UIsystem Pedestal_UIsystem;
    internal bool active;
    internal void ShowMyUI()
    {
      pedestal_Interface?.SetState((UIState) Pedestal_UIsystem);
      active = true;
    }
    internal void HideMyUI()
    {
      pedestal_Interface?.SetState((UIState) null);
      active = false;
    }
    public override void Load()
    {
      if (Main.dedServ)
        return;
      Pedestal_UIsystem = new Pedestal_UIsystem();
      pedestal_Interface = new UserInterface();
      pedestal_Interface.SetState((UIState) Pedestal_UIsystem);
    }
    public override void UpdateUI(GameTime gameTime) => pedestal_Interface?.Update(gameTime);
    public override void ModifyInterfaceLayers(List<GameInterfaceLayer> layers)
    {
      int index = layers.FindIndex((Predicate<GameInterfaceLayer>) (layer => layer.Name.Equals("Vanilla: Resource Bars")));
      if (index == -1)
        return;
      layers.Insert(index, (GameInterfaceLayer) new LegacyGameInterfaceLayer("Resource Bar", (GameInterfaceDrawMethod) (() =>
      {
        pedestal_Interface.Draw((SpriteBatch) Main.spriteBatch, new GameTime());
        return true;
      }), InterfaceScaleType.UI));
    }
  }
}
