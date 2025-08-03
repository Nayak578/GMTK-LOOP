using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public GameObject ghostPrefab;
    public GM GM;
    public Vector3 respawn;
    void Start() {
    }
    private void Update() {
        if (Input.GetKeyDown(KeyCode.K)) {
            OnDeath();
        }
    }
    public void OnDeath() {
        Debug.Log("PlayerDeath");
        ResetEnemy();
        if(GM.recording)GM.stopRecording();
        // Reset or disable player
        if (GM.columnIndex < 5) {
            GetComponent<CharacterController>().enabled = false;
            transform.position =respawn;
            GetComponent<CharacterController>().enabled = true;

        } else
            gameObject.SetActive(false);
    }
    private void ResetEnemy() {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");

        foreach (GameObject enemy in enemies) {
            EnemyAI ai = enemy.GetComponent<EnemyAI>();
            if (ai != null) ai.ResetAI();
        }
        GameObject[] snipers = GameObject.FindGameObjectsWithTag("Enemy2");
        foreach (GameObject sniper in snipers) {
            EnemyAISniper sniperAI = sniper.GetComponent<EnemyAISniper>();
            if (sniperAI != null)
                sniperAI.ResetAI();
        }
    }



}
