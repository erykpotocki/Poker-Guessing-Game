using UnityEngine;
using UnityEngine.UI;

// The visible avatar is itself a circle, even if a nested canvas bypasses stencil masking.
public sealed class CircularAvatarMesh : BaseMeshEffect
{
    public override void ModifyMesh(VertexHelper mesh)
    {
        if(!IsActive())return;
        Image image=graphic as Image;
        if(image==null || image.sprite==null)return;
        Rect rect=image.rectTransform.rect;
        Vector4 uv=UnityEngine.Sprites.DataUtility.GetOuterUV(image.sprite);
        Vector2 middle=new Vector2((uv.x+uv.z)*.5f,(uv.y+uv.w)*.5f);
        Vector2 span=new Vector2((uv.z-uv.x)*.5f,(uv.w-uv.y)*.5f);
        float aspect=image.sprite.rect.width/image.sprite.rect.height;
        if(aspect>1)span.x/=aspect;else span.y*=aspect;
        float radius=Mathf.Min(rect.width,rect.height)*.49f;
        mesh.Clear();mesh.AddVert(rect.center,image.color,middle);
        const int sides=64;
        for(int i=0;i<sides;i++)
        {
            float angle=i*Mathf.PI*2/sides;
            Vector2 direction=new Vector2(Mathf.Cos(angle),Mathf.Sin(angle));
            mesh.AddVert(rect.center+direction*radius,image.color,middle+Vector2.Scale(direction,span));
        }
        for(int i=0;i<sides;i++)mesh.AddTriangle(0,i+1,(i+1)%sides+1);
    }
}
