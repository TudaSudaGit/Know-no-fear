using UnityEngine;

public class TutorialDoor : MonoBehaviour
{
    public Transform mainLevelStartPoint;

    void OnTriggerStay2D(Collider2D other)
    {
        if (TutorialManager.Instance == null || TutorialManager.Instance.currentStep != TutorialManager.TutorialStep.Finished)
            return;

        if (other.CompareTag("Player") && Input.GetKeyDown(InputManager.InteractKey))
        {
            other.transform.position = mainLevelStartPoint.position;
            if (TutorialManager.Instance.tutorialPanel != null)
                TutorialManager.Instance.tutorialPanel.gameObject.SetActive(false);
            gameObject.SetActive(false);
        }
    }
}