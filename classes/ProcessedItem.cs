// classes/ProcessedItem.cs

using System.Text;
using event_planner_mupvp.enums;
using event_planner_mupvp.lib;

// Para ExcManager

// Para StringBuilder

namespace event_planner_mupvp.classes;

/// <summary>
///     Representa una instancia de un item con sus modificaciones específicas
///     tal como se maneja en la lista de recompensas.
/// </summary>
public class ProcessedItem
{
    #region Base Item Properties

    public ItemGroups Group { get; set; }
    public int Type { get; set; }
    public string Name { get; set; }
    public SlotTypes Slot { get; set; }

    #endregion

    #region Modifiable Properties

    public int PlusLevel { get; set; }
    public bool Skill { get; set; }
    public bool Luck { get; set; }
    public List<Option> OptionsList { get; set; }
    public int ExcellentOption { get; set; } // Represents original 'Option' field

    #endregion

    #region Constructors

    public ProcessedItem(ItemGroups group, int type, string name, SlotTypes slot)
    {
        Group = group;
        Type = type;
        Name = name ?? "Unknown Item";
        Slot = slot;
        OptionsList = new List<Option>();
        // Default values (0, false, 0) assigned implicitly or explicitly if needed
    }

    public ProcessedItem(int setTypeIndex, string setName) // For ArmorSet
    {
        Group = ItemGroups.SETS; // Or appropriate group
        Type = setTypeIndex;
        Name = setName ?? "Unknown Set";
        Slot = SlotTypes.SET_TYPE; // Representative slot
        OptionsList = new List<Option>();
    }

    #endregion

    #region Methods

    public string GetCommand()
    {
        return GetCommand(PlusLevel, Skill, Luck, OptionsList, ExcellentOption);
    }

    public string GetCommand(int plusLevel, bool skill, bool luck, List<Option> excOptions, int excellentAncientOption)
    {
        var excellentOptionsValue = excOptions?.Sum(opt => opt.Value) ?? 0;
        // !!! ADJUST FORMAT TO YOUR SERVER !!!
        var command = "/make";

        if (Slot == SlotTypes.SET_TYPE)
            command = "/makeset";
        return $"{command} {(int)Group} {Type} {plusLevel} {(skill ? 1 : 0)} {(luck ? 1 : 0)} {excellentAncientOption} {excellentOptionsValue}";
        
    }

    public bool IsFullExcellent()
    {
        List<Option>? fullOptionSet = ExcManager.GetApplicableOptions(Slot);
        if (fullOptionSet == null || fullOptionSet.Count == 0) return false;
        return OptionsList.Count == fullOptionSet.Count && fullOptionSet.All(opt => OptionsList.Contains(opt));
    }

    public ProcessedItem Clone()
    {
        var clone = new ProcessedItem(Group, Type, Name, Slot)
        {
            PlusLevel = PlusLevel,
            Skill = Skill,
            Luck = Luck,
            ExcellentOption = ExcellentOption,
            OptionsList = new List<Option>(OptionsList) // New list, shared Option refs
        };
        return clone;
    }

    public string GetDecoratedName()
    {
        var sb = new StringBuilder();
        sb.Append(Name);
        if (PlusLevel > 0) sb.Append($" +{PlusLevel}");
        if (Skill) sb.Append(" +S");
        if (Luck) sb.Append(" +L");
        var excCount = OptionsList.Count;
        if (excCount > 0) sb.Append($" +{excCount}Exc");
        // Add other decorators if needed (e.g., Ancient)
        return sb.ToString();
    }

    #endregion
}