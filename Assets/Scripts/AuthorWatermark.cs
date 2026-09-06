using TMPro;
using UnityEngine;

public sealed class AuthorWatermark : MonoBehaviour
{
    private TMP_Text label;
    private Canvas owner;
    public static void Ensure(Canvas canvas)
    {
        if(canvas==null||canvas.rootCanvas.transform.Find("AuthorWatermark")!=null)return;
        GameObject obj=new GameObject("AuthorWatermark",typeof(RectTransform),typeof(TextMeshProUGUI),typeof(AuthorWatermark));
        obj.transform.SetParent(canvas.rootCanvas.transform,false);AuthorWatermark mark=obj.GetComponent<AuthorWatermark>();mark.owner=canvas.rootCanvas;mark.label=obj.GetComponent<TextMeshProUGUI>();
        mark.label.text="© Eryk Potocki";mark.label.fontSize=21;mark.label.alignment=TextAlignmentOptions.BottomRight;mark.label.color=new Color(1f,.86f,.62f,.58f);mark.label.raycastTarget=false;mark.Layout();
        // Keep the signature above scene artwork, but below modal canvases.
        obj.transform.SetAsLastSibling();
    }
    private void Update()=>Layout();
    private void Layout()
    {
        if(owner==null||label==null||Screen.width<=0||Screen.height<=0)return;RectTransform root=owner.transform as RectTransform,rect=label.rectTransform;
        float sx=root.rect.width/Screen.width,sy=root.rect.height/Screen.height;rect.anchorMin=rect.anchorMax=new Vector2(1,0);rect.pivot=new Vector2(1,0);
        rect.anchoredPosition=new Vector2(-(Screen.width-Screen.safeArea.xMax)*sx-22,Screen.safeArea.yMin*sy+18);rect.sizeDelta=new Vector2(280,38);
    }
}
