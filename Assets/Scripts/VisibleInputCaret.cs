using TMPro;
using UnityEngine;
using UnityEngine.UI;

public sealed class VisibleInputCaret : MonoBehaviour
{
    private TMP_InputField field;
    private Image caret;
    private void Awake()
    {
        field=GetComponent<TMP_InputField>();field.customCaretColor=true;field.caretColor=Color.clear;
        var r=new GameObject("VisibleCaret",typeof(RectTransform),typeof(Image)).GetComponent<RectTransform>();r.SetParent(transform,false);r.pivot=new Vector2(.5f,.5f);
        caret=r.GetComponent<Image>();caret.color=Color.black;caret.raycastTarget=false;
    }
    private void LateUpdate()
    {
        if(field==null||field.textComponent==null)return;
        caret.enabled=field.isFocused&&field.selectionAnchorPosition==field.selectionFocusPosition&&Mathf.Repeat(Time.unscaledTime,1.1f)<.7f;
        if(!caret.enabled)return;
        var text=field.textComponent;text.ForceMeshUpdate();var info=text.textInfo;
        int pos=Mathf.Clamp(field.caretPosition,0,info.characterCount);float x=text.rectTransform.rect.xMin,y=text.rectTransform.rect.center.y,height=text.fontSize;
        if(info.characterCount>0)
        {
            var ch=info.characterInfo[Mathf.Clamp(pos-1,0,info.characterCount-1)];
            x=pos==0?info.characterInfo[0].origin:ch.xAdvance;y=(ch.ascender+ch.descender)/2;height=ch.ascender-ch.descender;
        }
        var r=caret.rectTransform;r.position=text.rectTransform.TransformPoint(new Vector3(x+1,y,0));
        var parent=(RectTransform)transform;var p=parent.InverseTransformPoint(r.position);p.x=Mathf.Clamp(p.x,parent.rect.xMin+4,parent.rect.xMax-4);r.position=parent.TransformPoint(p);
        r.sizeDelta=new Vector2(3,Mathf.Max(20,height));r.SetAsLastSibling();
    }
}
