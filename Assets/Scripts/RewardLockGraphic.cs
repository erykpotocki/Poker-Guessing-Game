using UnityEngine;
using UnityEngine.UI;

/// <summary>A high-contrast padlock with an open, rounded shackle and keyhole.</summary>
[RequireComponent(typeof(CanvasRenderer))]
public sealed class RewardLockGraphic : MaskableGraphic
{
    protected override void OnPopulateMesh(VertexHelper mesh)
    {
        mesh.Clear();
        Color dark=new Color(.09f,.055f,.025f,1);
        // Dark silhouette separates the lock from both light and dark portraits.
        Arch(mesh,12,6,8,dark);
        Rounded(mesh,-19,-23,38,31,5,dark);
        Arch(mesh,10,5,8,new Color(.94f,.77f,.39f));
        Arch(mesh,8.8f,1.3f,8,new Color(1,.94f,.68f));
        Rounded(mesh,-17,-20,34,27,4,new Color(.62f,.36f,.10f));
        Rounded(mesh,-16,-17,32,24,3,new Color(.95f,.72f,.28f));
        Rounded(mesh,-14,-5,28,10,2,new Color(1,.84f,.43f));
        // A round opening and tapered stem read as a keyhole, not a slit.
        Rounded(mesh,-4,-8,8,8,4,dark);
        Rounded(mesh,-2,-14,4,9,1,dark);
        Rounded(mesh,-11,-16,22,1.2f,.6f,new Color(1,.87f,.49f,.8f));
    }

    private void Arch(VertexHelper mesh,float radius,float thickness,float y,Color tint)
    {
        for(int i=0;i<24;i++)
        {
            float a=i*Mathf.PI/24,b=(i+1)*Mathf.PI/24;
            int first=mesh.currentVertCount;
            Vertex(mesh,new Vector2(Mathf.Cos(a)*radius,y+Mathf.Sin(a)*radius),tint);
            Vertex(mesh,new Vector2(Mathf.Cos(b)*radius,y+Mathf.Sin(b)*radius),tint);
            Vertex(mesh,new Vector2(Mathf.Cos(b)*(radius-thickness),y+Mathf.Sin(b)*(radius-thickness)),tint);
            Vertex(mesh,new Vector2(Mathf.Cos(a)*(radius-thickness),y+Mathf.Sin(a)*(radius-thickness)),tint);
            mesh.AddTriangle(first,first+1,first+2);mesh.AddTriangle(first,first+2,first+3);
        }
        Rounded(mesh,-radius,1,thickness,y,0,tint);
        Rounded(mesh,radius-thickness,1,thickness,y,0,tint);
    }

    private void Rounded(VertexHelper mesh,float x,float y,float width,float height,float radius,Color tint)
    {
        int first=mesh.currentVertCount;
        Vertex(mesh,new Vector2(x+width/2,y+height/2),tint);
        for(int corner=0;corner<4;corner++)
        {
            Vector2 center=new Vector2(corner==0||corner==3?x+width-radius:x+radius,
                corner<2?y+height-radius:y+radius);
            for(int step=0;step<=6;step++)
            {
                float angle=(corner*90+step*15)*Mathf.Deg2Rad;
                Vertex(mesh,center+new Vector2(Mathf.Cos(angle),Mathf.Sin(angle))*radius,tint);
            }
        }
        for(int i=0;i<28;i++)mesh.AddTriangle(first,first+1+i,first+1+(i+1)%28);
    }

    private void Vertex(VertexHelper mesh,Vector2 point,Color tint)
    {
        var vertex=UIVertex.simpleVert;
        vertex.position=point;vertex.color=tint*color;mesh.AddVert(vertex);
    }
}
