using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;
using Terraria.Graphics.Effects;
using Terraria.Graphics.Shaders;
using Terraria.ID;
namespace Jarkos.systems
{
    public class TenderodScreenSystem : ModSystem
    {
        public static float DarkenIntensity = 0f;
        public static float ScreenShakeAmount = 0f;
        public static Vector2 PortalPosition = Vector2.Zero;
        public static float InvertIntensity = 0f;
        public static float RoarDistortion = 0f;
        public static Vector2 DarknessCenter = Vector2.Zero;
        public static float DarknessRadius = 0f;
        public const string FilterName = "jarkos:TenderodDarkness";
        public static float TargetDarken = 0f;
        public override void OnWorldUnload()
        {
            DarkenIntensity = 0f;
            TargetDarken = 0f;
            ScreenShakeAmount = 0f;
            InvertIntensity = 0f;
            RoarDistortion = 0f;
            PortalPosition = Vector2.Zero;
            DarknessCenter = Vector2.Zero;
            DarknessRadius = 0f;

            if (!Main.dedServ)
            {
                if (SkyManager.Instance["Jarkos:TenderoteSky"] != null && SkyManager.Instance["Jarkos:TenderoteSky"].IsActive())
                {
                    SkyManager.Instance.Deactivate("Jarkos:TenderoteSky");
                }
                if (Filters.Scene["Jarkos:TenderoteRoarFilter"].IsActive())
                {
                    Filters.Scene["Jarkos:TenderoteRoarFilter"].Deactivate();
                }
            }
        }
        public override void OnWorldLoad()
        {
            DarkenIntensity = 0f;
            TargetDarken = 0f;
            ScreenShakeAmount = 0f;
            InvertIntensity = 0f;
            RoarDistortion = 0f;
            DarknessCenter = Vector2.Zero;
            DarknessRadius = 0f;
        }
        public override void PostUpdateEverything()
        {
            bool bossAlive = NPC.AnyNPCs(ModContent.NPCType<jarkos.npc.tenderote.Tenderote_head>());
            bool portalAlive = false;

            for (int i = 0; i < Main.maxProjectiles; i++) 
            {
                if (Main.projectile[i].active && Main.projectile[i].type == ModContent.ProjectileType<projectiles.enemy.tenderote.Tenderote_portal>()) 
                {
                    portalAlive = true;
                    break;
                }
            }
            if (!bossAlive && !portalAlive)
            {
                TargetDarken = 0f;
            }
            if (!bossAlive)
            {
                DarknessRadius = 0f;
            }
            else
            {
                NPC boss = null;

                for(int i = 0; i < Main.maxNPCs; i++) 
                {
                    if (Main.npc[i].active && Main.npc[i].type == ModContent.NPCType<jarkos.npc.tenderote.Tenderote_head>()) 
                    { 
                        boss = Main.npc[i]; 
                        break; 
                    }
                }
                if (boss != null && boss.localAI[0] == 1f)
                {
                    if (DarknessRadius <= 0f) DarknessCenter = boss.Center;
                    DarknessRadius += 30f;
                }
                else
                {
                    DarknessRadius = 0f;
                }
            }
            DarkenIntensity = MathHelper.Lerp(DarkenIntensity, TargetDarken, 0.02f);
            if (DarkenIntensity < 0.01f && TargetDarken < 0.01f)
            {
                DarkenIntensity = 0f;
            }

            ScreenShakeAmount = MathHelper.Lerp(ScreenShakeAmount, 0f, 0.1f);
            InvertIntensity = MathHelper.Lerp(InvertIntensity, 0f, 0.05f);
            RoarDistortion = MathHelper.Lerp(RoarDistortion, 0f, 0.1f);

            if (Main.netMode == NetmodeID.Server) 
                return;
            if (InvertIntensity > 0.01f || RoarDistortion > 0.01f)
            {
                if (!Filters.Scene["Jarkos:TenderoteRoarFilter"].IsActive())
                {
                    Filters.Scene.Activate("Jarkos:TenderoteRoarFilter", Main.LocalPlayer.Center);
                }
                Filters.Scene["Jarkos:TenderoteRoarFilter"].GetShader().UseIntensity(InvertIntensity).UseProgress(RoarDistortion);
            }
            else
            {
                if (Filters.Scene["Jarkos:TenderoteRoarFilter"].IsActive())
                {
                    Filters.Scene.Deactivate("Jarkos:TenderoteRoarFilter");
                }
            }
            if (Main.netMode == NetmodeID.Server) 
                return;
                
            if (DarkenIntensity > 0.01f)
            {
                if (!SkyManager.Instance["Jarkos:TenderoteSky"].IsActive())
                {
                    SkyManager.Instance.Activate("Jarkos:TenderoteSky");
                }
            }
            else
            {
                if (SkyManager.Instance["Jarkos:TenderoteSky"].IsActive())
                {
                    SkyManager.Instance.Deactivate("Jarkos:TenderoteSky");
                }
            }
        }
        public override void ModifyScreenPosition()
        {
            if (ScreenShakeAmount > 0.1f)
            {
                Main.screenPosition += Main.rand.NextVector2Circular(ScreenShakeAmount, ScreenShakeAmount);
            }
        }
    }
}
