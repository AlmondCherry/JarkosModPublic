using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;

namespace jarkos.systems
{
	public class LightningSystemLaser : ModSystem
	{
		public static void MakeProjectileLighting(Vector2 source, Vector2 dest, int dustId, int damage, float knockback, float scale, float sway = 80f, float jagednessNumerator = 1f)
		{
			List<Vector2> dustPoints = CreateBolt(source, dest, sway, jagednessNumerator);
			for (int i = 1; i < dustPoints.Count; i++)
			{
				Vector2 start = dustPoints[i - 1];
				Vector2 end = dustPoints[i];
				float numDust = (end - start).Length() * 0.05f;
				Vector2 dir = end - start;
				dir.Normalize();
				for (int j = 0; j < numDust; j++)
				{
					float lerp = j / numDust;
					Vector2 dustPosition = Vector2.Lerp(start, end, lerp);
					Vector2 tangent2 = dest - source;
					tangent2.Normalize();
					Projectile.NewProjectile(null, dustPosition, dir, dustId, damage, knockback);
				}
			}
		}
		public static List<Vector2> CreateBolt(Vector2 source, Vector2 dest, float sway = 80f, float jagednessNumerator = 1f)
		{
			List<Vector2> results = new();
			Vector2 tangent = dest - source;
			Vector2 normal = Vector2.Normalize(new Vector2(tangent.Y, -tangent.X));
			float length = tangent.Length();
			List<float> positions = new() { 0 };

			for (int i = 0; i < length / 16f; i++)
			{
				positions.Add(Main.rand.NextFloat());
			}

			positions.Sort();
			float jaggedness = jagednessNumerator / sway;
			Vector2 prevPoint = source;
			float prevDisplacement = 0f;
			
			for (int i = 1; i < positions.Count; i++)
			{
				float pos = positions[i];
				float scale = length * jaggedness * (pos - positions[i - 1]);
				float envelope = pos > 0.95f ? 20 * (1 - pos) : 1;
				float displacement = Main.rand.NextFloat(-sway, sway);
				displacement -= (displacement - prevDisplacement) * (1 - scale);
				displacement *= envelope;
				Vector2 point = source + pos * tangent + displacement * normal;
				results.Add(point);
				prevPoint = point;
				prevDisplacement = displacement;
			}

			results.Add(prevPoint);
			results.Add(dest);
			results.Insert(0, source);
			return results;
		}

	
		public static void MakeDust(Vector2 source, Vector2 dest, int dustId, float scale, float sway = 80f, float jagednessNumerator = 1f)
		{
			List<Vector2> dustPoints = CreateBolts(source, dest, sway, jagednessNumerator);
			for (int i = 1; i < dustPoints.Count; i++)
			{
				Vector2 start = dustPoints[i - 1];
				Vector2 end = dustPoints[i];
				float numDust = (end - start).Length() * 0.4f;
				for (int j = 0; j < numDust; j++)
				{
					float lerp = j / numDust;
					Vector2 dustPosition = Vector2.Lerp(start, end, lerp);
					Dust d = Dust.NewDustPerfect(dustPosition, dustId, Scale: scale);
					d.noGravity = true;
					d.velocity = Main.rand.NextVector2Circular(0.3f, 0.3f);
				}
			}
		}
		public static List<Vector2> CreateBolts(Vector2 source, Vector2 dest, float sway = 80f, float jagednessNumerator = 1f)
		{
			List<Vector2> results = new();
			Vector2 tangent = dest - source;
			Vector2 normal = Vector2.Normalize(new Vector2(tangent.Y, -tangent.X));
			float length = tangent.Length();
			List<float> positions = new()
			{
			0
			};
			for (int i = 0; i < length / 16f; i++) {
				positions.Add(Main.rand.NextFloat());
			}
			positions.Sort();
			float jaggedness = jagednessNumerator / sway;
			Vector2 prevPoint = source;
			float prevDisplacement = 0f;
			for (int i = 1; i < positions.Count; i++)
			{
				float pos = positions[i];
				float scale = length * jaggedness * (pos - positions[i - 1]);
				float envelope = pos > 0.95f ? 20 * (1 - pos) : 1;
				float displacement = Main.rand.NextFloat(-sway, sway);
				displacement -= (displacement - prevDisplacement) * (1 - scale);
				displacement *= envelope;
				
				Vector2 point = source + pos * tangent + displacement * normal;
				results.Add(point);
				prevPoint = point;
				prevDisplacement = displacement;
			}
			results.Add(prevPoint);
			results.Add(dest);
			results.Insert(0, source);
			return results;
		}
	}
}
