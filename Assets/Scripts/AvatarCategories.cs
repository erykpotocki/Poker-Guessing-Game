using System.Collections.Generic;

// Categorized by visual review; stable filenames, never runtime sort positions.
public static class AvatarCategories
{
    private static readonly Dictionary<string,int> Categories=new Dictionary<string,int>
    {
        { "Obraz Codex 6 wrz 2026, 20_18_05", 4 },
        { "Obraz Codex 6 wrz 2026, 20_18_16", 4 },
        { "Obraz Codex 6 wrz 2026, 20_18_22", 4 },
        { "Obraz Codex 6 wrz 2026, 20_18_26", 4 },
        { "Obraz Codex 6 wrz 2026, 20_18_35", 4 },
        { "Obraz Codex 6 wrz 2026, 20_18_41", 4 },
        { "Obraz Codex 6 wrz 2026, 20_18_47", 4 },
        { "Obraz Codex 6 wrz 2026, 20_18_52", 4 },
        { "Obraz Codex 6 wrz 2026, 20_19_00", 4 },
        { "Obraz Codex 6 wrz 2026, 20_19_07", 4 },
        { "Obraz Codex 6 wrz 2026, 20_21_28", 1 },
        { "Obraz Codex 6 wrz 2026, 20_21_35", 1 },
        { "Obraz Codex 6 wrz 2026, 20_21_40", 1 },
        { "Obraz Codex 6 wrz 2026, 20_21_46", 1 },
        { "Obraz Codex 6 wrz 2026, 20_21_55", 1 },
        { "Obraz Codex 6 wrz 2026, 20_22_00", 1 },
        { "Obraz Codex 6 wrz 2026, 20_22_07", 1 },
        { "Obraz Codex 6 wrz 2026, 20_22_14", 1 },
        { "Obraz Codex 6 wrz 2026, 20_22_24", 1 },
        { "Obraz Codex 6 wrz 2026, 20_22_32", 1 },
        { "Obraz Codex 6 wrz 2026, 20_24_47", 1 },
        { "Obraz Codex 6 wrz 2026, 20_24_54", 1 },
        { "Obraz Codex 6 wrz 2026, 20_25_00", 1 },
        { "Obraz Codex 6 wrz 2026, 20_25_07", 1 },
        { "Obraz Codex 6 wrz 2026, 20_25_16", 1 },
        { "Obraz Codex 6 wrz 2026, 20_25_22", 1 },
        { "Obraz Codex 6 wrz 2026, 20_25_29", 1 },
        { "Obraz Codex 6 wrz 2026, 20_25_38", 1 },
        { "Obraz Codex 6 wrz 2026, 20_25_44", 1 },
        { "Obraz Codex 6 wrz 2026, 20_25_49", 1 },
        { "Obraz Codex 6 wrz 2026, 20_27_41", 2 },
        { "Obraz Codex 6 wrz 2026, 20_27_48", 2 },
        { "Obraz Codex 6 wrz 2026, 20_27_54", 2 },
        { "Obraz Codex 6 wrz 2026, 20_28_01", 2 },
        { "Obraz Codex 6 wrz 2026, 20_28_08", 2 },
        { "Obraz Codex 6 wrz 2026, 20_28_15", 2 },
        { "Obraz Codex 6 wrz 2026, 20_28_37", 2 },
        { "Obraz Codex 6 wrz 2026, 20_28_44", 2 },
        { "Obraz Codex 6 wrz 2026, 20_28_55", 2 },
        { "Obraz Codex 6 wrz 2026, 20_32_11", 4 },
        { "Obraz Codex 6 wrz 2026, 20_32_18", 4 },
        { "Obraz Codex 6 wrz 2026, 20_32_24", 4 },
        { "Obraz Codex 6 wrz 2026, 20_32_32", 4 },
        { "Obraz Codex 6 wrz 2026, 20_32_43", 4 },
        { "Obraz Codex 6 wrz 2026, 20_32_50", 4 },
        { "Obraz Codex 6 wrz 2026, 20_32_59", 4 },
        { "Obraz Codex 6 wrz 2026, 20_33_13", 4 },
        { "Obraz Codex 6 wrz 2026, 20_36_54", 3 },
        { "Obraz Codex 6 wrz 2026, 20_37_00", 3 },
        { "Obraz Codex 6 wrz 2026, 20_37_06", 3 },
        { "Obraz Codex 6 wrz 2026, 20_37_13", 3 },
        { "Obraz Codex 6 wrz 2026, 20_37_22", 3 },
        { "Obraz Codex 6 wrz 2026, 20_37_31", 3 },
        { "Obraz Codex 6 wrz 2026, 20_37_37", 3 },
        { "Obraz Codex 6 wrz 2026, 20_37_43", 3 },
        { "Obraz Codex 6 wrz 2026, 20_37_55", 3 },
    };
    public static int Category(int index,string name)
    {
        if(index<10)return 0;
        if(Categories.TryGetValue(name,out int category))return category;
        // Unity's sliced sprite names append _0 to the original filename.
        if(name.EndsWith("_0"))name=name.Substring(0,name.Length-2);
        return Categories.TryGetValue(name,out category)?category:4;
    }
}
