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
            current.Achievements ??= new System.Collections.Generic.List<string>();
            current.Receipts ??= new System.Collections.Generic.List<string>();
            current.Opponents ??= new System.Collections.Generic.List<OpponentRecord>();
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
        get { string id = Data.Profile.SelectedAvatarId; return id != null && id.StartsWith("avatar_") && int.TryParse(id.Substring(7),out int index) ? Mathf.Max(0,index) : 0; }
    }
    public static bool CompleteMatch(string id,bool won)
    {
        bool completed = ProgressionRules.CompleteMatch(Data,id,won,DateTime.UtcNow);
        if (completed) Save();
        return completed;
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
            long ticks = Data.Wheel.NextFreeUtcTicks - DateTime.UtcNow.Ticks;
            return ticks > 0 ? TimeSpan.FromTicks(ticks) : TimeSpan.Zero;
        }
    }
    public static bool CanSpin => SpinRemaining <= TimeSpan.Zero;
    public static string Spin()
    {
        DateTime now = DateTime.UtcNow;
        if (Data.Wheel.NextFreeUtcTicks > now.Ticks) return null;
        Data.Wheel.FreeUsed = false;
        string reward = ProgressionRules.Spin(Data,UnityEngine.Random.Range(0,1000),now);
        if (reward != null)
        {
            Data.Wheel.NextFreeUtcTicks = now.AddHours(1).Ticks;
            Save();
        }
        return reward;
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
