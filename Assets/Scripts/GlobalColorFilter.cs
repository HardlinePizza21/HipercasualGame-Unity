using UnityEngine;

[ExecuteAlways]
public class GlobalColorFilter : MonoBehaviour
{
    public Material filterMaterial;

    void OnRenderImage(RenderTexture src, RenderTexture dest)
    {
        if (filterMaterial != null)
            Graphics.Blit(src, dest, filterMaterial);
        else
            Graphics.Blit(src, dest);
    }
}
