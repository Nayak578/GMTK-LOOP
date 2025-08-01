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
        List<Dataframe> replayData = GM.GetReplay();
        GameObject ghost = Instantiate(ghostPrefab);
        ghost.transform.position = replayData[0].location; // Start where the player spawned
        ghost.GetComponent<Ghost>().replayData = replayData;
        if (GM.columnIndex < 4) transform.position = new Vector3(0, 0, 0);
        else gameObject.SetActive(false);
    }

}
