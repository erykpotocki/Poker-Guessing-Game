using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public sealed class ProfileSwipeScroll : ScrollRect
{
    public Action<int> Navigate;
    private Vector2 start;
    private bool horizontalGesture;
    public override void OnBeginDrag(PointerEventData eventData)
    {
        start=eventData.pressPosition;
        Vector2 delta=eventData.position-start;
        horizontalGesture=Mathf.Abs(delta.x)>Mathf.Abs(delta.y);
        if(!horizontalGesture)base.OnBeginDrag(eventData);
    }
    public override void OnDrag(PointerEventData eventData)
    {
        if(!horizontalGesture)base.OnDrag(eventData);
    }
    public override void OnEndDrag(PointerEventData eventData)
    {
        if(!horizontalGesture){base.OnEndDrag(eventData);return;}
        Vector2 delta=eventData.position-start;
        if(Mathf.Abs(delta.x)>=Mathf.Max(40,Screen.width*.08f))Navigate?.Invoke(delta.x>0?-1:1);
    }
}

