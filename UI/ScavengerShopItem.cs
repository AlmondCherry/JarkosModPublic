namespace jarkos.ui
{
    public class ScavengerShopItem
    {
        public int ItemType { get; set; }
        public int PollenPrice { get; set; }
        public ScavengerShopItem(int type, int price)
        {
            ItemType = type;
            PollenPrice = price;
        }
    }
}
