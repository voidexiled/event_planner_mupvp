using System;
using System.Collections.Generic;
using event_planner_mupvp.enums;

public class Item
{
    public ItemGroups Group { get; set; }
    public int Type { get; set; }
    public SlotTypes Slot { get; set; }
    public SkillTypes Skill { get; set; }
    public int Width { get; set; }
    public int Height { get; set; }
    public bool HaveSerial { get; set; }
    public bool HaveOption { get; set; }
    public bool DropItem { get; set; }
    public string Name { get; set; } = "";
    public int Level { get; set; }

    public Item()
    {

    }
}