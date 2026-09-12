using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public sealed class ShopUI : MonoBehaviour
{
    private string category="avatar";
    private int currency;
    public static RectTransform Rect(string name,Transform parent,float x,float y,float w,float h)
    {
        var r=new GameObject(name,typeof(RectTransform)).GetComponent<RectTransform>();r.SetParent(parent,false);
        r.anchorMin=r.anchorMax=r.pivot=new Vector2(0,1);r.anchoredPosition=new Vector2(x,-y);r.sizeDelta=new Vector2(w,h);return r;
    }
    public static TMP_Text Text(Transform parent,string caption,float x,float y,float w,float h,float size=28)
    {
        var t=Rect("Label",parent,x,y,w,h).gameObject.AddComponent<TextMeshProUGUI>();t.text=caption;t.fontSize=size;
        t.color=new Color(1,.9f,.7f);t.alignment=TextAlignmentOptions.Center;t.raycastTarget=false;return t;
    }
    public static Button Button(Transform parent,string caption,float x,float y,float w,float h,Action action)
    {
        var r=Rect("Action",parent,x,y,w,h);var im=r.gameObject.AddComponent<Image>();var b=r.gameObject.AddComponent<Button>();b.targetGraphic=im;
        Text(r,caption,4,0,w-8,h,25);b.onClick.AddListener(()=>action());PokerButtonTheme.ApplyTo(b);return b;
    }
    public static void Show(Canvas canvas)
    {
        if(canvas==null)return;
        var old=canvas.rootCanvas.transform.Find("ShopOverlay");if(old!=null)Destroy(old.gameObject);
        var root=Overlay(canvas,"ShopOverlay");root.gameObject.AddComponent<ShopUI>().Build();
    }
    public static RectTransform Overlay(Canvas canvas,string name)
    {
        var r=Rect(name,canvas.rootCanvas.transform,0,0,0,0);r.anchorMin=Vector2.zero;r.anchorMax=Vector2.one;r.offsetMin=r.offsetMax=Vector2.zero;
        r.gameObject.AddComponent<Image>().color=new Color(.015f,.025f,.022f,.98f);
        var layer=r.gameObject.AddComponent<Canvas>();layer.overrideSorting=true;layer.sortingOrder=650;r.gameObject.AddComponent<GraphicRaycaster>();
        PortraitMenuTopBar.ApplyOverlayInset(r);Canvas.ForceUpdateCanvases();return r;
    }
    private void Build()
    {
        foreach(Transform child in transform){child.gameObject.SetActive(false);Destroy(child.gameObject);}
        var root=(RectTransform)transform;float w=root.rect.width,h=root.rect.height;
        Text(root,"SKLEP",20,20,w-140,70,44);Button(root,"×",w-100,20,76,66,()=>Destroy(gameObject));
        Text(root,$"Złoto: {PlayerProfileService.Data.Wallet.Coins}    ◆ {PlayerProfileService.Data.Wallet.RewardCurrency}",20,94,w-40,56,30);
        string[] ids={"avatar","back","frame"},labels={"AVATARY","REWERSY","RAMKI"};
        for(int i=0;i<3;i++){string id=ids[i];Button(root,labels[i],20+i*(w-40)/3,164,(w-46)/3,60,()=>{category=id;Build();});}
        float sidebar=Mathf.Min(175,w*.23f);string[] filters={"Wszystko","Złoto","Diamenty"};
        for(int i=0;i<3;i++){int n=i;Button(root,(currency==i?"• ":"")+filters[i],14,242+i*70,sidebar-20,60,()=>{currency=n;Build();});}
        var view=Rect("Products",root,sidebar,242,w-sidebar-18,Mathf.Max(100,h-270));view.gameObject.AddComponent<Image>().color=Color.clear;view.gameObject.AddComponent<RectMask2D>();
        var scroll=view.gameObject.AddComponent<ScrollRect>();scroll.horizontal=false;scroll.movementType=ScrollRect.MovementType.Clamped;
        float width=view.rect.width,cell=width/3;
        var items=CosmeticCatalog.All(category).FindAll(o=>currency==0 || (currency==1?o.Gold>0:o.Diamonds>0));
        var content=Rect("Content",view,0,0,width,Mathf.Ceil(items.Count/3f)*(cell*1.6f+90));scroll.viewport=view;scroll.content=content;
        for(int i=0;i<items.Count;i++)
        {
            var offer=items[i];float x=i%3*cell,y=i/3*(cell*1.6f+90);
            var tile=Rect("Product",content,x+5,y,cell-10,cell*1.6f+78);tile.gameObject.AddComponent<Image>().color=new Color(.1f,.065f,.035f);
            var art=Rect("Preview",tile,8,8,cell-26,cell*1.35f).gameObject.AddComponent<Image>();art.sprite=offer.Sprite;art.preserveAspect=true;art.raycastTarget=false;
            Text(tile,CosmeticCatalog.Owned(category,offer.Id)?"Posiadane":Price(offer),4,cell*1.35f+12,cell-18,60,22);
            var button=tile.gameObject.AddComponent<Button>();button.onClick.AddListener(()=>Preview(GetComponent<Canvas>(),offer.Category,offer.Id,Build));
        }
    }
    private static string Price(CosmeticCatalog.Offer o)=>!o.Purchasable?(o.Id=="6"?"Darmowy":o.Category=="frame"?"Nagroda za misję":o.Spin?"Tylko ze spina":"Niedostępne"):o.Gold>0&&o.Diamonds>0?$"{o.Gold} złota\n{(o.Both?"+":"lub")} {o.Diamonds} ◆":o.Gold>0?$"{o.Gold} złota":$"{o.Diamonds} ◆";
    public static void Preview(Canvas canvas,string category,string id,Action changed=null)
    {
        if(canvas==null)return;var offer=CosmeticCatalog.Get(category,id);var root=Overlay(canvas,"PurchasePreview");root.GetComponent<Canvas>().sortingOrder=700;
        float w=root.rect.width,h=root.rect.height;float artH=Mathf.Min(h*.48f,600),artW=Mathf.Min(w-60,artH);
        Text(root,offer.Title,20,20,w-40,65,34);
        var art=Rect("Artwork",root,(w-artW)/2,100,artW,artH).gameObject.AddComponent<Image>();art.sprite=offer.Sprite;art.preserveAspect=true;
        float y=110+artH;Text(root,Price(offer),20,y,w-40,90,34);y+=100;
        bool owned=CosmeticCatalog.Owned(category,id);bool allowed=category!="frame"||CosmeticCatalog.CanUnlockFrame(PlayerProfileService.Data,id);
        var status=Text(root,"",20,y+74,w-40,74,25);
        Action<bool> buy=gems=>{if(PlayerProfileService.BuyCosmetic(category,id,gems)){Destroy(root.gameObject);changed?.Invoke();UnlockPresentationUI.ShowPending(canvas);}else status.text="Za mało środków lub przedmiot już odblokowany.";};
        if(owned)Button(root,"ZAŁÓŻ",20,y,w-40,65,()=>{PlayerProfileService.Equip(category,id);Destroy(root.gameObject);changed?.Invoke();});
        else if(!allowed)status.text=id=="classic_wood"?"Ukończ pierwszą grę i odbierz ramkę w Misjach.":"Odblokuj najpierw poprzednie ramki.\nRamki otrzymujesz też za poziom.";
        else if(offer.Both)Button(root,$"KUP: {offer.Gold} złota + {offer.Diamonds} ◆",20,y,w-40,65,()=>buy(true));
        else if(offer.Purchasable)
        {
            float bw=(w-50)/2;if(offer.Gold>0)Button(root,$"KUP: {offer.Gold} złota",20,y,bw,65,()=>buy(false));
            if(offer.Diamonds>0)Button(root,$"KUP: {offer.Diamonds} ◆",30+bw,y,bw,65,()=>buy(true));
        }
        Button(root,"WRÓĆ",20,h-84,w-40,64,()=>Destroy(root.gameObject));
    }
}
