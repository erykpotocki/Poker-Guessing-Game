using System.Collections;
using System.Collections.Generic;
using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.UI;

public sealed class PlayerReactions : MonoBehaviour, IOnEventCallback
{
    private const byte EventCode=83;
    public int Actor;
    private static float lastSent=-10;
    private float lastReceived=-10;
    private static readonly Dictionary<int,Sprite> faces=new();
    private void OnEnable()=>PhotonNetwork.AddCallbackTarget(this);
    private void OnDisable()=>PhotonNetwork.RemoveCallbackTarget(this);
    public static void Send(int face)
    {
        if(!PhotonNetwork.InRoom||Time.unscaledTime-lastSent<2f)return;
        lastSent=Time.unscaledTime;
        PhotonNetwork.RaiseEvent(EventCode,face,new RaiseEventOptions{Receivers=ReceiverGroup.All},SendOptions.SendReliable);
    }
    public void OnEvent(EventData data)
    {
        if(data.Code!=EventCode||data.Sender!=Actor||!(data.CustomData is int face)||face<0||face>=6||Time.unscaledTime-lastReceived<1.5f)return;
        lastReceived=Time.unscaledTime;StartCoroutine(Play(face));
    }
    private IEnumerator Play(int face)
    {
        var r=new GameObject("AnimatedReaction",typeof(RectTransform),typeof(Image)).GetComponent<RectTransform>();r.SetParent(transform,false);
        r.anchorMin=r.anchorMax=new Vector2(.5f,.5f);r.sizeDelta=Vector2.one*110;
        var image=r.GetComponent<Image>();image.sprite=Face(face);image.raycastTarget=false;
        for(float t=0;t<2.3f;t+=Time.unscaledDeltaTime)
        {
            r.anchoredPosition=new Vector2(0,80+25*t);r.localScale=Vector3.one*Mathf.Min(1,t*6)*(1+.08f*Mathf.Sin(t*12));r.localRotation=Quaternion.Euler(0,0,Mathf.Sin(t*9)*8);
            image.color=new Color(1,1,1,Mathf.Min(1,(2.3f-t)*2));yield return null;
        }
        Destroy(r.gameObject);
    }
    public static Sprite Face(int kind)
    {
        if(faces.TryGetValue(kind,out var found))return found;
        const int n=96;var texture=new Texture2D(n,n,TextureFormat.RGBA32,false);var pixels=new Color[n*n];
        for(int y=0;y<n;y++)for(int x=0;x<n;x++)
        {
            float dx=x-48,dy=y-48;Color c=dx*dx+dy*dy<44*44?new Color(1,.76f,.12f):Color.clear;
            bool eye=((x-32)*(x-32)+(y-58)*(y-58)<16)||((x-64)*(x-64)+(y-58)*(y-58)<16);
            float arc=(x-48)*(x-48)+(y-43)*(y-43);
            bool mouth=kind==3?((x-48)*(x-48)+(y-29)*(y-29)<75):y<40&&arc>340&&arc<470&&x>27&&x<69;
            if(kind==4)mouth=y>26&&y<31&&x>30&&x<66;
            if(kind==5)eye=(y>54&&y<64&&((x>23&&x<41)||(x>55&&x<73)));
            if(eye||mouth)c=new Color(.14f,.08f,.025f);
            if(kind==1&&y<52&&y>32&&((x-23)*(x-23)+(y-42)*(y-42)<50||(x-73)*(x-73)+(y-42)*(y-42)<50))c=new Color(.2f,.75f,1);
            if(kind==2&&((x-30)*(x-30)+(y-58)*(y-58)<50||(x-66)*(x-66)+(y-58)*(y-58)<50))c=new Color(1,.15f,.3f);
            pixels[y*n+x]=c;
        }
        texture.SetPixels(pixels);texture.Apply();var sprite=Sprite.Create(texture,new Rect(0,0,n,n),new Vector2(.5f,.5f));faces[kind]=sprite;return sprite;
    }
}
