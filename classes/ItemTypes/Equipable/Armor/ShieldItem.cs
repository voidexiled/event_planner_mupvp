public class ShieldItem : Equipable
{
    public int Defense { get; set; }
    public int MagicDefense { get; set; }
    public int DefenseSuccessRate { get; set; }

    

    public ShieldItem()
    {
        this.Group = ItemGroups.SHIELD_ITEM;
    }
}