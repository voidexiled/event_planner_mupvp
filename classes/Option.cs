// classes/Option.cs

using System; // Necesario para ArgumentNullException

namespace event_planner_mupvp.classes;

/// <summary>
///     Representa una opción excelente que puede tener un item.
/// </summary>
public class Option : IEquatable<Option> // Implement IEquatable for better comparisons
{
    #region Properties

    /// <summary>
    ///     Identificador único interno de la opción (e.g., 1-6).
    /// </summary>
    public int Id { get; } // Hacer readonly si no cambian después de crear

    /// <summary>
    ///     Nombre legible por humanos de la opción.
    /// </summary>
    public string Name { get; }

    /// <summary>
    ///     Nombre corto para mostrar en combos.
    /// </summary>
    public string ShortName { get; } // Nombre corto para mostrar en combos

    /// <summary>
    ///     Valor numérico (a menudo bitmask: 1, 2, 4, 8, 16, 32) usado en comandos.
    /// </summary>
    public int Value { get; }

    /// <summary>
    ///     Descripción opcional mostrada en tooltips.
    /// </summary>
    public string? Description { get; }

    #endregion

    #region Constructor

    public Option(int id, int value, string name, string shortName)
    {
        Id = id;
        Name = name ?? throw new ArgumentNullException(nameof(name));
        ShortName = shortName ?? throw new ArgumentNullException(nameof(shortName));
        Value = value;
        // Description = description;
    }

    #endregion

    #region Equality Implementation
    public bool Equals(Option? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;
        return Id == other.Id; // Equality based on ID
    }

    public override bool Equals(object? obj)
    {
        return Equals(obj as Option);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Id);
    }

    public static bool operator ==(Option? left, Option? right)
    {
        return Equals(left, right);
    }

    public static bool operator !=(Option? left, Option? right)
    {
        return !Equals(left, right);
    }
    #endregion

    public override string ToString()
    {
        return Name; // Útil para debugging y combos simples
    }

}