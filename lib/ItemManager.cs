using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows.Markup;
using ImGuiNET;
public static partial class ItemManager
{

    public static Dictionary<ItemGroups, int> Headers { get; set; } = new(){
        {ItemGroups.SWORD_ITEM, GetColumns("data/Swords.txt").Length},
        {ItemGroups.AXE_ITEM, GetColumns("data/Axes.txt").Length},
        {ItemGroups.MACE_ITEM, GetColumns("data/Maces.txt").Length},
        {ItemGroups.SPEAR_ITEM, GetColumns("data/Spears.txt").Length},
        {ItemGroups.BOW_ITEM, GetColumns("data/Bows.txt").Length},
        {ItemGroups.STAFF_ITEM, GetColumns("data/Staffs.txt").Length},
        {ItemGroups.SHIELD_ITEM, GetColumns("data/Shields.txt").Length},
        {ItemGroups.HELMET_ITEM, GetColumns("data/Helmets.txt").Length},
        {ItemGroups.ARMOR_ITEM, GetColumns("data/Armors.txt").Length},
        {ItemGroups.PANTS_ITEM, GetColumns("data/Pants.txt").Length},
        {ItemGroups.GLOVES_ITEM, GetColumns("data/Gloves.txt").Length},
        {ItemGroups.BOOTS_ITEM, GetColumns("data/Boots.txt").Length},
        {ItemGroups.WINGS_ITEM, GetColumns("data/Wings.txt").Length},
        {ItemGroups.PETS_ACC_ITEM, GetColumns("data/PetsAcc.txt").Length},
    };


    public static List<ArmorSet> ArmorSets { get; set; } = new();
    public static List<Item> AllItems { get; private set; } = new List<Item>();
    public static List<SwordItem> Swords { get; set; } = new List<SwordItem>();
    public static List<AxeItem> Axes { get; set; } = new List<AxeItem>();
    public static List<MaceItem> Maces { get; set; } = new List<MaceItem>();
    public static List<SpearItem> Spears { get; set; } = new List<SpearItem>();
    public static List<BowItem> Bows { get; set; } = new List<BowItem>();
    public static List<StaffItem> Staffs { get; set; } = new List<StaffItem>();
    public static List<ShieldItem> Shields { get; set; } = new List<ShieldItem>();
    public static List<HelmetItem> Helmets { get; set; } = new List<HelmetItem>();
    public static List<ArmorItem> Armors { get; set; } = new List<ArmorItem>();
    public static List<PantsItem> Pants { get; set; } = new List<PantsItem>();
    public static List<GlovesItem> Gloves { get; set; } = new List<GlovesItem>();
    public static List<BootsItem> Boots { get; set; } = new List<BootsItem>();
    public static List<WingsItem> Wings { get; set; } = new List<WingsItem>();
    public static List<PetsAccItem> PetsAccItems { get; set; } = new List<PetsAccItem>();
    public static List<ConsumablesJewelsBoxesItem> ConsumablesJewelsBoxesItems { get; set; } = new List<ConsumablesJewelsBoxesItem>();
    public static List<ScrollsParchmentsItem> ScrollsParchmentsItems { get; set; } = new List<ScrollsParchmentsItem>();

    private static string[] GetColumns(string file)
    {
        string filePath = $"{file}";
        string[] lines = File.ReadAllLines(filePath);
        string[] values = MyRegex().Split(lines[0]).Select(value => value.Trim()).ToArray();
        return values;
    }

    public static void PopulateItems()
    {
        PopulateSwords(); // Group 0
        PopulateAxes(); // Group 1
        PopulateMaces(); // Group 2
        PopulateSpears(); // Group 3
        PopulateBows(); // Group 4
        PopulateStaffs(); // Group 5
        PopulateShields(); // Group 6
        PopulateHelmets(); // Group 7
        PopulateArmors(); // Group 8
        PopulatePants(); // Group 9
        PopulateGloves(); // Group 10
        PopulateBoots(); // Group 11
        PopulateWings(); // Group 12
        PopulatePetsAccItems(); // Group 13
        PopulateConsumablesJewelsBoxesItems(); // Group 14
        // PopulateScrollsParchmentsItems(); // Group 15
        //Console.WriteLine($"{Shields.Count}");
        ConsolidateItemsList();

        PopulateSets();
    }

    private static void ConsolidateItemsList()
    {
        AllItems.Clear();
        AllItems.AddRange(Swords);
        AllItems.AddRange(Axes);
        AllItems.AddRange(Maces);
        AllItems.AddRange(Spears);
        AllItems.AddRange(Bows);
        AllItems.AddRange(Staffs);
        AllItems.AddRange(Shields);
        AllItems.AddRange(Helmets);
        AllItems.AddRange(Armors);
        AllItems.AddRange(Pants);
        AllItems.AddRange(Gloves);
        AllItems.AddRange(Boots);
        AllItems.AddRange(Wings);
        AllItems.AddRange(PetsAccItems);
        AllItems.AddRange(ConsumablesJewelsBoxesItems);
        // AllItems.AddRange(ScrollsParchmentsItems);
    }

    public static void PopulateSwords()
    {
        string fileName = "data/Swords.txt";

        try
        {
            string[] lines = File.ReadAllLines(fileName);
            List<SwordItem> swords = new();
            foreach (string line in lines)
            {
                if (line == "")
                {
                    continue;
                }
                string[] values = MyRegex1().Split(line).Select(value => value.Trim()).ToArray();

                //Console.WriteLine(string.Join(", ", values));
                if (values.Length == Headers[ItemGroups.SWORD_ITEM])
                {
                    SwordItem swordItem = new()
                    {
                        Type = int.Parse(values[0]),
                        Slot = (SlotTypes)Enum.Parse(typeof(SlotTypes), values[1]),
                        Skill = (SkillTypes)Enum.Parse(typeof(SkillTypes), values[2]),
                        Width = int.Parse(values[3]),
                        Height = int.Parse(values[4]),
                        HaveSerial = int.Parse(values[5]) == 1,
                        HaveOption = int.Parse(values[6]) == 1,
                        DropItem = int.Parse(values[7]) == 1,
                        Name = values[8].Trim('"'),
                        Level = int.Parse(values[9]),
                        DamageMin = int.Parse(values[10]),
                        DamageMax = int.Parse(values[11]),
                        AttackSpeed = int.Parse(values[12]),
                        Durability = int.Parse(values[13]),
                        MagicDurability = int.Parse(values[14]),
                        MagicDamageRate = int.Parse(values[15]),
                        ReqLevel = int.Parse(values[16]),
                        ReqStrength = int.Parse(values[17]),
                        ReqDexterity = int.Parse(values[18]),
                        ReqEnergy = int.Parse(values[19]),
                        ReqVitality = int.Parse(values[20]),
                        ReqLeadership = int.Parse(values[21]),
                        // el 22 es el None
                        Classes = ClassManager.GetListOfClasses(int.Parse(values[23]), int.Parse(values[24]), int.Parse(values[25]), int.Parse(values[26]), int.Parse(values[27]), int.Parse(values[28]), int.Parse(values[29]))
                    };
                    swords.Add(swordItem);
                }
                else
                {
                    Console.WriteLine($"Error: Faltan columnas a la linea {line}");
                }

            }

            Swords = swords;
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
        }
    }

    public static void PopulateAxes()
    {
        string filePath = "data/Axes.txt";

        try
        {
            string[] lines = File.ReadAllLines(filePath);
            List<AxeItem> axes = new();
            foreach (string line in lines)
            {
                if (line == "")
                {
                    continue;
                }
                string[] values = MyRegex1().Split(line).Select(value => value.Trim()).ToArray();

                //Console.WriteLine(string.Join(", ", values));
                if (values.Length == Headers[ItemGroups.AXE_ITEM])
                {
                    AxeItem axeItem = new()
                    {
                        Type = int.Parse(values[0]),
                        Slot = (SlotTypes)Enum.Parse(typeof(SlotTypes), values[1]),
                        Skill = (SkillTypes)Enum.Parse(typeof(SkillTypes), values[2]),
                        Width = int.Parse(values[3]),
                        Height = int.Parse(values[4]),
                        HaveSerial = int.Parse(values[5]) == 1,
                        HaveOption = int.Parse(values[6]) == 1,
                        DropItem = int.Parse(values[7]) == 1,
                        Name = values[8].Trim('"'),
                        Level = int.Parse(values[9]),
                        DamageMin = int.Parse(values[10]),
                        DamageMax = int.Parse(values[11]),
                        AttackSpeed = int.Parse(values[12]),
                        Durability = int.Parse(values[13]),
                        MagicDurability = int.Parse(values[14]),
                        MagicDamageRate = int.Parse(values[15]),
                        ReqLevel = int.Parse(values[16]),
                        ReqStrength = int.Parse(values[17]),
                        ReqDexterity = int.Parse(values[18]),
                        ReqEnergy = int.Parse(values[19]),
                        ReqVitality = int.Parse(values[20]),
                        ReqLeadership = int.Parse(values[21]),
                        // el 22 es el None
                        Classes = ClassManager.GetListOfClasses(int.Parse(values[23]), int.Parse(values[24]), int.Parse(values[25]), int.Parse(values[26]), int.Parse(values[27]), int.Parse(values[28]), int.Parse(values[29]))
                    };
                    axes.Add(axeItem);
                }
                else
                {
                    Console.WriteLine($"Error: Faltan columnas a la linea {line}");
                }

            }

            Axes = axes;
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
        }

    }

    public static void PopulateMaces()
    {
        string filePath = "data/Maces.txt";

        try
        {
            string[] lines = File.ReadAllLines(filePath);
            List<MaceItem> maces = new();
            foreach (string line in lines)
            {
                if (line == "")
                {
                    continue;
                }
                string[] values = Regex.Split(line, @"\s{2,}").Select(value => value.Trim()).ToArray();

                //Console.WriteLine(string.Join(", ", values));
                if (values.Length == Headers[ItemGroups.MACE_ITEM])
                {
                    MaceItem maceItem = new()
                    {
                        Type = int.Parse(values[0]),
                        Slot = (SlotTypes)Enum.Parse(typeof(SlotTypes), values[1]),
                        Skill = (SkillTypes)Enum.Parse(typeof(SkillTypes), values[2]),
                        Width = int.Parse(values[3]),
                        Height = int.Parse(values[4]),
                        HaveSerial = int.Parse(values[5]) == 1,
                        HaveOption = int.Parse(values[6]) == 1,
                        DropItem = int.Parse(values[7]) == 1,
                        Name = values[8].Trim('"'),
                        Level = int.Parse(values[9]),
                        DamageMin = int.Parse(values[10]),
                        DamageMax = int.Parse(values[11]),
                        AttackSpeed = int.Parse(values[12]),
                        Durability = int.Parse(values[13]),
                        MagicDurability = int.Parse(values[14]),
                        MagicDamageRate = int.Parse(values[15]),
                        ReqLevel = int.Parse(values[16]),
                        ReqStrength = int.Parse(values[17]),
                        ReqDexterity = int.Parse(values[18]),
                        ReqEnergy = int.Parse(values[19]),
                        ReqVitality = int.Parse(values[20]),
                        ReqLeadership = int.Parse(values[21]),
                        // el 22 es el None
                        Classes = ClassManager.GetListOfClasses(int.Parse(values[23]), int.Parse(values[24]), int.Parse(values[25]), int.Parse(values[26]), int.Parse(values[27]), int.Parse(values[28]), int.Parse(values[29]))
                    };
                    maces.Add(maceItem);
                }
                else
                {
                    Console.WriteLine($"Error: Faltan columnas a la linea {line}");
                }

            }

            Maces = maces;
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
        }
    }

    public static void PopulateSpears()
    {
        string filePath = "data/Spears.txt";

        try
        {
            string[] lines = File.ReadAllLines(filePath);
            List<SpearItem> spears = new();
            foreach (string line in lines)
            {
                if (line == "")
                {
                    continue;
                }
                string[] values = Regex.Split(line, @"\s{2,}").Select(value => value.Trim()).ToArray();

                //Console.WriteLine(string.Join(", ", values));
                if (values.Length == Headers[ItemGroups.SPEAR_ITEM])
                {
                    SpearItem spearItem = new()
                    {
                        Type = int.Parse(values[0]),
                        Slot = (SlotTypes)Enum.Parse(typeof(SlotTypes), values[1]),
                        Skill = (SkillTypes)Enum.Parse(typeof(SkillTypes), values[2]),
                        Width = int.Parse(values[3]),
                        Height = int.Parse(values[4]),
                        HaveSerial = int.Parse(values[5]) == 1,
                        HaveOption = int.Parse(values[6]) == 1,
                        DropItem = int.Parse(values[7]) == 1,
                        Name = values[8].Trim('"'),
                        Level = int.Parse(values[9]),
                        DamageMin = int.Parse(values[10]),
                        DamageMax = int.Parse(values[11]),
                        AttackSpeed = int.Parse(values[12]),
                        Durability = int.Parse(values[13]),
                        MagicDurability = int.Parse(values[14]),
                        MagicDamageRate = int.Parse(values[15]),
                        ReqLevel = int.Parse(values[16]),
                        ReqStrength = int.Parse(values[17]),
                        ReqDexterity = int.Parse(values[18]),
                        ReqEnergy = int.Parse(values[19]),
                        ReqVitality = int.Parse(values[20]),
                        ReqLeadership = int.Parse(values[21]),
                        // el 22 es el None
                        Classes = ClassManager.GetListOfClasses(int.Parse(values[23]), int.Parse(values[24]), int.Parse(values[25]), int.Parse(values[26]), int.Parse(values[27]), int.Parse(values[28]), int.Parse(values[29]))
                    };
                    spears.Add(spearItem);
                }
                else
                {
                    Console.WriteLine($"Error: Faltan columnas a la linea {line}");
                }

            }

            Spears = spears;
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
        }
    }
    public static void PopulateBows()
    {
        string filePath = "data/Bows.txt";

        try
        {
            string[] lines = File.ReadAllLines(filePath);
            List<BowItem> bows = new();
            foreach (string line in lines)
            {
                if (line == "")
                {
                    continue;
                }
                string[] values = Regex.Split(line, @"\s{2,}").Select(value => value.Trim()).ToArray();

                //Console.WriteLine(string.Join(", ", values));
                if (values.Length == Headers[ItemGroups.BOW_ITEM])
                {
                    BowItem bowItem = new()
                    {
                        Type = int.Parse(values[0]),
                        Slot = (SlotTypes)Enum.Parse(typeof(SlotTypes), values[1]),
                        Skill = (SkillTypes)Enum.Parse(typeof(SkillTypes), values[2]),
                        Width = int.Parse(values[3]),
                        Height = int.Parse(values[4]),
                        HaveSerial = int.Parse(values[5]) == 1,
                        HaveOption = int.Parse(values[6]) == 1,
                        DropItem = int.Parse(values[7]) == 1,
                        Name = values[8].Trim('"'),
                        Level = int.Parse(values[9]),
                        DamageMin = int.Parse(values[10]),
                        DamageMax = int.Parse(values[11]),
                        AttackSpeed = int.Parse(values[12]),
                        Durability = int.Parse(values[13]),
                        MagicDurability = int.Parse(values[14]),
                        MagicDamageRate = int.Parse(values[15]),
                        ReqLevel = int.Parse(values[16]),
                        ReqStrength = int.Parse(values[17]),
                        ReqDexterity = int.Parse(values[18]),
                        ReqEnergy = int.Parse(values[19]),
                        ReqVitality = int.Parse(values[20]),
                        ReqLeadership = int.Parse(values[21]),
                        // el 22 es el None
                        Classes = ClassManager.GetListOfClasses(int.Parse(values[23]), int.Parse(values[24]), int.Parse(values[25]), int.Parse(values[26]), int.Parse(values[27]), int.Parse(values[28]), int.Parse(values[29]))
                    };
                    bows.Add(bowItem);
                }
                else
                {
                    Console.WriteLine($"Error: Faltan columnas a la linea {line}");
                }

            }

            Bows = bows;
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
        }
    }

    public static void PopulateStaffs()
    {
        string filePath = "data/Staffs.txt";
        Console.WriteLine("Populating Staffs");
        try
        {
            string[] lines = File.ReadAllLines(filePath);
            List<StaffItem> staffs = new();
            foreach (string line in lines)
            {
                if (line == "")
                {
                    continue;
                }
                string[] values = Regex.Split(line, @"\s{2,}").Select(value => value.Trim()).ToArray();

                //Console.WriteLine(string.Join(", ", values));
                if (values.Length == Headers[ItemGroups.STAFF_ITEM])
                {
                    StaffItem staffItem = new()
                    {
                        Type = int.Parse(values[0]),
                        Slot = (SlotTypes)Enum.Parse(typeof(SlotTypes), values[1]),
                        Skill = (SkillTypes)Enum.Parse(typeof(SkillTypes), values[2]),
                        Width = int.Parse(values[3]),
                        Height = int.Parse(values[4]),
                        HaveSerial = int.Parse(values[5]) == 1,
                        HaveOption = int.Parse(values[6]) == 1,
                        DropItem = int.Parse(values[7]) == 1,
                        Name = values[8].Trim('"'),
                        Level = int.Parse(values[9]),
                        DamageMin = int.Parse(values[10]),
                        DamageMax = int.Parse(values[11]),
                        AttackSpeed = int.Parse(values[12]),
                        Durability = int.Parse(values[13]),
                        MagicDurability = int.Parse(values[14]),
                        MagicDamageRate = int.Parse(values[15]),
                        ReqLevel = int.Parse(values[16]),
                        ReqStrength = int.Parse(values[17]),
                        ReqDexterity = int.Parse(values[18]),
                        ReqEnergy = int.Parse(values[19]),
                        ReqVitality = int.Parse(values[20]),
                        ReqLeadership = int.Parse(values[21]),
                        // el 22 es el None
                        Classes = ClassManager.GetListOfClasses(int.Parse(values[23]), int.Parse(values[24]), int.Parse(values[25]), int.Parse(values[26]), int.Parse(values[27]), int.Parse(values[28]), int.Parse(values[29]))
                    };
                    staffs.Add(staffItem);
                }
                else
                {
                    Console.WriteLine($"Error: Faltan columnas a la linea {line}");
                }

            }

            Staffs = staffs;
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
        }
    }

    public static void PopulateShields()
    {
        string filePath = "data/Shields.txt";
        List<ShieldItem> shields = new();
        try
        {
            string[] lines = File.ReadAllLines(filePath);

            foreach (string line in lines)
            {
                if (line == "")
                {
                    continue;
                }
                string[] values = Regex.Split(line, @"\s{2,}").Select(value => value.Trim()).ToArray();
                //Console.WriteLine(string.Join(", ", values));
                if (values.Length == Headers[ItemGroups.SHIELD_ITEM])
                {
                    ShieldItem shieldItem = new()
                    {
                        Type = int.Parse(values[0]),
                        Slot = (SlotTypes)Enum.Parse(typeof(SlotTypes), values[1]),
                        Skill = (SkillTypes)Enum.Parse(typeof(SkillTypes), values[2]),
                        Width = int.Parse(values[3]),
                        Height = int.Parse(values[4]),
                        HaveSerial = int.Parse(values[5]) == 1,
                        HaveOption = int.Parse(values[6]) == 1,
                        DropItem = int.Parse(values[7]) == 1,
                        Name = values[8].Trim('"'),
                        Level = int.Parse(values[9]),
                        Defense = int.Parse(values[10]),
                        DefenseSuccessRate = int.Parse(values[11]),
                        Durability = int.Parse(values[12]),
                        ReqLevel = int.Parse(values[13]),
                        ReqStrength = int.Parse(values[14]),
                        ReqDexterity = int.Parse(values[15]),
                        ReqEnergy = int.Parse(values[16]),
                        ReqVitality = int.Parse(values[17]),
                        ReqLeadership = int.Parse(values[18]),
                        // el 20 es el None
                        Classes = ClassManager.GetListOfClasses(int.Parse(values[20]), int.Parse(values[21]), int.Parse(values[22]), int.Parse(values[23]), int.Parse(values[24]), int.Parse(values[25]), int.Parse(values[26]))
                    };
                    shields.Add(shieldItem);
                }
                else
                {
                    Console.WriteLine($"Error: Faltan columnas a la linea {line}");
                }

            }

            Shields = shields;
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
        }
    }

    public static void PopulateHelmets()
    {
        string filePath = "data/Helmets.txt";

        try
        {
            string[] lines = File.ReadAllLines(filePath);
            List<HelmetItem> helmets = new();
            foreach (string line in lines)
            {
                if (line == "")
                {
                    continue;
                }
                string[] values = Regex.Split(line, @"\s{2,}").Select(value => value.Trim()).ToArray();
                //Console.WriteLine(string.Join(", ", values));
                if (values.Length == Headers[ItemGroups.HELMET_ITEM])
                {
                    HelmetItem helmetItem = new()
                    {
                        Type = int.Parse(values[0]),
                        Slot = (SlotTypes)Enum.Parse(typeof(SlotTypes), values[1]),
                        Skill = (SkillTypes)Enum.Parse(typeof(SkillTypes), values[2]),
                        Width = int.Parse(values[3]),
                        Height = int.Parse(values[4]),
                        HaveSerial = int.Parse(values[5]) == 1,
                        HaveOption = int.Parse(values[6]) == 1,
                        DropItem = int.Parse(values[7]) == 1,
                        Name = values[8].Trim('"'),
                        Level = int.Parse(values[9]),
                        Defense = int.Parse(values[10]),
                        MagicDefense = int.Parse(values[11]),
                        Durability = int.Parse(values[12]),
                        ReqLevel = int.Parse(values[13]),
                        ReqStrength = int.Parse(values[14]),
                        ReqDexterity = int.Parse(values[15]),
                        ReqEnergy = int.Parse(values[16]),
                        ReqVitality = int.Parse(values[17]),
                        ReqLeadership = int.Parse(values[18]),
                        // el 20 es el None
                        Classes = ClassManager.GetListOfClasses(int.Parse(values[20]), int.Parse(values[21]), int.Parse(values[22]), int.Parse(values[23]), int.Parse(values[24]), int.Parse(values[25]), int.Parse(values[26]))
                    };
                    helmets.Add(helmetItem);
                }
                else
                {
                    Console.WriteLine($"Error: Faltan columnas a la linea {line}");
                }

            }

            Helmets = helmets;
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
        }
    }

    public static void PopulateArmors()
    {
        string filePath = "data/Armors.txt";

        try
        {
            string[] lines = File.ReadAllLines(filePath);
            List<ArmorItem> armors = new();
            foreach (string line in lines)
            {
                if (line == "")
                {
                    continue;
                }
                string[] values = Regex.Split(line, @"\s{2,}").Select(value => value.Trim()).ToArray();
                //Console.WriteLine(string.Join(", ", values));
                if (values.Length == Headers[ItemGroups.ARMOR_ITEM])
                {
                    ArmorItem armorItem = new()
                    {
                        Type = int.Parse(values[0]),
                        Slot = (SlotTypes)Enum.Parse(typeof(SlotTypes), values[1]),
                        Skill = (SkillTypes)Enum.Parse(typeof(SkillTypes), values[2]),
                        Width = int.Parse(values[3]),
                        Height = int.Parse(values[4]),
                        HaveSerial = int.Parse(values[5]) == 1,
                        HaveOption = int.Parse(values[6]) == 1,
                        DropItem = int.Parse(values[7]) == 1,
                        Name = values[8].Trim('"'),
                        Level = int.Parse(values[9]),
                        Defense = int.Parse(values[10]),
                        MagicDefense = int.Parse(values[11]),
                        Durability = int.Parse(values[12]),
                        ReqLevel = int.Parse(values[13]),
                        ReqStrength = int.Parse(values[14]),
                        ReqDexterity = int.Parse(values[15]),
                        ReqEnergy = int.Parse(values[16]),
                        ReqVitality = int.Parse(values[17]),
                        ReqLeadership = int.Parse(values[18]),
                        // el 20 es el None
                        Classes = ClassManager.GetListOfClasses(int.Parse(values[20]), int.Parse(values[21]), int.Parse(values[22]), int.Parse(values[23]), int.Parse(values[24]), int.Parse(values[25]), int.Parse(values[26]))
                    };
                    armors.Add(armorItem);
                }
                else
                {
                    Console.WriteLine($"Error: Faltan columnas a la linea {line}");
                }

            }

            Armors = armors;
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
        }
    }

    public static void PopulatePants()
    {
        string filePath = "data/Pants.txt";

        try
        {
            string[] lines = File.ReadAllLines(filePath);
            List<PantsItem> pants = new();

            foreach (string line in lines)
            {
                if (line == "")
                {
                    continue;
                }
                string[] values = Regex.Split(line, @"\s{2,}").Select(value => value.Trim()).ToArray();
                //Console.WriteLine(string.Join(", ", values));
                if (values.Length == Headers[ItemGroups.PANTS_ITEM])
                {
                    PantsItem pantsItem = new()
                    {
                        Type = int.Parse(values[0]),
                        Slot = (SlotTypes)Enum.Parse(typeof(SlotTypes), values[1]),
                        Skill = (SkillTypes)Enum.Parse(typeof(SkillTypes), values[2]),
                        Width = int.Parse(values[3]),
                        Height = int.Parse(values[4]),
                        HaveSerial = int.Parse(values[5]) == 1,
                        HaveOption = int.Parse(values[6]) == 1,
                        DropItem = int.Parse(values[7]) == 1,
                        Name = values[8].Trim('"'),
                        Level = int.Parse(values[9]),
                        Defense = int.Parse(values[10]),
                        MagicDefense = int.Parse(values[11]),
                        Durability = int.Parse(values[12]),
                        ReqLevel = int.Parse(values[13]),
                        ReqStrength = int.Parse(values[14]),
                        ReqDexterity = int.Parse(values[15]),
                        ReqEnergy = int.Parse(values[16]),
                        ReqVitality = int.Parse(values[17]),
                        ReqLeadership = int.Parse(values[18]),
                        // el 20 es el None
                        Classes = ClassManager.GetListOfClasses(int.Parse(values[20]), int.Parse(values[21]), int.Parse(values[22]), int.Parse(values[23]), int.Parse(values[24]), int.Parse(values[25]), int.Parse(values[26]))
                    };
                    pants.Add(pantsItem);
                }
                else
                {
                    Console.WriteLine($"Error: Faltan columnas a la linea {line}");
                }

            }

            Pants = pants;
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
        }
    }

    public static void PopulateGloves()
    {
        string filePath = "data/Gloves.txt";

        try
        {
            string[] lines = File.ReadAllLines(filePath);
            List<GlovesItem> gloves = new();
            foreach (string line in lines)
            {
                if (line == "")
                {
                    continue;
                }
                string[] values = Regex.Split(line, @"\s{2,}").Select(value => value.Trim()).ToArray();
                //Console.WriteLine(string.Join(", ", values));

                if (values.Length == Headers[ItemGroups.GLOVES_ITEM])
                {
                    GlovesItem glovesItem = new()
                    {
                        Type = int.Parse(values[0]),
                        Slot = (SlotTypes)Enum.Parse(typeof(SlotTypes), values[1]),
                        Skill = (SkillTypes)Enum.Parse(typeof(SkillTypes), values[2]),
                        Width = int.Parse(values[3]),
                        Height = int.Parse(values[4]),
                        HaveSerial = int.Parse(values[5]) == 1,
                        HaveOption = int.Parse(values[6]) == 1,
                        DropItem = int.Parse(values[7]) == 1,
                        Name = values[8].Trim('"'),
                        Level = int.Parse(values[9]),
                        Defense = int.Parse(values[10]),
                        AttackSpeed = int.Parse(values[11]),
                        Durability = int.Parse(values[12]),
                        ReqLevel = int.Parse(values[13]),
                        ReqStrength = int.Parse(values[14]),
                        ReqDexterity = int.Parse(values[15]),
                        ReqEnergy = int.Parse(values[16]),
                        ReqVitality = int.Parse(values[17]),
                        ReqLeadership = int.Parse(values[18]),
                        // el 20 es el None
                        Classes = ClassManager.GetListOfClasses(int.Parse(values[20]), int.Parse(values[21]), int.Parse(values[22]), int.Parse(values[23]), int.Parse(values[24]), int.Parse(values[25]), int.Parse(values[26]))
                    };
                    gloves.Add(glovesItem);
                }
                else
                {
                    Console.WriteLine($"Error: Faltan columnas a la linea {line}");
                }

            }

            Gloves = gloves;
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
        }
    }

    public static void PopulateBoots()
    {
        string filePath = "data/Boots.txt";

        try
        {
            string[] lines = File.ReadAllLines(filePath);
            List<BootsItem> boots = new();
            foreach (string line in lines)
            {
                if (line == "")
                {
                    continue;
                }
                string[] values = Regex.Split(line, @"\s{2,}").Select(value => value.Trim()).ToArray();
                //Console.WriteLine(string.Join(", ", values));
                if (values.Length == Headers[ItemGroups.BOOTS_ITEM])
                {
                    BootsItem bootsItem = new()
                    {
                        Type = int.Parse(values[0]),
                        Slot = (SlotTypes)Enum.Parse(typeof(SlotTypes), values[1]),
                        Skill = (SkillTypes)Enum.Parse(typeof(SkillTypes), values[2]),
                        Width = int.Parse(values[3]),
                        Height = int.Parse(values[4]),
                        HaveSerial = int.Parse(values[5]) == 1,
                        HaveOption = int.Parse(values[6]) == 1,
                        DropItem = int.Parse(values[7]) == 1,
                        Name = values[8].Trim('"'),
                        Level = int.Parse(values[9]),
                        Defense = int.Parse(values[10]),
                        MagicDefense = int.Parse(values[11]),
                        Durability = int.Parse(values[12]),
                        ReqLevel = int.Parse(values[13]),
                        ReqStrength = int.Parse(values[14]),
                        ReqDexterity = int.Parse(values[15]),
                        ReqEnergy = int.Parse(values[16]),
                        ReqVitality = int.Parse(values[17]),
                        ReqLeadership = int.Parse(values[18]),
                        // el 20 es el None
                        Classes = ClassManager.GetListOfClasses(int.Parse(values[20]), int.Parse(values[21]), int.Parse(values[22]), int.Parse(values[23]), int.Parse(values[24]), int.Parse(values[25]), int.Parse(values[26]))
                    };
                    boots.Add(bootsItem);
                }
                else
                {
                    Console.WriteLine($"Error: Faltan columnas a la linea {line}");
                }

            }

            Boots = boots;
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
        }
    }

    public static void PopulateWings()
    {
        string filePath = "data/Wings.txt";

        try
        {

            string[] lines = File.ReadAllLines(filePath);
            List<WingsItem> wings = new();
            foreach (string line in lines)
            {
                if (line == "")
                {
                    continue;
                }

                string[] values = Regex.Split(line, @"\s{2,}").Select(value => value.Trim()).ToArray();
                if (values[2].Equals('*'))
                {
                    values[2] = "11";
                }
                //Console.WriteLine(string.Join(", ", values));
                if (values.Length == Headers[ItemGroups.WINGS_ITEM])
                {
                    WingsItem wingsItem = new()
                    {
                        Type = int.Parse(values[0]),
                        Slot = (SlotTypes)Enum.Parse(typeof(SlotTypes), values[1]),
                        Skill = (SkillTypes)Enum.Parse(typeof(SkillTypes), values[2]),
                        Width = int.Parse(values[3]),
                        Height = int.Parse(values[4]),
                        HaveSerial = int.Parse(values[5]) == 1,
                        HaveOption = int.Parse(values[6]) == 1,
                        DropItem = int.Parse(values[7]) == 1,
                        Name = values[8].Trim('"'),
                        Level = int.Parse(values[9]),
                        Defense = int.Parse(values[10]),
                        Durability = int.Parse(values[11]),
                        ReqLevel = int.Parse(values[12]),
                        ReqEnergy = int.Parse(values[13]),
                        ReqStrength = int.Parse(values[14]),
                        ReqDexterity = int.Parse(values[15]),
                        ReqLeadership = int.Parse(values[16]),
                        BuyMoney = int.Parse(values[17]),
                        Classes = ClassManager.GetListOfClasses(int.Parse(values[18]), int.Parse(values[19]), int.Parse(values[20]), int.Parse(values[21]), int.Parse(values[22]), int.Parse(values[23]), int.Parse(values[24]))
                    };
                    wings.Add(wingsItem);
                }
                else
                {
                    Console.WriteLine($"Error: Faltan columnas a la linea {line}");
                }

            }

            Wings = wings;
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
        }
    }

    public static void PopulatePetsAccItems()
    {
        string filePath = "data/PetsAcc.txt";

        try
        {
            string[] lines = File.ReadAllLines(filePath);
            List<PetsAccItem> petsAccItems = new();
            foreach (string line in lines)
            {
                if (line == "")
                {
                    continue;
                }

                string[] values = Regex.Split(line, @"\s{2,}").Select(value => value.Trim()).ToArray();

                //Console.WriteLine(string.Join(", ", values));
                if (values.Length == Headers[ItemGroups.PETS_ACC_ITEM])
                {
                    PetsAccItem petsAccItem = new()
                    {
                        Type = int.Parse(values[0]),
                        Slot = (SlotTypes)Enum.Parse(typeof(SlotTypes), values[1]),
                        Skill = (SkillTypes)Enum.Parse(typeof(SkillTypes), values[2]),
                        Width = int.Parse(values[3]),
                        Height = int.Parse(values[4]),
                        HaveSerial = int.Parse(values[5]) == 1,
                        HaveOption = int.Parse(values[6]) == 1,
                        DropItem = int.Parse(values[7]) == 1,
                        Name = values[8].Trim('"'),
                        Level = int.Parse(values[9]),
                        Durability = int.Parse(values[10]),
                        // Defense = int.Parse(values[9]),
                        // Durability = int.Parse(values[10]),
                        // ReqLevel = int.Parse(values[11]),
                        // ReqEnergy = int.Parse(values[12]),
                        // ReqStrength = int.Parse(values[13]),
                        // ReqDexterity = int.Parse(values[14]),
                        // ReqLeadership = int.Parse(values[15]),
                        // BuyMoney = int.Parse(values[16]),
                        Classes = ClassManager.GetListOfClasses(int.Parse(values[19]), int.Parse(values[20]), int.Parse(values[21]), int.Parse(values[22]), int.Parse(values[23]), int.Parse(values[24]), int.Parse(values[25]))
                    };
                    petsAccItems.Add(petsAccItem);
                }
                else
                {
                    Console.WriteLine($"Error: Faltan columnas a la linea {line}");
                }

            }

            PetsAccItems = petsAccItems;
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
        }
    }

    public static void PopulateConsumablesJewelsBoxesItems()
    {
        string filePath = "data/ConsumablesJewelsBoxes.txt";

        try
        {
            string[] lines = File.ReadAllLines(filePath);
            List<ConsumablesJewelsBoxesItem> consumablesJewelsBoxesItems = new();
            foreach (string line in lines)
            {
                if (line == "")
                {
                    continue;
                }

                string[] values = Regex.Split(line, @"\s{2,}").Select(value => value.Trim()).ToArray();

                //Console.WriteLine(string.Join(", ", values));
                if (values.Length == 11)
                {
                    ConsumablesJewelsBoxesItem consumablesJewelsBoxesItem = new()
                    {
                        Type = int.Parse(values[0]),
                        Slot = (SlotTypes)Enum.Parse(typeof(SlotTypes), values[1]),
                        Skill = (SkillTypes)Enum.Parse(typeof(SkillTypes), values[2]),
                        Width = int.Parse(values[3]),
                        Height = int.Parse(values[4]),
                        HaveSerial = int.Parse(values[5]) == 1,
                        HaveOption = int.Parse(values[6]) == 1,
                        DropItem = int.Parse(values[7]) == 1,
                        Name = values[8].Trim('"'),
                        Value = int.Parse(values[9]),
                        Level = int.Parse(values[10]),

                        // Level = int.Parse(values[9]),
                        // Durability = int.Parse(values[10]),
                        // Defense = int.Parse(values[9]),
                        // Durability = int.Parse(values[10]),
                        // ReqLevel = int.Parse(values[11]),
                        // ReqEnergy = int.Parse(values[12]),
                        // ReqStrength = int.Parse(values[13]),
                        // ReqDexterity = int.Parse(values[14]),
                        // ReqLeadership = int.Parse(values[15]),
                        // BuyMoney = int.Parse(values[16]),
                        // Classes = ClassManager.GetListOfClasses(int.Parse(values[19]), int.Parse(values[20]), int.Parse(values[21]), int.Parse(values[22]), int.Parse(values[23]), int.Parse(values[24]), int.Parse(values[25]))
                    };
                    consumablesJewelsBoxesItems.Add(consumablesJewelsBoxesItem);
                }
                else
                {
                    Console.WriteLine($"Error: Faltan columnas a la linea {line}");
                }

            }

            ConsumablesJewelsBoxesItems = consumablesJewelsBoxesItems;
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
        }
    }

    public static void PopulateSets()
    {
        string filePath = "data/Sets.txt";

        try
        {
            string[] lines = File.ReadAllLines(filePath);
            List<ArmorSet> sets = new();
            foreach (string line in lines)
            {
                if (line == "")
                {
                    continue;
                }
                string[] values = Regex.Split(line, @"\s{2,}").Select(value => value.Trim()).ToArray();
                Console.WriteLine(string.Join(", ", values));
                Console.WriteLine(values.Length);
                if (values.Length == 9)
                {
                    ArmorSet setItem = new()
                    {
                        Type = int.Parse(values[0]),
                        Name = values[1].Trim('"'),
                        Classes = ClassManager.GetListOfClasses(int.Parse(values[2]), int.Parse(values[3]), int.Parse(values[4]), int.Parse(values[5]), int.Parse(values[6]), int.Parse(values[7]), int.Parse(values[8]))
                    };
                    sets.Add(setItem);
                }
                else
                {
                    Console.WriteLine($"Error: Faltan columnas a la linea {line}");
                }

            }

            ArmorSets = sets;
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
        }

    }


    [GeneratedRegex("\\s{2,}")]
    private static partial Regex MyRegex();
    [GeneratedRegex("\\s{2,}")]
    private static partial Regex MyRegex1();
}