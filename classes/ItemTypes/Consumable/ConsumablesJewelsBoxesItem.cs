using event_planner_mupvp.enums;

public class ConsumablesJewelsBoxesItem : Item
{
    public int Value { get; set; }

    public ConsumablesJewelsBoxesItem()
    {
        Group = ItemGroups.CONSUMABLES_JEWELS_BOXES_ITEM;
    }
}