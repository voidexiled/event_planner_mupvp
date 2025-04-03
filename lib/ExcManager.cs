// lib/ExcManager.cs

using event_planner_mupvp.classes;
// Para Option, ProcessedItem
// Para SlotTypes

namespace event_planner_mupvp.lib;

/// <summary>
///     Gestiona la lógica relacionada con las opciones excelentes de los items.
/// </summary>
public static class ExcManager
{
    #region Option Definitions

    // Hacerlas readonly para asegurar que no se modifiquen externamente

    public static List<Option> WeaponPendantOptions = new()
    {
        new Option(1, 1, "Mana recovery after monster hunt +mana/8", "MANA"),
        new Option(2, 2, "Health recovery after monster hunt +HP/8", "HP"),
        new Option(3, 4, "+7 Speed", "SPEED"),
        new Option(4, 8, "More damage +2%", "DMG2"),
        new Option(5, 16, "More damage +level/20", "DMGLVL"),
        new Option(6, 32, "Excellent damage rate +10%", "EXC10")
    };

    public static List<Option> SetShieldRingOptions = new()
    {
        new Option(1, 1, "Increase Zen After Hunt 40%", "ZEN"),
        new Option(2, 2, "Defense Success Rate 10%", "DEF"),
        new Option(3, 4, "Reflect Damage 5%", "REFL"),
        new Option(4, 8, "Damage Decrease 4%", "DMGDEC"),
        new Option(5, 16, "Increase MP 4%", "MP"),
        new Option(6, 32, "Increase HP 4%", "HP")
    };

    #endregion

    #region Helper Methods

    /// <summary>
    ///     Comprueba si un slot corresponde a un Arma o Pendiente.
    /// </summary>
    public static bool IsWeaponPendant(SlotTypes slot)
    {
        return slot is SlotTypes.WEAPON_TYPE or SlotTypes.PENDANT_TYPE;
    }

    /// <summary>
    ///     Comprueba si un slot corresponde a una parte de Set, Escudo o Anillo.
    /// </summary>
    public static bool IsSetShieldRing(SlotTypes slot)
    {
        return slot is SlotTypes.HELMET_TYPE or SlotTypes.ARMOR_TYPE or SlotTypes.PANTS_TYPE or SlotTypes.GLOVES_TYPE or SlotTypes.BOOTS_TYPE
            or SlotTypes.SHIELD_TYPE or SlotTypes.RING_TYPE or SlotTypes.SET_TYPE;
    }

    /// <summary>
    ///     Devuelve la lista de opciones excelentes aplicables según el slot del item.
    /// </summary>
    /// <param name="slot">El tipo de slot del item.</param>
    /// <returns>La lista de opciones aplicables, o null si no aplican opciones excelentes.</returns>
    public static List<Option>? GetApplicableOptions(SlotTypes slot)
    {
        if (IsWeaponPendant(slot)) return WeaponPendantOptions;
        if (IsSetShieldRing(slot)) return SetShieldRingOptions;
        return null; // No aplican opciones para otros slots (e.g., Wings, Pets)
    }


    /// <summary>
    ///     Obtiene el nombre modificado de un item (delegando a ProcessedItem).
    /// </summary>
    public static string GetItemModifiedName(ProcessedItem item)
    {
        // La lógica está ahora centralizada en ProcessedItem.GetDecoratedName()
        return item?.GetDecoratedName() ?? "Invalid Item";
    }

    #endregion
}