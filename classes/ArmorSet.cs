public class ArmorSet
{
    public string Name { get; set; } = "";
    public int Type { get; set; }
    public HelmetItem Helmet { get; set; } = new HelmetItem();
    public ArmorItem Armor { get; set; } = new ArmorItem();
    public PantsItem Pants { get; set; } = new PantsItem();
    public BootsItem Boots { get; set; } = new BootsItem();
    public GlovesItem Gloves { get; set; } = new GlovesItem();

    public List<ClassTypes> Classes { get; set; } = new();

    public ArmorSet()
    {

    }

    public ArmorSet(int _type, string _name)
    {
        Type = _type;
        Name = _name;
    }
    public ArmorSet(string _name, HelmetItem _helmet, ArmorItem _armor, PantsItem _pants, BootsItem _boots, GlovesItem _gloves)
    {
        Name = _name;
        Helmet = _helmet;
        Armor = _armor;
        Pants = _pants;
        Boots = _boots;
        Gloves = _gloves;

    }
}