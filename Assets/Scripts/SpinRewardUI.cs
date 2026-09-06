using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public sealed class SpinRewardUI : MonoBehaviour
{
    private TMP_Text timer, result;
    private Button spin;
    public static void Show(Canvas owner)
    {
        if(owner==null)return;
        Transform old=owner.rootCanvas.transform.Find("SpinRewardOverlay"); if(old!=null){old.gameObject.SetActive(true);return;}
        GameObject obj=new GameObject("SpinRewardOverlay",typeof(RectTransform),typeof(Image),typeof(Canvas),typeof(GraphicRaycaster),typeof(SpinRewardUI));
        obj.transform.SetParent(owner.rootCanvas.transform,false);
        RectTransform root=obj.transform as RectTransform; root.anchorMin=Vector2.zero;root.anchorMax=Vector2.one;root.offsetMin=root.offsetMax=Vector2.zero;
        obj.GetComponent<Image>().color=new Color(0,0,0,.82f); Canvas c=obj.GetComponent<Canvas>();c.overrideSorting=true;c.sortingOrder=440;
        obj.GetComponent<SpinRewardUI>().Build();
    }
    private RectTransform Box(string name,Transform parent,Vector2 pos,Vector2 size)
    {
        RectTransform r=new GameObject(name,typeof(RectTransform)).GetComponent<RectTransform>();r.SetParent(parent,false);
        r.anchorMin=r.anchorMax=r.pivot=new Vector2(.5f,.5f);r.anchoredPosition=pos;r.sizeDelta=size;return r;
    }
    private TMP_Text Text(Transform parent,string value,Vector2 pos,Vector2 size,float font)
    {
        TMP_Text t=Box("Label",parent,pos,size).gameObject.AddComponent<TextMeshProUGUI>();t.text=value;t.fontSize=font;
        t.alignment=TextAlignmentOptions.Center;t.color=new Color(1,.9f,.65f);t.raycastTarget=false;return t;
    }
    private Button Action(Transform parent,string label,Vector2 pos,Vector2 size,UnityEngine.Events.UnityAction action)
    {
        RectTransform r=Box(label,parent,pos,size);Image image=r.gameObject.AddComponent<Image>();Button b=r.gameObject.AddComponent<Button>();b.targetGraphic=image;
        TMP_Text t=Text(r,label,Vector2.zero,size,30);t.rectTransform.anchorMin=Vector2.zero;t.rectTransform.anchorMax=Vector2.one;t.rectTransform.offsetMin=t.rectTransform.offsetMax=Vector2.zero;
        b.onClick.AddListener(action);PokerButtonTheme.ApplyTo(b);return b;
    }
    private void Build()
    {
        RectTransform panel=Box("SpinPanel",transform,Vector2.zero,new Vector2(620,500));panel.gameObject.AddComponent<Image>().color=new Color(.12f,.025f,.018f,.98f);
        Text(panel,"KOŁO NAGRÓD",new Vector2(0,170),new Vector2(560,70),42);
        Text(panel,"✦",new Vector2(0,62),new Vector2(180,150),110);
        result=Text(panel,"Darmowa próba co godzinę",new Vector2(0,-38),new Vector2(560,60),28);
        timer=Text(panel,"",new Vector2(0,-98),new Vector2(560,54),27);
        spin=Action(panel,"ZAKRĘĆ",new Vector2(0,-174),new Vector2(330,72),Spin);
        Action(panel,"ZAMKNIJ",new Vector2(0,-244),new Vector2(230,56),()=>Destroy(gameObject));
        Refresh();
    }
    private void Spin(){string reward=PlayerProfileService.Spin();result.text=reward==null?"Następna próba jeszcze niedostępna":"Wygrywasz: "+reward;Refresh();}
    private void Update(){Refresh();}
    private void Refresh()
    {
        if(timer==null||spin==null)return;TimeSpan left=PlayerProfileService.SpinRemaining;spin.interactable=left<=TimeSpan.Zero;
        timer.text=left<=TimeSpan.Zero?"DARMOWY SPIN GOTOWY":$"Następny spin za {Mathf.CeilToInt((float)left.TotalMinutes):00}:{left.Seconds:00}";
    }
}
