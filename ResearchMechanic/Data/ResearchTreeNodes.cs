using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria.Localization;
using Terraria.ModLoader;

namespace jarkos.core.Data
{
    public class BeginningNode : ConstellationEntry
    {
        public override string DisplayName => NodesLanguageSystem.BeginningNode_Name?.Value ?? "Beginning Node 1"; // "Beginning"
        public override string Description => NodesLanguageSystem.BeginningNode_Desc?.Value ?? "Beginning Desc Node 1"; // "The start of a grand journey."
        public override string IconTexture => "jarkos/textures/Essences/Terra";

        public override EssenceType Tier => EssenceType.Terra;
        public override Vector2 WorldPosition => Vector2.Zero;

        public override bool IsRoot => true;

        public override string PuzzleShapeID => $"{Mod.Name}:Puzzle_Beginning";
    }

    public abstract class BranchNode : ConstellationEntry
    {
        public abstract int Index { get; }
        public abstract float Angle { get; }
        public abstract string BranchPrefix { get; }

        public override string DisplayName => $"{BranchPrefix} Node {Index}";
        public override string Description => $"The path of {BranchPrefix}.";

        public override Vector2 WorldPosition => new Vector2((float)Math.Cos(Angle) * 300f * Index, (float)Math.Sin(Angle) * 300f * Index);

        public override List<string> Prerequisites => new List<string> 
        {
            Index == 1 ? $"{Mod.Name}:BeginningNode" : $"{Mod.Name}:{BranchPrefix}Node{Index - 1}"
        };

        public override string PuzzleShapeID => $"{Mod.Name}:Puzzle_{BranchPrefix}_{Index}";
    }
    
    public abstract class AquaNodeBase : BranchNode
    {
        public override float Angle => 0f;
        public override string BranchPrefix => "Aqua";
        public override EssenceType Tier => EssenceType.Aqua;
        public override string IconTexture => "jarkos/textures/Essences/Aqua";
    }
    public class AquaNode1 : AquaNodeBase
    {
        public override int Index => 1;
        public override string DisplayName => NodesLanguageSystem.AquaNode1_Name?.Value ?? "Aqua Node 1"; // "Spontaneous luck 1"
        public override string Description => NodesLanguageSystem.AquaNode1_Desc?.Value ?? "Aqua Node 1 Desc"; // "..."
    }
    public class AquaNode2 : AquaNodeBase
    {
        public override int Index => 2;
        public override string DisplayName => NodesLanguageSystem.AquaNode2_Name?.Value ?? "Aqua Node 2"; // "Good luck in fishing!"
        public override string Description => NodesLanguageSystem.AquaNode2_Desc?.Value ?? "Aqua Node 2"; // "..."
    }
    public class AquaNode3 : AquaNodeBase
    {
        public override int Index => 3;
        public override string DisplayName => NodesLanguageSystem.AquaNode3_Name?.Value ?? "Aqua Node 3"; // "Spontaneous luck 2"
        public override string Description => NodesLanguageSystem.AquaNode3_Desc?.Value ?? "Aqua Node 3"; // "..."
    }
    public class AquaNode4 : AquaNodeBase
    {
        public override int Index => 4;
        public override string DisplayName => NodesLanguageSystem.AquaNode4_Name?.Value ?? "Aqua Node 4"; // "Essence production"
        public override string Description => NodesLanguageSystem.AquaNode4_Desc?.Value ?? "Aqua Node 4"; // "..."
    }
    public class AquaNode5 : AquaNodeBase
    {
        public override int Index => 5;
        public override string DisplayName => NodesLanguageSystem.AquaNode5_Name?.Value ?? "Aqua Node 5"; // "The blessing"
        public override string Description => NodesLanguageSystem.AquaNode5_Desc?.Value ?? "Aqua Node 5"; // "..."
    }
    public class AquaNode6 : AquaNodeBase
    {
        public override int Index => 6;
        public override string DisplayName => NodesLanguageSystem.AquaNode6_Name?.Value ?? "Aqua Node 6"; // "Better production 1"
        public override string Description => NodesLanguageSystem.AquaNode6_Desc?.Value ?? "Aqua Node 6"; // "..."
    }
    public class AquaNode7 : AquaNodeBase
    {
        public override int Index => 7;
        public override string DisplayName => NodesLanguageSystem.AquaNode7_Name?.Value ?? "Aqua Node 7"; // "Better production 2"
        public override string Description => NodesLanguageSystem.AquaNode7_Desc?.Value ?? "Aqua Node 7"; // "..."
    }
    public class AquaNode8 : AquaNodeBase
    {
        public override int Index => 8;
        public override string DisplayName => NodesLanguageSystem.AquaNode8_Name?.Value ?? "Aqua Node 8"; // "Better production 3"
        public override string Description => NodesLanguageSystem.AquaNode8_Desc?.Value ?? "Aqua Node 8"; // "..."
    }
    public abstract class VitaNodeBase : BranchNode
    {
        public override float Angle => 72f * (float)Math.PI / 180f;
        public override string BranchPrefix => "Vita";
        public override EssenceType Tier => EssenceType.Vita;
        public override string IconTexture => "jarkos/textures/Essences/Vita";
    }
    public class VitaNode1 : VitaNodeBase
    {
        public override int Index => 1;
        public override string DisplayName => NodesLanguageSystem.VitaNode1_Name?.Value; // "Vita gain 1"
        public override string Description => NodesLanguageSystem.VitaNode1_Desc?.Value; // "..."
    }
    public class VitaNode2 : VitaNodeBase
    {
        public override int Index => 2;
        public override string DisplayName => NodesLanguageSystem.VitaNode2_Name?.Value; // "Bloody Amulet"
        public override string Description => NodesLanguageSystem.VitaNode2_Desc?.Value; // "..."
    }
    public class VitaNode3 : VitaNodeBase
    {
        public override int Index => 3;
        public override string DisplayName => NodesLanguageSystem.VitaNode3_Name?.Value; // "Bloodletting blade"
        public override string Description => NodesLanguageSystem.VitaNode3_Desc?.Value; // "..."
    }
    public class VitaNode4 : VitaNodeBase
    {
        public override int Index => 4;
        public override string DisplayName => NodesLanguageSystem.VitaNode4_Name?.Value; // "Retribution"
        public override string Description => NodesLanguageSystem.VitaNode4_Desc?.Value; // "..."
    }
    public class VitaNode5 : VitaNodeBase
    {
        public override int Index => 5;
        public override string DisplayName => NodesLanguageSystem.VitaNode5_Name?.Value; // "Biomechanical heart"
        public override string Description => NodesLanguageSystem.VitaNode5_Desc?.Value; // "..."
    }
    public class VitaNode6 : VitaNodeBase
    {
        public override int Index => 6;
        public override string DisplayName => NodesLanguageSystem.VitaNode6_Name?.Value; // "Blood Reaper"
        public override string Description => NodesLanguageSystem.VitaNode6_Desc?.Value; // "..."
    }
    public class VitaNode7 : VitaNodeBase
    {
        public override int Index => 7;
        public override string DisplayName => NodesLanguageSystem.VitaNode7_Name?.Value; // "Blood Brother Hammer"
        public override string Description => NodesLanguageSystem.VitaNode7_Desc?.Value; // "..."
    }
    public class VitaNode8 : VitaNodeBase
    {
        public override int Index => 8;
        public override string DisplayName => NodesLanguageSystem.VitaNode8_Name?.Value; // "Vitality?"
        public override string Description => NodesLanguageSystem.VitaNode8_Desc?.Value; // "..."
    }
    public abstract class VitaSub1NodeBase : ConstellationEntry
    {
        public abstract int Index { get; }
        public float Angle => (72f + 30f) * (float)Math.PI / 180f;
        public string BranchPrefix => "VitaSub1";

        public override string DisplayName => $"{BranchPrefix} Node {Index}";
        public override string Description => $"The path of {BranchPrefix}.";
        public override Vector2 WorldPosition
        {
            get
            {
                Vector2 basePos = new Vector2((float)Math.Cos(72f * Math.PI / 180f) * 300f * 1, (float)Math.Sin(72f * Math.PI / 180f) * 300f * 1);
                Vector2 offset = new Vector2((float)Math.Cos(Angle) * 300f * Index, (float)Math.Sin(Angle) * 300f * Index);
                return basePos + offset;
            }
        }
        public override List<string> Prerequisites => new List<string> 
        {
            Index == 1 ? $"{Mod.Name}:VitaNode1" : $"{Mod.Name}:{BranchPrefix}Node{Index - 1}"
        };
        public override EssenceType Tier => EssenceType.Vita;
        public override string IconTexture => "jarkos/textures/Essences/Vita";
        public override string PuzzleShapeID => $"{Mod.Name}:Puzzle_{BranchPrefix}_{Index}";
    }
    public class VitaSub1Node1 : VitaSub1NodeBase
    {
        public override int Index => 1;
        public override string DisplayName => NodesLanguageSystem.VitaSub1Node1_Name?.Value; // "Vita Sub 1"
        public override string Description => NodesLanguageSystem.VitaSub1Node1_Desc?.Value; // "..."
    }
    public class VitaSub1Node2 : VitaSub1NodeBase
    {
        public override int Index => 2;
        public override string DisplayName => NodesLanguageSystem.VitaSub1Node2_Name?.Value; // "Vita Sub 2"
        public override string Description => NodesLanguageSystem.VitaSub1Node2_Desc?.Value; // "..."
    }
    public class VitaSub1Node3 : VitaSub1NodeBase
    {
        public override int Index => 3;
        public override string DisplayName => NodesLanguageSystem.VitaSub1Node3_Name?.Value; // "Vita Sub 3"
        public override string Description => NodesLanguageSystem.VitaSub1Node3_Desc?.Value; // "..."
    }
    public class VitaSub1Node4 : VitaSub1NodeBase
    {
        public override int Index => 4;
        public override string DisplayName => NodesLanguageSystem.VitaSub1Node4_Name?.Value; // "Vita Sub 4"
        public override string Description => NodesLanguageSystem.VitaSub1Node4_Desc?.Value; // "..."
    }
    public abstract class UmbraNodeBase : BranchNode
    {
        public override float Angle => 144f * (float)Math.PI / 180f;
        public override string BranchPrefix => "Umbra";
        public override EssenceType Tier => EssenceType.Umbra;
        public override string IconTexture => "jarkos/textures/Essences/Umbra";
    }
    public class UmbraNode1 : UmbraNodeBase
    {
        public override int Index => 1;
        public override string DisplayName => NodesLanguageSystem.UmbraNode1_Name?.Value; // "Umbra gain 1"
        public override string Description => NodesLanguageSystem.UmbraNode1_Desc?.Value; // "..."
    }
    public class UmbraNode2 : UmbraNodeBase
    {
        public override int Index => 2;
        public override string DisplayName => NodesLanguageSystem.UmbraNode2_Name?.Value; // "Seal of Shadow"
        public override string Description => NodesLanguageSystem.UmbraNode2_Desc?.Value; // "..."
    }
    public class UmbraNode3 : UmbraNodeBase
    {
        public override int Index => 3;
        public override string DisplayName => NodesLanguageSystem.UmbraNode3_Name?.Value; // "Otherwordly Plea"
        public override string Description => NodesLanguageSystem.UmbraNode3_Desc?.Value; // "..."
    }
    public class UmbraNode4 : UmbraNodeBase
    {
        public override int Index => 4;
        public override string DisplayName => NodesLanguageSystem.UmbraNode4_Name?.Value; // "Umbra immune"
        public override string Description => NodesLanguageSystem.UmbraNode4_Desc?.Value; // "..."
    }
    public class UmbraNode5 : UmbraNodeBase
    {
        public override int Index => 5;
        public override string DisplayName => NodesLanguageSystem.UmbraNode5_Name?.Value; // "Thermal Scythe"
        public override string Description => NodesLanguageSystem.UmbraNode5_Desc?.Value; // "..."
    }
    public class UmbraNode6 : UmbraNodeBase
    {
        public override int Index => 6;
        public override string DisplayName => NodesLanguageSystem.UmbraNode6_Name?.Value; // "Chaos Devouver"
        public override string Description => NodesLanguageSystem.UmbraNode6_Desc?.Value; // "..."
    }
    public class UmbraNode7 : UmbraNodeBase
    {
        public override int Index => 7;
        public override string DisplayName => NodesLanguageSystem.UmbraNode7_Name?.Value; // "Smoldering Lotus"
        public override string Description => NodesLanguageSystem.UmbraNode7_Desc?.Value; // "..."
    }
    public class UmbraNode8 : UmbraNodeBase
    {
        public override int Index => 8;
        public override string DisplayName => NodesLanguageSystem.UmbraNode8_Name?.Value; // "Mutagen"
        public override string Description => NodesLanguageSystem.UmbraNode8_Desc?.Value; // "..."
    }
    public abstract class UmbraSub1NodeBase : ConstellationEntry
    {
        public abstract int Index { get; }
        public float Angle => (144f + 30f) * (float)Math.PI / 180f;
        public string BranchPrefix => "UmbraSub1";
        public override string DisplayName => $"{BranchPrefix} Node {Index}";
        public override string Description => $"The path of {BranchPrefix}.";
        public override Vector2 WorldPosition
        {
            get
            {
                Vector2 basePos = new Vector2((float)Math.Cos(144f * Math.PI / 180f) * 300f * 1, (float)Math.Sin(144f * Math.PI / 180f) * 300f * 1);
                Vector2 offset = new Vector2((float)Math.Cos(Angle) * 300f * Index, (float)Math.Sin(Angle) * 300f * Index);
                return basePos + offset;
            }
        }
        public override List<string> Prerequisites => new List<string>
        {
            Index == 1 ? $"{Mod.Name}:UmbraNode1" : $"{Mod.Name}:{BranchPrefix}Node{Index - 1}"
        };
        public override EssenceType Tier => EssenceType.Umbra;
        public override string IconTexture => "jarkos/textures/Essences/Umbra";
        public override string PuzzleShapeID => $"{Mod.Name}:Puzzle_{BranchPrefix}_{Index}";
    }
    public class UmbraSub1Node1 : UmbraSub1NodeBase
    {
        public override int Index => 1;
        public override string DisplayName => NodesLanguageSystem.UmbraSub1Node1_Name?.Value; // "Umbra Sub 1"
        public override string Description => NodesLanguageSystem.UmbraSub1Node1_Desc?.Value; // "..."
    }
    public class UmbraSub1Node2 : UmbraSub1NodeBase
    {
        public override int Index => 2;
        public override string DisplayName => NodesLanguageSystem.UmbraSub1Node2_Name?.Value; // "Umbra Sub 2"
        public override string Description => NodesLanguageSystem.UmbraSub1Node2_Desc?.Value; // "..."
    }
    public class UmbraSub1Node3 : UmbraSub1NodeBase
    {
        public override int Index => 3;
        public override string DisplayName => NodesLanguageSystem.UmbraSub1Node3_Name?.Value; // "Umbra Sub 3"
        public override string Description => NodesLanguageSystem.UmbraSub1Node3_Desc?.Value; // "..."
    }
    public abstract class VenenumNodeBase : BranchNode
    {
        public override float Angle => 216f * (float)Math.PI / 180f;
        public override string BranchPrefix => "Venenum";
        public override EssenceType Tier => EssenceType.Venenum;
        public override string IconTexture => "jarkos/textures/Essences/Venenum";
    }
    public class VenenumNode1 : VenenumNodeBase
    {
        public override int Index => 1;
        public override string DisplayName => NodesLanguageSystem.VenenumNode1_Name?.Value; // "Venenum immune 1"
        public override string Description => NodesLanguageSystem.VenenumNode1_Desc?.Value; // "..."
    }
    public class VenenumNode2 : VenenumNodeBase
    {
        public override int Index => 2;
        public override string DisplayName => NodesLanguageSystem.VenenumNode2_Name?.Value; // "Venenum immune 2"
        public override string Description => NodesLanguageSystem.VenenumNode2_Desc?.Value; // "..."
    }
    public class VenenumNode3 : VenenumNodeBase
    {
        public override int Index => 3;
        public override string DisplayName => NodesLanguageSystem.VenenumNode3_Name?.Value; // "Fragment of weakness"
        public override string Description => NodesLanguageSystem.VenenumNode3_Desc?.Value; // "..."
    }
    public class VenenumNode4 : VenenumNodeBase
    {
        public override int Index => 4;
        public override string DisplayName => NodesLanguageSystem.VenenumNode4_Name?.Value; // "Swamp infection"
        public override string Description => NodesLanguageSystem.VenenumNode4_Desc?.Value; // "..."
    }
    public class VenenumNode5 : VenenumNodeBase
    {
        public override int Index => 5;
        public override string DisplayName => NodesLanguageSystem.VenenumNode5_Name?.Value; // "Nature's God Armor"
        public override string Description => NodesLanguageSystem.VenenumNode5_Desc?.Value; // "..."
    }
    public class VenenumNode6 : VenenumNodeBase
    {
        public override int Index => 6;
        public override string DisplayName => NodesLanguageSystem.VenenumNode6_Name?.Value; // "Ring of Dysterian"
        public override string Description => NodesLanguageSystem.VenenumNode6_Desc?.Value; // "..."
    }
    public class VenenumNode7 : VenenumNodeBase
    {
        public override int Index => 7;
        public override string DisplayName => NodesLanguageSystem.VenenumNode7_Name?.Value; // "The sign of the God of Nature"
        public override string Description => NodesLanguageSystem.VenenumNode7_Desc?.Value; // "..."
    }
    public class VenenumNode8 : VenenumNodeBase
    {
        public override int Index => 8;
        public override string DisplayName => NodesLanguageSystem.VenenumNode8_Name?.Value; // "Nature singularity"
        public override string Description => NodesLanguageSystem.VenenumNode8_Desc?.Value; // "..."
    }
    public abstract class PyrosNodeBase : BranchNode
    {
        public override float Angle => 288f * (float)Math.PI / 180f;
        public override string BranchPrefix => "Pyros";
        public override EssenceType Tier => EssenceType.Pyros;
        public override string IconTexture => "jarkos/textures/Essences/Pyros";
    }
    public class PyrosNode1 : PyrosNodeBase
    {
        public override int Index => 1;
        public override string DisplayName => NodesLanguageSystem.PyrosNode1_Name?.Value; // "Pyro gain 1"
        public override string Description => NodesLanguageSystem.PyrosNode1_Desc?.Value; // "..."
    }
    public class PyrosNode2 : PyrosNodeBase
    {
        public override int Index => 2;
        public override string DisplayName => NodesLanguageSystem.PyrosNode2_Name?.Value; // "Pyro immune"
        public override string Description => NodesLanguageSystem.PyrosNode2_Desc?.Value; // "..."
    }
    public class PyrosNode3 : PyrosNodeBase
    {
        public override int Index => 3;
        public override string DisplayName => NodesLanguageSystem.PyrosNode3_Name?.Value; // "Solar chip"
        public override string Description => NodesLanguageSystem.PyrosNode3_Desc?.Value; // "..."
    }
    public class PyrosNode4 : PyrosNodeBase
    {
        public override int Index => 4;
        public override string DisplayName => NodesLanguageSystem.PyrosNode4_Name?.Value; // "Pyromelt"
        public override string Description => NodesLanguageSystem.PyrosNode4_Desc?.Value; // "..."
    }
    public class PyrosNode5 : PyrosNodeBase
    {
        public override int Index => 5;
        public override string DisplayName => NodesLanguageSystem.PyrosNode5_Name?.Value; // "Bastion armor set"
        public override string Description => NodesLanguageSystem.PyrosNode5_Desc?.Value; // "..."
    }
    public class PyrosNode6 : PyrosNodeBase
    {
        public override int Index => 6;
        public override string DisplayName => NodesLanguageSystem.PyrosNode6_Name?.Value; // "Pyro gain 2"
        public override string Description => NodesLanguageSystem.PyrosNode6_Desc?.Value; // "..."
    }
    public class PyrosNode7 : PyrosNodeBase
    {
        public override int Index => 7;
        public override string DisplayName => NodesLanguageSystem.PyrosNode7_Name?.Value; // "Pyros Node 7"
        public override string Description => NodesLanguageSystem.PyrosNode7_Desc?.Value; // "..."
    }
    public class PyrosNode8 : PyrosNodeBase
    {
        public override int Index => 8;
        public override string DisplayName => NodesLanguageSystem.PyrosNode8_Name?.Value; // "Pyros Node 8"
        public override string Description => NodesLanguageSystem.PyrosNode8_Desc?.Value; // "..."
    }
    public abstract class PyrosSub1NodeBase : ConstellationEntry
    {
        public abstract int Index { get; }
        public float Angle => (288f + 30f) * (float)Math.PI / 180f;
        public string BranchPrefix => "PyrosSub1";
        public override string DisplayName => $"{BranchPrefix} Node {Index}";
        public override string Description => $"The path of {BranchPrefix}.";
        public override Vector2 WorldPosition
        {
            get
            {
                Vector2 basePos = new Vector2((float)Math.Cos(288f * Math.PI / 180f) * 300f * 1, (float)Math.Sin(288f * Math.PI / 180f) * 300f * 1);
                Vector2 offset = new Vector2((float)Math.Cos(Angle) * 300f * Index, (float)Math.Sin(Angle) * 300f * Index);

                return basePos + offset;
            }
        }
        public override List<string> Prerequisites => new List<string> 
        {
            Index == 1 ? $"{Mod.Name}:PyrosNode1" : $"{Mod.Name}:{BranchPrefix}Node{Index - 1}"
        };
        public override EssenceType Tier => EssenceType.Pyros;
        public override string IconTexture => "jarkos/textures/Essences/Pyros";
        public override string PuzzleShapeID => $"{Mod.Name}:Puzzle_{BranchPrefix}_{Index}";
    }
    public class PyrosSub1Node1 : PyrosSub1NodeBase
    {
        public override int Index => 1;
        public override string DisplayName => NodesLanguageSystem.PyrosSub1Node1_Name?.Value; // "Pyros Sub 1"
        public override string Description => NodesLanguageSystem.PyrosSub1Node1_Desc?.Value; // "..."
    }
    public class PyrosSub1Node2 : PyrosSub1NodeBase
    {
        public override int Index => 2;
        public override string DisplayName => NodesLanguageSystem.PyrosSub1Node2_Name?.Value; // "Pyros Sub 2"
        public override string Description => NodesLanguageSystem.PyrosSub1Node2_Desc?.Value; // "..."
    }
    public class PyrosSub1Node3 : PyrosSub1NodeBase
    {
        public override int Index => 3;
        public override string DisplayName => NodesLanguageSystem.PyrosSub1Node3_Name?.Value; // "Pyros Sub 3"
        public override string Description => NodesLanguageSystem.PyrosSub1Node3_Desc?.Value; // "..."
    }
    public class PyrosSub1Node4 : PyrosSub1NodeBase
    {
        public override int Index => 4;
        public override string DisplayName => NodesLanguageSystem.PyrosSub1Node4_Name?.Value; // "Pyros Sub 4"
        public override string Description => NodesLanguageSystem.PyrosSub1Node4_Desc?.Value; // "..."
    }

    public abstract class AquaSub1NodeBase : ConstellationEntry
    {
        public abstract int Index { get; }
        public float Angle => (0f + 30f) * (float)Math.PI / 180f;
        public string BranchPrefix => "AquaSub1";
        public override string DisplayName => $"{BranchPrefix} Node {Index}";
        public override string Description => $"The path of {BranchPrefix}.";
        public override Vector2 WorldPosition
        {
            get
            {
                Vector2 basePos = new Vector2((float)Math.Cos(0f * Math.PI / 180f) * 300f * 1, (float)Math.Sin(0f * Math.PI / 180f) * 300f * 1);
                Vector2 offset = new Vector2((float)Math.Cos(Angle) * 300f * Index, (float)Math.Sin(Angle) * 300f * Index);

                return basePos + offset;
            }
        }
        public override List<string> Prerequisites => new List<string> 
        {
            Index == 1 ? $"{Mod.Name}:AquaNode1" : $"{Mod.Name}:{BranchPrefix}Node{Index - 1}"
        };
        public override EssenceType Tier => EssenceType.Aqua;
        public override string IconTexture => "jarkos/textures/Essences/Aqua";
        public override string PuzzleShapeID => $"{Mod.Name}:Puzzle_{BranchPrefix}_{Index}";
    }

    public class AquaSub1Node1 : AquaSub1NodeBase
    {
        public override int Index => 1;
        public override string DisplayName => NodesLanguageSystem.AquaSub1Node1_Name?.Value; // "Aqua Sub 1"
        public override string Description => NodesLanguageSystem.AquaSub1Node1_Desc?.Value;
    }
    public class AquaSub1Node2 : AquaSub1NodeBase
    {
        public override int Index => 2;
        public override string DisplayName => NodesLanguageSystem.AquaSub1Node2_Name?.Value; // "Aqua Sub 2"
        public override string Description => NodesLanguageSystem.AquaSub1Node2_Desc?.Value;
    }
    public class AquaSub1Node3 : AquaSub1NodeBase
    {
        public override int Index => 3;
        public override string DisplayName => NodesLanguageSystem.AquaSub1Node3_Name?.Value; // "Aqua Sub 3"
        public override string Description => NodesLanguageSystem.AquaSub1Node3_Desc?.Value;
    }

    public abstract class VenenumSub1NodeBase : ConstellationEntry
    {
        public abstract int Index { get; }
        public float Angle => (216f + 30f) * (float)Math.PI / 180f;
        public string BranchPrefix => "VenenumSub1";
        public override string DisplayName => $"{BranchPrefix} Node {Index}";
        public override string Description => $"The path of {BranchPrefix}.";
        public override Vector2 WorldPosition
        {
            get
            {
                Vector2 basePos = new Vector2((float)Math.Cos(216f * Math.PI / 180f) * 300f * 1, (float)Math.Sin(216f * Math.PI / 180f) * 300f * 1);
                Vector2 offset = new Vector2((float)Math.Cos(Angle) * 300f * Index, (float)Math.Sin(Angle) * 300f * Index);
                return basePos + offset;
            }
        }
        public override List<string> Prerequisites => new List<string> {
            Index == 1 ? $"{Mod.Name}:VenenumNode1" : $"{Mod.Name}:{BranchPrefix}Node{Index - 1}"
        };
        public override EssenceType Tier => EssenceType.Venenum;
        public override string IconTexture => "jarkos/textures/Essences/Venenum";
        public override string PuzzleShapeID => $"{Mod.Name}:Puzzle_{BranchPrefix}_{Index}";
    }

    public class VenenumSub1Node1 : VenenumSub1NodeBase
    {
        public override int Index => 1;
        public override string DisplayName => NodesLanguageSystem.VenenumSub1Node1_Name?.Value; // "Venenum Sub 1"
        public override string Description => NodesLanguageSystem.VenenumSub1Node1_Desc?.Value;
    }
    public class VenenumSub1Node2 : VenenumSub1NodeBase
    {
        public override int Index => 2;
        public override string DisplayName => NodesLanguageSystem.VenenumSub1Node2_Name?.Value; // "Venenum Sub 2"
        public override string Description => NodesLanguageSystem.VenenumSub1Node2_Desc?.Value;
    }
    public class VenenumSub1Node3 : VenenumSub1NodeBase
    {
        public override int Index => 3;
        public override string DisplayName => NodesLanguageSystem.VenenumSub1Node3_Name?.Value; // "Venenum Sub 3"
        public override string Description => NodesLanguageSystem.VenenumSub1Node3_Desc?.Value;
    }

    public class NodesLanguageSystem : ModSystem
    {
        // Beginning
        public static LocalizedText BeginningNode_Name { get; private set; }
        public static LocalizedText BeginningNode_Desc { get; private set; }
        // Aqua
        public static LocalizedText AquaNode1_Name { get; private set; }
        public static LocalizedText AquaNode1_Desc { get; private set; }
        public static LocalizedText AquaNode2_Name { get; private set; }
        public static LocalizedText AquaNode2_Desc { get; private set; }
        public static LocalizedText AquaNode3_Name { get; private set; }
        public static LocalizedText AquaNode3_Desc { get; private set; }
        public static LocalizedText AquaNode4_Name { get; private set; }
        public static LocalizedText AquaNode4_Desc { get; private set; }
        public static LocalizedText AquaNode5_Name { get; private set; }
        public static LocalizedText AquaNode5_Desc { get; private set; }
        public static LocalizedText AquaNode6_Name { get; private set; }
        public static LocalizedText AquaNode6_Desc { get; private set; }
        public static LocalizedText AquaNode7_Name { get; private set; }
        public static LocalizedText AquaNode7_Desc { get; private set; }
        public static LocalizedText AquaNode8_Name { get; private set; }
        public static LocalizedText AquaNode8_Desc { get; private set; }
        public static LocalizedText AquaSub1Node1_Name { get; private set; }
        public static LocalizedText AquaSub1Node1_Desc { get; private set; }
        public static LocalizedText AquaSub1Node2_Name { get; private set; }
        public static LocalizedText AquaSub1Node2_Desc { get; private set; }
        public static LocalizedText AquaSub1Node3_Name { get; private set; }
        public static LocalizedText AquaSub1Node3_Desc { get; private set; }
        // Vita
        public static LocalizedText VitaNode1_Name { get; private set; }
        public static LocalizedText VitaNode1_Desc { get; private set; }
        public static LocalizedText VitaNode2_Name { get; private set; }
        public static LocalizedText VitaNode2_Desc { get; private set; }
        public static LocalizedText VitaNode3_Name { get; private set; }
        public static LocalizedText VitaNode3_Desc { get; private set; }
        public static LocalizedText VitaNode4_Name { get; private set; }
        public static LocalizedText VitaNode4_Desc { get; private set; }
        public static LocalizedText VitaNode5_Name { get; private set; }
        public static LocalizedText VitaNode5_Desc { get; private set; }
        public static LocalizedText VitaNode6_Name { get; private set; }
        public static LocalizedText VitaNode6_Desc { get; private set; }
        public static LocalizedText VitaNode7_Name { get; private set; }
        public static LocalizedText VitaNode7_Desc { get; private set; }
        public static LocalizedText VitaNode8_Name { get; private set; }
        public static LocalizedText VitaNode8_Desc { get; private set; }
        public static LocalizedText VitaSub1Node1_Name { get; private set; }
        public static LocalizedText VitaSub1Node1_Desc { get; private set; }
        public static LocalizedText VitaSub1Node2_Name { get; private set; }
        public static LocalizedText VitaSub1Node2_Desc { get; private set; }
        public static LocalizedText VitaSub1Node3_Name { get; private set; }
        public static LocalizedText VitaSub1Node3_Desc { get; private set; }
        public static LocalizedText VitaSub1Node4_Name { get; private set; }
        public static LocalizedText VitaSub1Node4_Desc { get; private set; }
        // Umbra
        public static LocalizedText UmbraNode1_Name { get; private set; }
        public static LocalizedText UmbraNode1_Desc { get; private set; }
        public static LocalizedText UmbraNode2_Name { get; private set; }
        public static LocalizedText UmbraNode2_Desc { get; private set; }
        public static LocalizedText UmbraNode3_Name { get; private set; }
        public static LocalizedText UmbraNode3_Desc { get; private set; }
        public static LocalizedText UmbraNode4_Name { get; private set; }
        public static LocalizedText UmbraNode4_Desc { get; private set; }
        public static LocalizedText UmbraNode5_Name { get; private set; }
        public static LocalizedText UmbraNode5_Desc { get; private set; }
        public static LocalizedText UmbraNode6_Name { get; private set; }
        public static LocalizedText UmbraNode6_Desc { get; private set; }
        public static LocalizedText UmbraNode7_Name { get; private set; }
        public static LocalizedText UmbraNode7_Desc { get; private set; }
        public static LocalizedText UmbraNode8_Name { get; private set; }
        public static LocalizedText UmbraNode8_Desc { get; private set; }
        public static LocalizedText UmbraSub1Node1_Name { get; private set; }
        public static LocalizedText UmbraSub1Node1_Desc { get; private set; }
        public static LocalizedText UmbraSub1Node2_Name { get; private set; }
        public static LocalizedText UmbraSub1Node2_Desc { get; private set; }
        public static LocalizedText UmbraSub1Node3_Name { get; private set; }
        public static LocalizedText UmbraSub1Node3_Desc { get; private set; }
        // Venenum
        public static LocalizedText VenenumNode1_Name { get; private set; }
        public static LocalizedText VenenumNode1_Desc { get; private set; }
        public static LocalizedText VenenumNode2_Name { get; private set; }
        public static LocalizedText VenenumNode2_Desc { get; private set; }
        public static LocalizedText VenenumNode3_Name { get; private set; }
        public static LocalizedText VenenumNode3_Desc { get; private set; }
        public static LocalizedText VenenumNode4_Name { get; private set; }
        public static LocalizedText VenenumNode4_Desc { get; private set; }
        public static LocalizedText VenenumNode5_Name { get; private set; }
        public static LocalizedText VenenumNode5_Desc { get; private set; }
        public static LocalizedText VenenumNode6_Name { get; private set; }
        public static LocalizedText VenenumNode6_Desc { get; private set; }
        public static LocalizedText VenenumNode7_Name { get; private set; }
        public static LocalizedText VenenumNode7_Desc { get; private set; }
        public static LocalizedText VenenumNode8_Name { get; private set; }
        public static LocalizedText VenenumNode8_Desc { get; private set; }
        public static LocalizedText VenenumSub1Node1_Name { get; private set; }
        public static LocalizedText VenenumSub1Node1_Desc { get; private set; }
        public static LocalizedText VenenumSub1Node2_Name { get; private set; }
        public static LocalizedText VenenumSub1Node2_Desc { get; private set; }
        public static LocalizedText VenenumSub1Node3_Name { get; private set; }
        public static LocalizedText VenenumSub1Node3_Desc { get; private set; }
        // Pyros
        public static LocalizedText PyrosNode1_Name { get; private set; }
        public static LocalizedText PyrosNode1_Desc { get; private set; }
        public static LocalizedText PyrosNode2_Name { get; private set; }
        public static LocalizedText PyrosNode2_Desc { get; private set; }
        public static LocalizedText PyrosNode3_Name { get; private set; }
        public static LocalizedText PyrosNode3_Desc { get; private set; }
        public static LocalizedText PyrosNode4_Name { get; private set; }
        public static LocalizedText PyrosNode4_Desc { get; private set; }
        public static LocalizedText PyrosNode5_Name { get; private set; }
        public static LocalizedText PyrosNode5_Desc { get; private set; }
        public static LocalizedText PyrosNode6_Name { get; private set; }
        public static LocalizedText PyrosNode6_Desc { get; private set; }
        public static LocalizedText PyrosNode7_Name { get; private set; }
        public static LocalizedText PyrosNode7_Desc { get; private set; }
        public static LocalizedText PyrosNode8_Name { get; private set; }
        public static LocalizedText PyrosNode8_Desc { get; private set; }
        public static LocalizedText PyrosSub1Node1_Name { get; private set; }
        public static LocalizedText PyrosSub1Node1_Desc { get; private set; }
        public static LocalizedText PyrosSub1Node2_Name { get; private set; }
        public static LocalizedText PyrosSub1Node2_Desc { get; private set; }
        public static LocalizedText PyrosSub1Node3_Name { get; private set; }
        public static LocalizedText PyrosSub1Node3_Desc { get; private set; }
        public static LocalizedText PyrosSub1Node4_Name { get; private set; }
        public static LocalizedText PyrosSub1Node4_Desc { get; private set; }

        public override void Load()
        {
            string category = "Nodes";
            // Beginning
            BeginningNode_Name ??= Mod.GetLocalization($"{category}.BeginningNode_Name");
            BeginningNode_Desc ??= Mod.GetLocalization($"{category}.BeginningNode_Desc");
            // Aqua
            AquaNode1_Name ??= Mod.GetLocalization($"{category}.AquaNode1_Name");
            AquaNode1_Desc ??= Mod.GetLocalization($"{category}.AquaNode1_Desc");
            AquaNode2_Name ??= Mod.GetLocalization($"{category}.AquaNode2_Name");
            AquaNode2_Desc ??= Mod.GetLocalization($"{category}.AquaNode2_Desc");
            AquaNode3_Name ??= Mod.GetLocalization($"{category}.AquaNode3_Name");
            AquaNode3_Desc ??= Mod.GetLocalization($"{category}.AquaNode3_Desc");
            AquaNode4_Name ??= Mod.GetLocalization($"{category}.AquaNode4_Name");
            AquaNode4_Desc ??= Mod.GetLocalization($"{category}.AquaNode4_Desc");
            AquaNode5_Name ??= Mod.GetLocalization($"{category}.AquaNode5_Name");
            AquaNode5_Desc ??= Mod.GetLocalization($"{category}.AquaNode5_Desc");
            AquaNode6_Name ??= Mod.GetLocalization($"{category}.AquaNode6_Name");
            AquaNode6_Desc ??= Mod.GetLocalization($"{category}.AquaNode6_Desc");
            AquaNode7_Name ??= Mod.GetLocalization($"{category}.AquaNode7_Name");
            AquaNode7_Desc ??= Mod.GetLocalization($"{category}.AquaNode7_Desc");
            AquaNode8_Name ??= Mod.GetLocalization($"{category}.AquaNode8_Name");
            AquaNode8_Desc ??= Mod.GetLocalization($"{category}.AquaNode8_Desc");
            AquaSub1Node1_Name ??= Mod.GetLocalization($"{category}.AquaSub1Node1_Name");
            AquaSub1Node1_Desc ??= Mod.GetLocalization($"{category}.AquaSub1Node1_Desc");
            AquaSub1Node2_Name ??= Mod.GetLocalization($"{category}.AquaSub1Node2_Name");
            AquaSub1Node2_Desc ??= Mod.GetLocalization($"{category}.AquaSub1Node2_Desc");
            AquaSub1Node3_Name ??= Mod.GetLocalization($"{category}.AquaSub1Node3_Name");
            AquaSub1Node3_Desc ??= Mod.GetLocalization($"{category}.AquaSub1Node3_Desc");
            // Vita
            VitaNode1_Name ??= Mod.GetLocalization($"{category}.VitaNode1_Name");
            VitaNode1_Desc ??= Mod.GetLocalization($"{category}.VitaNode1_Desc");
            VitaNode2_Name ??= Mod.GetLocalization($"{category}.VitaNode2_Name");
            VitaNode2_Desc ??= Mod.GetLocalization($"{category}.VitaNode2_Desc");
            VitaNode3_Name ??= Mod.GetLocalization($"{category}.VitaNode3_Name");
            VitaNode3_Desc ??= Mod.GetLocalization($"{category}.VitaNode3_Desc");
            VitaNode4_Name ??= Mod.GetLocalization($"{category}.VitaNode4_Name");
            VitaNode4_Desc ??= Mod.GetLocalization($"{category}.VitaNode4_Desc");
            VitaNode5_Name ??= Mod.GetLocalization($"{category}.VitaNode5_Name");
            VitaNode5_Desc ??= Mod.GetLocalization($"{category}.VitaNode5_Desc");
            VitaNode6_Name ??= Mod.GetLocalization($"{category}.VitaNode6_Name");
            VitaNode6_Desc ??= Mod.GetLocalization($"{category}.VitaNode6_Desc");
            VitaNode7_Name ??= Mod.GetLocalization($"{category}.VitaNode7_Name");
            VitaNode7_Desc ??= Mod.GetLocalization($"{category}.VitaNode7_Desc");
            VitaNode8_Name ??= Mod.GetLocalization($"{category}.VitaNode8_Name");
            VitaNode8_Desc ??= Mod.GetLocalization($"{category}.VitaNode8_Desc");
            VitaSub1Node1_Name ??= Mod.GetLocalization($"{category}.VitaSub1Node1_Name");
            VitaSub1Node1_Desc ??= Mod.GetLocalization($"{category}.VitaSub1Node1_Desc");
            VitaSub1Node2_Name ??= Mod.GetLocalization($"{category}.VitaSub1Node2_Name");
            VitaSub1Node2_Desc ??= Mod.GetLocalization($"{category}.VitaSub1Node2_Desc");
            VitaSub1Node3_Name ??= Mod.GetLocalization($"{category}.VitaSub1Node3_Name");
            VitaSub1Node3_Desc ??= Mod.GetLocalization($"{category}.VitaSub1Node3_Desc");
            VitaSub1Node4_Name ??= Mod.GetLocalization($"{category}.VitaSub1Node4_Name");
            VitaSub1Node4_Desc ??= Mod.GetLocalization($"{category}.VitaSub1Node4_Desc");
            // Umbra
            UmbraNode1_Name ??= Mod.GetLocalization($"{category}.UmbraNode1_Name");
            UmbraNode1_Desc ??= Mod.GetLocalization($"{category}.UmbraNode1_Desc");
            UmbraNode2_Name ??= Mod.GetLocalization($"{category}.UmbraNode2_Name");
            UmbraNode2_Desc ??= Mod.GetLocalization($"{category}.UmbraNode2_Desc");
            UmbraNode3_Name ??= Mod.GetLocalization($"{category}.UmbraNode3_Name");
            UmbraNode3_Desc ??= Mod.GetLocalization($"{category}.UmbraNode3_Desc");
            UmbraNode4_Name ??= Mod.GetLocalization($"{category}.UmbraNode4_Name");
            UmbraNode4_Desc ??= Mod.GetLocalization($"{category}.UmbraNode4_Desc");
            UmbraNode5_Name ??= Mod.GetLocalization($"{category}.UmbraNode5_Name");
            UmbraNode5_Desc ??= Mod.GetLocalization($"{category}.UmbraNode5_Desc");
            UmbraNode6_Name ??= Mod.GetLocalization($"{category}.UmbraNode6_Name");
            UmbraNode6_Desc ??= Mod.GetLocalization($"{category}.UmbraNode6_Desc");
            UmbraNode7_Name ??= Mod.GetLocalization($"{category}.UmbraNode7_Name");
            UmbraNode7_Desc ??= Mod.GetLocalization($"{category}.UmbraNode7_Desc");
            UmbraNode8_Name ??= Mod.GetLocalization($"{category}.UmbraNode8_Name");
            UmbraNode8_Desc ??= Mod.GetLocalization($"{category}.UmbraNode8_Desc");
            UmbraSub1Node1_Name ??= Mod.GetLocalization($"{category}.UmbraSub1Node1_Name");
            UmbraSub1Node1_Desc ??= Mod.GetLocalization($"{category}.UmbraSub1Node1_Desc");
            UmbraSub1Node2_Name ??= Mod.GetLocalization($"{category}.UmbraSub1Node2_Name");
            UmbraSub1Node2_Desc ??= Mod.GetLocalization($"{category}.UmbraSub1Node2_Desc");
            UmbraSub1Node3_Name ??= Mod.GetLocalization($"{category}.UmbraSub1Node3_Name");
            UmbraSub1Node3_Desc ??= Mod.GetLocalization($"{category}.UmbraSub1Node3_Desc");
            // Venenum
            VenenumNode1_Name ??= Mod.GetLocalization($"{category}.VenenumNode1_Name");
            VenenumNode1_Desc ??= Mod.GetLocalization($"{category}.VenenumNode1_Desc");
            VenenumNode2_Name ??= Mod.GetLocalization($"{category}.VenenumNode2_Name");
            VenenumNode2_Desc ??= Mod.GetLocalization($"{category}.VenenumNode2_Desc");
            VenenumNode3_Name ??= Mod.GetLocalization($"{category}.VenenumNode3_Name");
            VenenumNode3_Desc ??= Mod.GetLocalization($"{category}.VenenumNode3_Desc");
            VenenumNode4_Name ??= Mod.GetLocalization($"{category}.VenenumNode4_Name");
            VenenumNode4_Desc ??= Mod.GetLocalization($"{category}.VenenumNode4_Desc");
            VenenumNode5_Name ??= Mod.GetLocalization($"{category}.VenenumNode5_Name");
            VenenumNode5_Desc ??= Mod.GetLocalization($"{category}.VenenumNode5_Desc");
            VenenumNode6_Name ??= Mod.GetLocalization($"{category}.VenenumNode6_Name");
            VenenumNode6_Desc ??= Mod.GetLocalization($"{category}.VenenumNode6_Desc");
            VenenumNode7_Name ??= Mod.GetLocalization($"{category}.VenenumNode7_Name");
            VenenumNode7_Desc ??= Mod.GetLocalization($"{category}.VenenumNode7_Desc");
            VenenumNode8_Name ??= Mod.GetLocalization($"{category}.VenenumNode8_Name");
            VenenumNode8_Desc ??= Mod.GetLocalization($"{category}.VenenumNode8_Desc");
            VenenumSub1Node1_Name ??= Mod.GetLocalization($"{category}.VenenumSub1Node1_Name");
            VenenumSub1Node1_Desc ??= Mod.GetLocalization($"{category}.VenenumSub1Node1_Desc");
            VenenumSub1Node2_Name ??= Mod.GetLocalization($"{category}.VenenumSub1Node2_Name");
            VenenumSub1Node2_Desc ??= Mod.GetLocalization($"{category}.VenenumSub1Node2_Desc");
            VenenumSub1Node3_Name ??= Mod.GetLocalization($"{category}.VenenumSub1Node3_Name");
            VenenumSub1Node3_Desc ??= Mod.GetLocalization($"{category}.VenenumSub1Node3_Desc");
            // Pyros
            PyrosNode1_Name ??= Mod.GetLocalization($"{category}.PyrosNode1_Name");
            PyrosNode1_Desc ??= Mod.GetLocalization($"{category}.PyrosNode1_Desc");
            PyrosNode2_Name ??= Mod.GetLocalization($"{category}.PyrosNode2_Name");
            PyrosNode2_Desc ??= Mod.GetLocalization($"{category}.PyrosNode2_Desc");
            PyrosNode3_Name ??= Mod.GetLocalization($"{category}.PyrosNode3_Name");
            PyrosNode3_Desc ??= Mod.GetLocalization($"{category}.PyrosNode3_Desc");
            PyrosNode4_Name ??= Mod.GetLocalization($"{category}.PyrosNode4_Name");
            PyrosNode4_Desc ??= Mod.GetLocalization($"{category}.PyrosNode4_Desc");
            PyrosNode5_Name ??= Mod.GetLocalization($"{category}.PyrosNode5_Name");
            PyrosNode5_Desc ??= Mod.GetLocalization($"{category}.PyrosNode5_Desc");
            PyrosNode6_Name ??= Mod.GetLocalization($"{category}.PyrosNode6_Name");
            PyrosNode6_Desc ??= Mod.GetLocalization($"{category}.PyrosNode6_Desc");
            PyrosNode7_Name ??= Mod.GetLocalization($"{category}.PyrosNode7_Name");
            PyrosNode7_Desc ??= Mod.GetLocalization($"{category}.PyrosNode7_Desc");
            PyrosNode8_Name ??= Mod.GetLocalization($"{category}.PyrosNode8_Name");
            PyrosNode8_Desc ??= Mod.GetLocalization($"{category}.PyrosNode8_Desc");
            PyrosSub1Node1_Name ??= Mod.GetLocalization($"{category}.PyrosSub1Node1_Name");
            PyrosSub1Node1_Desc ??= Mod.GetLocalization($"{category}.PyrosSub1Node1_Desc");
            PyrosSub1Node2_Name ??= Mod.GetLocalization($"{category}.PyrosSub1Node2_Name");
            PyrosSub1Node2_Desc ??= Mod.GetLocalization($"{category}.PyrosSub1Node2_Desc");
            PyrosSub1Node3_Name ??= Mod.GetLocalization($"{category}.PyrosSub1Node3_Name");
            PyrosSub1Node3_Desc ??= Mod.GetLocalization($"{category}.PyrosSub1Node3_Desc");
            PyrosSub1Node4_Name ??= Mod.GetLocalization($"{category}.PyrosSub1Node4_Name");
            PyrosSub1Node4_Desc ??= Mod.GetLocalization($"{category}.PyrosSub1Node4_Desc");
        }
    }
}
