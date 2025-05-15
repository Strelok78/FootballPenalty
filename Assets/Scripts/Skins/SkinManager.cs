using UnityEngine;

public class SkinManager : MonoBehaviour
{
    public Material[] ballMaterials;
    public Renderer ballRenderer;

    public void ApplySkin(int index)
    {
        if (index < ballMaterials.Length)
            ballRenderer.material = ballMaterials[index];
    }
}