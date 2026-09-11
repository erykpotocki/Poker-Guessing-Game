using System;
using NUnit.Framework;
using PokerProfile;

public class ProfileLevelProgressionTests
{
    [TestCase("Q Q 9 9","dama dama 99",true)]
    [TestCase("J J","jupek dupek",true)]
    [TestCase("K K","król king",true)]
    [TestCase("Kolor ♥","serce",true)]
    [TestCase("Kolor ♦","dzwonek",true)]
    [TestCase("Kolor ♠","wino",true)]
    [TestCase("Kolor ♣","żołądź",true)]
    [TestCase("Kolor ♣","clubs",true)]
    [TestCase("Kolor ♥","wino",false)]
    [TestCase("Q Q","dama dama dama",false)]
    public void SearchUnderstandsCardAliases(string candidate,string query,bool expected)
    {
        Assert.That(HandRankPanelUI.MatchesSearch(candidate,query),Is.EqualTo(expected));
    }
    [Test]
    public void EmptyDeserializedPrizeCannotBlockTheNextSpin()
    {
        var save=new PlayerSave();save.Wheel.PendingPrize=UnityEngine.JsonUtility.FromJson<SpinPrize>("{}");
        Assert.That(save.Wheel.PendingPrize.IsValid,Is.False);
        Assert.That(ProgressionRules.ClaimSpinPrize(save),Is.False);
        Assert.That(save.Wheel.PendingPrize,Is.Null);
        Assert.That(save.Wallet.Coins,Is.Zero);
    }
    [Test]
    public void FramePrizeIsClaimedExactlyOnce()
    {
        var save=new PlayerSave();save.Wheel.PendingPrize=new SpinPrize{Category="frame",ItemId="level:10"};
        Assert.That(ProgressionRules.ClaimSpinPrize(save),Is.True);
        Assert.That(ProgressionRules.ClaimSpinPrize(save),Is.False);
        Assert.That(save.Inventory.OwnedFrames,Does.Contain("level:10"));
        Assert.That(save.Wheel.AvatarsWon,Is.EqualTo(1));
    }
    [Test]
    public void PendingSpinDoesNotGrantGoldUntilClaimAndCannotBeClaimedTwice()
    {
        var save=new PlayerSave();save.Wheel.PendingPrize=new SpinPrize{Category="gold",Amount=44};
        Assert.That(save.Wallet.Coins,Is.Zero);
        Assert.That(ProgressionRules.ClaimSpinPrize(save),Is.True);
        Assert.That(save.Wallet.Coins,Is.EqualTo(44));
        Assert.That(ProgressionRules.ClaimSpinPrize(save),Is.False);
        Assert.That(save.Wallet.Coins,Is.EqualTo(44));
    }
    [TestCase("avatar","download:test")]
    [TestCase("back","3clasic")]
    public void CosmeticsStayLockedUntilSpinClaim(string category,string id)
    {
        var save=new PlayerSave();save.Wheel.PendingPrize=new SpinPrize{Category=category,ItemId=id};
        var inventory=category=="avatar"?save.Inventory.OwnedAvatars:save.Inventory.OwnedCardBacks;
        Assert.That(inventory,Does.Not.Contain(id));
        ProgressionRules.ClaimSpinPrize(save);
        Assert.That(inventory,Does.Contain(id));
        Assert.That(save.Wheel.AvatarsWon,Is.EqualTo(1));
    }
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
        Assert.That(save.Wallet.RewardCurrency,Is.EqualTo(2));
        Assert.That(ProgressionRules.CompleteMatch(save,"online:test",false,DateTime.UtcNow,20,25),Is.False);
        Assert.That(save.Wallet.Coins,Is.EqualTo(25));
        Assert.That(Progression.LevelGold(1000),Is.EqualTo(100));
    }
    [Test]
    public void LevelTenUnlocksFrameWithoutEquippingIt()
    {
        var save=new PlayerSave();save.Progression.Experience=Progression.Threshold(10)-10;
        ProgressionRules.CompleteMatch(save,"frame-test",true,DateTime.UtcNow,20,25);
        Assert.That(save.Inventory.OwnedFrames,Does.Contain("level:10"));
        Assert.That(save.Profile.SelectedFrameId,Is.EqualTo("none"));
        Assert.That(save.Wallet.RewardCurrency,Is.EqualTo(10));
    }
}
