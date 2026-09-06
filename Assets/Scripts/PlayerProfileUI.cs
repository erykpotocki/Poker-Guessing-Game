using System;
using PokerProfile;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public sealed class PlayerProfileUI : MonoBehaviour
{
    private RectTransform root, body;
    private string message = "";
    public static void Show(Canvas canvas)
    {
        if (canvas.rootCanvas.transform.Find("PlayerProfileOverlay") != null) return;
        GameObject obj = new GameObject("PlayerProfileOverlay",typeof(RectTransform),typeof(Image),typeof(Canvas),typeof(GraphicRaycaster),typeof(PlayerProfileUI));
        obj.transform.SetParent(canvas.rootCanvas.transform,false);
        RectTransform rect = obj.transform as RectTransform;
        rect.anchorMin = Vector2.zero; rect.anchorMax = Vector2.one; rect.offsetMin = rect.offsetMax = Vector2.zero;
        obj.GetComponent<Image>().color = new Color(.025f,.032f,.025f,.99f);
        Canvas modal = obj.GetComponent<Canvas>(); modal.overrideSorting = true; modal.sortingOrder = 400;
        obj.GetComponent<PlayerProfileUI>().Build();
    }
    private static RectTransform Rect(string name,Transform parent,float x,float y,float width,float height)
    {
        RectTransform rect = new GameObject(name,typeof(RectTransform)).GetComponent<RectTransform>();
        rect.SetParent(parent,false); rect.anchorMin = rect.anchorMax = new Vector2(0f,1f); rect.pivot = new Vector2(0f,1f);
        rect.anchoredPosition = new Vector2(x,-y); rect.sizeDelta = new Vector2(width,height); return rect;
    }
    private static TMP_Text Text(Transform parent,string value,float x,float y,float width,float height,float size = 32)
    {
        TMP_Text text = Rect("Label",parent,x,y,width,height).gameObject.AddComponent<TextMeshProUGUI>();
        text.text = value; text.richText = false; text.fontSize = size;
        text.color = new Color(1f,.91f,.7f); text.raycastTarget = false;
        text.alignment = TextAlignmentOptions.MidlineLeft; return text;
    }
    private static Button Button(Transform parent,string value,float x,float y,float width,float height,Action action,bool enabled = true)
    {
        RectTransform rect = Rect("ProfileAction",parent,x,y,width,height);
        Image image = rect.gameObject.AddComponent<Image>();
        Button button = rect.gameObject.AddComponent<Button>(); button.targetGraphic = image;
        TMP_Text text = Text(rect,value,0,0,width,height,30); text.alignment = TextAlignmentOptions.Center;
        button.interactable = enabled; button.onClick.AddListener(()=>action()); PokerButtonTheme.ApplyTo(button); return button;
    }
    private void Build()
    {
        foreach (Transform child in transform) { child.gameObject.SetActive(false); Destroy(child.gameObject); }
        root = transform as RectTransform;
        Canvas.ForceUpdateCanvases();
        float sx = root.rect.width / Mathf.Max(1,Screen.width), sy = root.rect.height / Mathf.Max(1,Screen.height);
        float left = Screen.safeArea.xMin*sx+24f, top = (Screen.height-Screen.safeArea.yMax)*sy+24f;
        float width = Screen.safeArea.width*sx-48f;
        var data = PlayerProfileService.Data;
        ProgressionRules.RefreshPeriods(data,DateTime.UtcNow);
        Text(root,"MÓJ PROFIL",left,top,width-240,72,42);
        Button(root,"ZAMKNIJ",left+width-230,top,230,72,()=>Destroy(gameObject));
        Text(root,$"Monety: {data.Wallet.Coins}    Żetony: {data.Wallet.RewardCurrency}    Poziom: {data.Progression.Level}",left,top+80,width,64,30);
        RectTransform inputRect = Rect("ProfileNickname",root,left,top+154,width,86);
        inputRect.gameObject.AddComponent<Image>().color = new Color(.13f,.12f,.09f);
        TMP_InputField input = inputRect.gameObject.AddComponent<TMP_InputField>();
        TMP_Text label = Text(inputRect,"",22,8,width-44,70,36);
        input.textViewport = label.rectTransform; input.textComponent = label;
        input.text = data.Profile.Nickname; input.characterLimit = 20;
        input.onFocusSelectAll = false; input.resetOnDeActivation = false;
        input.richText = false; input.shouldHideSoftKeyboard = false;
        input.lineType = TMP_InputField.LineType.SingleLine;
        input.onEndEdit.AddListener(value=> { if (!PlayerProfileService.SetNickname(value)) input.SetTextWithoutNotify(PlayerProfileService.Data.Profile.Nickname); });

        RectTransform view = Rect("ProfileScroll",root,left,top+266,width,Mathf.Max(180,Screen.safeArea.height*sy-320));
        view.gameObject.AddComponent<Image>().color = Color.clear;
        view.gameObject.AddComponent<RectMask2D>();
        ScrollRect scroll = view.gameObject.AddComponent<ScrollRect>();
        body = Rect("Content",view,0,0,width,1200); scroll.viewport = view; scroll.content = body;
        scroll.horizontal = false; scroll.vertical = true; scroll.movementType = ScrollRect.MovementType.Clamped; scroll.scrollSensitivity = 65;
        float y = 0f;
        if (!string.IsNullOrEmpty(message)) { Text(body,message,8,y,width-16,72); y += 80; }
        Text(body,$"Ukończone gry: {data.Statistics.GamesPlayed}   Wygrane: {data.Statistics.GamesWon}",8,y,width-16,64); y += 72;
        Text(body,"AVATARY",8,y,width-16,64,38); y += 70;
        AvatarDatabase avatars = Resources.Load<AvatarDatabase>("ProfileAvatars");
        if (avatars != null && avatars.avatars != null)
            for (int i=0;i<avatars.avatars.Length;i++)
            {
                string id = "avatar_"+i;
                ItemRow("avatar",id,"Avatar "+(i+1),avatars.avatars[i],data.Inventory.OwnedAvatars.Contains(id),data.Profile.SelectedAvatarId==id,ref y,width);
            }
        Text(body,"RAMKI",8,y,width-16,64,38); y += 70;
        ItemRow("frame","none","Bez ramki",null,true,data.Profile.SelectedFrameId=="none",ref y,width);
        ItemRow("frame","classic_wood","CLASSIC WOOD · ukończ 1 grę",Resources.Load<Sprite>("Cosmetics/ClassicWood"),data.Inventory.OwnedFrames.Contains("classic_wood"),data.Profile.SelectedFrameId=="classic_wood",ref y,width);
        Text(body,"REWERSY",8,y,width-16,64,38); y += 70;
        CardBackDatabase backs = gameObject.GetComponent<CardBackDatabase>();
        if (backs == null) backs = gameObject.AddComponent<CardBackDatabase>();
        for (int i=0;i<backs.BackCount;i++)
        {
            Sprite sprite = backs.GetBackSprite(i); string id = sprite.texture.name;
            ItemRow("back",id,id.Replace("HotSeatBack_",""),sprite,data.Inventory.OwnedCardBacks.Contains(id),data.Profile.SelectedCardBackId==id,ref y,width);
        }
        Text(body,"OSIĄGNIĘCIA",8,y,width-16,64,38); y += 70;
        Achievement("Pierwsza gra → CLASSIC WOOD",data.Statistics.GamesPlayed,1,ref y,width);
        Achievement("10 gier → avatar",data.Statistics.GamesPlayed,10,ref y,width);
        Achievement("50 gier → avatar",data.Statistics.GamesPlayed,50,ref y,width);
        Achievement("100 gier → rewers",data.Statistics.GamesPlayed,100,ref y,width);
        Achievement("10 wygranych → 150 monet",data.Statistics.GamesWon,10,ref y,width);
        Achievement("10 reklam → 5 żetonów",data.Statistics.AdsWatched,10,ref y,width);
        Achievement("10 spinów → 100 monet",data.Statistics.Spins,10,ref y,width);
        for (int period=0;period<2;period++)
        {
            bool weekly = period==1; MissionPeriod missions = weekly ? data.Weekly : data.Daily;
            Text(body,weekly ? "MISJE TYGODNIOWE" : "MISJE DZIENNE",8,y,width-16,64,38); y += 70;
            string[] kinds = {"games","wins","rounds"}; string[] names = {"Gry","Wygrane","Rundy"};
            int[] progress = {data.Statistics.GamesPlayed-missions.GamesBaseline,data.Statistics.GamesWon-missions.WinsBaseline,data.Statistics.RoundsPlayed-missions.RoundsBaseline};
            int[] targets = weekly ? new[]{10,3,30} : new[]{2,1,5};
            for (int k=0;k<3;k++)
            {
                string kind=kinds[k]; bool claimed=missions.Claimed.Contains(kind);
                Text(body,names[k]+": "+Mathf.Min(progress[k],targets[k])+" / "+targets[k],8,y,width-260,72);
                Button(body,claimed?"ODEBRANO":"ODBIERZ",width-250,y,242,72,()=>{ PlayerProfileService.ClaimMission(weekly,kind); Build(); },!claimed && progress[k]>=targets[k]); y+=84;
            }
        }
        Text(body,"DZIENNE LOSOWANIE",8,y,width-16,64,38); y+=70;
        Button(body,data.Wheel.FreeUsed?"DARMOWY SPIN WYKORZYSTANY":"ODBIERZ DARMOWY SPIN",8,y,width-16,80,()=>{ message=PlayerProfileService.Spin()??"Kolejny darmowy spin jutro."; Build(); },!data.Wheel.FreeUsed); y+=96;
        Text(body,"Dodatkowe spiny za reklamy: niedostępne.\nIntegracja reklam nie jest uruchomiona.",8,y,width-16,110,28); y+=122;
        body.sizeDelta = new Vector2(width,y);
    }
    private void Achievement(string title,int value,int target,ref float y,float width)
    { Text(body,title+"   "+Mathf.Min(value,target)+" / "+target,8,y,width-16,76,28); y+=84; }
    private void ItemRow(string category,string id,string title,Sprite sprite,bool owned,bool selected,ref float y,float width)
    {
        if (sprite != null)
        {
            Image image = Rect("Cosmetic",body,8,y+8,100,100).gameObject.AddComponent<Image>();
            image.sprite=sprite; image.preserveAspect=true; image.raycastTarget=false;
        }
        Text(body,title,126,y+4,width-398,110,28);
        Button(body,selected?"WYBRANO":owned?"ZAŁÓŻ":"ZABLOKOWANE",width-260,y+20,250,76,()=>{ PlayerProfileService.Equip(category,id); Build(); },owned&&!selected);
        y+=126;
    }
}
