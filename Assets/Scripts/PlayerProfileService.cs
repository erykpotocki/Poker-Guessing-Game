using System;
using UnityEngine;
using PokerProfile;

public static class PlayerProfileService
{
#if UNITY_WEBGL && !UNITY_EDITOR
    [System.Runtime.InteropServices.DllImport("__Internal")] private static extern string PokerLoadProfile();
    [System.Runtime.InteropServices.DllImport("__Internal")] private static extern int PokerSaveProfile(string json);
#endif
    private sealed class PrefsStore : IProfileStore
    {
        public string Load()
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            string browserSave = PokerLoadProfile();
            if (!string.IsNullOrEmpty(browserSave)) return browserSave;
#endif
            return PlayerPrefs.GetString("playerProfile.v1", "");
        }
        public void Save(string json)
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            // Synchronous browser copy survives closing the tab before IndexedDB finishes.
            if (PokerSaveProfile(json) == 0) Debug.LogWarning("Zapis przeglądarki niedostępny; używam zapisu Unity.");
#endif
            PlayerPrefs.SetString("playerProfile.backup",PlayerPrefs.GetString("playerProfile.v1",""));
            PlayerPrefs.SetString("playerProfile.v1",json); PlayerPrefs.Save();
        }
    }
    private static IProfileStore store = new PrefsStore();
    private static PlayerSave current;
    public static event Action Changed;
    public static event Action PurchaseCompleted;
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
            if(current.Progression.CurveVersion==0)
            {
                long oldXp=Math.Max(0,current.Progression.Experience);
                int oldLevel=1+(int)(oldXp/100);
                current.Progression.Experience=Progression.Threshold(oldLevel)+(oldXp%100)*(100L+50L*(oldLevel-1))/100;
                current.Progression.CurveVersion=1;
            }
            current.Daily ??= new MissionPeriod();
            current.Weekly ??= new MissionPeriod();
            current.Wheel ??= new WheelState();
            current.Achievements ??= new System.Collections.Generic.List<string>();
            current.Receipts ??= new System.Collections.Generic.List<string>();
            current.Opponents ??= new System.Collections.Generic.List<OpponentRecord>();
            current.PendingUnlocks ??= new System.Collections.Generic.List<PendingUnlock>();
            // Old wheel rewards used the global presentation queue. Keep the
            // owned items, but discard their duplicate notification on upgrade.
            current.PendingUnlocks.RemoveAll(item => item.Category == "avatar" &&
                (item.ItemId ?? "").StartsWith("download:") && string.IsNullOrEmpty(item.Source));
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
        ProgressionRules.Unlock(Data,category,id,title,"shop");
        Save();
        PurchaseCompleted?.Invoke();
        return true;
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
            ClampTestSpinCooldown(now);
            long ticks = Data.Wheel.NextFreeUtcTicks - now.Ticks;
            return ticks > 0 ? TimeSpan.FromTicks(ticks) : TimeSpan.Zero;
        }
    }
    public static bool CanSpin => SpinRemaining <= TimeSpan.Zero;
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
        wonAvatar = null;
        sector = -1;
        DateTime now = DateTime.UtcNow;
        ClampTestSpinCooldown(now);
        if (Data.Wheel.NextFreeUtcTicks > now.Ticks) return null;
        // Clockwise from the top of the supplied wheel: ?, diamond, diamond
        // bag, gold, ?, diamond, gold bag, gold. One draw drives money AND art.
        sector = UnityEngine.Random.Range(0,8);
        string reward;
        if (sector == 0 || sector == 4)
        {
            var available = new System.Collections.Generic.List<Sprite>();
            foreach (Sprite sprite in Resources.LoadAll<Sprite>("ShopAvatars"))
                if (!Data.Inventory.OwnedAvatars.Contains("download:"+sprite.name)) available.Add(sprite);
            if (available.Count > 0)
            {
                Sprite sprite = available[UnityEngine.Random.Range(0,available.Count)];
                ProgressionRules.Own(Data.Inventory.OwnedAvatars,"download:"+sprite.name);
                wonAvatar = sprite;
                reward = "Nowy avatar";
            }
            else
            {
                int amount = UnityEngine.Random.Range(1,4);
                Data.Wallet.RewardCurrency += amount;
                reward = DiamondAmount(amount);
            }
        }
        else if (sector == 1 || sector == 5 || sector == 2)
        {
            int amount = sector == 2 ? UnityEngine.Random.Range(4,7) : UnityEngine.Random.Range(1,4);
            Data.Wallet.RewardCurrency += amount; reward = DiamondAmount(amount);
        }
        else
        {
            int amount = sector == 6 ? UnityEngine.Random.Range(51,100) : UnityEngine.Random.Range(10,51);
            Data.Wallet.Coins += amount; reward = amount+" złota";
        }
        Data.Statistics.Spins++;
        Data.Wheel.FreeUsed = true;
        if (reward != null)
        {
            // Temporary tuning value for rapid testing of the reward wheel.
            Data.Wheel.NextFreeUtcTicks = now.AddMinutes(1).Ticks;
            Save();
        }
        return reward;
    }
    private static void ClampTestSpinCooldown(DateTime now)
    {
        long testMaximum = now.AddMinutes(1).Ticks;
        if (Data.Wheel.NextFreeUtcTicks <= testMaximum) return;
        Data.Wheel.NextFreeUtcTicks = testMaximum;
        Save();
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
