using UnityEngine;
using UnityEngine.UI;

// Geometry avoids missing icon glyphs in the project's TMP font.
[RequireComponent(typeof(CanvasRenderer))]
public sealed class GameUtilityGlyph : MaskableGraphic
{
    public bool settings, muted;
    protected override void OnPopulateMesh(VertexHelper mesh)
    {
        mesh.Clear();
        if (settings)
        {
            for (int i = 0; i < 3; i++) { Stroke(mesh, new Vector2(-14f, 10f-i*10f), new Vector2(14f, 10f-i*10f), 2f); Stroke(mesh, new Vector2(i==1?6f:-5f, 14f-i*10f), new Vector2(i==1?6f:-5f, 6f-i*10f), 5f); }
        }
        else
        {
            Stroke(mesh, new Vector2(-12f,-5f), new Vector2(-12f,5f), 6f);
            Stroke(mesh, new Vector2(-10f,5f), new Vector2(0f,12f), 3f);
            Stroke(mesh, new Vector2(0f,12f), new Vector2(0f,-12f), 3f);
            Stroke(mesh, new Vector2(0f,-12f), new Vector2(-10f,-5f), 3f);
            if (muted) Stroke(mesh, new Vector2(-15f,-15f), new Vector2(15f,15f), 3f);
            else { Stroke(mesh, new Vector2(7f,-6f), new Vector2(7f,6f), 2f); Stroke(mesh, new Vector2(13f,-11f), new Vector2(13f,11f), 2f); }
        }
    }
    private void Stroke(VertexHelper mesh, Vector2 a, Vector2 b, float width)
    {
        Vector2 delta = b-a;
        Vector2 normal = new Vector2(-delta.y,delta.x).normalized * width * 0.5f;
        int index = mesh.currentVertCount;
        mesh.AddVert(a-normal, color, Vector2.zero); mesh.AddVert(a+normal, color, Vector2.zero);
        mesh.AddVert(b+normal, color, Vector2.zero); mesh.AddVert(b-normal, color, Vector2.zero);
        mesh.AddTriangle(index,index+1,index+2); mesh.AddTriangle(index,index+2,index+3);
    }
}
