using Terraria.ModLoader;
namespace jarkos.systems
{
  public class Keybindsystem : ModSystem
  {
    public static ModKeybind charger_health_bind { get; private set; }
    public static ModKeybind rocketmk2_bind { get; private set; }
    public static ModKeybind filth_bind { get; private set; }
    public static ModKeybind accessory_ability_bind { get; private set; }
    public override void Load()
    {
      accessory_ability_bind = KeybindLoader.RegisterKeybind(Mod, "Accessory ability", "O");
    }
    public override void Unload()
    {
      charger_health_bind = null;
      rocketmk2_bind = null;
      filth_bind = null;
      accessory_ability_bind = null;
    }
  }
}
