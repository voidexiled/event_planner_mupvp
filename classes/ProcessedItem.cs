public class ProcessedItem
{

    public string Name { get; set; } = "";
    public SlotTypes Slot { get; set; }

    public ItemGroups Group { get; set; }
    public int Type { get; set; }
    public int PlusLevel { get; set; } = 0;
    public bool Skill { get; set; } = false;
    public bool Luck { get; set; } = false;
    public int Option { get; set; } = 0;
    public List<Option> OptionsList { get; set; }

    public int OptionTotal { get; set; }
    public int Time { get; set; } = 1;

    public ArmorSet? Set { get; set; }

    public ProcessedItem()
    {
        OptionsList = new List<Option>();
    }
    public ProcessedItem(ItemGroups group, int type, string name, SlotTypes slot)
    {
        Group = group;
        Type = type;
        Name = name;
        Slot = slot;
        OptionsList = new List<Option>();
    }
    public ProcessedItem(ItemGroups group, int type, string name, SlotTypes slot, int plusLevel, bool skill, bool luck, int option)
    {
        Group = group;
        Type = type;
        Name = name;
        Slot = slot;
        PlusLevel = plusLevel;
        Skill = skill;
        Luck = luck;
        Option = option;
        OptionsList = new List<Option>();

    }

    public ProcessedItem(int setType, string name)
    {
        Set = new(setType, name); // set constructor
        Name = name;
        OptionsList = new List<Option>();
    }


    public ProcessedItem(int setType, string name, int plusLevel, bool skill, bool luck, int option)
    {
        Set = new(setType, name); // set constructor
        Type = setType;
        Name = name;
        PlusLevel = plusLevel;
        Skill = skill;
        Luck = luck;
        Option = option;
        OptionsList = new List<Option>();

    }

    public string GetCommand()
    {
        return ToString();
    }

    public bool IsSet() => Set != null;

    public bool IsItem() => !IsSet();

    public override string ToString()
    {
        string s = "";

        int skill = Skill ? 1 : 0;
        int luck = Luck ? 1 : 0;
        if (IsSet())
        {
            s = $"/makeset {Set.Type} {PlusLevel} {skill} {luck} {Option} {ExcManager.GetExcOptionValue(OptionsList)}";

        }
        else
        {
            int group = (int)Group;
            s = $"/make {group} {Type} {PlusLevel} {skill} {luck} {Option} {ExcManager.GetExcOptionValue(OptionsList)}";
        }
        return s;
    }
}