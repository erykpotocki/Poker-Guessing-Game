using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public sealed class HotSeatReviewUI : MonoBehaviour
{
    public sealed class Hand
    {
        public string Name;
        public List<CardSpriteEntry> Cards;
    }
    private sealed class Entry
    {
        public CardSpriteEntry Card;
        public Image Image;
        public int Row;
    }
    private readonly List<Entry> entries=new List<Entry>();
    private readonly List<Image> slots=new List<Image>();
    private readonly List<CardSpriteEntry> revealed=new List<CardSpriteEntry>();
    private ScrollRect scroll;
    private TMP_Text verdict;
    private TMP_Text handStatus;
    private Button action;
    private TMP_Text actionLabel;
    private bool fast,finished;
    private bool paused;
    private string handId,result;
    private Action continueAction;
    private float rowHeight,contentHeight;

    private static RectTransform Box(Transform parent,string name,Vector2 min,Vector2 max)
    {
        var r=new GameObject(name,typeof(RectTransform)).GetComponent<RectTransform>();r.SetParent(parent,false);
        r.anchorMin=min;r.anchorMax=max;r.offsetMin=r.offsetMax=Vector2.zero;return r;
    }
    private static TMP_Text Text(Transform parent,string value,Vector2 min,Vector2 max,float size=32)
    {
        var t=Box(parent,"Label",min,max).gameObject.AddComponent<TextMeshProUGUI>();
        t.text=value;t.fontSize=size;t.enableAutoSizing=true;t.fontSizeMin=16;t.fontSizeMax=size;
        t.alignment=TextAlignmentOptions.Center;t.color=new Color(1,.88f,.60f);t.raycastTarget=false;return t;
    }
    public static void Show(Canvas owner,List<Hand> hands,string id,string title,string outcome,Sprite back,Action next)
    {
        var root=Box(owner.rootCanvas.transform,"HotSeatReview",Vector2.zero,Vector2.one);
        Rect safe=Screen.safeArea;
        root.anchorMin=new Vector2(safe.xMin/Screen.width,safe.yMin/Screen.height);
        root.anchorMax=new Vector2(safe.xMax/Screen.width,safe.yMax/Screen.height);
        root.gameObject.AddComponent<Image>().color=new Color(.015f,.045f,.040f,1);
        var canvas=root.gameObject.AddComponent<Canvas>();canvas.overrideSorting=true;canvas.sortingOrder=1500;
        root.gameObject.AddComponent<GraphicRaycaster>();
        var ui=root.gameObject.AddComponent<HotSeatReviewUI>();ui.handId=id;ui.result=outcome;ui.continueAction=next;
        Canvas.ForceUpdateCanvases();ui.Build(root,hands,title,back);ui.StartCoroutine(ui.Reveal());
    }
    private void Build(RectTransform root,List<Hand> hands,string title,Sprite back)
    {
        Text(root,"SPRAWDZANIE: "+title,new Vector2(.04f,.93f),new Vector2(.96f,.985f),40);
        Text(root,"KARTY TWORZĄCE UKŁAD",new Vector2(.05f,.89f),new Vector2(.95f,.925f),25);
        int count=handId.StartsWith("HIGH_")?1:handId.StartsWith("PAIR_")?2:handId.StartsWith("TRIPS_")?3:
            handId.StartsWith("QUADS_")||handId.StartsWith("TWOPAIR_")?4:5;
        float slotWidth=Mathf.Min(root.rect.width*.15f,root.rect.height*.125f/1.39f);
        for(int i=0;i<count;i++)
        {
            var slot=Box(root,"EvidenceSlot",new Vector2(.5f,.8f),new Vector2(.5f,.8f));
            slot.sizeDelta=new Vector2(slotWidth,slotWidth*1.39f);slot.anchoredPosition=new Vector2((i-(count-1)*.5f)*(slotWidth+16),0);
            var image=slot.gameObject.AddComponent<Image>();image.color=new Color(.10f,.16f,.13f);image.raycastTarget=false;slots.Add(image);
            for(int edge=0;edge<4;edge++)for(int dash=0;dash<7;dash++)
            {
                float p=dash/7f;
                Vector2 min=edge<2?new Vector2(p,edge==0?0:.98f):new Vector2(edge==2?0:.98f,p);
                Vector2 max=edge<2?new Vector2(p+.075f,edge==0?.02f:1):new Vector2(edge==2?.02f:1,p+.075f);
                var line=Box(slot,"Dash",min,max).gameObject.AddComponent<Image>();line.color=new Color(.7f,.6f,.35f);line.raycastTarget=false;
            }
        }
        handStatus=Text(root,"Sprawdzamy układ…",new Vector2(.045f,.685f),new Vector2(.955f,.73f),32);
        var viewport=Box(root,"PlayersViewport",new Vector2(.035f,.18f),new Vector2(.965f,.68f));
        viewport.gameObject.AddComponent<Image>().color=new Color(.025f,.08f,.06f);
        viewport.gameObject.AddComponent<RectMask2D>();
        scroll=viewport.gameObject.AddComponent<ScrollRect>();scroll.horizontal=false;scroll.movementType=ScrollRect.MovementType.Clamped;
        scroll.scrollSensitivity=45;scroll.viewport=viewport;
        var content=Box(viewport,"Players",new Vector2(0,1),Vector2.one);content.pivot=new Vector2(.5f,1);scroll.content=content;
        float width=root.rect.width*.93f;
        float baseline=Mathf.Min(125,(width*.76f-48)/6f);
        float cardWidth=Mathf.Min((width*.76f-36)/3f,baseline*Mathf.Lerp(2f,1f,Mathf.InverseLerp(2,6,hands.Count)));
        rowHeight=cardWidth*1.39f+32;contentHeight=rowHeight*hands.Count;content.sizeDelta=new Vector2(0,contentHeight);
        for(int row=0;row<hands.Count;row++)
        {
            Hand hand=hands[row];
            var rowRect=Box(content,"PlayerRow",new Vector2(0,1),Vector2.one);rowRect.pivot=new Vector2(.5f,1);
            rowRect.sizeDelta=new Vector2(0,rowHeight-8);rowRect.anchoredPosition=new Vector2(0,-row*rowHeight);
            rowRect.gameObject.AddComponent<Image>().color=row%2==0?new Color(.055f,.12f,.085f):new Color(.025f,.08f,.06f);
            var name=Text(rowRect,hand.Name+"\n<size=70%>"+hand.Cards.Count+" kart</size>",new Vector2(.01f,.08f),new Vector2(.22f,.92f),29);
            name.richText=false;name.text=hand.Name+"\n("+hand.Cards.Count+")";
            var handView=Box(rowRect,"HandViewport",new Vector2(.24f,0),new Vector2(1,1));
            handView.gameObject.AddComponent<RectMask2D>();
            var handContent=Box(handView,"Cards",new Vector2(0,.5f),new Vector2(0,.5f));handContent.pivot=new Vector2(0,.5f);
            handContent.sizeDelta=new Vector2(hand.Cards.Count*(cardWidth+6)+12,cardWidth*1.39f);
            if(hand.Cards.Count>6)
            {
                var horizontal=handView.gameObject.AddComponent<ScrollRect>();horizontal.horizontal=true;horizontal.vertical=false;
                horizontal.viewport=handView;horizontal.content=handContent;horizontal.movementType=ScrollRect.MovementType.Clamped;
            }
            for(int c=0;c<hand.Cards.Count;c++)
            {
                var r=Box(handContent,"HiddenCard",new Vector2(0,.5f),new Vector2(0,.5f));
                r.pivot=new Vector2(0,.5f);r.anchoredPosition=new Vector2(6+c*(cardWidth+6),0);r.sizeDelta=new Vector2(cardWidth,cardWidth*1.39f);
                var image=r.gameObject.AddComponent<Image>();image.sprite=back;image.raycastTarget=true;
                entries.Add(new Entry{Card=hand.Cards[c],Image=image,Row=row});
                CardSpriteEntry selected=hand.Cards[c];string playerName=hand.Name;
                var inspect=r.gameObject.AddComponent<Button>();inspect.targetGraphic=image;inspect.transition=Selectable.Transition.None;
                inspect.onClick.AddListener(()=>{if(revealed.Contains(selected))InspectCard(selected,playerName);});
            }
        }
        var footer=Box(root,"ResultBackground",new Vector2(.035f,.015f),new Vector2(.965f,.18f));
        footer.gameObject.AddComponent<Image>().color=new Color(.025f,.08f,.06f);
        verdict=Text(root,"Odkrywam karty kolejno…",new Vector2(.045f,.09f),new Vector2(.955f,.175f),34);
        var button=Box(root,"ReviewAction",new Vector2(.30f,.025f),new Vector2(.96f,.085f));
        var background=button.gameObject.AddComponent<Image>();action=button.gameObject.AddComponent<Button>();action.targetGraphic=background;
        actionLabel=Text(button,"PRZERWIJ ANIMACJĘ",Vector2.zero,Vector2.one,29);
        action.onClick.AddListener(()=>{if(!finished){fast=true;action.interactable=false;}else{gameObject.SetActive(false);Destroy(gameObject);continueAction();}});
        PokerButtonTheme.ApplyTo(action);
        var pauseRect=Box(root,"UtilityReviewPause",new Vector2(.04f,.025f),new Vector2(.28f,.085f));
        pauseRect.gameObject.AddComponent<Image>().color=new Color(.12f,.2f,.16f);
        var pauseButton=pauseRect.gameObject.AddComponent<Button>();
        var pauseLabel=Text(pauseRect,"PAUZA",Vector2.zero,Vector2.one,24);
        pauseButton.onClick.AddListener(()=>{paused=!paused;pauseLabel.text=paused?"WZNÓW":"PAUZA";});
    }
    private IEnumerator Reveal()
    {
        foreach(Entry entry in entries)
        {
            float duration=0;
            while(duration<(fast ? .035f : ProfileTestTools.RevealDelay)){if(!paused||fast)duration+=Time.unscaledDeltaTime;yield return null;}
            if(!fast && contentHeight>scroll.viewport.rect.height)
                scroll.verticalNormalizedPosition=1-Mathf.Clamp01(entry.Row*rowHeight/(contentHeight-scroll.viewport.rect.height));
            entry.Image.sprite=entry.Card.sprite;entry.Image.color=Color.white;
            HotSeatCardFace.Show(entry.Image,entry.Card);revealed.Add(entry.Card);
            RefreshEvidence();
        }
        MultiplayerHandRules.MatchingCards(handId,revealed,out bool complete);
        handStatus.text=complete?"<color=#57E878>UKŁAD JEST NA STOLE</color>":"<color=#FF6660>UKŁADU NIE MA NA STOLE</color>";
        var visibleLines=new List<string>();
        foreach(string line in result.Split('\n'))
            if(line.IndexOf("WYGRYWA",StringComparison.OrdinalIgnoreCase)<0) visibleLines.Add(line);
        verdict.text=string.Join("\n",visibleLines);
        finished=true;RefreshEvidence();action.interactable=true;actionLabel.text="KONTYNUUJ";
        scroll.verticalNormalizedPosition=1;
    }
    private void InspectCard(CardSpriteEntry card,string playerName)
    {
        bool wasPaused=paused;paused=true;
        var panel=Box(transform,"CardInspection",Vector2.zero,Vector2.one);
        var background=panel.gameObject.AddComponent<Image>();background.color=new Color(.01f,.025f,.02f,.99f);
        var close=panel.gameObject.AddComponent<Button>();close.targetGraphic=background;close.transition=Selectable.Transition.None;
        close.onClick.AddListener(()=>{paused=wasPaused;Destroy(panel.gameObject);});
        var name=Text(panel,playerName,new Vector2(.06f,.85f),new Vector2(.94f,.94f),40);name.richText=false;
        var r=Box(panel,"LargeCard",new Vector2(.5f,.5f),new Vector2(.5f,.5f));
        Rect bounds=((RectTransform)transform).rect;
        float width=Mathf.Min(bounds.width*.68f,bounds.height*.62f/1.39f);
        r.sizeDelta=new Vector2(width,width*1.39f);
        var image=r.gameObject.AddComponent<Image>();image.sprite=card.sprite;image.raycastTarget=false;
        HotSeatCardFace.Show(image,card);
        Text(panel,"DOTKNIJ, ABY WRÓCIĆ DO PODSUMOWANIA",new Vector2(.05f,.07f),new Vector2(.95f,.15f),30);
    }
    private void RefreshEvidence()
    {
        List<CardSpriteEntry> matches=MultiplayerHandRules.MatchingCards(handId,revealed,out bool complete);
        foreach(Entry entry in entries)
        {
            var outline=entry.Image.GetComponent<Outline>();
            bool selected=matches.Contains(entry.Card);
            if(selected && outline==null)outline=entry.Image.gameObject.AddComponent<Outline>();
            if(outline!=null){outline.enabled=selected;outline.effectDistance=new Vector2(5,-5);outline.effectColor=complete?new Color(.2f,1,.4f):finished?new Color(1,.25f,.2f):new Color(1,.77f,.15f);}
        }
        for(int i=0;i<slots.Count;i++)
        {
            Image slot=slots[i];HotSeatCardFace.Clear(slot);
            for(int c=0;c<slot.transform.childCount;c++)if(slot.transform.GetChild(c).name=="Dash")slot.transform.GetChild(c).gameObject.SetActive(i>=matches.Count);
            if(i<matches.Count){slot.sprite=matches[i].sprite;slot.color=Color.white;HotSeatCardFace.Show(slot,matches[i]);}
            else{slot.sprite=null;slot.color=new Color(.1f,.16f,.13f);}
            var border=slot.GetComponent<Outline>();
            if(border==null)border=slot.gameObject.AddComponent<Outline>();
            border.enabled=finished;border.effectDistance=new Vector2(5,-5);
            border.effectColor=complete?new Color(.2f,1,.4f):new Color(1,.25f,.2f);
        }
    }
}
