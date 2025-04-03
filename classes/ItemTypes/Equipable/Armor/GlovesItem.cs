using event_planner_mupvp.enums;

public class GlovesItem : Armor
{
    public int AttackSpeed { get; set; }
    public GlovesItem()
    {
        this.Group = ItemGroups.GLOVES_ITEM;
    }
}