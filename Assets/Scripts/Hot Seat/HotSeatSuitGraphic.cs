using UnityEngine;
using UnityEngine.UI;

// Vector pips: independent of font glyph availability on phones.
public sealed class HotSeatSuitGraphic : MaskableGraphic
{
    public CardSuit Suit;
    protected override void OnPopulateMesh(VertexHelper mesh)
    {
        mesh.Clear();
        if(Suit==CardSuit.Karo)
            Polygon(mesh,new[]{new Vector2(0,1),new Vector2(1,0),new Vector2(0,-1),new Vector2(-1,0)},Vector2.zero);
        else if(Suit==CardSuit.Trefl)
        {
            Circle(mesh,new Vector2(0,.43f),.49f);
            Circle(mesh,new Vector2(-.43f,-.08f),.49f);
            Circle(mesh,new Vector2(.43f,-.08f),.49f);
            Stem(mesh);
        }
        else
        {
            var points=new Vector2[64];
            for(int i=0;i<points.Length;i++)
            {
                float t=i*Mathf.PI*2/points.Length;
                float x=Mathf.Pow(Mathf.Sin(t),3);
                float y=(13*Mathf.Cos(t)-5*Mathf.Cos(2*t)-2*Mathf.Cos(3*t)-Mathf.Cos(4*t)+2)/17f;
                points[i]=Suit==CardSuit.Kier?new Vector2(x,y):new Vector2(x,-y*.8f+.18f);
            }
            Polygon(mesh,points,Vector2.zero);
            if(Suit==CardSuit.Pik)Stem(mesh);
        }
    }
    private void Stem(VertexHelper mesh)=>Polygon(mesh,new[]{new Vector2(-.12f,-.15f),new Vector2(.12f,-.15f),new Vector2(.35f,-.94f),new Vector2(-.35f,-.94f)},new Vector2(0,-.5f));
    private void Circle(VertexHelper mesh,Vector2 center,float radius)
    {
        var points=new Vector2[24];
        for(int i=0;i<24;i++){float t=i*Mathf.PI*2/24;points[i]=center+new Vector2(Mathf.Cos(t),Mathf.Sin(t))*radius;}
        Polygon(mesh,points,center);
    }
    private void Polygon(VertexHelper mesh,Vector2[] points,Vector2 center)
    {
        Rect r=rectTransform.rect;int first=mesh.currentVertCount;
        mesh.AddVert(r.center+Vector2.Scale(center,r.size*.5f),color,Vector2.zero);
        foreach(Vector2 point in points)mesh.AddVert(r.center+Vector2.Scale(point,r.size*.5f),color,Vector2.zero);
        for(int i=0;i<points.Length;i++)mesh.AddTriangle(first,first+1+i,first+1+(i+1)%points.Length);
    }
}
