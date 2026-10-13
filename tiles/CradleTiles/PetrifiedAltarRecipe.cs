using System.Collections.Generic;
using Terraria;
namespace jarkos.tiles.CradleTiles
{
    public struct StrictIngredient
    {
        public int ItemType;
        public int Dx;
        public int Dy;
        public int Amount;
        public StrictIngredient(int itemType, int dx, int dy, int amount) {
            ItemType = itemType;
            Dx = dx;
            Dy = dy;
            Amount = amount;
        }
    }
    public class PetrifiedAltarRecipe
    {
        public List<int> LeftIngredients { get; } = new List<int>();
        public List<int> RightIngredients { get; } = new List<int>();
        public List<StrictIngredient> StrictIngredients { get; } = new List<StrictIngredient>();
        public int ResultItem { get; }
        public int ResultStack { get; }
        public PetrifiedAltarRecipe(int resultItem, int resultStack = 1) {
            ResultItem = resultItem;
            ResultStack = resultStack;
        }
        public PetrifiedAltarRecipe AddLeftIngredient(int itemType, int count = 1) {
            for (int i = 0; i < count; i++) {
                LeftIngredients.Add(itemType);
            }
            return this;
        }
        public PetrifiedAltarRecipe AddRightIngredient(int itemType, int count = 1) {
            for (int i = 0; i < count; i++) {
                RightIngredients.Add(itemType);
            }
            return this;
        }
        public PetrifiedAltarRecipe AddStrictIngredient(int itemType, int dx, int dy, int amount = 1) {
            StrictIngredients.Add(new StrictIngredient(itemType, dx, dy, amount));
            return this;
        }
        public bool TryMatch(int centerX, int centerY, List<PetrifiedMinorAltarTileEntity> allAltars, List<PetrifiedMinorAltarTileEntity> availableLeft, List<PetrifiedMinorAltarTileEntity> availableRight, out Dictionary<PetrifiedMinorAltarTileEntity, int> consumed) {
            consumed = new Dictionary<PetrifiedMinorAltarTileEntity, int>();
            List<PetrifiedMinorAltarTileEntity> poolLeft = new List<PetrifiedMinorAltarTileEntity>(availableLeft);
            List<PetrifiedMinorAltarTileEntity> poolRight = new List<PetrifiedMinorAltarTileEntity>(availableRight);
            List<PetrifiedMinorAltarTileEntity> poolAll = new List<PetrifiedMinorAltarTileEntity>(allAltars);
            foreach (var strict in StrictIngredients) {
                int targetX = centerX + strict.Dx;
                int targetY = centerY + strict.Dy;
                int index = poolAll.FindIndex(a => a.Position.X == targetX && a.Position.Y == targetY && a.HasItem && a.HeldItem.type == strict.ItemType && a.HeldItem.stack >= strict.Amount);
                if (index != -1) {
                    var altar = poolAll[index];
                    consumed[altar] = strict.Amount;
                    poolAll.RemoveAt(index);
                    poolLeft.Remove(altar);
                    poolRight.Remove(altar);
                } else {
                    return false;
                }
            }
            var matchedLeft = GetMatches(poolLeft, LeftIngredients);
            if (matchedLeft == null) return false;
            foreach(var m in matchedLeft) {
                consumed[m] = 1;
                poolLeft.Remove(m);
                poolAll.Remove(m);
            }
            var matchedRight = GetMatches(poolRight, RightIngredients);
            if (matchedRight == null) return false;
            foreach(var m in matchedRight) {
                consumed[m] = 1;
                poolRight.Remove(m);
                poolAll.Remove(m);
            }
            return true;
        }
        private List<PetrifiedMinorAltarTileEntity> GetMatches(List<PetrifiedMinorAltarTileEntity> altars, List<int> requiredIngredients) {
            if (requiredIngredients.Count == 0) {
                return new List<PetrifiedMinorAltarTileEntity>();
            }
            List<PetrifiedMinorAltarTileEntity> matched = new List<PetrifiedMinorAltarTileEntity>();
            List<PetrifiedMinorAltarTileEntity> pool = new List<PetrifiedMinorAltarTileEntity>(altars);
            foreach (int req in requiredIngredients) {
                int index = pool.FindIndex(a => a.HasItem && a.HeldItem.type == req);
                if (index != -1) {
                    matched.Add(pool[index]);
                    pool.RemoveAt(index);
                } else {
                    return null;
                }
            }
            return matched;
        }
    }
}
