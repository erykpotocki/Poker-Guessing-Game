using System;
using PokerProfile;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public sealed class PlayerProfileUI : MonoBehaviour
{
    private RectTransform root, body;
    private string message = "";
    private string section = "avatar";
    public static void Show(Canvas canvas)
    {
        if (canvas.rootCanvas.transform.Find("PlayerProfileOverlay") != null) return;
        GameObject obj = new GameObject("PlayerProfileOverlay",typeof(RectTransform),typeof(Image),typeof(Canvas),typeof(GraphicRaycaster),typeof(PlayerProfileUI));
        obj.transform.SetParent(canvas.rootCanvas.transform,false);
        RectTransform rect = obj.transform as RectTransform;
        rect.anchorMin = Vector2.zero; rect.anchorMax = Vector2.one; rect.offsetMin = rect.offsetMax = Vector2.zero;
        PortraitMenuTopBar.ApplyOverlayInset(rect);
        obj.GetComponent<Image>().color = new Color(.025f,.032f,.025f,.99f);
        Canvas modal = obj.GetComponent<Canvas>(); modal.overrideSorting = true; modal.sortingOrder = 620;
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
        AvatarDatabase headerAvatars = Resources.Load<AvatarDatabase>("ProfileAvatars");
        if (headerAvatars != null && headerAvatars.avatars != null && headerAvatars.avatars.Length > 0)
        {
            int selected = Mathf.Clamp(PlayerProfileService.AvatarIndex,0,headerAvatars.avatars.Length-1);
            Image currentAvatar = Rect("CurrentAvatar",root,left,top,88,88).gameObject.AddComponent<Image>();
            currentAvatar.sprite = headerAvatars.avatars[selected]; currentAvatar.preserveAspect = true;
            AvatarCircleUtility.Apply(currentAvatar);
            currentAvatar.raycastTarget = false;
        }
        Text(root,"MÓJ PROFIL",left+104,top,width-344,72,42);
        Button(root,"ZAMKNIJ",left+width-230,top,230,72,()=>Destroy(gameObject));
        Text(root,$"Monety: {data.Wallet.Coins}    Diamenty: {data.Wallet.RewardCurrency}    Poziom: {data.Progression.Level}",left,top+80,width,64,30);
        TMP_Text signature=Text(root,"© Eryk Potocki",left+(width-320f)*.5f,root.rect.height-28,320,22,16);
        signature.alignment=TextAlignmentOptions.Bottom;signature.color=new Color(1f,.86f,.62f,.36f);
        RectTransform inputRect = Rect("ProfileNickname",root,left,top+154,width,86);
        inputRect.gameObject.AddComponent<Image>().color = new Color(.13f,.12f,.09f);
        TMP_InputField input = inputRect.gameObject.AddComponent<TMP_InputField>();
        TMP_Text label = Text(inputRect,"",22,8,width-44,70,36);
        input.textViewport = label.rectTransform; input.textComponent = label;
        input.text = data.Profile.Nickname; input.characterLimit = 20;
        input.onFocusSelectAll = false; input.resetOnDeActivation = false;
        input.richText = false; input.shouldHideSoftKeyboard = false;
        input.lineType = TMP_InputField.LineType.SingleLine;
        input.onValueChanged.AddListener(value=> { if (value.Trim().Length >= 2) PlayerProfileService.SetNickname(value); });
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
        string[] tabIds={"avatar","frame","back"}; string[] tabNames={"AVATARY","RAMKI","REWERSY"};
        float tabWidth=(width-20f)/3f;
        for(int tab=0;tab<3;tab++)
        {
            string id=tabIds[tab];
            Button(body,tabNames[tab],8+tab*tabWidth,y,tabWidth-6,64,()=>{section=id;Build();},section!=id);
        }
        y+=78;
        if(section=="avatar")
        {
        Text(body,"AVATARY",8,y,width-16,64,38); y += 70;
        AvatarDatabase avatars = Resources.Load<AvatarDatabase>("ProfileAvatars");
        if (avatars != null && avatars.avatars != null)
        {
            const int columns = 7;
            float cell = (width-16f)/columns;
            float icon = Mathf.Min(112f,cell-10f);
            var ordered = new System.Collections.Generic.List<int>();
            for (int i=0;i<avatars.avatars.Length;i++) ordered.Add(i);
            ordered.Sort((a,b)=> {
                bool first=data.Inventory.OwnedAvatars.Contains(PlayerProfileService.AvatarId(a,avatars.avatars[a]));
                bool second=data.Inventory.OwnedAvatars.Contains(PlayerProfileService.AvatarId(b,avatars.avatars[b]));
                return first==second?a.CompareTo(b):first?-1:1;
            });
            for (int position=0;position<ordered.Count;position++)
            {
                int i=ordered[position]; string id=PlayerProfileService.AvatarId(i,avatars.avatars[i]);
                float x=8f+(position%columns)*cell+(cell-icon)*.5f;
                float rowY=y+(position/columns)*(icon+18f);
                AvatarTile(id,avatars.avatars[i],x,rowY,icon,data.Profile.SelectedAvatarId==id);
            }
            y += Mathf.Ceil(avatars.avatars.Length/(float)columns)*(icon+18f)+12f;
        }
        }
        if(section=="frame")
        {
        Text(body,"RAMKI",8,y,width-16,64,38); y += 70;
        FrameTile("none",null,8,y,150,true,data.Profile.SelectedFrameId=="none");
        FrameTile("classic_wood",Resources.Load<Sprite>("Cosmetics/ClassicWood"),176,y,150,
            data.Inventory.OwnedFrames.Contains("classic_wood"),data.Profile.SelectedFrameId=="classic_wood");
        y+=174;
        }
        if(section=="back")
        {
        Text(body,"REWERSY",8,y,width-16,64,38); y += 70;
        CardBackDatabase backs = gameObject.GetComponent<CardBackDatabase>();
        if (backs == null) backs = gameObject.AddComponent<CardBackDatabase>();
        for (int i=0;i<backs.BackCount;i++)
        {
            Sprite sprite = backs.GetBackSprite(i); string id = sprite.texture.name;
            ProgressionRules.Own(data.Inventory.OwnedCardBacks,id);
            ItemRow("back",id,id.Replace("HotSeatBack_",""),sprite,true,data.Profile.SelectedCardBackId==id,ref y,width);
        }
        }
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

    private void AvatarTile(string id,Sprite sprite,float x,float y,float size,bool selected)
    {
        RectTransform tile=Rect("AvatarTile",body,x,y,size,size);
        Image border=tile.gameObject.AddComponent<Image>();
        border.color=selected?new Color(1f,.72f,.18f,1f):new Color(.2f,.12f,.07f,.75f);
        Button button=tile.gameObject.AddComponent<Button>();button.targetGraphic=border;button.transition=Selectable.Transition.ColorTint;
        RectTransform imageRect=Rect("Avatar",tile,5,5,size-10,size-10);
        Image image=imageRect.gameObject.AddComponent<Image>();image.sprite=sprite;image.preserveAspect=true;image.raycastTarget=false;
        AvatarCircleUtility.Apply(image);
        bool owned=PlayerProfileService.Data.Inventory.OwnedAvatars.Contains(id);
        button.interactable=owned;
        if(!owned)
        {
            image.enabled=false;
            Color gold=new Color(1f,.75f,.25f,.95f);
            Rect("LockTop",tile,size*.39f,size*.25f,size*.22f,size*.24f).gameObject.AddComponent<Image>().color=gold;
            Rect("LockBody",tile,size*.32f,size*.44f,size*.36f,size*.3f).gameObject.AddComponent<Image>().color=gold;
        }
        button.onClick.AddListener(()=>{PlayerProfileService.Equip("avatar",id);Build();});
    }

    private void FrameTile(string id,Sprite sprite,float x,float y,float size,bool owned,bool selected)
    {
        RectTransform tile=Rect("FrameTile",body,x,y,size,size);
        Image border=tile.gameObject.AddComponent<Image>();
        border.color=selected?new Color(1f,.72f,.18f,1f):new Color(.2f,.12f,.07f,.75f);
        Button button=tile.gameObject.AddComponent<Button>();button.targetGraphic=border;button.interactable=owned;
        if(sprite!=null)
        {
            Image image=Rect("Frame",tile,6,6,size-12,size-12).gameObject.AddComponent<Image>();
            image.sprite=sprite;image.preserveAspect=true;image.raycastTarget=false;
            image.color=owned?Color.white:new Color(.3f,.3f,.3f,.6f);
        }
        else
        {
            TMP_Text empty=Text(tile,"BEZ\nRAMKI",8,8,size-16,size-16,24);empty.alignment=TextAlignmentOptions.Center;
        }
        if(!owned)
        {
            Image shade=Rect("LockedShade",tile,3,3,size-6,size-6).gameObject.AddComponent<Image>();shade.color=new Color(0,0,0,.46f);shade.raycastTarget=false;
            Color lockColor=new Color(1f,.75f,.25f,.95f);
            Rect("LockBody",tile,size*.35f,size*.47f,size*.3f,size*.25f).gameObject.AddComponent<Image>().color=lockColor;
            Rect("LockTop",tile,size*.39f,size*.32f,size*.22f,size*.18f).gameObject.AddComponent<Image>().color=lockColor;
        }
        if(owned)button.onClick.AddListener(()=>{PlayerProfileService.Equip("frame",id);Build();});
    }
}
