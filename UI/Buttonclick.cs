using Microsoft.Xna.Framework;
using Terraria.GameContent.UI.Elements;
using Terraria.UI;

namespace jarkos.UI
{
  public class Buttonclick : UIElement
  {
    private object _text;
    private MouseEvent _clickAction;
    public UIPanel _uiPanel;
    private UIText _uiText;
    public string Text
    {
      get => _uiText?.Text ?? string.Empty;
      set => _text = value;
    }
    public Buttonclick(MouseEvent clickAction) => _clickAction = clickAction;
    public override void OnInitialize()
    {
      _uiPanel = new UIPanel();
      Width = StyleDimension.Fill;
      _uiPanel.Height = StyleDimension.Fill;
      _uiPanel.BorderColor = Color.Transparent;
      _uiPanel.BackgroundColor = Color.Transparent;
      Append(_uiPanel);
      _uiPanel.OnLeftClick += _clickAction;
    }
    public override void Update(GameTime gameTime)
    {
      base.Update(gameTime);
      if (_text == null)
        return;

      _uiText.SetText(_text.ToString());
      _text = null;
      Recalculate();
      MinWidth = _uiText.MinWidth;
      MinHeight = _uiText.MinHeight;
    }
  }
}
