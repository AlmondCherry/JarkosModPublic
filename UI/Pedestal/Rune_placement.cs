using jarkos.UI;
using Terraria.GameContent.UI.Elements;
using Terraria.UI;
namespace jarkos.UI.Pedestal
{
  public static class Rune_placement
  {
    public static void runes_placement(UIImage barFrame, UIImage texture_btn1, UIImage texture_btn2, UIImage texture_btn3, UIImage texture_btn4, UIElement area)
    {
      area.Append((UIElement) barFrame);
      area.Append((UIElement) texture_btn1);
      area.Append((UIElement) texture_btn2);
      area.Append((UIElement) texture_btn3);
      area.Append((UIElement) texture_btn4);
    }
  }
}
