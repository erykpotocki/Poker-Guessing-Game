using System.Collections.Generic;
public static class AdvancedBotOdds
{
    public static float Estimate(string hand,List<CardSpriteEntry> own,int total)
    {
        var deck=new List<CardSpriteEntry>();
        foreach(CardSuit suit in System.Enum.GetValues(typeof(CardSuit)))
            foreach(CardRank rank in System.Enum.GetValues(typeof(CardRank)))
                if(!own.Exists(c=>c.suit==suit&&c.rank==rank))deck.Add(new CardSpriteEntry{suit=suit,rank=rank});
        var rng=new System.Random();int hits=0;
        for(int trial=0;trial<96;trial++)
        {
            var remaining=new List<CardSpriteEntry>(deck);var sample=new List<CardSpriteEntry>(own);
            while(sample.Count<total&&remaining.Count>0){int i=rng.Next(remaining.Count);sample.Add(remaining[i]);remaining.RemoveAt(i);}
            MultiplayerHandRules.MatchingCards(hand,sample,out bool exists);if(exists)hits++;
        }
        return hits/96f;
    }
}
