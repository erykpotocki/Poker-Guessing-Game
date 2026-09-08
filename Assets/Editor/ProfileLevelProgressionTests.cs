using System;
using NUnit.Framework;
using PokerProfile;

public class ProfileLevelProgressionTests
{
    [Test]
    public void ThresholdsAndNextLevelCostsAgree()
    {
        for(int level=1;level<=1000;level++)
        {
            var progression=new Progression{Experience=Progression.Threshold(level)};
            Assert.That(progression.Level,Is.EqualTo(level));
            Assert.That(progression.CurrentExperience,Is.Zero);
            Assert.That(progression.RequiredExperience,Is.EqualTo(100L+50L*(level-1)));
            progression.Experience+=progression.RequiredExperience-1;
            Assert.That(progression.Level,Is.EqualTo(level));
            progression.Experience++;
            Assert.That(progression.Level,Is.EqualTo(level+1));
        }
    }

    [Test]
    public void CompletionGrantsLevelGoldOnlyOnce_AndNeverForHotSeat()
    {
        var save=new PlayerSave();save.Progression.Experience=90;
        Assert.That(ProgressionRules.CompleteMatch(save,"hotseat:test",false,DateTime.UtcNow,20,25),Is.False);
        Assert.That(save.Progression.Experience,Is.EqualTo(90));
        Assert.That(ProgressionRules.CompleteMatch(save,"online:test",false,DateTime.UtcNow,20,25),Is.True);
        Assert.That(save.Progression.Level,Is.EqualTo(2));
        Assert.That(save.Progression.CurrentExperience,Is.EqualTo(15));
        Assert.That(save.Wallet.Coins,Is.EqualTo(25));
        Assert.That(ProgressionRules.CompleteMatch(save,"online:test",false,DateTime.UtcNow,20,25),Is.False);
        Assert.That(save.Wallet.Coins,Is.EqualTo(25));
        Assert.That(Progression.LevelGold(1000),Is.EqualTo(100));
    }
}
