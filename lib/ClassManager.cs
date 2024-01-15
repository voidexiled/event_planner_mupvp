using SharpDX.Direct3D11;

public static class ClassManager
{

    private static ClassTypes GetClassEvolution(ClassTypes classTypes, int evolution)
    {
        return classTypes switch
        {
            ClassTypes.DARK_WIZARD => evolution switch
            {
                1 => ClassTypes.DARK_WIZARD,
                2 => ClassTypes.SOUL_MASTER,
                3 => ClassTypes.GRAND_MASTER,
                4 => ClassTypes.SOUL_WIZARD,
                _ => ClassTypes.DARK_WIZARD,
            },
            ClassTypes.DARK_KNIGHT => evolution switch
            {
                1 => ClassTypes.DARK_KNIGHT,
                2 => ClassTypes.BLADE_KNIGHT,
                3 => ClassTypes.BLADE_MASTER,
                4 => ClassTypes.DRAGON_KNIGHT,
                _ => ClassTypes.DARK_KNIGHT,
            },
            ClassTypes.FAIRY_ELF => evolution switch
            {
                1 => ClassTypes.FAIRY_ELF,
                2 => ClassTypes.MUSE_ELF,
                3 => ClassTypes.HIGH_ELF,
                4 => ClassTypes.NOBLE_ELF,
                _ => ClassTypes.FAIRY_ELF,
            },
            ClassTypes.MAGIC_GLADIATOR => evolution switch
            {
                1 => ClassTypes.MAGIC_GLADIATOR,
                2 => ClassTypes.DUEL_MASTER,
                3 => ClassTypes.MAGIC_KNIGHT,
                _ => ClassTypes.MAGIC_GLADIATOR,
            },
            /* TODO: AGREGAR LAS DEMAS CLASES */
            // case ClassTypes.DARK_LORD:
            //     switch (evolution)
            //     {
            //         case 1:
            //             return ClassTypes.LORD_EMPEROR;
            //         default:
            //             return ClassTypes.DARK_LORD;
            //     }
            // case ClassTypes.SUMMONER:
            //     switch (evolution)
            //     {
            //         case 1:
            //             return ClassTypes.BLOODY_SUMMONER;
            //         case 2:
            //             return ClassTypes.DIMENSION_MASTER;
            //         default:
            //             return ClassTypes.SUMMONER;
            //     }
            // case ClassTypes.RAGE_FIGHTER:
            //     switch (evolution)
            //     {
            //         case 1:
            //             return ClassTypes.FIST_MASTER;
            //         case 2:
            //             return ClassTypes.FIST_MASTER;
            //         default:
            //             return ClassTypes.RAGE_FIGHTER;
            //     }
            _ => ClassTypes.NONE,
        };
    }
    public static List<ClassTypes> GetListOfClasses(int DW, int DK, int FE, int MG, int DL, int SU, int RF)
    {
        List<ClassTypes> Classes = new List<ClassTypes>();
        if (DW != 0)
        {
            ClassTypes classType = GetClassEvolution(ClassTypes.DARK_WIZARD, DW);
            Classes.Add(classType);
        }
        if (DK != 0)
        {
            ClassTypes classType = GetClassEvolution(ClassTypes.DARK_KNIGHT, DK);
            Classes.Add(classType);
        }
        if (FE != 0)
        {
            ClassTypes classType = GetClassEvolution(ClassTypes.FAIRY_ELF, FE);
            Classes.Add(classType);
        }
        if (MG != 0)
        {
            ClassTypes classType = GetClassEvolution(ClassTypes.MAGIC_GLADIATOR, MG);
            Classes.Add(classType);
        }
        /* TODO: AGREGAR LAS DEMAS CLASES */
        // if (DL != 0)
        // {
        //     // Agrega la lógica para Dark Lord
        //     ClassTypes classType = GetClassEvolution(ClassTypes.DARK_LORD, DL);
        //     Classes.Add(classType);
        // }
        // if (SU != 0)
        // {
        //     // Agrega la lógica para Summoner
        //     ClassTypes classType = GetClassEvolution(ClassTypes.SUMMONER, SU);
        //     Classes.Add(classType);
        // }
        // if (RF != 0)
        // {
        //     // Agrega la lógica para Rage Fighter
        //     ClassTypes classType = GetClassEvolution(ClassTypes.RAGE_FIGHTER, RF);
        //     Classes.Add(classType);
        // }
        return Classes;
    }
}