public class Equipable : Item
{
    public List<ClassTypes> Classes { get; set; } = new List<ClassTypes>();
    public int Durability { get; set; }
    public int ReqLevel { get; set; }
    public int ReqStrength { get; set; }
    public int ReqDexterity { get; set; }
    public int ReqEnergy { get; set; }
    public int ReqVitality { get; set; }
    public int ReqLeadership { get; set; }

}