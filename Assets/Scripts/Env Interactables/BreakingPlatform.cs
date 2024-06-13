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
        platformRenderer.enabled = false;
        platformCollider.enabled = false;

        
        yield return new WaitForSeconds(3f);
        platformRenderer.enabled = true;
        platformCollider.enabled = true;

        isBreaking = false;
    }
}


