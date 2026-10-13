using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Runtime.CompilerServices;
using Terraria;
using Terraria.GameContent.UI.Elements;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.UI;
using System.Collections.Generic;
using Terraria.GameContent;
using jarkos;
using jarkos.systems;
namespace jarkos.UI.Pedestal
{
  internal class Pedestal_UIsystem : UIState
  {
    private UIImage texture_btn1;
    private UIImage texture_btn2;
    private UIImage texture_btn3;
    private UIImage texture_btn4;
    private UIElement area;
    private UIImage barFrame;
    private float max_size_image = 2f;
    public override void OnInitialize()
    {
      area = new UIElement();
      area.Width.Set(270f, 0.0f);
      area.Height.Set(240f, 0.0f);
      area.HAlign = 0.84f;
      area.VAlign = 0.34f;
      barFrame = new UIImage(ModContent.Request<Texture2D>("jarkos/UI/Pedestal/pedestal"));
      barFrame.ScaleToFit = false;
      barFrame.Width.Set(270f, 0.0f);
      barFrame.Height.Set(180f, 0.0f);
      area.HAlign = 0.84f;
      area.VAlign = 0.34f;
      texture_btn1 = new UIImage(ModContent.Request<Texture2D>("jarkos/UI/Pedestal/rune_filth_sign"));
      texture_btn1.Width.Set(24f, 0.0f);
      texture_btn1.Height.Set(24f, 0.0f);
      texture_btn1.Top.Set(10f, 0.0f);
      texture_btn1.Left.Set(120f, 0.0f);
      texture_btn1.Color = new(255, 255, 255, 0);
      texture_btn1.ImageScale = 1f;
      texture_btn2 = new UIImage(ModContent.Request<Texture2D>("jarkos/UI/Pedestal/rune_steampunk_sign"));
      texture_btn2.Width.Set(24f, 0.0f);
      texture_btn2.Height.Set(24f, 0.0f);
      texture_btn2.Top.Set(35f, 0.0f);
      texture_btn2.Left.Set(0f, 0.0f);
      texture_btn2.Color = new(255, 255, 255, 0);
      texture_btn2.ImageScale = 1f;
      texture_btn3 = new UIImage(ModContent.Request<Texture2D>("jarkos/UI/Pedestal/rune_elder_sign"));
      texture_btn3.Width.Set(24f, 0.0f);
      texture_btn3.Height.Set(24f, 0.0f);
      texture_btn3.Top.Set(10f, 0.0f);
      texture_btn3.Left.Set(40f, 0.0f);
      texture_btn3.Color = new(255, 255, 255, 0);
      texture_btn3.ImageScale = 1f;
      texture_btn4 = new UIImage(ModContent.Request<Texture2D>("jarkos/UI/Pedestal/rune_flame_sign"));
      texture_btn4.Width.Set(24f, 0.0f);
      texture_btn4.Height.Set(24f, 0.0f);
      texture_btn4.Top.Set(35f, 0.0f);
      texture_btn4.Left.Set(80f, 0.0f);
      texture_btn4.Color = new(255, 255, 255, 0);
      texture_btn4.ImageScale = 1f;
      Rune_placement.runes_placement(barFrame, texture_btn1, texture_btn2, texture_btn3, texture_btn4, area);
      Append(area);
    }
    public override void Update(GameTime gameTime)
    {
      base.Update(gameTime);
      Player player = Main.LocalPlayer;
      var Jarkos_player = player.GetModPlayer<Jarkos_player>();
      if(Jarkos_player.filth_min)
      {
        texture_btn1.Color = new(255, 255, 255);
      }
      else{
        texture_btn1.Color = new(0, 0, 0, 255);
      }
      if(Jarkos_player.steampunk_min)
      {
        texture_btn2.Color = new(255, 255, 255);
      }
      else{
        texture_btn2.Color = new(0, 0, 0, 255);
      }
      if(Jarkos_player.elder_min)
      {
        texture_btn3.Color = new(255, 255, 255);
      }
      else{
        texture_btn3.Color = new(0, 0, 0, 255);
      }
      if(Jarkos_player.flame_min)
      {
        texture_btn4.Color = new(255, 255, 255);
      }
      else{
        texture_btn4.Color = new(0, 0, 0, 255);
      }
      if (texture_btn1.IsMouseHovering && Jarkos_player.filth_min)
      {
        Main.instance.MouseText((string)LangSystem.filth_rune_desc);
        if(texture_btn1.ImageScale < max_size_image)
        {
        texture_btn1.ImageScale += 0.2f;
        }
      }
      else
      {
        if(texture_btn1.ImageScale > 1)
        {
        texture_btn1.ImageScale -= 0.2f;
        }
        else
        {
          texture_btn1.ImageScale = 1f;
        }
      }
      if (texture_btn2.IsMouseHovering && Jarkos_player.steampunk_min)
      {
        Main.instance.MouseText((string)LangSystem.steampunk_rune_desc);
        if(texture_btn2.ImageScale < max_size_image)
        {
        texture_btn2.ImageScale += 0.2f;
        }
      }
      else
      {
        if(texture_btn2.ImageScale > 1)
        {
        texture_btn2.ImageScale -= 0.2f;
        }
        else
        {
          texture_btn2.ImageScale = 1f;
        }
      }
      if (texture_btn3.IsMouseHovering && Jarkos_player.elder_min)
      {
        Main.instance.MouseText((string)LangSystem.elder_rune_desc);
        if(texture_btn3.ImageScale < max_size_image)
        {
        texture_btn3.ImageScale += 0.2f;
        }
      }
      else
      {
        if(texture_btn3.ImageScale > 1)
        {
        texture_btn3.ImageScale -= 0.2f;
        }
        else
        {
          texture_btn3.ImageScale = 1f;
        }
      }
      if (texture_btn4.IsMouseHovering && Jarkos_player.flame_min)
      {
        Main.instance.MouseText((string)LangSystem.flame_rune_desc);
        if(texture_btn4.ImageScale < max_size_image)
        {
        texture_btn4.ImageScale += 0.2f;
        }
      }
      else
      {
        if(texture_btn4.ImageScale > 1)
        {
        texture_btn4.ImageScale -= 0.2f;
        }
        else
        {
          texture_btn4.ImageScale = 1f;
        }
      }
    }
  }
}
