using Microsoft.Xna.Framework;

namespace jarkos.core.Data
{
    public enum PuzzleNodeType
    {
        Start, Normal, OR, AND, Final, Trap
    }

    public class PuzzleNodeData
    {
        public int ID { get; }

        public PuzzleNodeType Type { get; }
        public EssenceType RequiredEssence { get; }
        public Vector2 LocalPosition { get; }

        public PuzzleNodeData(int id, PuzzleNodeType type, EssenceType essence, Vector2 localPos)
        {
            ID = id;
            Type = type;
            RequiredEssence = essence;
            LocalPosition = localPos;
        }
    }

    public class PuzzleEdgeData
    {
        public int FromID { get; }
        public int ToID { get; }

        public PuzzleEdgeData(int fromId, int toId)
        {
            FromID = fromId;
            ToID = toId;
        }
    }
}
