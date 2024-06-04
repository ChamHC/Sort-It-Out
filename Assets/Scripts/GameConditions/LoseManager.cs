using UnityEngine;
using UnityEngine.UI;

public class LoseManager : MonoBehaviour
{
    [SerializeField] private int startingPoints = 100;
    private int currentPoints;
    private bool hasLost = false;
    [SerializeField] private Text pointsText;

    private float decrementInterval = 1.0f; // Decrement interval in seconds
    private float timeSinceLastDecrement = 0.0f;

    private void Start()
    {
        currentPoints = startingPoints;
    }

    private void Update()
    {
        // Update the time since the last decrement
        timeSinceLastDecrement += Time.deltaTime;

        // If the interval has passed, decrement points
        if (timeSinceLastDecrement >= decrementInterval)
        {
            currentPoints = Mathf.Max(currentPoints - 1, 0);
            timeSinceLastDecrement = 0.0f; // Reset the timer
        }

        if (currentPoints == 0)
        {
            hasLost = true;
            Debug.Log("Lose Bruh!");
            pointsText.text = "Points: 0";
        }
        else
        {
            pointsText.text = $"Points: {currentPoints}";
        }
    }

    public bool GetLostValue()
    {
        return hasLost;
    }
}


