using UnityEngine;

public class Interactable : MonoBehaviour {
    private bool isPlayerInRange = false;

    void Update() {
        if (isPlayerInRange && Input.GetKeyDown(KeyCode.E)) {
            Interact();
        }
    }

    private void OnTriggerEnter(Collider other) {
        if (other.CompareTag("Player")) {
            isPlayerInRange = true;
        }
    }

    private void OnTriggerExit(Collider other) {
        if (other.CompareTag("Player")) {
            isPlayerInRange = false;
        }
    }

    public void Interact() {
        Debug.Log("Interacting");
        
    }
}