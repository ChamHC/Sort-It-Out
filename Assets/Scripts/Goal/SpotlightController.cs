using UnityEngine;

public class SpotlightController : MonoBehaviour
{
    public float rotationSpeed = 50f;
    private int randomSeed;

    void Start(){
        randomSeed = Random.Range(0, 2) == 0 ? -1 : 1;
    }

    void Update()
    {
        transform.Rotate(0f, 0f, rotationSpeed * randomSeed * Time.deltaTime);
    }
}
