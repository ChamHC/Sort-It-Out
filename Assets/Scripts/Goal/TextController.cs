using TMPro;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Experimental.Rendering.RenderGraphModule;

public class TextController : MonoBehaviour
{
    public float rotationSpeed = 50f;
    private int randomSeed;
    private GameObject parent;
    private GameObject block;
    private GameObject hologram;

    void Start()
    {
        randomSeed = Random.Range(0, 2) == 0 ? -1 : 1;

        parent = transform.parent.gameObject;
        GameObject[] taggedObject = GameObject.FindGameObjectsWithTag(parent.tag);
        foreach (GameObject obj in taggedObject)
        {
            if (obj.layer == LayerMask.NameToLayer("Blocks"))
            {
                block = obj;
            }
        }

        hologram = Instantiate(block, parent.transform.position, parent.transform.rotation, transform);
        hologram.tag = "Untagged";
        hologram.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
        hologram.GetComponent<BoxCollider>().enabled = false;
        hologram.GetComponent<Rigidbody>().isKinematic = true;
        hologram.GetComponent<MeshRenderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;

        GetComponent<TextMeshPro>().color = block.GetComponent<MeshRenderer>().material.color;
        GetComponent<TextMeshPro>().outlineColor = new Color(255, 255, 255);
        GetComponent<TextMeshPro>().outlineWidth = 0.1f;
    }

    void Update()
    {
        transform.Rotate(0f, rotationSpeed * randomSeed * Time.deltaTime, 0f);

        float bounceSpeed = 2f;
        float bounceHeight = 0.2f;

        if (hologram != null)
        {
            hologram.transform.Rotate(0f, rotationSpeed * -randomSeed * 2f * Time.deltaTime, 0f);

            Vector3 newPosition = hologram.transform.position;
            newPosition.y = Mathf.Sin(Time.time * bounceSpeed) * bounceHeight + parent.transform.position.y;
            hologram.transform.position = newPosition;
        }
    }
}
