using System.Linq;
using PokerProfile;
using UnityEngine;
using UnityEngine.UI;

public sealed class MissionsUI : MonoBehaviour
{
    private int tab;
    private static readonly string[] ids={"first_game","read_rules","first_win","ten_rounds"};
    private static readonly string[] names={"Ukończ pierwszą grę","Przeczytaj zasady","Wygraj pierwszą grę","Rozegraj 10 rund"};
    private static readonly string[] prizes={"Ramka za pierwszą grę","10 diamentów","100 złota","50 złota"};
    public static void Show(Canvas owner)
    {
        var old=owner.rootCanvas.transform.Find("MissionsOverlay");if(old!=null)Destroy(old.gameObject);
        ShopUI.Overlay(owner,"MissionsOverlay").gameObject.AddComponent<MissionsUI>().Build();
    }
    private static bool Ready(int i)
    {
        var d=PlayerProfileService.Data;
        return i==0?d.Statistics.GamesPlayed>0:i==1?d.RulesRead:i==2?d.Statistics.GamesWon>0:d.Statistics.RoundsPlayed>=10;
    }
    private void Build()
    {
        foreach(Transform child in transform){child.gameObject.SetActive(false);Destroy(child.gameObject);}
        var d=PlayerProfileService.Data;d.ClaimedIntroMissions??=new System.Collections.Generic.List<string>();
        // Preserve a frame already awarded before missions were introduced.
        if(d.Inventory.OwnedFrames.Contains("classic_wood")&&!d.ClaimedIntroMissions.Contains(ids[0]))d.ClaimedIntroMissions.Add(ids[0]);
        var r=(RectTransform)transform;float w=r.rect.width,h=r.rect.height;
        ShopUI.Text(r,"MISJE",20,20,w-140,70,44);ShopUI.Button(r,"×",w-100,20,76,66,()=>Destroy(gameObject));
        string[] tabs={"Wszystkie misje","Nieukończone / w trakcie","Ukończone"};
        for(int i=0;i<3;i++){int n=i;ShopUI.Button(r,tabs[i],16+i*(w-32)/3,110,(w-38)/3,90,()=>{tab=n;Build();});}
        var view=ShopUI.Rect("MissionList",r,16,220,w-32,h-240);view.gameObject.AddComponent<Image>().color=Color.clear;view.gameObject.AddComponent<RectMask2D>();
        var scroll=view.gameObject.AddComponent<ScrollRect>();scroll.horizontal=false;scroll.movementType=ScrollRect.MovementType.Clamped;scroll.viewport=view;
        var visible=Enumerable.Range(0,ids.Length).Where(i=>tab==2?d.ClaimedIntroMissions.Contains(ids[i]):!d.ClaimedIntroMissions.Contains(ids[i])&&(tab==0||!Ready(i))).OrderByDescending(Ready).ToArray();
        var content=ShopUI.Rect("Content",view,0,0,w-32,visible.Length*160);scroll.content=content;
        for(int n=0;n<visible.Length;n++)
        {
            int i=visible[n];bool claimed=d.ClaimedIntroMissions.Contains(ids[i]),ready=Ready(i);
            var row=ShopUI.Rect("Mission",content,0,n*160,w-32,148);row.gameObject.AddComponent<Image>().color=ready&&!claimed?new Color(.055f,.25f,.1f):new Color(.09f,.07f,.035f);
            ShopUI.Text(row,names[i]+"\n"+prizes[i],12,12,(w-32)*.61f,85,28);
            ShopUI.Text(row,ready?"Ukończono":"W trakcie",12,100,(w-32)*.58f,36,23);
            var button=ShopUI.Button(row,claimed?"Ukończono":"Odbierz nagrodę",(w-32)*.64f,30,(w-32)*.34f,85,()=>Claim(i));button.interactable=ready&&!claimed;
        }
        if(visible.Length==0)ShopUI.Text(content,"Brak misji w tej zakładce",10,20,w-52,80,30);
    }
    private void Claim(int i)
    {
        var d=PlayerProfileService.Data;if(!Ready(i)||d.ClaimedIntroMissions.Contains(ids[i]))return;
        d.ClaimedIntroMissions.Add(ids[i]);
        if(i==0)ProgressionRules.Unlock(d,"frame","classic_wood","Ramka za pierwszą grę","mission");
        else if(i==1)d.Wallet.RewardCurrency+=10;
        else d.Wallet.Coins+=i==2?100:50;
        PlayerProfileService.Save();Build();
        if(i==0)UnlockPresentationUI.ShowPending(GetComponent<Canvas>());
    }
}
