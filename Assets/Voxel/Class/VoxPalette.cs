using UnityEngine;

public class VoxPalette : ScriptableObject
{
    public enum MaterialType { DIFFUSE, METAL, GLASS, EMIT };

    public Color32[] colors;
    public MaterialType[] matRefs;
    public Texture2D mainTexture;

    public Color GetColor(byte index)
    {
        return colors[index];
    }

    public void SetTexture(){
        mainTexture = GetTexture();
    }
    public Texture2D GetTexture()
    {
        Texture2D paletteTexture = new Texture2D(256, 1, TextureFormat.RGBA32, false);
        paletteTexture.SetPixels32(colors, 0);
        paletteTexture.filterMode = FilterMode.Point;
        paletteTexture.Apply();
        return paletteTexture;
    }
}