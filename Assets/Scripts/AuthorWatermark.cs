using TMPro;
using UnityEngine;

[ExecuteAlways]
public sealed class AuthorWatermark : MonoBehaviour
{
    private TMP_Text label;
    private Canvas owner;
    public static void Ensure(Canvas canvas)
    {
        if(canvas==null)return;
        Transform existing=canvas.rootCanvas.transform.Find("AuthorWatermark");
        if(existing!=null)
        {
            AuthorWatermark existingMark=existing.GetComponent<AuthorWatermark>();
            if(existingMark==null)existingMark=existing.gameObject.AddComponent<AuthorWatermark>();
            existingMark.owner=canvas.rootCanvas;
            existingMark.label=existing.GetComponent<TMP_Text>();
            existingMark.Layout();
            return;
        }
        GameObject obj=new GameObject("AuthorWatermark",typeof(RectTransform),typeof(TextMeshProUGUI),typeof(AuthorWatermark));
        obj.transform.SetParent(canvas.rootCanvas.transform,false);AuthorWatermark mark=obj.GetComponent<AuthorWatermark>();mark.owner=canvas.rootCanvas;mark.label=obj.GetComponent<TextMeshProUGUI>();
        mark.label.text="© Eryk Potocki";mark.label.fontSize=16;mark.label.alignment=TextAlignmentOptions.Bottom;mark.label.color=new Color(1f,.86f,.62f,.36f);mark.label.raycastTarget=false;mark.Layout();
        // Keep the signature above scene artwork, but below modal canvases.
        obj.transform.SetAsLastSibling();
    }
    private void OnEnable()=>Layout();
    private void LateUpdate()=>Layout();
    private void Layout()
    {
        // Non-serialized references are lost during an editor domain reload.
        if(label==null)label=GetComponent<TMP_Text>();
        if(owner==null)
        {
            Canvas parentCanvas=GetComponentInParent<Canvas>();
            if(parentCanvas!=null)owner=parentCanvas.rootCanvas;
        }
        if(owner==null||label==null||Screen.width<=0||Screen.height<=0)return;RectTransform root=owner.transform as RectTransform,rect=label.rectTransform;
        if(rect.parent!=root)rect.SetParent(root,false);
        label.text="© Eryk Potocki";
        label.fontSize=16;
        label.enableAutoSizing=false;
        label.alignment=TextAlignmentOptions.Bottom;
        label.color=new Color(1f,.86f,.62f,.36f);
        label.raycastTarget=false;
        label.margin=Vector4.zero;
        rect.localScale=Vector3.one;
        rect.localRotation=Quaternion.identity;
        rect.anchorMin=rect.anchorMax=new Vector2(.5f,0);rect.pivot=new Vector2(.5f,0);
        rect.offsetMin=rect.offsetMax=Vector2.zero;
        rect.anchoredPosition=new Vector2(0,4);rect.sizeDelta=new Vector2(320,22);
        if(gameObject.scene.name=="Game")
        {
            float right=(Screen.width-Screen.safeArea.xMax)*root.rect.width/Screen.width;
            float bottom=Screen.safeArea.yMin*root.rect.height/Screen.height;
            rect.anchorMin=rect.anchorMax=new Vector2(1,0);rect.pivot=new Vector2(1,0);
            rect.anchoredPosition=new Vector2(-right-48,bottom+12);
            rect.sizeDelta=new Vector2(250,22);label.alignment=TextAlignmentOptions.BottomRight;
        }
    }
}
