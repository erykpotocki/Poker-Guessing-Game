using System;
using UnityEngine;
using PokerProfile;

public static class PlayerProfileService
{
    private sealed class PrefsStore : IProfileStore
    {
        public string Load() => PlayerPrefs.GetString("playerProfile.v1", "");
        public void Save(string json)
        {
            PlayerPrefs.SetString("playerProfile.backup",PlayerPrefs.GetString("playerProfile.v1",""));
            PlayerPrefs.SetString("playerProfile.v1",json); PlayerPrefs.Save();
        }
    }
    private static IProfileStore store = new PrefsStore();
    private static PlayerSave current;
    public static event Action Changed;
    public static PlayerSave Data
    {
        get
        {
            if (current != null) return current;
            string json = store.Load();
            try { if (!string.IsNullOrEmpty(json)) current = JsonUtility.FromJson<PlayerSave>(json); } catch (Exception) { }
            if (current == null || current.Profile == null || current.Inventory == null || current.Wallet == null || current.Statistics == null)
            {
                current = new PlayerSave();
                current.Profile.Nickname = PlayerPrefs.GetString("lastNick","Gracz");
                string avatar = "avatar_" + Mathf.Max(0,PlayerPrefs.GetInt("avatarIndex",0));
                current.Profile.SelectedAvatarId = avatar;
                ProgressionRules.Own(current.Inventory.OwnedAvatars,avatar);
            }
            current.Profile ??= new PlayerProfile();
            current.Inventory ??= new Inventory();
            current.Wallet ??= new Wallet();
            current.Statistics ??= new Statistics();
            current.Progression ??= new Progression();
            current.Daily ??= new MissionPeriod();
            current.Weekly ??= new MissionPeriod();
            current.Wheel ??= new WheelState();
            if(current.Wheel.PendingPrize!=null && !current.Wheel.PendingPrize.IsValid)current.Wheel.PendingPrize=null;
            current.Achievements ??= new System.Collections.Generic.List<string>();
            current.Receipts ??= new System.Collections.Generic.List<string>();
            current.Opponents ??= new System.Collections.Generic.List<OpponentRecord>();
            current.PendingUnlocks ??= new System.Collections.Generic.List<PendingUnlock>();
            for (int i = 0; i < 10; i++) ProgressionRules.Own(current.Inventory.OwnedAvatars,"avatar_"+i);
            ProgressionRules.RefreshPeriods(current,DateTime.UtcNow);
            return current;
        }
    }
    public static void Save() { store.Save(JsonUtility.ToJson(Data)); Changed?.Invoke(); }
    public static bool SetNickname(string nickname)
    {
        nickname = (nickname ?? "").Trim().Replace("<", "").Replace(">", "");
        if (nickname.Length < 2 || nickname.Length > 20) return false;
        Data.Profile.Nickname = nickname; Save(); return true;
    }
    public static bool Equip(string category,string id)
    {
        if (category == "avatar" && Data.Inventory.OwnedAvatars.Contains(id)) Data.Profile.SelectedAvatarId = id;
        else if (category == "frame" && (id == "none" || Data.Inventory.OwnedFrames.Contains(id))) Data.Profile.SelectedFrameId = id;
        else if (category == "back" && Data.Inventory.OwnedCardBacks.Contains(id)) Data.Profile.SelectedCardBackId = id;
        else return false;
        Save(); return true;
    }
    public static int AvatarIndex
    {
        get {
            string id = Data.Profile.SelectedAvatarId;
            if (id != null && id.StartsWith("avatar_") && int.TryParse(id.Substring(7),out int index)) return Mathf.Max(0,index);
            AvatarDatabase db = Resources.Load<AvatarDatabase>("ProfileAvatars");
            if (db != null && db.avatars != null)
                for (int i=0;i<db.avatars.Length;i++) if (AvatarId(i,db.avatars[i]) == id) return i;
            return 0;
        }
    }
    public static string AvatarId(int index, Sprite sprite) => index < 10 ? "avatar_"+index : "download:"+sprite.name;
    public static bool ShopAvailable => false;
    public static bool CompleteMatch(string id,bool won,int bots=0,int humans=1,int durationSeconds=0)
    {
        if ((id??"").StartsWith("hotseat:",StringComparison.OrdinalIgnoreCase)) return false;
        int minutes = Mathf.Clamp(durationSeconds / 60,0,20);
        int coins = Mathf.Clamp(10 + Mathf.Clamp(bots,0,5)*10 + minutes*7,10,200);
        int xp = Mathf.Clamp(15 + Mathf.Clamp(bots,0,5)*5 + minutes*3 + (won?10:0),15,100);
        bool completed = ProgressionRules.CompleteMatch(Data,id,won,DateTime.UtcNow,coins,xp,humans>=2);
        if (completed)
        {
            Save();
        }
        return completed;
    }
    private static int StableDrop(string value,int modulo)
    {
        unchecked { int hash=17; foreach(char c in value??"") hash=hash*31+c; return (hash&int.MaxValue)%Mathf.Max(1,modulo); }
    }
    public static PendingUnlock PeekUnlock() => Data.PendingUnlocks.Count > 0 ? Data.PendingUnlocks[0] : null;
    public static void AcceptUnlock()
    {
        if (Data.PendingUnlocks.Count == 0) return;
        Data.PendingUnlocks.RemoveAt(0); Save();
    }
    public static bool BuyWithDiamonds(string category,string id,int price,string title)
    {
        if (!ShopAvailable) return false;
        if (price < 0 || Data.Wallet.RewardCurrency < price) return false;
        bool owned = category == "avatar" ? Data.Inventory.OwnedAvatars.Contains(id) :
            category == "back" ? Data.Inventory.OwnedCardBacks.Contains(id) : Data.Inventory.OwnedFrames.Contains(id);
        if (owned) return false;
        Data.Wallet.RewardCurrency -= price;
        ProgressionRules.Unlock(Data,category,id,title);
        Save(); return true;
    }
    public static void RecordOpponentMatch(string profileId,string nickname,bool localWon)
    {
        if (string.IsNullOrWhiteSpace(profileId)) profileId = "nick:" + (nickname ?? "Gracz");
        if (Data.Opponents == null) Data.Opponents = new System.Collections.Generic.List<OpponentRecord>();
        OpponentRecord record = Data.Opponents.Find(item => item.ProfileId == profileId);
        if (record == null)
        {
            record = new OpponentRecord { ProfileId = profileId };
            Data.Opponents.Add(record);
        }
        record.Nickname = string.IsNullOrWhiteSpace(nickname) ? "Gracz" : nickname;
        record.GamesTogether++;
        if (localWon) record.WinsAgainst++;
        Save();
    }
    public static OpponentRecord GetOpponent(string profileId)
    {
        if (Data.Opponents == null || string.IsNullOrWhiteSpace(profileId)) return null;
        return Data.Opponents.Find(item => item.ProfileId == profileId);
    }
    public static void CompleteRound(string id) { if (ProgressionRules.CompleteRound(Data,id,DateTime.UtcNow)) Save(); }
    public static TimeSpan SpinRemaining
    {
        get
        {
            DateTime now = DateTime.UtcNow;
            RefreshSpinCharges(now);
            long ticks = Data.Wheel.NextFreeUtcTicks - now.Ticks;
            return ticks > 0 ? TimeSpan.FromTicks(ticks) : TimeSpan.Zero;
        }
    }
    public static int SpinCharges { get { RefreshSpinCharges(DateTime.UtcNow); return Data.Wheel.Charges; } }
    public static bool CanSpin => SpinCharges > 0;
    public static float AvatarSpinChance => Data.Wheel.AvatarsWon==0?.10f:Data.Wheel.AvatarsWon==1?.05f:.10f/6f;
    public static string Spin() => Spin(out _);
    public static string Spin(out int sector)
    {
        return Spin(out sector, out _);
    }
    public static string DiamondAmount(int amount)
    {
        int last = amount % 10, lastTwo = amount % 100;
        return amount + (amount == 1 ? " diament" : last >= 2 && last <= 4 && (lastTwo < 12 || lastTwo > 14) ? " diamenty" : " diamentów");
    }
    public static string Spin(out int sector, out Sprite wonAvatar)
    {
        if(Data.Wheel.PendingPrize!=null && (!Data.Wheel.PendingPrize.IsValid || Data.Wheel.PendingPrize.Category=="frame" || ((Data.Wheel.PendingPrize.Category=="avatar" || Data.Wheel.PendingPrize.Category=="back") && !CosmeticCatalog.Get(Data.Wheel.PendingPrize.Category,Data.Wheel.PendingPrize.ItemId).Spin))){Data.Wheel.PendingPrize=null;Save();}
        if(Data.Wheel.PendingPrize!=null)
        {
            var pending=Data.Wheel.PendingPrize;sector=pending.Sector;
            // Older saves can have an empty label or an obsolete sector.
            if(pending.Category=="gold")pending.Sector=pending.Amount>50?6:3;
            if(pending.Category=="diamonds")pending.Sector=pending.Amount>3?2:1;
            sector=pending.Sector;
            wonAvatar=ResolveSpinPreview(pending);return pending.Category=="gold"?pending.Amount+" złota":pending.Category=="diamonds"?DiamondAmount(pending.Amount):pending.Category=="frame"?"Nowa ramka":pending.Category=="back"?"Nowy rewers":"Nowy avatar";
        }
        wonAvatar = null;
        sector = -1;
        DateTime now = DateTime.UtcNow;
        RefreshSpinCharges(now);
        if (Data.Wheel.Charges <= 0) return null;
        // Clockwise from the top of the supplied wheel: ?, diamond, diamond
        // bag, gold, ?, diamond, gold bag, gold. One draw drives money AND art.
        float roll=UnityEngine.Random.value;
        float avatarChance=AvatarSpinChance;
        sector=roll<avatarChance?(UnityEngine.Random.value<.5f?0:4):
            roll<avatarChance+.30f?(UnityEngine.Random.value<.8f?(UnityEngine.Random.value<.5f?1:5):2):
            (UnityEngine.Random.value<.15f?6:(UnityEngine.Random.value<.5f?3:7));
        string reward;
        var prize=new SpinPrize{Sector=sector};
        if (sector == 0 || sector == 4)
        {
            var available = new System.Collections.Generic.List<Sprite>();
            foreach (Sprite sprite in Resources.LoadAll<Sprite>("ShopAvatars"))
                if (CosmeticCatalog.Get("avatar","download:"+sprite.name).Spin && !Data.Inventory.OwnedAvatars.Contains("download:"+sprite.name)) available.Add(sprite);
            var backs=CosmeticCatalog.All("back").FindAll(o=>o.Spin&&!CosmeticCatalog.Owned("back",o.Id));
            if(backs.Count>0 && (available.Count==0||UnityEngine.Random.value<.5f))
            {
                var back=backs[UnityEngine.Random.Range(0,backs.Count)];
                prize.Category="back";prize.ItemId=back.Id;wonAvatar=back.Sprite;reward="Nowy rewers";
            }            else if (available.Count > 0)
            {
                Sprite sprite = available[UnityEngine.Random.Range(0,available.Count)];
                prize.Category="avatar";prize.ItemId="download:"+sprite.name;
                wonAvatar = sprite;
                reward = "Nowy avatar";
            }
            else
            {
                int amount = UnityEngine.Random.Range(1,4);
                prize.Category="diamonds";prize.Amount=amount;
                // With the cosmetic collection complete, land on a currency
                // sector instead of presenting currency under a question mark.
                sector=1;prize.Sector=sector;
                reward = DiamondAmount(amount);
            }
        }
        else if (sector == 1 || sector == 5 || sector == 2)
        {
            int amount = sector == 2 ? UnityEngine.Random.Range(4,7) : UnityEngine.Random.Range(1,4);
            prize.Category="diamonds";prize.Amount=amount; reward = DiamondAmount(amount);
        }
        else
        {
            int amount = sector == 6 ? UnityEngine.Random.Range(51,100) : UnityEngine.Random.Range(10,51);
            prize.Category="gold";prize.Amount=amount; reward = amount+" złota";
        }
        Data.Statistics.Spins++;
        Data.Wheel.FreeUsed = true;
        if (reward != null)
        {
            if(Data.Wheel.Charges==3)Data.Wheel.NextFreeUtcTicks=now.AddHours(4).Ticks;
            Data.Wheel.Charges--;
            prize.Label=reward;Data.Wheel.PendingPrize=prize;
            Save();
        }
        return reward;
    }
    public static Sprite ResolveSpinPreview(SpinPrize prize)
    {
        if(prize.Category=="avatar")return Resources.Load<Sprite>("ShopAvatars/"+prize.ItemId.Substring(9));
        if(prize.Category=="back")return CardBackDatabase.FindOnline(prize.ItemId);
        if(prize.Category=="frame")return LevelFrameCatalog.Resolve(prize.ItemId);
        return null;
    }
    public static void ClaimSpinPrize()
    {
        if(ProgressionRules.ClaimSpinPrize(Data))Save();
    }
    private static void RefreshSpinCharges(DateTime now)
    {
        var wheel=Data.Wheel;bool changed=false;
        if(wheel.ChargeVersion==0)
        {
            wheel.ChargeVersion=1;wheel.Charges=3;wheel.NextFreeUtcTicks=0;
            wheel.AvatarsWon=Data.Inventory.OwnedAvatars.FindAll(id=>id.StartsWith("download:")).Count;
            changed=true;
        }
        if(wheel.Charges<3 && wheel.NextFreeUtcTicks>0 && now.Ticks>=wheel.NextFreeUtcTicks)
        {
            long interval=TimeSpan.FromHours(4).Ticks;
            long recovered=1+(now.Ticks-wheel.NextFreeUtcTicks)/interval;
            wheel.Charges=(int)Math.Min(3,wheel.Charges+recovered);
            wheel.NextFreeUtcTicks=wheel.Charges==3?0:wheel.NextFreeUtcTicks+recovered*interval;
            changed=true;
        }
        if(changed)Save();
    }
    public static bool ClaimMission(bool weekly,string kind) { bool result = ProgressionRules.ClaimMission(Data,weekly,kind,DateTime.UtcNow); if (result) Save(); return result; }
    private static bool adInFlight;
    public static void RequestRewardedAd(IRewardedAdProvider provider,AdReward reward,Action<AdOutcome> done)
    {
        if (adInFlight || provider == null || !provider.IsAvailable) { done?.Invoke(AdOutcome.Unavailable); return; }
        adInFlight = true;
        string request = Guid.NewGuid().ToString("N");
        bool completed = false;
        try
        {
            provider.Show(request,outcome => {
                if (completed) return;
                completed = true; adInFlight = false;
                if (ProgressionRules.CompleteAd(Data,request,outcome,reward,DateTime.UtcNow)) Save();
                done?.Invoke(outcome);
            });
        }
        catch (Exception) { adInFlight = false; if (!completed) done?.Invoke(AdOutcome.Failed); }
    }
}
