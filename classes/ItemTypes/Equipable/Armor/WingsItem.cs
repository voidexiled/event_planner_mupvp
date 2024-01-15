public class WingsItem : Equipable
{
    public int Defense { get; set; }
    public long BuyMoney { get; set; }

    public WingsItem()
    {
        this.Group = ItemGroups.WINGS_ITEM;
    }
}