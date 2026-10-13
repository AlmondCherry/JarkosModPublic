using Terraria.ModLoader;
using Terraria;
using Terraria.ModLoader.IO;
namespace jarkos
{
	public class Jarkos_player_statboost : ModPlayer
	{
		public bool DFF_used = false;
		public int DFFlimit = 1;
		public int DFFup = 0;
		public bool Steak_used = false;
		public int Steaklimit = 1;
		public int Steak_up = 0;
		public override void ModifyMaxStats(out StatModifier health, out StatModifier mana)
		{
			mana = StatModifier.Default;
			mana.Base += DFFup * 40;
			health = StatModifier.Default;
			health.Base += Steak_up * 25;
		}
        public override void Initialize()
    	{
			DFFlimit = 1;
			DFFup = 0;
			Steaklimit = 1;
			Steak_up = 0;
		}
		public override void ResetEffects()
    	{
			Player.statManaMax2 += DFFup * 40;
			Player.statLifeMax2 += Steak_up * 25;
		}
		public override void SaveData(TagCompound tag)
    	{
			tag.Add("manaup01", (object) DFFup);
			tag.Add("lifeup01", (object) Steak_up);
    	}
    	public override void LoadData(TagCompound tag)
    	{
			DFFup = tag.GetInt("manaup01");
			Steak_up = tag.GetInt("lifeup01");
    	}
	}
}
