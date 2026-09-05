using System;
using System.Collections.Generic;

public static class MultiplayerHandRules
{
    private static readonly string[] Ranks = { "9", "10", "J", "Q", "K", "A" };

    public static string NextHigher(string currentId)
    {
        foreach (string id in HandRankCatalog.GetAllIds())
            if (HandRankCatalog.CanBeat(id, currentId))
                return id;
        return null;
    }

    // Returns only cards needed for this declaration, or the available subset
    // when incomplete. This also drives the visual explanation after a check.
    public static List<CardSpriteEntry> MatchingCards(string id, IList<CardSpriteEntry> cards, out bool complete)
    {
        List<CardSpriteEntry> result = new List<CardSpriteEntry>();
        complete = false;
        if (!HandRankCatalog.Contains(id) || cards == null) return result;
        string[] parts = id.Split('_');
        CardSuit? suit = null;
        if (parts[0] == "FLUSH" || parts[0] == "POKER")
        {
            switch (parts[parts.Length - 1])
            {
                case "DIAMOND": suit = CardSuit.Karo; break;
                case "HEART": suit = CardSuit.Kier; break;
                case "CLUB": suit = CardSuit.Trefl; break;
                case "SPADE": suit = CardSuit.Pik; break;
            }
        }
        Dictionary<CardRank, int> needed = new Dictionary<CardRank, int>();
        if (parts[0] == "FLUSH")
        {
            foreach (CardSpriteEntry card in cards)
                if (card != null && card.suit == suit && result.Count < 5)
                    result.Add(card);
            complete = result.Count == 5;
            return result;
        }
        if (parts[0] == "STRAIGHT" || parts[0] == "POKER")
        {
            int first = parts[1] == "SMALL" ? 0 : 1;
            for (int rank = first; rank < first + 5; rank++)
                needed[(CardRank)rank] = 1;
        }
        else
        {
            int amount = parts[0] == "HIGH" ? 1 : parts[0] == "PAIR" ? 2 :
                parts[0] == "TRIPS" || parts[0] == "FULL" ? 3 :
                parts[0] == "QUADS" ? 4 : 2;
            needed[(CardRank)Array.IndexOf(Ranks, parts[1])] = amount;
            if (parts[0] == "TWOPAIR" || parts[0] == "FULL")
                needed[(CardRank)Array.IndexOf(Ranks, parts[2])] = 2;
        }
        int totalNeeded = 0;
        foreach (int amount in needed.Values) totalNeeded += amount;
        foreach (CardSpriteEntry card in cards)
        {
            if (card == null || (suit.HasValue && card.suit != suit.Value)) continue;
            if (needed.TryGetValue(card.rank, out int amount) && amount > 0)
            {
                result.Add(card);
                needed[card.rank] = amount - 1;
            }
        }
        complete = result.Count == totalNeeded;
        return result;
    }

    public static string Strongest(IList<CardSpriteEntry> cards)
    {
        string best = null;
        foreach (string id in HandRankCatalog.GetAllIds())
        {
            MatchingCards(id, cards, out bool complete);
            if (complete && HandRankCatalog.CanBeat(id, best)) best = id;
        }
        return best;
    }
}
