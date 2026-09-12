using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// Horizontal cards inside a vertical history: choose the axis once per gesture.
public sealed class HistoryRowScroll : ScrollRect
{
    private ScrollRect outer;
    private bool verticalGesture;
    public override void OnBeginDrag(PointerEventData data)
    {
        outer = transform.parent.GetComponentInParent<ScrollRect>();
        verticalGesture = outer != null && Mathf.Abs(data.delta.y) > Mathf.Abs(data.delta.x);
        if (verticalGesture) outer.OnBeginDrag(data); else base.OnBeginDrag(data);
    }
    public override void OnDrag(PointerEventData data)
    { if (verticalGesture && outer != null) outer.OnDrag(data); else base.OnDrag(data); }
    public override void OnEndDrag(PointerEventData data)
    { if (verticalGesture && outer != null) outer.OnEndDrag(data); else base.OnEndDrag(data); }
    public override void OnScroll(PointerEventData data)
    {
        ScrollRect parentScroll = transform.parent.GetComponentInParent<ScrollRect>();
        if (parentScroll != null && Mathf.Abs(data.scrollDelta.y) >= Mathf.Abs(data.scrollDelta.x)) parentScroll.OnScroll(data);
        else base.OnScroll(data);
    }
}
