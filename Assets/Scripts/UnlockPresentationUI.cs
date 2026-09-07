using System.Collections.Generic;
using PokerProfile;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public sealed class UnlockPresentationUI : MonoBehaviour
{
    private readonly List<RectTransform> stars=new();
    public static void ShowPending(Canvas owner)
    {
        if(owner==null||PlayerProfileService.PeekUnlock()==null)return;
        if(owner.rootCanvas.transform.Find("UnlockPresentation")!=null)return;
        GameObject obj=new GameObject("UnlockPresentation",typeof(RectTransform),typeof(Image),typeof(Canvas),typeof(GraphicRaycaster),typeof(UnlockPresentationUI));
        obj.transform.SetParent(owner.rootCanvas.transform,false);RectTransform root=obj.transform as RectTransform;
        root.anchorMin=Vector2.zero;root.anchorMax=Vector2.one;root.offsetMin=root.offsetMax=Vector2.zero;
        obj.GetComponent<Image>().color=new Color(0,0,0,.86f);Canvas c=obj.GetComponent<Canvas>();c.overrideSorting=true;c.sortingOrder=500;
        obj.GetComponent<UnlockPresentationUI>().Build(owner);
    }
    private RectTransform Rect(string name,Transform parent,Vector2 pos,Vector2 size)
    {RectTransform r=new GameObject(name,typeof(RectTransform)).GetComponent<RectTransform>();r.SetParent(parent,false);r.anchorMin=r.anchorMax=r.pivot=new Vector2(.5f,.5f);r.anchoredPosition=pos;r.sizeDelta=size;return r;}
    private TMP_Text Text(Transform parent,string value,Vector2 pos,Vector2 size,float font)
    {TMP_Text t=Rect("Label",parent,pos,size).gameObject.AddComponent<TextMeshProUGUI>();t.text=value;t.fontSize=font;t.alignment=TextAlignmentOptions.Center;t.color=new Color(1,.82f,.26f);t.raycastTarget=false;return t;}
    private void Build(Canvas owner)
    {
        PendingUnlock item=PlayerProfileService.PeekUnlock();if(item==null){Destroy(gameObject);return;}
        RectTransform panel=Rect("GoldenUnlock",transform,Vector2.zero,new Vector2(700,720));panel.gameObject.AddComponent<Image>().color=new Color(.13f,.025f,.012f,.98f);
        Text(panel,item.Title,new Vector2(0,270),new Vector2(640,90),43);
        Text(panel,"GRATULUJEMY!",new Vector2(0,205),new Vector2(620,60),30);
        Sprite preview=Resolve(item);
        Image image=Rect("UnlockedItem",panel,new Vector2(0,20),new Vector2(280,280)).gameObject.AddComponent<Image>();image.sprite=preview;image.preserveAspect=true;image.raycastTarget=false;
        if(item.Category=="avatar") AvatarCircleUtility.Apply(image);
        for(int i=0;i<12;i++){float a=i*Mathf.PI*2/12;RectTransform star=Rect("Star",panel,new Vector2(Mathf.Cos(a)*230,20+Mathf.Sin(a)*190),new Vector2(46,46));Text(star,"✦",Vector2.zero,new Vector2(46,46),36);stars.Add(star);}
        Button accept=Rect("Continue",panel,new Vector2(0,-280),new Vector2(390,82)).gameObject.AddComponent<Button>();Image bg=accept.gameObject.AddComponent<Image>();accept.targetGraphic=bg;
        TMP_Text label=Text(accept.transform,"KONTYNUUJ",Vector2.zero,new Vector2(390,82),31);label.rectTransform.anchorMin=Vector2.zero;label.rectTransform.anchorMax=Vector2.one;label.rectTransform.offsetMin=label.rectTransform.offsetMax=Vector2.zero;
        accept.onClick.AddListener(()=>{PlayerProfileService.AcceptUnlock();transform.SetParent(null);Destroy(gameObject);ShowPending(owner);});PokerButtonTheme.ApplyTo(accept);
    }
    private static Sprite Resolve(PendingUnlock item)
    {
        if(item.Category=="avatar" && item.ItemId.StartsWith("download:")) return Resources.Load<Sprite>("ShopAvatars/"+item.ItemId.Substring(9));
        if(item.Category=="frame")return Resources.Load<Sprite>("Cosmetics/ClassicWood");
        if(item.Category=="avatar"){AvatarDatabase db=Resources.Load<AvatarDatabase>("ProfileAvatars");if(db!=null&&int.TryParse(item.ItemId.Replace("avatar_",""),out int i)&&db.avatars!=null&&i>=0&&i<db.avatars.Length)return db.avatars[i];}
        if(item.Category=="back"){CardBackDatabase db=new GameObject("UnlockBackResolver").AddComponent<CardBackDatabase>();for(int i=0;i<db.BackCount;i++){Sprite s=db.GetBackSprite(i);if(s!=null&&s.texture.name==item.ItemId){Destroy(db.gameObject);return s;}}Destroy(db.gameObject);}
        return null;
    }
    private void Update(){for(int i=0;i<stars.Count;i++)if(stars[i]!=null){float pulse=1f+.22f*Mathf.Sin(Time.unscaledTime*3f+i);stars[i].localScale=Vector3.one*pulse;stars[i].Rotate(0,0,25f*Time.unscaledDeltaTime);}}
}
