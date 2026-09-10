using System;
using System.Collections.Generic;
using UnityEngine;
using PokerProfile;

public static class CosmeticCatalog
{
    public sealed class Offer
    {
        public string Category, Id, Title;
        public Sprite Sprite;
        public int Gold, Diamonds;
        public bool Both, Spin;
        public bool Purchasable => Gold > 0 || Diamonds > 0;
    }
    public static Offer Get(string category,string id)
    {
        var o=new Offer{Category=category,Id=id,Title=id};
        if(category=="avatar" && id.StartsWith("download:"))
        {
            string name=id.Substring(9); o.Sprite=Resources.Load<Sprite>("ShopAvatars/"+name);
            int group=AvatarCategories.Category(10,name);
            o.Title=group==1?"Zwierzęta":group==2?"Halloween":group==3?"Wakacje":"Pokerzyści";
            if(group==1){o.Gold=5000;o.Spin=true;}
            else if(group==2||group==3)o.Gold=1000;
            else if(name.Contains("20_32_")||name.Contains("20_33_")){o.Gold=2000;o.Spin=true;}
            else o.Diamonds=50;
        }
        else if(category=="back")
        {
            o.Sprite=CardBackDatabase.FindOnline(id);o.Title="Rewers "+id;
            if(id.EndsWith("prestige") && int.TryParse(id.Replace("prestige",""),out int tier))
            {o.Gold=tier<=6?5000:tier<=14?7500:10000;o.Diamonds=tier<=6?500:tier<=14?750:1000;o.Both=tier>=15;o.Spin=tier<15;}
            else if(id.Contains("wood")){o.Gold=250;o.Spin=true;}
            else if(id.Contains("clasic")){o.Gold=50;o.Spin=true;}
            else if(id.Length==1){o.Gold=1000;o.Spin=true;}
            else o.Spin=true;
        }
        else if(category=="frame" && id.StartsWith("level:") && int.TryParse(id.Substring(6),out int level))
        {o.Diamonds=level*10;o.Sprite=LevelFrameCatalog.Resolve(id);o.Title="Ramka · poziom "+level;}
        return o;
    }
    public static bool Owned(string category,string id)
    {
        var d=PlayerProfileService.Data;
        return category=="avatar"?d.Inventory.OwnedAvatars.Contains(id):category=="back"?d.Inventory.OwnedCardBacks.Contains(id):id=="none"||d.Inventory.OwnedFrames.Contains(id);
    }
    public static bool CanUnlockFrame(PlayerSave d,string id)
    {
        foreach(int level in LevelFrameCatalog.Levels)
            if(d.Progression.Level<level&&!d.Inventory.OwnedFrames.Contains("level:"+level))return id=="level:"+level;
        return false;
    }
    public static List<Offer> All(string category)
    {
        var result=new List<Offer>();
        if(category=="avatar")foreach(var s in Resources.LoadAll<Sprite>("ShopAvatars"))result.Add(Get(category,"download:"+s.name));
        if(category=="back")foreach(var s in CardBackDatabase.OnlineSprites)result.Add(Get(category,s.name));
        if(category=="frame")foreach(int level in LevelFrameCatalog.Levels)result.Add(Get(category,"level:"+level));
        return result;
    }
}
