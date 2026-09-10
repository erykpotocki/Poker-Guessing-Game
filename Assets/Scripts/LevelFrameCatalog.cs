using System.Collections.Generic;
using UnityEngine;

public static class LevelFrameCatalog
{
    public static readonly int[] Levels={10,20,30,40,50,60,70,80,90,100,120,140,160,180,190,200,220,240,260,280,299,300,400};
    private static readonly Dictionary<int,Sprite> sprites=new Dictionary<int,Sprite>();
    public static Sprite Resolve(string id)
    {
        if(id=="classic_wood")return Resources.Load<Sprite>("Cosmetics/ClassicWood");
        if(id==null||!id.StartsWith("level:")||!int.TryParse(id.Substring(6),out int level))return null;
        if(sprites.TryGetValue(level,out var sprite))return sprite;
        var texture=Resources.Load<Texture2D>("LevelFrames/"+level);
        if(texture==null)return null;
        sprite=Sprite.Create(texture,new Rect(0,0,texture.width,texture.height),new Vector2(.5f,.5f));
        sprites[level]=sprite;return sprite;
    }
}
