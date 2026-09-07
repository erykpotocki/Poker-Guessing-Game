using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public sealed class ShopUI : MonoBehaviour
{
    private RectTransform panel,content;
    private Canvas owner;
    private string tab="online";
    public static void Show(Canvas owner)
    {
        if(owner==null)return;Transform old=owner.rootCanvas.transform.Find("ShopOverlay");if(old!=null){old.gameObject.SetActive(true);return;}
        GameObject obj=new GameObject("ShopOverlay",typeof(RectTransform),typeof(Image),typeof(Canvas),typeof(GraphicRaycaster),typeof(ShopUI));obj.transform.SetParent(owner.rootCanvas.transform,false);
        RectTransform root=obj.transform as RectTransform;root.anchorMin=Vector2.zero;root.anchorMax=Vector2.one;root.offsetMin=root.offsetMax=Vector2.zero;PortraitMenuTopBar.ApplyOverlayInset(root);
        obj.GetComponent<Image>().color=new Color(.01f,.025f,.018f,.99f);Canvas canvas=obj.GetComponent<Canvas>();canvas.overrideSorting=true;canvas.sortingOrder=600;
        ShopUI shop=obj.GetComponent<ShopUI>();shop.owner=owner.rootCanvas;shop.Build();
    }
    private RectTransform Rect(string n,Transform p,float x,float y,float w,float h){RectTransform r=new GameObject(n,typeof(RectTransform)).GetComponent<RectTransform>();r.SetParent(p,false);r.anchorMin=r.anchorMax=new Vector2(.5f,1);r.pivot=new Vector2(.5f,1);r.anchoredPosition=new Vector2(x,-y);r.sizeDelta=new Vector2(w,h);return r;}
    private TMP_Text Text(Transform p,string v,float x,float y,float w,float h,float s=30){TMP_Text t=Rect("Label",p,x,y,w,h).gameObject.AddComponent<TextMeshProUGUI>();t.text=v;t.fontSize=s;t.alignment=TextAlignmentOptions.Center;t.color=new Color(1,.89f,.66f);t.raycastTarget=false;return t;}
    private Button Button(Transform p,string v,float x,float y,float w,float h,UnityEngine.Events.UnityAction a,bool active=true){RectTransform r=Rect(v,p,x,y,w,h);Image image=r.gameObject.AddComponent<Image>();Button b=r.gameObject.AddComponent<Button>();b.targetGraphic=image;b.interactable=active;TMP_Text t=Text(r,v,0,0,w,h,26);t.rectTransform.anchorMin=Vector2.zero;t.rectTransform.anchorMax=Vector2.one;t.rectTransform.offsetMin=t.rectTransform.offsetMax=Vector2.zero;b.onClick.AddListener(a);PokerButtonTheme.ApplyTo(b);return b;}
    private void Build()
    {
        foreach(Transform child in transform){child.gameObject.SetActive(false);Destroy(child.gameObject);}RectTransform root=transform as RectTransform;float w=Mathf.Min(900,root.rect.width-48);
        Text(root,"SKLEP",0,30,w-260,72,46);Button(root,"ZAMKNIJ",w*.5f-120,32,210,66,()=>Destroy(gameObject));
        if(!PlayerProfileService.ShopAvailable)
        {
            float center=Mathf.Max(210,root.rect.height*.36f);
            Color gold=new Color(1f,.75f,.25f);
            Rect("LockTop",root,0,center,100,110).gameObject.AddComponent<Image>().color=gold;
            Rect("LockHole",root,0,center+20,60,80).gameObject.AddComponent<Image>().color=new Color(.01f,.025f,.018f);
            Rect("LockBody",root,0,center+80,160,120).gameObject.AddComponent<Image>().color=gold;
            Text(root,"SKLEP W PRZYGOTOWANIU",0,center+240,w,84,36);
            Text(root,"Zbieraj złoto i diamenty.\nZakupy będą dostępne później.",0,center+334,w-40,120,29);
            return;
        }
        TMP_Text diamonds=Text(root,"◆ "+PlayerProfileService.Data.Wallet.RewardCurrency+" NIEBIESKICH DIAMENTÓW",0,106,w,58,29);
        diamonds.color=new Color(.35f,.78f,1f);
        Button(root,"ONLINE",-w*.24f,174,w*.46f,68,()=>{tab="online";Build();},tab!="online");Button(root,"OFFLINE",w*.24f,174,w*.46f,68,()=>{tab="offline";Build();},tab!="offline");
        content=Rect("ShopContent",root,0,260,w,Mathf.Max(300,root.rect.height-300));
        if(tab=="online")Online(w);else Offline(w);
        Text(root,"Zakup diamentów za prawdziwe pieniądze będzie dostępny po bezpiecznej integracji płatności.",0,root.rect.height-62,w-40,44,21);
        TMP_Text signature=Text(root,"© Eryk Potocki",0,root.rect.height-28,320,22,16);signature.alignment=TextAlignmentOptions.Bottom;signature.color=new Color(1f,.86f,.62f,.36f);
    }
    private void Online(float w)
    {
        Text(content,"PRZEDMIOTY ONLINE",0,28,w,58,32);
        Sprite[] sprites = Resources.LoadAll<Sprite>("ShopAvatars");
        if (sprites == null || sprites.Length == 0)
        {
            Text(content,"Brak dodatkowych avatarów w katalogu.",0,112,w,60,25);
            return;
        }

        List<Sprite> animals = new List<Sprite>();
        List<Sprite> vacation = new List<Sprite>();
        List<Sprite> relaxed = new List<Sprite>();
        foreach (Sprite sprite in sprites)
        {
            string name = sprite.name.ToLowerInvariant();
            if (name.Contains("zwierz") || name.Contains("animal") || name.Contains("cat") || name.Contains("dog") || name.Contains("fox") || name.Contains("bear")) animals.Add(sprite);
            else if (name.Contains("wakac") || name.Contains("summer") || name.Contains("beach") || name.Contains("holiday") || name.Contains("trop")) vacation.Add(sprite);
            else relaxed.Add(sprite);
        }

        float y = 112f;
        AvatarCategory("ZWIERZĘTA", animals, w, ref y);
        AvatarCategory("WAKACYJNE", vacation, w, ref y);
        AvatarCategory("NA LUZIE", relaxed, w, ref y);
        Sprite[] frames = Resources.LoadAll<Sprite>("ShopFrames");
        ShopCosmeticCategory("RAMKI", frames, "frame", w, ref y);
        Sprite[] backs = Resources.LoadAll<Sprite>("ShopBacks");
        ShopCosmeticCategory("REWERSY", backs, "back", w, ref y);
    }

    private void AvatarCategory(string title, List<Sprite> sprites, float width, ref float y)
    {
        if (sprites == null || sprites.Count == 0) return;
        sprites.Sort((a, b) => IsOwned("avatar", b.name).CompareTo(IsOwned("avatar", a.name)));
        Text(content, title, 0, y, width, 42, 27);
        y += 48f;
        float viewportHeight = 136f;
        RectTransform viewport = Rect("AvatarRow", content, 0, y, width - 16f, viewportHeight);
        viewport.gameObject.AddComponent<Image>().color = new Color(0f, 0f, 0f, .16f);
        viewport.gameObject.AddComponent<RectMask2D>();
        ScrollRect scroll = viewport.gameObject.AddComponent<ScrollRect>();
        scroll.horizontal = true; scroll.vertical = false; scroll.movementType = ScrollRect.MovementType.Clamped;
        scroll.scrollSensitivity = 80f; scroll.horizontalScrollbar = null;
        float cell = 116f, gap = 10f;
        float contentWidth = Mathf.Max(width - 16f, 20f + sprites.Count * (cell + gap));
        RectTransform row = Rect("AvatarRowContent", viewport, 0, 0, contentWidth, viewportHeight);
        row.anchorMin = new Vector2(0f, 1f); row.anchorMax = new Vector2(0f, 1f); row.pivot = new Vector2(0f, 1f);
        row.anchoredPosition = Vector2.zero;
        scroll.viewport = viewport; scroll.content = row;
        for (int i = 0; i < sprites.Count; i++)
        {
            RectTransform tile = Rect("AvatarShopTile", row, 12f + i * (cell + gap), 10f, cell, 112f);
            Image tileBackground = tile.gameObject.AddComponent<Image>();
            tileBackground.color = new Color(.18f, .07f, .035f, .9f);
            Image avatar = Rect("Avatar", tile, 8f, 8f, 100f, 96f).gameObject.AddComponent<Image>();
            avatar.sprite = sprites[i]; avatar.preserveAspect = true; avatar.raycastTarget = false;
            AvatarCircleUtility.Apply(avatar);
            AddLock(tile, !IsOwned("avatar", sprites[i].name));
        }
        y += viewportHeight + 24f;
    }

    private void ShopCosmeticCategory(string title, Sprite[] source, string category, float width, ref float y)
    {
        if (source == null || source.Length == 0) return;
        List<Sprite> sprites = new List<Sprite>(source);
        sprites.Sort((a, b) => IsOwned(category, b.name).CompareTo(IsOwned(category, a.name)));
        Text(content, title, 0, y, width, 42, 27); y += 48f;
        float viewportHeight = 136f, cell = 116f, gap = 10f;
        RectTransform viewport = Rect("CosmeticRow", content, 0, y, width - 16f, viewportHeight);
        viewport.gameObject.AddComponent<Image>().color = new Color(0f, 0f, 0f, .16f);
        viewport.gameObject.AddComponent<RectMask2D>();
        ScrollRect scroll = viewport.gameObject.AddComponent<ScrollRect>(); scroll.horizontal = true; scroll.vertical = false;
        scroll.movementType = ScrollRect.MovementType.Clamped; scroll.scrollSensitivity = 80f;
        RectTransform row = Rect("CosmeticRowContent", viewport, 0, 0, Mathf.Max(width - 16f, 20f + sprites.Count * (cell + gap)), viewportHeight);
        row.anchorMin = new Vector2(0f, 1f); row.anchorMax = new Vector2(0f, 1f); row.pivot = new Vector2(0f, 1f); row.anchoredPosition = Vector2.zero;
        scroll.viewport = viewport; scroll.content = row;
        for (int i = 0; i < sprites.Count; i++)
        {
            RectTransform tile = Rect("CosmeticShopTile", row, 12f + i * (cell + gap), 10f, cell, 112f);
            Image bg = tile.gameObject.AddComponent<Image>(); bg.color = new Color(.18f, .07f, .035f, .9f);
            Image image = Rect("Cosmetic", tile, 8f, 8f, 100f, 96f).gameObject.AddComponent<Image>(); image.sprite = sprites[i]; image.preserveAspect = true; image.raycastTarget = false;
            AddLock(tile, !IsOwned(category, sprites[i].name));
        }
        y += viewportHeight + 24f;
    }

    private static bool IsOwned(string category, string id)
    {
        var inventory = PlayerProfileService.Data.Inventory;
        if (category == "avatar") return inventory.OwnedAvatars.Contains(id) || inventory.OwnedAvatars.Contains("avatar_" + id);
        if (category == "frame") return inventory.OwnedFrames.Contains(id) || inventory.OwnedFrames.Contains("frame_" + id) || (id == "ClassicWood" && inventory.OwnedFrames.Contains("classic_wood"));
        return inventory.OwnedCardBacks.Contains(id);
    }

    private void AddLock(RectTransform tile, bool locked)
    {
        if (!locked) return;
        Image shade = Rect("Locked", tile, 0, 0, tile.sizeDelta.x, tile.sizeDelta.y).gameObject.AddComponent<Image>();
        shade.color = new Color(0f, 0f, 0f, .58f); shade.raycastTarget = false;
        TMP_Text lockText = Text(tile, "LOCK", 0f, 0f, 110f, 112f, 34f);
        lockText.alignment = TextAlignmentOptions.Center; lockText.color = new Color(1f, .8f, .3f, .95f);
    }
    private void Offline(float w)
    {
        Text(content,"REWERSY TRYBU OFFLINE",0,12,w,56,32);CardBackDatabase backs=gameObject.GetComponent<CardBackDatabase>();if(backs==null)backs=gameObject.AddComponent<CardBackDatabase>();
        float size=Mathf.Min(130,(w-40)/Mathf.Max(1,backs.BackCount));for(int i=0;i<backs.BackCount;i++){Sprite s=backs.GetBackSprite(i);Image image=Rect("Back",content,-w*.5f+30+size*.5f+i*size,92,size-14,size*1.35f).gameObject.AddComponent<Image>();image.sprite=s;image.preserveAspect=true;}
        Text(content,"Domyślne rewersy są dostępne bez opłat.",0,290,w,54,25);
    }
}
