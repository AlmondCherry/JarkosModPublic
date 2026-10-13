using System.Collections.Generic;
using jarkos.core.Data;
using Microsoft.Xna.Framework;
namespace jarkos.tiles.Puzzles
{
    public class BasicTrianglePuzzle : PuzzleForm
    {
        public override string ID => "jarkos:basic_triangle";
        public override List<PuzzleNodeData> Nodes => new List<PuzzleNodeData>        {
            new PuzzleNodeData(0, PuzzleNodeType.Start, EssenceType.Lux, new Vector2(0, -80)), new PuzzleNodeData(1, PuzzleNodeType.Normal, EssenceType.Lux, new Vector2(-60, 20)), new PuzzleNodeData(2, PuzzleNodeType.Final, EssenceType.Lux, new Vector2(60, 20))
        };
        public override List<PuzzleEdgeData> Edges => new List<PuzzleEdgeData>        {
            new PuzzleEdgeData(0, 1), new PuzzleEdgeData(1, 2), new PuzzleEdgeData(0, 2)
        };
    }
}
