using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public GameObject ghostPrefab;
    public GM GM;

    void Start() {
    }
    private void Update() {
        if (Input.GetKeyDown(KeyCode.K)) {
            OnDeath();
        }
    }
    public void OnDeath() {
        GM.stopRecording();
        // Reset or disable player
        if (GM.columnIndex < 4)
            transform.position = Vector3.zero;
        else
            gameObject.SetActive(false);
    }


}
