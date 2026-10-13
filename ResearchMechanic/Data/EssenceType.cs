using jarkos.systems;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria.ModLoader;

namespace jarkos.core.Data
{
    public enum EssenceType
    {
        Acus,
        Aer,
        Anima,
        Aqua,
        Arena,
        Caro,
        Chitinus,
        Cruor,
        Fulgur,
        Fungus,
        Glacies,
        Imperium,
        Luna,
        Lux,
        Mechanus,
        Nebula,
        Ordo,
        Pelagus,
        Pyros,
        Regalis,
        Silva,
        Solar,
        Stellaris,
        Terra,
        Umbra,
        Venenum,
        Vita,
        Vortex
    }

    public static class EssencePalette
    {
        public static Color ColorOf(EssenceType type) => type switch
        {
            EssenceType.Acus => new Color(182, 182, 182), 
            EssenceType.Aer => new Color(255, 253, 135), 
            EssenceType.Anima => new Color(234, 224, 194), 
            EssenceType.Aqua => new Color(0, 255, 250), 
            EssenceType.Arena => new Color(255, 208, 23), 
            EssenceType.Caro => new Color(255, 145, 145), 
            EssenceType.Chitinus => new Color(155, 90, 172), 
            EssenceType.Cruor => new Color(190, 0, 22), 
            EssenceType.Fulgur => new Color(0, 255, 242), 
            EssenceType.Fungus => new Color(80, 103, 255), 
            EssenceType.Glacies => new Color(0, 220, 255), 
            EssenceType.Imperium => new Color(255, 120, 75), 
            EssenceType.Luna => new Color(224, 255, 238), 
            EssenceType.Lux => new Color(255, 220, 65), 
            EssenceType.Mechanus => new Color(255, 255, 255), 
            EssenceType.Nebula => new Color(228, 111, 249), 
            EssenceType.Ordo => new Color(173, 173, 173), 
            EssenceType.Pelagus => new Color(0, 255, 148), 
            EssenceType.Pyros => new Color(255, 131, 49), 
            EssenceType.Regalis => new Color(255, 227, 43), 
            EssenceType.Silva => new Color(37, 159, 0), 
            EssenceType.Solar => new Color(255, 144, 44), 
            EssenceType.Stellaris => new Color(115, 245, 255), 
            EssenceType.Terra => new Color(61, 255, 18), 
            EssenceType.Umbra => new Color(123, 0, 190), 
            EssenceType.Venenum => new Color(0, 195, 9), 
            EssenceType.Vita => new Color(255, 34, 34), 
            EssenceType.Vortex => new Color(49, 255, 206), 
            _ => Color.White
        };

        public static string DisplayName(EssenceType type) => type.ToString();
        
        public static string GetBonusText(EssenceType type, float amount)
        {
            if (amount <= 0) return "";
            
            float mult = Jarkos_config.Instance?.EssenceEffectMultiplier ?? 1.0f;
            float eff = amount * mult;

            return type switch
            {
                EssenceType.Terra => LangSystem.EssenceBonus_Terra.Format(eff * 0.0002f), 
                EssenceType.Vita => LangSystem.EssenceBonus_Vita.Format(eff * 0.0005f), 
                EssenceType.Aer => LangSystem.EssenceBonus_Aer.Format(eff * 0.00002f), 
                EssenceType.Aqua => LangSystem.EssenceBonus_Aqua.Format(eff * 0.00005f), 
                EssenceType.Pyros => LangSystem.EssenceBonus_Pyros.Format(eff * 0.00005f), 
                EssenceType.Umbra => LangSystem.EssenceBonus_Umbra.Format(eff * 0.001f), 
                EssenceType.Cruor => LangSystem.EssenceBonus_Cruor.Format(eff * 0.0001f), 
                EssenceType.Venenum => LangSystem.EssenceBonus_Venenum.Format(eff * 0.00001f), 
                EssenceType.Arena => LangSystem.EssenceBonus_Arena.Format(eff * 0.00001f), 
                EssenceType.Glacies => LangSystem.EssenceBonus_Glacies.Format(eff * 0.0001f), 
                EssenceType.Anima => LangSystem.EssenceBonus_Anima.Format(eff * 0.00005f), 
                EssenceType.Regalis => LangSystem.EssenceBonus_Regalis.Format(eff * 0.00005f), 
                EssenceType.Fulgur => LangSystem.EssenceBonus_Fulgur.Format(eff * 0.00001f), 
                EssenceType.Luna => LangSystem.EssenceBonus_Luna.Format(eff * 0.0001f), 
                EssenceType.Lux => LangSystem.EssenceBonus_Lux.Format(eff * 0.00001f), 
                EssenceType.Imperium => LangSystem.EssenceBonus_Imperium.Format(eff * 0.00001f), 
                EssenceType.Acus => LangSystem.EssenceBonus_Acus.Format(eff * 0.00001f), 
                EssenceType.Caro => LangSystem.EssenceBonus_Caro.Format(eff * 0.00005f), 
                EssenceType.Mechanus => LangSystem.EssenceBonus_Mechanus.Format(eff * 0.0001f), 
                EssenceType.Silva => LangSystem.EssenceBonus_Silva.Format(eff * 0.000075f), 
                EssenceType.Chitinus => LangSystem.EssenceBonus_Chitinus.Format(eff * 0.00005f), 
                EssenceType.Pelagus => LangSystem.EssenceBonus_Pelagus.Format(eff * 0.0001f), 
                EssenceType.Solar => LangSystem.EssenceBonus_Solar.Format(eff * 0.00005f), 
                EssenceType.Vortex => LangSystem.EssenceBonus_Vortex.Format(eff * 0.00005f), 
                EssenceType.Nebula => LangSystem.EssenceBonus_Nebula.Format(eff * 0.00005f), 
                EssenceType.Stellaris => LangSystem.EssenceBonus_Stellaris.Format(eff * 0.00005f), 
                EssenceType.Ordo => LangSystem.EssenceBonus_Ordo.Value, _ => ""
            };
        }
        public static string GetBonusText(EssenceType type, int amount) => GetBonusText(type, (float)amount);

        public static Texture2D GetTexture(object type)
        {
            if(type == null) 
                return null;
            string path = "jarkos/textures/Essences/" + type.ToString();

            if(ModContent.HasAsset(path))
            {
                return ModContent.Request<Texture2D>(path).Value;
            }
            return null;
        }
    }
}
