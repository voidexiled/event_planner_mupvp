/* WeaponPendantOptions
1 = Mana recovery after monster hunt +mana/8
2 = Health recovery after monster hunt +HP/8
4 = +7 Speed
8 = More damage +2%
16 = More damage +level/20
32 = Excellent damage rate +10%
*/

/* SetsShieldRingOptions
1 = Increase Zen After Hunt 40%
2 = Defense Succes Rate 10%
4 = Reflect Damage 5%
8 = Damage Decrease 4%
16 = Increase MP 4%
32 = Increase HP 4%
*/

using System.Reflection.Metadata.Ecma335;

public class Option
{
    public int Value { get; set; }
    public string Name { get; set; } = "";

    public string ShortName { get; set; } = "";

    public Option(int value, string name, string shortName)
    {
        Value = value;
        Name = name;
        ShortName = shortName;

    }
}

public static class ExcManager
{

    public static bool IsWeaponPendant(SlotTypes slot)
    {
        return slot == SlotTypes.WEAPON_TYPE || slot == SlotTypes.PENDANT_TYPE;
    }

    public static bool IsSetShieldRing(SlotTypes slot)
    {
        return slot == SlotTypes.SHIELD_TYPE || slot == SlotTypes.RING_TYPE || slot == SlotTypes.HELMET_TYPE || slot == SlotTypes.ARMOR_TYPE || slot == SlotTypes.PANTS_TYPE || slot == SlotTypes.GLOVES_TYPE || slot == SlotTypes.BOOTS_TYPE || slot == SlotTypes.WING_TYPE || slot == SlotTypes.PET_TYPE;
    }



    public static List<Option> WeaponPendantOptions = new()
    {
        new Option(1, "Mana recovery after monster hunt +mana/8", "MANA"),
        new Option(2, "Health recovery after monster hunt +HP/8", "HP"),
        new Option(4, "+7 Speed", "SPEED"),
        new Option(8, "More damage +2%", "DMG2"),
        new Option(16, "More damage +level/20", "DMGLVL"),
        new Option(32, "Excellent damage rate +10%", "EXC10")
    };

    public static List<Option> SetShieldRingOptions = new()
    {
        new Option(1, "Increase Zen After Hunt 40%", "ZEN"),
        new Option(2, "Defense Success Rate 10%", "DEF"),
        new Option(4, "Reflect Damage 5%", "REFL"),
        new Option(8, "Damage Decrease 4%", "DMGDEC"),
        new Option(16, "Increase MP 4%", "MP"),
        new Option(32, "Increase HP 4%", "HP")
    };

    public static string GetItemModifiedName(ProcessedItem processedItem)
    {
        string initName = "";
        if (processedItem.OptionsList.Count > 0)
        {
            initName += $"Excellent ";
        }
        initName += $"{processedItem.Name}";
        if (processedItem.PlusLevel > 0)
        {
            initName += $" +{processedItem.PlusLevel}";
        }
        if (processedItem.Skill)
        {
            initName += " +Skill";
        }
        if (processedItem.Luck)
        {
            initName += " +Luck";
        }

        return initName;
    }

    public static int GetExcOptionValue(List<Option> options)
    {
        int value = 0;

        foreach (Option option in options)
        {
            value += option.Value;
        }

        return value;
    }
}