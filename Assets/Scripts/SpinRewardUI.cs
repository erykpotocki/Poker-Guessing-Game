using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public sealed class SpinRewardUI : MonoBehaviour
{
    private TMP_Text timer, result;
    private Button spin;
    private RectTransform wheel;
    private bool spinning;
    public static void Show(Canvas owner)
    {
        if(owner==null)return;
        Transform old=owner.rootCanvas.transform.Find("SpinRewardOverlay"); if(old!=null){old.gameObject.SetActive(true);return;}
        GameObject obj=new GameObject("SpinRewardOverlay",typeof(RectTransform),typeof(Image),typeof(Canvas),typeof(GraphicRaycaster),typeof(SpinRewardUI));
        obj.transform.SetParent(owner.rootCanvas.transform,false);
        RectTransform root=obj.transform as RectTransform; root.anchorMin=Vector2.zero;root.anchorMax=Vector2.one;root.offsetMin=root.offsetMax=Vector2.zero;PortraitMenuTopBar.ApplyOverlayInset(root);
        obj.GetComponent<Image>().color=new Color(0,0,0,.94f); Canvas c=obj.GetComponent<Canvas>();c.overrideSorting=true;c.sortingOrder=620;
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
        RectTransform panel=Box("SpinPanel",transform,Vector2.zero,new Vector2(820,1320));panel.gameObject.AddComponent<Image>().color=new Color(.12f,.025f,.018f,.995f);
        Text(panel,"KOŁO NAGRÓD",new Vector2(0,590),new Vector2(720,76),46);
        GameObject wheelObject = new GameObject("RewardWheel", typeof(RectTransform), typeof(Image)); wheelObject.transform.SetParent(panel, false);
        wheel = wheelObject.transform as RectTransform; wheel.anchorMin=wheel.anchorMax=wheel.pivot=new Vector2(.5f,.5f); wheel.anchoredPosition=new Vector2(0,185); wheel.sizeDelta=new Vector2(620,620);
        Image wheelImage=wheelObject.GetComponent<Image>(); wheelImage.sprite=Resources.Load<Sprite>("SpinAssets/spin kolo"); wheelImage.preserveAspect=true; wheelImage.raycastTarget=false;
        Text(panel,"▼",new Vector2(0,515),new Vector2(88,76),52).color=new Color(1f,.78f,.18f);
        result=Text(panel,"Darmowa próba co minutę",new Vector2(0,-245),new Vector2(720,64),30);
        timer=Text(panel,"",new Vector2(0,-335),new Vector2(720,54),27);
        spin=Action(panel,"ZAKRĘĆ",new Vector2(0,-445),new Vector2(370,76),Spin);
        Action(panel,"ZAMKNIJ",new Vector2(0,-565),new Vector2(280,64),()=>Destroy(gameObject));
        Refresh();
    }
    private void Spin(){if(spinning)return;StartCoroutine(SpinWheel());}
    private IEnumerator SpinWheel()
    {
        if (PlayerProfileService.SpinRemaining > TimeSpan.Zero) { Refresh(); yield break; }
        spinning=true; spin.interactable=false; result.text="Koło się kręci…";
        // Persist the single result before animation: closing/reopening the
        // overlay cannot grant it twice or lose an already-won diamond.
        string reward=PlayerProfileService.Spin(out int sector);
        if (reward == null) { spinning=false; Refresh(); yield break; }
        float start=wheel.localEulerAngles.z;
        float target=start+1440f+Mathf.Repeat(sector*45f-start,360f), elapsed=0f;
        while(elapsed<4f){elapsed+=Time.unscaledDeltaTime;float t=Mathf.Clamp01(elapsed/4f);float eased=1f-Mathf.Pow(1f-t,3f);wheel.localEulerAngles=new Vector3(0,0,Mathf.Lerp(start,target,eased));yield return null;}
        result.text="Wygrywasz: "+reward; spinning=false; Refresh();
    }
    private void Update(){Refresh();}
    private void Refresh()
    {
        if(timer==null||spin==null)return;TimeSpan left=PlayerProfileService.SpinRemaining;spin.interactable=!spinning&&left<=TimeSpan.Zero;
        timer.text=left<=TimeSpan.Zero?"DARMOWY SPIN GOTOWY":$"Następny spin za {(int)left.TotalMinutes:00}:{left.Seconds:00}";
    }
}
