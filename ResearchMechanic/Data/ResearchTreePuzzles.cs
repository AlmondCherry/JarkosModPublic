using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace jarkos.core.Data
{
    public abstract class BaseProceduralPuzzle : PuzzleForm
    {
        public abstract int Level { get; }
        public abstract EssenceType MainEssence { get; }
        public virtual EssenceType[] SecondaryEssences => null;

        public override List<PuzzleNodeData> Nodes => PuzzleShapeBuilder.BuildNodes(Level, MainEssence, SecondaryEssences);
        public override List<PuzzleEdgeData> Edges => PuzzleShapeBuilder.BuildEdges(Level);
    }

    public static class PuzzleShapeBuilder
    {
        public static List<PuzzleNodeData> BuildNodes(int level, EssenceType main, EssenceType[] secondary = null)
        {
            var nodes = new List<PuzzleNodeData>();
            nodes.Add(new PuzzleNodeData(0, PuzzleNodeType.Start, main, new Vector2(0, -100)));

            EssenceType alt1 = GetSecondary(main, secondary, 0);
            EssenceType alt2 = GetSecondary(main, secondary, 1);
            EssenceType alt3 = GetSecondary(main, secondary, 2);
            EssenceType alt4 = GetSecondary(main, secondary, 3);

            switch (level)
            {
                case 1:
                    nodes.Add(new PuzzleNodeData(1, PuzzleNodeType.Normal, main, new Vector2(-60, 40)));
                    nodes.Add(new PuzzleNodeData(2, PuzzleNodeType.Final, main, new Vector2(60, 40)));
                    break;
                case 2:
                    nodes.Add(new PuzzleNodeData(1, PuzzleNodeType.Normal, main, new Vector2(-80, 0)));
                    nodes.Add(new PuzzleNodeData(2, PuzzleNodeType.Normal, alt1, new Vector2(80, 0)));
                    nodes.Add(new PuzzleNodeData(3, PuzzleNodeType.Final, main, new Vector2(0, 100)));
                    break;
                case 3:
                    nodes.Add(new PuzzleNodeData(1, PuzzleNodeType.Normal, main, new Vector2(-100, -20)));
                    nodes.Add(new PuzzleNodeData(2, PuzzleNodeType.Normal, alt1, new Vector2(100, -20)));
                    nodes.Add(new PuzzleNodeData(3, PuzzleNodeType.Normal, alt2, new Vector2(-60, 80)));
                    nodes.Add(new PuzzleNodeData(4, PuzzleNodeType.Final, main, new Vector2(60, 80)));
                    break;
                case 4:
                    nodes.Add(new PuzzleNodeData(1, PuzzleNodeType.Normal, main, new Vector2(-100, -50)));
                    nodes.Add(new PuzzleNodeData(2, PuzzleNodeType.Normal, alt1, new Vector2(100, -50)));
                    nodes.Add(new PuzzleNodeData(3, PuzzleNodeType.Normal, alt2, new Vector2(-100, 50)));
                    nodes.Add(new PuzzleNodeData(4, PuzzleNodeType.Normal, alt3, new Vector2(100, 50)));
                    nodes.Add(new PuzzleNodeData(5, PuzzleNodeType.Final, alt1, new Vector2(0, 100)));
                    break;
                case 5:
                    nodes.Add(new PuzzleNodeData(1, PuzzleNodeType.Normal, main, new Vector2(-120, -40)));
                    nodes.Add(new PuzzleNodeData(2, PuzzleNodeType.Normal, alt1, new Vector2(120, -40)));
                    nodes.Add(new PuzzleNodeData(3, PuzzleNodeType.Normal, alt2, new Vector2(-150, 60)));
                    nodes.Add(new PuzzleNodeData(4, PuzzleNodeType.Normal, alt3, new Vector2(150, 60)));
                    nodes.Add(new PuzzleNodeData(5, PuzzleNodeType.Normal, alt4, new Vector2(-60, 120)));
                    nodes.Add(new PuzzleNodeData(6, PuzzleNodeType.Final, alt1, new Vector2(60, 120)));
                    break;
                case 6:
                    nodes.Add(new PuzzleNodeData(1, PuzzleNodeType.Normal, main, new Vector2(-70, -30)));
                    nodes.Add(new PuzzleNodeData(2, PuzzleNodeType.Normal, alt1, new Vector2(70, -30)));
                    nodes.Add(new PuzzleNodeData(3, PuzzleNodeType.Normal, alt2, new Vector2(-140, 30)));
                    nodes.Add(new PuzzleNodeData(4, PuzzleNodeType.Normal, alt3, new Vector2(0, 30)));
                    nodes.Add(new PuzzleNodeData(5, PuzzleNodeType.Normal, alt4, new Vector2(140, 30)));
                    nodes.Add(new PuzzleNodeData(6, PuzzleNodeType.Normal, alt1, new Vector2(-70, 90)));
                    nodes.Add(new PuzzleNodeData(7, PuzzleNodeType.Final, main, new Vector2(70, 90)));
                    break;
                case 7:
                    nodes.Add(new PuzzleNodeData(1, PuzzleNodeType.Normal, alt1, new Vector2(-80, -20)));
                    nodes.Add(new PuzzleNodeData(2, PuzzleNodeType.Normal, main, new Vector2(80, -20)));
                    nodes.Add(new PuzzleNodeData(3, PuzzleNodeType.Normal, alt2, new Vector2(-160, 50)));
                    nodes.Add(new PuzzleNodeData(4, PuzzleNodeType.Normal, alt3, new Vector2(-40, 50)));
                    nodes.Add(new PuzzleNodeData(5, PuzzleNodeType.Normal, alt4, new Vector2(40, 50)));
                    nodes.Add(new PuzzleNodeData(6, PuzzleNodeType.Normal, main, new Vector2(160, 50)));
                    nodes.Add(new PuzzleNodeData(7, PuzzleNodeType.Normal, alt2, new Vector2(-80, 120)));
                    nodes.Add(new PuzzleNodeData(8, PuzzleNodeType.Final, main, new Vector2(80, 120)));
                    break;
                case 8:
                    nodes.Add(new PuzzleNodeData(1, PuzzleNodeType.Normal, alt1, new Vector2(-60, -40)));
                    nodes.Add(new PuzzleNodeData(2, PuzzleNodeType.Normal, main, new Vector2(60, -40)));
                    nodes.Add(new PuzzleNodeData(3, PuzzleNodeType.Normal, alt2, new Vector2(-120, 20)));
                    nodes.Add(new PuzzleNodeData(4, PuzzleNodeType.Normal, alt3, new Vector2(120, 20)));
                    nodes.Add(new PuzzleNodeData(5, PuzzleNodeType.Normal, alt4, new Vector2(-180, 80)));
                    nodes.Add(new PuzzleNodeData(6, PuzzleNodeType.Normal, main, new Vector2(-60, 80)));
                    nodes.Add(new PuzzleNodeData(7, PuzzleNodeType.Normal, alt1, new Vector2(60, 80)));
                    nodes.Add(new PuzzleNodeData(8, PuzzleNodeType.Normal, alt2, new Vector2(180, 80)));
                    nodes.Add(new PuzzleNodeData(9, PuzzleNodeType.Normal, alt3, new Vector2(-120, 140)));
                    nodes.Add(new PuzzleNodeData(10, PuzzleNodeType.Normal, alt4, new Vector2(120, 140)));
                    nodes.Add(new PuzzleNodeData(11, PuzzleNodeType.Final, main, new Vector2(0, 180)));
                    break;
                default:
                    nodes.Add(new PuzzleNodeData(1, PuzzleNodeType.Final, main, new Vector2(0, 50)));
                    break;
            }
            return nodes;
        }

        public static List<PuzzleEdgeData> BuildEdges(int level)
        {
            var edges = new List<PuzzleEdgeData>();
            switch (level)
            {
                case 1:
                    edges.Add(new PuzzleEdgeData(0, 1));
                    edges.Add(new PuzzleEdgeData(0, 2));
                    edges.Add(new PuzzleEdgeData(1, 2));
                    break;
                case 2:
                    edges.Add(new PuzzleEdgeData(0, 1));
                    edges.Add(new PuzzleEdgeData(0, 2));
                    edges.Add(new PuzzleEdgeData(1, 3));
                    edges.Add(new PuzzleEdgeData(2, 3));
                    break;
                case 3:
                    edges.Add(new PuzzleEdgeData(0, 1));
                    edges.Add(new PuzzleEdgeData(0, 2));
                    edges.Add(new PuzzleEdgeData(1, 3));
                    edges.Add(new PuzzleEdgeData(2, 4));
                    edges.Add(new PuzzleEdgeData(3, 4));
                    break;
                case 4:
                    edges.Add(new PuzzleEdgeData(0, 1));
                    edges.Add(new PuzzleEdgeData(0, 2));
                    edges.Add(new PuzzleEdgeData(1, 3));
                    edges.Add(new PuzzleEdgeData(2, 4));
                    edges.Add(new PuzzleEdgeData(3, 5));
                    edges.Add(new PuzzleEdgeData(4, 5));
                    break;
                case 5:
                    edges.Add(new PuzzleEdgeData(0, 1));
                    edges.Add(new PuzzleEdgeData(0, 2));
                    edges.Add(new PuzzleEdgeData(1, 3));
                    edges.Add(new PuzzleEdgeData(1, 4));
                    edges.Add(new PuzzleEdgeData(2, 4));
                    edges.Add(new PuzzleEdgeData(3, 5));
                    edges.Add(new PuzzleEdgeData(4, 6));
                    edges.Add(new PuzzleEdgeData(5, 6));
                    break;
                case 6:
                    edges.Add(new PuzzleEdgeData(0, 1));
                    edges.Add(new PuzzleEdgeData(0, 2));
                    edges.Add(new PuzzleEdgeData(1, 4));
                    edges.Add(new PuzzleEdgeData(2, 5));
                    edges.Add(new PuzzleEdgeData(1, 3));
                    edges.Add(new PuzzleEdgeData(2, 3));
                    edges.Add(new PuzzleEdgeData(4, 3));
                    edges.Add(new PuzzleEdgeData(5, 3));
                    edges.Add(new PuzzleEdgeData(3, 6));
                    edges.Add(new PuzzleEdgeData(3, 7));
                    edges.Add(new PuzzleEdgeData(6, 7));
                    break;
                case 7:
                    edges.Add(new PuzzleEdgeData(0, 1));
                    edges.Add(new PuzzleEdgeData(0, 2));
                    edges.Add(new PuzzleEdgeData(1, 3));
                    edges.Add(new PuzzleEdgeData(1, 4));
                    edges.Add(new PuzzleEdgeData(2, 5));
                    edges.Add(new PuzzleEdgeData(2, 6));
                    edges.Add(new PuzzleEdgeData(4, 7));
                    edges.Add(new PuzzleEdgeData(5, 8));
                    edges.Add(new PuzzleEdgeData(7, 8));
                    break;
                case 8:
                    edges.Add(new PuzzleEdgeData(0, 1));
                    edges.Add(new PuzzleEdgeData(0, 2));
                    edges.Add(new PuzzleEdgeData(1, 3));
                    edges.Add(new PuzzleEdgeData(2, 4));
                    edges.Add(new PuzzleEdgeData(3, 5));
                    edges.Add(new PuzzleEdgeData(4, 6));
                    edges.Add(new PuzzleEdgeData(5, 7));
                    edges.Add(new PuzzleEdgeData(6, 8));
                    edges.Add(new PuzzleEdgeData(7, 9));
                    edges.Add(new PuzzleEdgeData(8, 10));
                    edges.Add(new PuzzleEdgeData(9, 11));
                    edges.Add(new PuzzleEdgeData(10, 11));
                    edges.Add(new PuzzleEdgeData(2, 3));
                    edges.Add(new PuzzleEdgeData(4, 5));
                    edges.Add(new PuzzleEdgeData(7, 8));
                    break;
                default:
                    edges.Add(new PuzzleEdgeData(0, 1));
                    break;
            }
            return edges;
        }

        private static EssenceType GetSecondary(EssenceType main, EssenceType[] secondary, int index)
        {
            if (secondary != null && index >= 0 && index < secondary.Length)
            {
                return secondary[index];
            }
            int totalEssences = Enum.GetValues(typeof(EssenceType)).Length;
            return (EssenceType)(((int)main + (index + 1) * 3) % totalEssences);
        }
    }

    public class Puzzle_Beginning : BaseProceduralPuzzle { public override int Level => 1; public override EssenceType MainEssence => EssenceType.Terra; }
    public class Puzzle_Aqua_1 : BaseProceduralPuzzle { public override int Level => 1; public override EssenceType MainEssence => EssenceType.Aqua; 
     
    public override EssenceType[] SecondaryEssences => new[] { EssenceType.Aer, EssenceType.Anima, EssenceType.Pyros, EssenceType.Caro }; }
    public class Puzzle_Aqua_2 : BaseProceduralPuzzle { public override int Level => 2; public override EssenceType MainEssence => EssenceType.Aqua; 
     
    public override EssenceType[] SecondaryEssences => new[] { EssenceType.Aer, EssenceType.Anima, EssenceType.Pyros, EssenceType.Caro }; }
    public class Puzzle_Aqua_3 : BaseProceduralPuzzle { public override int Level => 3; public override EssenceType MainEssence => EssenceType.Aqua; 
     
    public override EssenceType[] SecondaryEssences => new[] { EssenceType.Aer, EssenceType.Anima, EssenceType.Pyros, EssenceType.Caro }; }
    public class Puzzle_Aqua_4 : BaseProceduralPuzzle { public override int Level => 4; public override EssenceType MainEssence => EssenceType.Aqua; 
    public override EssenceType[] SecondaryEssences => new[] { EssenceType.Aer, EssenceType.Anima, EssenceType.Pyros, EssenceType.Caro }; }
    public class Puzzle_Aqua_5 : BaseProceduralPuzzle { public override int Level => 5; public override EssenceType MainEssence => EssenceType.Aqua; 
     
    public override EssenceType[] SecondaryEssences => new[] { EssenceType.Aer, EssenceType.Anima, EssenceType.Pyros, EssenceType.Caro }; }
    public class Puzzle_Aqua_6 : BaseProceduralPuzzle { public override int Level => 6; public override EssenceType MainEssence => EssenceType.Aqua; 
     
    public override EssenceType[] SecondaryEssences => new[] { EssenceType.Aer, EssenceType.Anima, EssenceType.Pyros, EssenceType.Caro }; }
    public class Puzzle_Aqua_7 : BaseProceduralPuzzle { public override int Level => 7; public override EssenceType MainEssence => EssenceType.Aqua; 
     
    public override EssenceType[] SecondaryEssences => new[] { EssenceType.Aer, EssenceType.Anima, EssenceType.Pyros, EssenceType.Caro }; }
    public class Puzzle_Aqua_8 : BaseProceduralPuzzle { public override int Level => 8; public override EssenceType MainEssence => EssenceType.Aqua; 
     
    public override EssenceType[] SecondaryEssences => new[] { EssenceType.Aer, EssenceType.Anima, EssenceType.Pyros, EssenceType.Caro }; }
    public class Puzzle_Vita_1 : BaseProceduralPuzzle { public override int Level => 1; public override EssenceType MainEssence => EssenceType.Vita; 
     
    public override EssenceType[] SecondaryEssences => new[] { EssenceType.Aer, EssenceType.Anima, EssenceType.Pyros, EssenceType.Caro }; }
    public class Puzzle_Vita_2 : BaseProceduralPuzzle { public override int Level => 2; public override EssenceType MainEssence => EssenceType.Vita; 
     
    public override EssenceType[] SecondaryEssences => new[] { EssenceType.Aer, EssenceType.Anima, EssenceType.Pyros, EssenceType.Caro }; }
    public class Puzzle_Vita_3 : BaseProceduralPuzzle { public override int Level => 3; public override EssenceType MainEssence => EssenceType.Vita; 
     
    public override EssenceType[] SecondaryEssences => new[] { EssenceType.Aer, EssenceType.Anima, EssenceType.Pyros, EssenceType.Caro }; }
    public class Puzzle_Vita_4 : BaseProceduralPuzzle { public override int Level => 4; public override EssenceType MainEssence => EssenceType.Vita; 
     
    public override EssenceType[] SecondaryEssences => new[] { EssenceType.Aer, EssenceType.Anima, EssenceType.Pyros, EssenceType.Caro }; }
    public class Puzzle_Vita_5 : BaseProceduralPuzzle { public override int Level => 5; public override EssenceType MainEssence => EssenceType.Vita; 
     
    public override EssenceType[] SecondaryEssences => new[] { EssenceType.Aer, EssenceType.Anima, EssenceType.Pyros, EssenceType.Caro }; }
    public class Puzzle_Vita_6 : BaseProceduralPuzzle { public override int Level => 6; public override EssenceType MainEssence => EssenceType.Vita; 
     
    public override EssenceType[] SecondaryEssences => new[] { EssenceType.Aer, EssenceType.Anima, EssenceType.Pyros, EssenceType.Caro }; }
    public class Puzzle_Vita_7 : BaseProceduralPuzzle { public override int Level => 7; public override EssenceType MainEssence => EssenceType.Vita; 
     
    public override EssenceType[] SecondaryEssences => new[] { EssenceType.Aer, EssenceType.Anima, EssenceType.Pyros, EssenceType.Caro }; }
    public class Puzzle_Vita_8 : BaseProceduralPuzzle { public override int Level => 8; public override EssenceType MainEssence => EssenceType.Vita; 
     
    public override EssenceType[] SecondaryEssences => new[] { EssenceType.Aer, EssenceType.Anima, EssenceType.Pyros, EssenceType.Caro }; }
    public class Puzzle_Umbra_1 : BaseProceduralPuzzle { public override int Level => 1; public override EssenceType MainEssence => EssenceType.Umbra; 
     
    public override EssenceType[] SecondaryEssences => new[] { EssenceType.Aer, EssenceType.Anima, EssenceType.Pyros, EssenceType.Caro }; }
    public class Puzzle_Umbra_2 : BaseProceduralPuzzle { public override int Level => 2; public override EssenceType MainEssence => EssenceType.Umbra; 
     
    public override EssenceType[] SecondaryEssences => new[] { EssenceType.Aer, EssenceType.Anima, EssenceType.Pyros, EssenceType.Caro }; }
    public class Puzzle_Umbra_3 : BaseProceduralPuzzle { public override int Level => 3; public override EssenceType MainEssence => EssenceType.Umbra; 
     
    public override EssenceType[] SecondaryEssences => new[] { EssenceType.Aer, EssenceType.Anima, EssenceType.Pyros, EssenceType.Caro }; }
    public class Puzzle_Umbra_4 : BaseProceduralPuzzle { public override int Level => 4; public override EssenceType MainEssence => EssenceType.Umbra; 
     
    public override EssenceType[] SecondaryEssences => new[] { EssenceType.Aer, EssenceType.Anima, EssenceType.Pyros, EssenceType.Caro }; }
    public class Puzzle_Umbra_5 : BaseProceduralPuzzle { public override int Level => 5; public override EssenceType MainEssence => EssenceType.Umbra; 
     
    public override EssenceType[] SecondaryEssences => new[] { EssenceType.Aer, EssenceType.Anima, EssenceType.Pyros, EssenceType.Caro }; }
    public class Puzzle_Umbra_6 : BaseProceduralPuzzle { public override int Level => 6; public override EssenceType MainEssence => EssenceType.Umbra; 
     
    public override EssenceType[] SecondaryEssences => new[] { EssenceType.Aer, EssenceType.Anima, EssenceType.Pyros, EssenceType.Caro }; }
    public class Puzzle_Umbra_7 : BaseProceduralPuzzle { public override int Level => 7; public override EssenceType MainEssence => EssenceType.Umbra; 
     
    public override EssenceType[] SecondaryEssences => new[] { EssenceType.Aer, EssenceType.Anima, EssenceType.Pyros, EssenceType.Caro }; }
    public class Puzzle_Umbra_8 : BaseProceduralPuzzle { public override int Level => 8; public override EssenceType MainEssence => EssenceType.Umbra; 
     
    public override EssenceType[] SecondaryEssences => new[] { EssenceType.Aer, EssenceType.Anima, EssenceType.Pyros, EssenceType.Caro }; }
    public class Puzzle_Venenum_1 : BaseProceduralPuzzle { public override int Level => 1; public override EssenceType MainEssence => EssenceType.Venenum; 
     
    public override EssenceType[] SecondaryEssences => new[] { EssenceType.Aer, EssenceType.Anima, EssenceType.Pyros, EssenceType.Caro }; }
    public class Puzzle_Venenum_2 : BaseProceduralPuzzle { public override int Level => 2; public override EssenceType MainEssence => EssenceType.Venenum; 
     
    public override EssenceType[] SecondaryEssences => new[] { EssenceType.Aer, EssenceType.Anima, EssenceType.Pyros, EssenceType.Caro }; }
    public class Puzzle_Venenum_3 : BaseProceduralPuzzle { public override int Level => 3; public override EssenceType MainEssence => EssenceType.Venenum; 
     
    public override EssenceType[] SecondaryEssences => new[] { EssenceType.Aer, EssenceType.Anima, EssenceType.Pyros, EssenceType.Caro }; }
    public class Puzzle_Venenum_4 : BaseProceduralPuzzle { public override int Level => 4; public override EssenceType MainEssence => EssenceType.Venenum; 
     
    public override EssenceType[] SecondaryEssences => new[] { EssenceType.Aer, EssenceType.Anima, EssenceType.Pyros, EssenceType.Caro }; }
    public class Puzzle_Venenum_5 : BaseProceduralPuzzle { public override int Level => 5; public override EssenceType MainEssence => EssenceType.Venenum; 
     
    public override EssenceType[] SecondaryEssences => new[] { EssenceType.Aer, EssenceType.Anima, EssenceType.Pyros, EssenceType.Caro }; }
    public class Puzzle_Venenum_6 : BaseProceduralPuzzle { public override int Level => 6; public override EssenceType MainEssence => EssenceType.Venenum; 
     
    public override EssenceType[] SecondaryEssences => new[] { EssenceType.Aer, EssenceType.Anima, EssenceType.Pyros, EssenceType.Caro }; 
    }
    public class Puzzle_Venenum_7 : BaseProceduralPuzzle { public override int Level => 7; public override EssenceType MainEssence => EssenceType.Venenum; 
     
    public override EssenceType[] SecondaryEssences => new[] { EssenceType.Aer, EssenceType.Anima, EssenceType.Pyros, EssenceType.Caro }; }
    public class Puzzle_Venenum_8 : BaseProceduralPuzzle { public override int Level => 8; public override EssenceType MainEssence => EssenceType.Venenum; 
     
    public override EssenceType[] SecondaryEssences => new[] { EssenceType.Aer, EssenceType.Anima, EssenceType.Pyros, EssenceType.Caro }; }
    public class Puzzle_Pyros_1 : BaseProceduralPuzzle { public override int Level => 1; public override EssenceType MainEssence => EssenceType.Pyros; 
     
    public override EssenceType[] SecondaryEssences => new[] { EssenceType.Aer, EssenceType.Anima, EssenceType.Pyros, EssenceType.Caro }; }
    public class Puzzle_Pyros_2 : BaseProceduralPuzzle { public override int Level => 2; public override EssenceType MainEssence => EssenceType.Pyros; 
     
    public override EssenceType[] SecondaryEssences => new[] { EssenceType.Aer, EssenceType.Anima, EssenceType.Pyros, EssenceType.Caro }; }
    public class Puzzle_Pyros_3 : BaseProceduralPuzzle { public override int Level => 3; public override EssenceType MainEssence => EssenceType.Pyros; 
     
    public override EssenceType[] SecondaryEssences => new[] { EssenceType.Aer, EssenceType.Anima, EssenceType.Pyros, EssenceType.Caro }; }
    public class Puzzle_Pyros_4 : BaseProceduralPuzzle { public override int Level => 4; public override EssenceType MainEssence => EssenceType.Pyros; 
     
    public override EssenceType[] SecondaryEssences => new[] { EssenceType.Aer, EssenceType.Anima, EssenceType.Pyros, EssenceType.Caro }; }
    public class Puzzle_Pyros_5 : BaseProceduralPuzzle { public override int Level => 5; public override EssenceType MainEssence => EssenceType.Pyros; 
     
    public override EssenceType[] SecondaryEssences => new[] { EssenceType.Aer, EssenceType.Anima, EssenceType.Pyros, EssenceType.Caro }; }
    public class Puzzle_Pyros_6 : BaseProceduralPuzzle { public override int Level => 6; public override EssenceType MainEssence => EssenceType.Pyros; 
     
    public override EssenceType[] SecondaryEssences => new[] { EssenceType.Aer, EssenceType.Anima, EssenceType.Pyros, EssenceType.Caro }; }
    public class Puzzle_Pyros_7 : BaseProceduralPuzzle { public override int Level => 7; public override EssenceType MainEssence => EssenceType.Pyros; 
     
    public override EssenceType[] SecondaryEssences => new[] { EssenceType.Aer, EssenceType.Anima, EssenceType.Pyros, EssenceType.Caro }; }
    public class Puzzle_Pyros_8 : BaseProceduralPuzzle { public override int Level => 8; public override EssenceType MainEssence => EssenceType.Pyros; 
     
    public override EssenceType[] SecondaryEssences => new[] { EssenceType.Aer, EssenceType.Anima, EssenceType.Pyros, EssenceType.Caro }; }
    public class Puzzle_AquaSub1_1 : BaseProceduralPuzzle { public override int Level => 1; public override EssenceType MainEssence => EssenceType.Aqua; public override EssenceType[] SecondaryEssences => new[] { EssenceType.Anima, EssenceType.Caro, EssenceType.Mechanus, EssenceType.Chitinus }; }
    public class Puzzle_AquaSub1_2 : BaseProceduralPuzzle { public override int Level => 2; public override EssenceType MainEssence => EssenceType.Aqua; public override EssenceType[] SecondaryEssences => new[] { EssenceType.Anima, EssenceType.Caro, EssenceType.Mechanus, EssenceType.Chitinus }; }
    public class Puzzle_AquaSub1_3 : BaseProceduralPuzzle { public override int Level => 3; public override EssenceType MainEssence => EssenceType.Aqua; public override EssenceType[] SecondaryEssences => new[] { EssenceType.Anima, EssenceType.Caro, EssenceType.Mechanus, EssenceType.Chitinus }; }
    public class Puzzle_VenenumSub1_1 : BaseProceduralPuzzle { public override int Level => 1; public override EssenceType MainEssence => EssenceType.Venenum; public override EssenceType[] SecondaryEssences => new[] { EssenceType.Anima, EssenceType.Caro, EssenceType.Mechanus, EssenceType.Chitinus }; }
    public class Puzzle_VenenumSub1_2 : BaseProceduralPuzzle { public override int Level => 2; public override EssenceType MainEssence => EssenceType.Venenum; public override EssenceType[] SecondaryEssences => new[] { EssenceType.Anima, EssenceType.Caro, EssenceType.Mechanus, EssenceType.Chitinus }; }
    public class Puzzle_VenenumSub1_3 : BaseProceduralPuzzle { public override int Level => 3; public override EssenceType MainEssence => EssenceType.Venenum; public override EssenceType[] SecondaryEssences => new[] { EssenceType.Anima, EssenceType.Caro, EssenceType.Mechanus, EssenceType.Chitinus }; }
    public class Puzzle_VitaSub1_1 : BaseProceduralPuzzle { public override int Level => 1; public override EssenceType MainEssence => EssenceType.Vita; public override EssenceType[] SecondaryEssences => new[] { EssenceType.Anima, EssenceType.Caro, EssenceType.Mechanus, EssenceType.Chitinus }; }
    public class Puzzle_VitaSub1_2 : BaseProceduralPuzzle { public override int Level => 2; public override EssenceType MainEssence => EssenceType.Vita; public override EssenceType[] SecondaryEssences => new[] { EssenceType.Anima, EssenceType.Caro, EssenceType.Mechanus, EssenceType.Chitinus }; }
    public class Puzzle_VitaSub1_3 : BaseProceduralPuzzle { public override int Level => 3; public override EssenceType MainEssence => EssenceType.Vita; public override EssenceType[] SecondaryEssences => new[] { EssenceType.Anima, EssenceType.Caro, EssenceType.Mechanus, EssenceType.Chitinus }; }
    public class Puzzle_VitaSub1_4 : BaseProceduralPuzzle { public override int Level => 4; public override EssenceType MainEssence => EssenceType.Vita; public override EssenceType[] SecondaryEssences => new[] { EssenceType.Anima, EssenceType.Caro, EssenceType.Mechanus, EssenceType.Chitinus }; }
    public class Puzzle_UmbraSub1_1 : BaseProceduralPuzzle { public override int Level => 1; public override EssenceType MainEssence => EssenceType.Umbra; public override EssenceType[] SecondaryEssences => new[] { EssenceType.Anima, EssenceType.Caro, EssenceType.Mechanus, EssenceType.Chitinus }; }
    public class Puzzle_UmbraSub1_2 : BaseProceduralPuzzle { public override int Level => 2; public override EssenceType MainEssence => EssenceType.Umbra; public override EssenceType[] SecondaryEssences => new[] { EssenceType.Anima, EssenceType.Caro, EssenceType.Mechanus, EssenceType.Chitinus }; }
    public class Puzzle_UmbraSub1_3 : BaseProceduralPuzzle { public override int Level => 3; public override EssenceType MainEssence => EssenceType.Umbra; public override EssenceType[] SecondaryEssences => new[] { EssenceType.Anima, EssenceType.Caro, EssenceType.Mechanus, EssenceType.Chitinus }; }
    public class Puzzle_PyrosSub1_1 : BaseProceduralPuzzle { public override int Level => 1; public override EssenceType MainEssence => EssenceType.Pyros; public override EssenceType[] SecondaryEssences => new[] { EssenceType.Anima, EssenceType.Caro, EssenceType.Mechanus, EssenceType.Chitinus }; }
    public class Puzzle_PyrosSub1_2 : BaseProceduralPuzzle { public override int Level => 2; public override EssenceType MainEssence => EssenceType.Pyros; public override EssenceType[] SecondaryEssences => new[] { EssenceType.Anima, EssenceType.Caro, EssenceType.Mechanus, EssenceType.Chitinus }; }
    public class Puzzle_PyrosSub1_3 : BaseProceduralPuzzle { public override int Level => 3; public override EssenceType MainEssence => EssenceType.Pyros; public override EssenceType[] SecondaryEssences => new[] { EssenceType.Anima, EssenceType.Caro, EssenceType.Mechanus, EssenceType.Chitinus }; }
    public class Puzzle_PyrosSub1_4 : BaseProceduralPuzzle { public override int Level => 4; public override EssenceType MainEssence => EssenceType.Pyros; public override EssenceType[] SecondaryEssences => new[] { EssenceType.Anima, EssenceType.Caro, EssenceType.Mechanus, EssenceType.Chitinus }; }
}
