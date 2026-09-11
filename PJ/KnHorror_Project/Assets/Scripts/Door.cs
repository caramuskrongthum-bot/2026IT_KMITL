using StarterAssets;
using UnityEngine;
using System.Collections;

public class Door : MonoBehaviour
{
    public Animator Door_Animator;
    public Transform Player_P_T;

    public float moveDuration = 0.5f;
    public float waitTime = 1f;

    private bool isLoading = false;

    private void OnTriggerStay(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        Player_Basic_Controller playerController =
            other.GetComponent<Player_Basic_Controller>();

        if (playerController == null)
            return;

        if (playerController.PressingFire1)
        {
            StartPushDoor();
        }
    }

    public void StartPushDoor()
    {
        if (isLoading)
            return;

        GameObject Player =
            GameObject.FindGameObjectWithTag("Player");

        if (Player == null)
            return;

        isLoading = true;
        StartCoroutine(TeleportPlayer(Player));
    }

    private IEnumerator TeleportPlayer(GameObject Player)
    {
        ThirdPersonController TPC =
            Player.GetComponent<ThirdPersonController>();

        CharacterController C =
            Player.GetComponent<CharacterController>();

        Animator A =
            Player.GetComponentInChildren<Animator>();

        RoomManager roomManager =
            FindAnyObjectByType<RoomManager>();

        if (roomManager != null)
            roomManager.LoadNextRoom();

        if (A != null)
            A.Play("PushDoor");

        if (TPC != null)
            TPC.CanMove = false;

        if (C != null)
            C.enabled = false;

        Vector3 startPosition =
            Player.transform.position;

        Vector3 targetPosition =
            Player_P_T.position;

        Quaternion startRotation =
            Player.transform.rotation;

        Quaternion targetRotation =
            Player_P_T.rotation;

        float elapsed = 0f;

        while (elapsed < moveDuration)
        {
            elapsed += Time.deltaTime;

            float t =
                Mathf.Clamp01(elapsed / moveDuration);

            t = Mathf.SmoothStep(0f, 1f, t);

            Player.transform.position =
                Vector3.Lerp(
                    startPosition,
                    targetPosition,
                    t
                );

            Player.transform.rotation =
                Quaternion.Slerp(
                    startRotation,
                    targetRotation,
                    t
                );

            yield return null;
        }

        Player.transform.SetPositionAndRotation(
            targetPosition,
            targetRotation
        );

        if (C != null)
            C.enabled = true;

        if (TPC != null)
            TPC.enabled = true;

        if (Door_Animator != null)
            Door_Animator.enabled = true;

        yield return new WaitForSeconds(waitTime);

        if (TPC != null)
            TPC.CanMove = true;

        isLoading = false;
    }
}