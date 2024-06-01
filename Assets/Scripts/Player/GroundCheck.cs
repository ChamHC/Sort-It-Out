using System;
using UnityEngine;

public class CheckGround : MonoBehaviour
{
    private enum Foot{
        Left,
        Right
    }
    [SerializeField] private Foot foot;
    private PlayerStateController player;

    private void Start() {
        player = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerStateController>();
    }

    private void OnTriggerStay(Collider other) {
        if (foot == Foot.Left) player.isLeftFootGrounded = true;
        else player.isRightFootGrounded = true;
    }
    private void OnTriggerExit(Collider other) {
        if (foot == Foot.Left) player.isLeftFootGrounded = false;
        else player.isRightFootGrounded = false;
    }
}
