using jarkos.UI;
using Terraria.GameContent.UI.Elements;
using Terraria.UI;
namespace jarkos.UI
{
  public static class Placebutton
  {
    public static void btn_place(UIImage barFrame, UIImage texture_btn1, UIImage texture_btn2, UIImage texture_btn3, UIImage texture_btn4, UIImage texture_btn5, UIImage texture_btn6, UIImage texture_btn7, UIImage texture_btn8, UIElement area, Buttonclick button1, Buttonclick button2, Buttonclick button3, Buttonclick button4, Buttonclick button5, Buttonclick button6, Buttonclick button7, Buttonclick button8)
    {
      button1.Width.Set(24f, 0.0f);
      button1.Height.Set(24f, 0.0f);
      button1.Top.Set(10f, 0.0f);
      button1.Left.Set(60f, 0.0f);
      button2.Width.Set(24f, 0.0f);
      button2.Height.Set(24f, 0.0f);
      button2.Top.Set(10f, 0.0f);
      button2.Left.Set(120f, 0.0f);
      button3.Width.Set(24f, 0.0f);
      button3.Height.Set(24f, 0.0f);
      button3.Top.Set(10f, 0.0f);
      button3.Left.Set(180f, 0.0f);
      button4.Width.Set(24f, 0.0f);
      button4.Height.Set(24f, 0.0f);
      button4.Top.Set(70f, 0.0f);
      button4.Left.Set(60f, 0.0f);
      button5.Width.Set(24f, 0.0f);
      button5.Height.Set(24f, 0.0f);
      button5.Top.Set(70f, 0.0f);
      button5.Left.Set(120f, 0.0f);
      button6.Width.Set(24f, 0.0f);
      button6.Height.Set(24f, 0.0f);
      button6.Top.Set(70f, 0.0f);
      button6.Left.Set(180f, 0.0f);
      button7.Width.Set(24f, 0.0f);
      button7.Height.Set(24f, 0.0f);
      button7.Top.Set(130f, 0.0f);
      button7.Left.Set(90f, 0.0f);
      button8.Width.Set(24f, 0.0f);
      button8.Height.Set(24f, 0.0f);
      button8.Top.Set(130f, 0.0f);
      button8.Left.Set(150f, 0.0f);
      area.Append((UIElement) barFrame);
      area.Append((UIElement) texture_btn1);
      area.Append((UIElement) texture_btn2);
      area.Append((UIElement) texture_btn3);
      area.Append((UIElement) texture_btn4);
      area.Append((UIElement) texture_btn5);
      area.Append((UIElement) texture_btn6);
      area.Append((UIElement) texture_btn7);
      area.Append((UIElement) texture_btn8);
      area.Append((UIElement) button1);
      area.Append((UIElement) button2);
      area.Append((UIElement) button3);
      area.Append((UIElement) button4);
      area.Append((UIElement) button5);
      area.Append((UIElement) button6);
      area.Append((UIElement) button7);
      area.Append((UIElement) button8);
    }
  }
}
