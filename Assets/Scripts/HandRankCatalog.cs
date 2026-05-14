using System.Collections.Generic;
using UnityEngine;

public static class HandRankCatalog
{
    private static readonly List<string> orderedIds = new List<string>();
    private static readonly Dictionary<string, int> indexById = new Dictionary<string, int>();
    private static readonly Dictionary<string, string> displayById = new Dictionary<string, string>();

    static HandRankCatalog()
    {
        Build();

        if (orderedIds.Count != 83)
        {
            Debug.LogError($"HandRankCatalog: liczba układów = {orderedIds.Count}, a powinno być 83.");
        }
    }

    public static int Count => orderedIds.Count;

    public static bool Contains(string handId)
    {
        return !string.IsNullOrEmpty(handId) && indexById.ContainsKey(handId);
    }

    public static int GetIndex(string handId)
    {
        if (string.IsNullOrEmpty(handId))
            return -1;

        return indexById.TryGetValue(handId, out int index) ? index : -1;
    }

    public static string GetDisplayName(string handId)
    {
        if (string.IsNullOrEmpty(handId))
            return string.Empty;

        return displayById.TryGetValue(handId, out string display) ? display : handId;
    }

    public static bool CanBeat(string candidateId, string currentId)
    {
        if (!Contains(candidateId))
            return false;

        if (string.IsNullOrEmpty(currentId))
            return true;

        if (!Contains(currentId))
            return true;

        return GetIndex(candidateId) > GetIndex(currentId);
    }

    public static bool IsSame(string a, string b)
    {
        return a == b;
    }

    public static List<string> GetAllIds()
    {
        return new List<string>(orderedIds);
    }

    private static void Build()
    {
        string[] ranks = { "9", "10", "J", "Q", "K", "A" };
        int strength = 0;

        foreach (string rank in ranks)
        {
            Add($"HIGH_{rank}", rank, strength++);
        }

        foreach (string rank in ranks)
        {
            Add($"PAIR_{rank}", $"{rank} {rank}", strength++);
        }

        for (int i = 0; i < ranks.Length - 1; i++)
        {
            for (int j = i + 1; j < ranks.Length; j++)
            {
                Add(
                    $"TWOPAIR_{ranks[i]}_{ranks[j]}",
                    $"{ranks[i]} {ranks[i]} {ranks[j]} {ranks[j]}",
                    strength++
                );
            }
        }

        Add("STRAIGHT_SMALL", "9 10 J Q K", strength++);
        Add("STRAIGHT_BIG", "10 J Q K A", strength++);

        foreach (string rank in ranks)
        {
            Add($"TRIPS_{rank}", $"{rank} {rank} {rank}", strength++);
        }

        for (int t = 0; t < ranks.Length; t++)
        {
            for (int p = 0; p < ranks.Length; p++)
            {
                if (t == p)
                    continue;

                Add(
                    $"FULL_{ranks[t]}_{ranks[p]}",
                    $"{ranks[t]} {ranks[t]} {ranks[t]} {ranks[p]} {ranks[p]}",
                    strength++
                );
            }
        }

        Add("FLUSH_DIAMOND", "Kolor ♦", strength);
        Add("FLUSH_HEART", "Kolor ♥", strength);
        Add("FLUSH_CLUB", "Kolor ♣", strength);
        strength++;

        Add("FLUSH_SPADE", "Kolor ♠", strength++);

        foreach (string rank in ranks)
        {
            Add($"QUADS_{rank}", $"{rank} {rank} {rank} {rank}", strength++);
        }

        Add("POKER_SMALL_DIAMOND", "Mały poker ♦", strength);
        Add("POKER_SMALL_HEART", "Mały poker ♥", strength);
        Add("POKER_SMALL_CLUB", "Mały poker ♣", strength);
        strength++;

        Add("POKER_SMALL_SPADE", "Mały poker ♠", strength++);

        Add("POKER_BIG_DIAMOND", "Duży poker ♦", strength);
        Add("POKER_BIG_HEART", "Duży poker ♥", strength);
        Add("POKER_BIG_CLUB", "Duży poker ♣", strength);
        strength++;

        Add("POKER_BIG_SPADE", "Duży poker ♠", strength++);
    }

    private static void Add(string id, string displayName, int strength)
    {
        if (indexById.ContainsKey(id))
        {
            Debug.LogError($"HandRankCatalog: duplikat id = {id}");
            return;
        }

        orderedIds.Add(id);
        indexById[id] = strength;
        displayById[id] = displayName;
    }
}