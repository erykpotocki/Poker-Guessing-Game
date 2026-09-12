using Photon.Pun;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public sealed class PublicPlayerProfileUI : MonoBehaviour
{
    public static void Show(Canvas canvas,Sprite avatar,string nickname,int games,int wins,string profileId,int actor=0)
    {
        if(canvas==null)return;
        var old=canvas.rootCanvas.transform.Find("PublicPlayerProfile");if(old!=null)Destroy(old.gameObject);
        var root=ShopUI.Overlay(canvas,"PublicPlayerProfile");root.GetComponent<Image>().color=new Color(0,0,0,.83f);
        float w=780,h=820;float scale=Mathf.Min(1,Mathf.Min((root.rect.width-30)/w,(root.rect.height-30)/h));
        var card=ShopUI.Rect("ProfileCard",root,(root.rect.width-w)/2,(root.rect.height-h)/2,w,h);card.pivot=new Vector2(.5f,.5f);card.anchorMin=card.anchorMax=new Vector2(.5f,.5f);card.anchoredPosition=Vector2.zero;card.localScale=Vector3.one*scale;card.gameObject.AddComponent<Image>().color=new Color(.055f,.075f,.06f);
        ShopUI.Button(card,"ZAMKNIJ",20,18,160,64,()=>Destroy(root.gameObject));
        var portrait=ShopUI.Rect("Avatar",card,w-190,22,150,150).gameObject.AddComponent<Image>();portrait.sprite=avatar;portrait.preserveAspect=true;AvatarCircleUtility.Apply(portrait);
        var name=ShopUI.Text(card,nickname,20,175,w-50,58,38);name.richText=false;name.alignment=TextAlignmentOptions.Right;
        int Stat(string key,int fallback=0)
        {
            var player=PhotonNetwork.CurrentRoom?.GetPlayer(actor);
            return player!=null&&player.CustomProperties.TryGetValue(key,out object value)&&value is int i?i:fallback;
        }
        string Value(string key,string fallback)
        {
            var player=PhotonNetwork.CurrentRoom?.GetPlayer(actor);
            return player!=null&&player.CustomProperties.TryGetValue(key,out object value)?value?.ToString()??fallback:fallback;
        }
        games=Stat(PhotonAvatarSync.GamesPlayedKey,games);wins=Stat(PhotonAvatarSync.GamesWonKey,wins);
        string[] rows={"Rozegrane gry: "+games,"Wygrane: "+wins+"   •   "+(games>0?100f*wins/games:0).ToString("0.0")+"%",
            "Wygrane rundy: "+Stat("roundWinsV1")+"   •   Eliminacje: "+Stat("eliminationsV1"),
            "Poziom: "+Stat("levelV1",1)+"   •   Dołączył: "+Value("joinedV1","—"),
            "Odblokowane: "+Stat("avatarsV1")+" avatarów · "+Stat("framesV1")+" ramek · "+Stat("backsV1")+" rewersów","Odznaki: Beta tester"};
        for(int i=0;i<rows.Length;i++)
        {
            float y=246+i*65;var label=ShopUI.Text(card,rows[i],24,y,w-54,58,27);label.alignment=TextAlignmentOptions.Right;
            var line=ShopUI.Rect("Divider",card,28,y+60,w-56,1).gameObject.AddComponent<Image>();line.color=new Color(.6f,.48f,.22f,.4f);line.raycastTarget=false;
        }
        if(PhotonNetwork.LocalPlayer!=null&&actor==PhotonNetwork.LocalPlayer.ActorNumber)
        {
            ShopUI.Text(card,"WYŚLIJ EMOTKĘ",20,650,w-40,48,26);
            for(int i=0;i<6;i++)
            {
                int face=i;var button=ShopUI.Button(card,"",20+i*(w-40)/6,710,(w-52)/6,76,()=>{PlayerReactions.Send(face);Destroy(root.gameObject);});
                var image=ShopUI.Rect("Face",button.transform,8,4,62,62).gameObject.AddComponent<Image>();image.sprite=PlayerReactions.Face(i);image.raycastTarget=false;
            }
        }
    }
}
