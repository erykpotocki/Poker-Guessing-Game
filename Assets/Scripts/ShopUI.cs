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
        RectTransform root=obj.transform as RectTransform;root.anchorMin=Vector2.zero;root.anchorMax=Vector2.one;root.offsetMin=root.offsetMax=Vector2.zero;
        obj.GetComponent<Image>().color=new Color(.01f,.025f,.018f,.99f);Canvas canvas=obj.GetComponent<Canvas>();canvas.overrideSorting=true;canvas.sortingOrder=420;
        ShopUI shop=obj.GetComponent<ShopUI>();shop.owner=owner.rootCanvas;shop.Build();
    }
    private RectTransform Rect(string n,Transform p,float x,float y,float w,float h){RectTransform r=new GameObject(n,typeof(RectTransform)).GetComponent<RectTransform>();r.SetParent(p,false);r.anchorMin=r.anchorMax=new Vector2(.5f,1);r.pivot=new Vector2(.5f,1);r.anchoredPosition=new Vector2(x,-y);r.sizeDelta=new Vector2(w,h);return r;}
    private TMP_Text Text(Transform p,string v,float x,float y,float w,float h,float s=30){TMP_Text t=Rect("Label",p,x,y,w,h).gameObject.AddComponent<TextMeshProUGUI>();t.text=v;t.fontSize=s;t.alignment=TextAlignmentOptions.Center;t.color=new Color(1,.89f,.66f);t.raycastTarget=false;return t;}
    private Button Button(Transform p,string v,float x,float y,float w,float h,UnityEngine.Events.UnityAction a,bool active=true){RectTransform r=Rect(v,p,x,y,w,h);Image image=r.gameObject.AddComponent<Image>();Button b=r.gameObject.AddComponent<Button>();b.targetGraphic=image;b.interactable=active;TMP_Text t=Text(r,v,0,0,w,h,26);t.rectTransform.anchorMin=Vector2.zero;t.rectTransform.anchorMax=Vector2.one;t.rectTransform.offsetMin=t.rectTransform.offsetMax=Vector2.zero;b.onClick.AddListener(a);PokerButtonTheme.ApplyTo(b);return b;}
    private void Build()
    {
        foreach(Transform child in transform){child.gameObject.SetActive(false);Destroy(child.gameObject);}RectTransform root=transform as RectTransform;float w=Mathf.Min(900,root.rect.width-48);
        Text(root,"SKLEP",0,30,w-260,72,46);Button(root,"ZAMKNIJ",w*.5f-120,32,210,66,()=>Destroy(gameObject));
        TMP_Text diamonds=Text(root,"◆ "+PlayerProfileService.Data.Wallet.RewardCurrency+" NIEBIESKICH DIAMENTÓW",0,106,w,58,29);
        diamonds.color=new Color(.35f,.78f,1f);
        Button(root,"ONLINE",-w*.24f,174,w*.46f,68,()=>{tab="online";Build();},tab!="online");Button(root,"OFFLINE",w*.24f,174,w*.46f,68,()=>{tab="offline";Build();},tab!="offline");
        content=Rect("ShopContent",root,0,260,w,Mathf.Max(300,root.rect.height-300));
        if(tab=="online")Online(w);else Offline(w);
        Text(root,"Zakup diamentów za prawdziwe pieniądze będzie dostępny po bezpiecznej integracji płatności.",0,root.rect.height-62,w-40,44,21);
        TMP_Text signature=Text(root,"© Eryk Potocki",w*.5f-155,root.rect.height-42,280,30,18);signature.alignment=TextAlignmentOptions.BottomRight;signature.color=new Color(1f,.86f,.62f,.55f);
    }
    private void Online(float w)
    {
        bool owned=PlayerProfileService.Data.Inventory.OwnedFrames.Contains("classic_wood");Sprite sprite=Resources.Load<Sprite>("Cosmetics/ClassicWood");
        Image item=Rect("ClassicWood",content,-w*.28f,20,180,180).gameObject.AddComponent<Image>();item.sprite=sprite;item.preserveAspect=true;item.color=owned?Color.white:new Color(.72f,.72f,.72f,1);
        Text(content,"CLASSIC WOOD",30,35,w*.48f,54,30);Text(content,owned?"W POSIADANIU":"◆ 25",30,92,w*.48f,46,25);
        Button(content,owned?"POSIADANE":"KUP",w*.29f,136,220,68,BuyClassicWood,!owned&&PlayerProfileService.Data.Wallet.RewardCurrency>=25);
    }
    private void BuyClassicWood()
    {
        if(!PlayerProfileService.BuyWithDiamonds("frame","classic_wood",25,"ODBLOKOWANO NOWĄ RAMKĘ"))return;
        Build();
        UnlockPresentationUI.ShowPending(owner);
    }
    private void Offline(float w)
    {
        Text(content,"REWERSY TRYBU OFFLINE",0,12,w,56,32);CardBackDatabase backs=gameObject.GetComponent<CardBackDatabase>();if(backs==null)backs=gameObject.AddComponent<CardBackDatabase>();
        float size=Mathf.Min(130,(w-40)/Mathf.Max(1,backs.BackCount));for(int i=0;i<backs.BackCount;i++){Sprite s=backs.GetBackSprite(i);Image image=Rect("Back",content,-w*.5f+30+size*.5f+i*size,92,size-14,size*1.35f).gameObject.AddComponent<Image>();image.sprite=s;image.preserveAspect=true;}
        Text(content,"Domyślne rewersy są dostępne bez opłat.",0,290,w,54,25);
    }
}
