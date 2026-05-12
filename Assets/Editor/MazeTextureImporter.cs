using UnityEditor;

/// <summary>
/// Automatically enables Read/Write on any texture placed inside Assets/Textures/Maze/
/// so LaserPointer.cs can call Texture2D.GetPixel() at runtime.
/// </summary>
public class MazeTextureImporter : AssetPostprocessor
{
    void OnPreprocessTexture()
    {
        if (!assetPath.Contains("Assets/Textures/Maze/")) return;

        TextureImporter ti = (TextureImporter)assetImporter;
        ti.isReadable = true;
        ti.alphaIsTransparency = true;
        ti.textureCompression  = TextureImporterCompression.Uncompressed;
    }
}
