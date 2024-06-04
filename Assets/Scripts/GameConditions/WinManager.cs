using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WinManager : MonoBehaviour
{
    [SerializeField] private List<GoalController> goalControllers = new List<GoalController>();
    [SerializeField] private LoseManager loseManager;

    bool runOnce = false;

    private void Update()
    {
        if (CheckWinCondition())
        {
            //if player has not lost
            if (!loseManager.GetLostValue())
            {
                if (!runOnce)
                {
                    runOnce = true;
                    Debug.Log("Win!");
                }

            }
        }
    }

    private bool CheckWinCondition()
    {
        foreach (var goalController in goalControllers)
        {
            if (!goalController.IsFulfilled)
            {
                return false;
            }
        }
        return true;
    }
}
