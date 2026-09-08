using TMPro;
using UnityEngine;
using UnityEngine.UI;

// Only called by HotSeatSetupUI; multiplayer card sprites are untouched.
public static class HotSeatCardFace
{
    public static void Clear(Image image)
    {
        Transform old=image.transform.Find("HotSeatFace");
        if(old==null)return;
        old.gameObject.SetActive(false);
        Object.Destroy(old.gameObject);
    }

    private static RectTransform Area(Transform parent,string name,Vector2 min,Vector2 max)
    {
        var rect=new GameObject(name,typeof(RectTransform)).GetComponent<RectTransform>();
        rect.SetParent(parent,false);rect.anchorMin=min;rect.anchorMax=max;
        rect.offsetMin=rect.offsetMax=Vector2.zero;
        return rect;
    }

    private static void Label(Transform parent,string text,Vector2 min,Vector2 max,Color color)
    {
        var label=Area(parent,"Rank",min,max).gameObject.AddComponent<TextMeshProUGUI>();
        label.text=text;label.color=color;label.alignment=TextAlignmentOptions.Center;
        label.fontSize=180;label.enableAutoSizing=true;label.fontSizeMin=3;label.fontSizeMax=180;
        label.fontStyle=FontStyles.Bold;label.raycastTarget=false;
    }

    public static void Show(Image image,CardSpriteEntry card)
    {
        Clear(image);
        var panel=Area(image.transform,"HotSeatFace",new Vector2(.065f,.055f),new Vector2(.935f,.945f));
        var paper=panel.gameObject.AddComponent<Image>();paper.color=new Color(.95f,.90f,.80f);paper.raycastTarget=false;
        Color ink=card.suit==CardSuit.Kier || card.suit==CardSuit.Karo?new Color(.65f,.035f,.025f):new Color(.08f,.065f,.055f);
        string rank=card.rank==CardRank.Nine?"9":card.rank==CardRank.Ten?"10":card.rank==CardRank.Jack?"J":card.rank==CardRank.Queen?"Q":card.rank==CardRank.King?"K":"A";
        string court=card.rank==CardRank.Jack?"Jack":card.rank==CardRank.Queen?"Queen":card.rank==CardRank.King?"King":null;
        if(court!=null)
        {
            for(int half=0;half<2;half++)
            {
                var portrait=Area(panel,"CourtPortrait",new Vector2(.19f,half==0?.5f:.08f),new Vector2(.81f,half==0?.92f:.5f)).gameObject.AddComponent<RawImage>();
                portrait.texture=Resources.Load<Texture2D>("HotSeatCourt/"+court);
                portrait.uvRect=new Rect(0,.34f,1,.66f);portrait.raycastTarget=false;
                if(half==1)portrait.rectTransform.localRotation=Quaternion.Euler(0,0,180);
            }
        }
        else if(card.rank==CardRank.Ace) Pip(panel,card.suit,ink,.5f,.5f,.38f,.30f,false);
        else
        {
            for(int row=0;row<4;row++)
            foreach(float x in new[]{.32f,.68f})
                Pip(panel,card.suit,ink,x,.22f+row*.1867f,.18f,.13f,row<2);
            if(card.rank==CardRank.Nine)Pip(panel,card.suit,ink,.5f,.5f,.18f,.13f,false);
            else {Pip(panel,card.suit,ink,.5f,.313f,.18f,.13f,true);Pip(panel,card.suit,ink,.5f,.687f,.18f,.13f,false);}
        }
        for(int corner=0;corner<2;corner++)
        {
            var index=Area(panel,"CornerIndex",corner==0?new Vector2(0,.74f):new Vector2(.79f,0),corner==0?new Vector2(.21f,1):new Vector2(1,.26f));
            if(corner==1)index.localRotation=Quaternion.Euler(0,0,180);
            Label(index,rank,new Vector2(0,.4f),Vector2.one,ink);
            Pip(index,card.suit,ink,.5f,.20f,.62f,.32f,false);
        }
    }

    private static void Pip(Transform parent,CardSuit suit,Color color,float x,float y,float width,float height,bool inverted)
    {
        var pip=Area(parent,"SuitPip",new Vector2(x-width*.5f,y-height*.5f),new Vector2(x+width*.5f,y+height*.5f)).gameObject.AddComponent<HotSeatSuitGraphic>();
        pip.Suit=suit;pip.color=color;pip.raycastTarget=false;
        if(inverted)pip.rectTransform.localRotation=Quaternion.Euler(0,0,180);
    }
}
