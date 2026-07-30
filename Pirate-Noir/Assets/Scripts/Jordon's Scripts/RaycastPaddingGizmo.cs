using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Graphic))]
public class raycastpaddinggizmo : MonoBehaviour
{
    void OnDrawGizmosSelected()
    {
        Graphic graphic = GetComponent<Graphic>();
        if (graphic == null) return;

        RectTransform rectTransform = transform as RectTransform;
        if (rectTransform == null) return;

        // Vector4 padding layout: X=Left, Y=Bottom, Z=Right, W=Top
        Vector4 padding = graphic.raycastPadding;
        Rect rect = rectTransform.rect;
        
        // Calculate the padded rectangle dimensions
        float paddedWidth = rect.width - (padding.x + padding.z);
        float paddedHeight = rect.height - (padding.y + padding.w);

        // Center calculation needs to shift depending on asymmetric padding offsets
        float centerX = rect.x + padding.x + (paddedWidth * 0.5f);
        float centerY = rect.y + padding.y + (paddedHeight * 0.5f);

        Vector3 paddedCenter = new Vector3(centerX, centerY, 0f);
        Vector3 paddedSize = new Vector3(paddedWidth, paddedHeight, 0.01f);

        // Apply local transformation matrix so the Gizmo scales and rotates with the UI
        Gizmos.matrix = rectTransform.localToWorldMatrix;
        
        // Draw the wireframe bounding zone
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(paddedCenter, paddedSize);
    }
}
