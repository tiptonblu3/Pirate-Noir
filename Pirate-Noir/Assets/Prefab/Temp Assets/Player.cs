using UnityEngine;

public class Player : MonoBehaviour
{
    private Renderer playerRenderer;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        playerRenderer = GetComponent<Renderer>();
    }

    // Update is called once per frame
    void Update()
    {
        playerRenderer.material.color = new Color(Random.value, Random.value, Random.value);
    }
}
