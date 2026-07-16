using UnityEngine;
using UnityEngine.UI;

public class CrosshairSwap : MonoBehaviour
{
    public Image crosshairImage;
    public Sprite defaultCrosshair;
    public Sprite grappleCrosshair;

    public GrappleScript grappleScript;
    
    public int grapplableLayerMask;

    private void awake()
    {
        // Ensure we have a reference to the GrappleScript
        if (grappleScript == null)
        {
            grappleScript = FindAnyObjectByType<GrappleScript>();
        }
    }

    private void Start()
    {
        // Get the layer index and convert it to a bitmask layer mask
        // This is much faster and more reliable than string comparisons in Update
        grapplableLayerMask = LayerMask.GetMask("Grappable");

        // Set initial crosshair
        if (crosshairImage != null && defaultCrosshair != null)
        {
            crosshairImage.sprite = defaultCrosshair;
        }
    }

    private void Update()
    {
        CheckTargetUnderCrosshair();
    }

    

    private void CheckTargetUnderCrosshair()
    {
        if (crosshairImage == null) return;

        // Create a ray shooting straight out of the center of the camera viewport
        Ray ray = Camera.main.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        RaycastHit hit;

        // Shoot the ray. 
        // Note: We DO NOT pass the grapplableLayerMask to the Raycast itself, 
        // because we want to know if physical walls block our view of the grapplable point!
        if (Physics.Raycast(ray, out hit, grappleScript.maxSwingDistance))
        {
            // Check if the object we hit is on the "Grapplable" layer.
            // (1 << hit.collider.gameObject.layer) converts the layer ID to its bitmask format
            if ((grapplableLayerMask & (1 << hit.collider.gameObject.layer)) != 0)
            {
                crosshairImage.sprite = grappleCrosshair;
                return;
            }
        }

        // If we hit nothing, or hit something else (like a normal wall), use the default
        crosshairImage.sprite = defaultCrosshair;
    }
}
