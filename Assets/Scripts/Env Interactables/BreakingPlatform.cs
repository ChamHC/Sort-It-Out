using System.Collections;
using UnityEngine;

public class BreakingPlatform : MonoBehaviour
{
    private Renderer platformRenderer;
    private Collider platformCollider;
    private bool isBreaking = false;

    void Start()
    {
        platformRenderer = GetComponent<Renderer>();
        platformCollider = GetComponent<Collider>();
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player") && !isBreaking)
        {
            StartCoroutine(BreakAndResetPlatform());
        }
    }

    IEnumerator BreakAndResetPlatform()
    {
        isBreaking = true;

        yield return new WaitForSeconds(3f);
        MeshRenderer[] childRenderers = transform.parent.gameObject.GetComponentsInChildren<MeshRenderer>();
        foreach (MeshRenderer childRenderer in childRenderers)
        {
            childRenderer.enabled = false;
        }
        transform.parent.gameObject.GetComponentInChildren<MeshCollider>().enabled = false;

        
        yield return new WaitForSeconds(3f);
        foreach (MeshRenderer childRenderer in childRenderers)
        {
            childRenderer.enabled = true;
        }
        transform.parent.gameObject.GetComponentInChildren<MeshCollider>().enabled = true;

        isBreaking = false;
    }
}


