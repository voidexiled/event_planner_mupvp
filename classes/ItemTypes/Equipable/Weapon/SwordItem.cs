using event_planner_mupvp.enums;

public class SwordItem : Weapon
{
    public SwordItem()
    {
    }

    public SwordItem(int Type, SlotTypes Slot, SkillTypes Skill, int Width, int Height, bool HaveSerial, bool HaveOption, bool DropItem, string Name, int Level, int DamageMin, int DamageMax, int AttackSpeed, int Durability, int MagicDurability, int MagicDamageRate, int ReqLevel, int ReqStrength, int ReqDexterity, int ReqEnergy, int ReqVitality, int ReqLeadership, List<ClassTypes> Classes)
    {
        Group = ItemGroups.SWORD_ITEM;
        this.Type = Type;
        this.Slot = Slot;
        this.Skill = Skill;
        this.Width = Width;
        this.Height = Height;
        this.HaveSerial = HaveSerial;
        this.HaveOption = HaveOption;
        this.DropItem = DropItem;
        this.Name = Name;
        this.Level = Level;
        this.DamageMin = DamageMin;
        this.DamageMax = DamageMax;
        this.AttackSpeed = AttackSpeed;
        this.Durability = Durability;
        this.MagicDurability = MagicDurability;
        this.MagicDamageRate = MagicDamageRate;
        this.ReqLevel = ReqLevel;
        this.ReqStrength = ReqStrength;
        this.ReqDexterity = ReqDexterity;
        this.ReqEnergy = ReqEnergy;
        this.ReqVitality = ReqVitality;
        this.ReqLeadership = ReqLeadership;
        this.Classes = Classes;
    }
}