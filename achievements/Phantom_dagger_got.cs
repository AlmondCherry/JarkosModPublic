using jarkos.npc.hell_mobs;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.Achievements;
using Terraria.GameContent.Achievements;
using Terraria.ID;
using Terraria.ModLoader;
namespace jarkos.achievements;
public class Phantom_dagger_got : ModAchievement
{
	public CustomIntCondition Condition { get; private set; }
	public override void SetStaticDefaults() {
		Achievement.SetCategory(AchievementCategory.Challenger);
		Condition = AddIntCondition(1);
	}
	public override IEnumerable<Position> GetModdedConstraints() {
		yield return new After(ModContent.GetInstance<KharonItem>());
	}
	public override void OnCompleted(Achievement achievement) {
		int fireworkProjectile = ProjectileID.RocketFireworksBoxRed + Main.rand.Next(4);
		Projectile.NewProjectile(Main.LocalPlayer.GetSource_FromThis(), Main.LocalPlayer.Top, new Vector2(Main.rand.NextFloat(-2, 2), -Main.rand.NextFloat(2f, 4f)).RotatedByRandom(0.3f), fireworkProjectile, 0, 0, Main.myPlayer);
	}
}
public class CustomTracker8 : ConditionFloatTracker
{
	private Dictionary<AchievementCondition, float> weightedConditions;
	public CustomTracker8(Dictionary<AchievementCondition, float> weightedConditions) {
		this.weightedConditions = weightedConditions;
		foreach ((AchievementCondition condition, float weight) in weightedConditions) {
			_maxValue += weight;
			condition.OnComplete += OnConditionCompleted;
		}
	}
	private void OnConditionCompleted(AchievementCondition condition) {
		SetValue(Math.Min(_value + weightedConditions[condition], _maxValue));
	}
	protected override void Load() {
		foreach ((AchievementCondition condition, float weight) in weightedConditions) {
			if (condition.IsCompleted)
				_value += weight;
		}
	}
}
