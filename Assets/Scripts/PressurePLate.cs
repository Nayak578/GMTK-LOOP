using UnityEngine;
using System.Collections.Generic;

public class PressurePLate : MonoBehaviour {
    private Renderer rend;
    private HashSet<Collider> activeColliders = new HashSet<Collider>();

    void Start() {
        rend = GetComponent<Renderer>();
    }

    void Update() {
        // Clean up any destroyed objects
        activeColliders.RemoveWhere(c => c == null|| !IsColliderStillInside(c));

        if (activeColliders.Count == 0) {
            SetInactive();
        }
    }

    private void OnTriggerEnter(Collider other) {
        activeColliders.Add(other);
        SetActive();
        MoveObstacle();
    }

    private void OnTriggerExit(Collider other) {
        activeColliders.Remove(other);
        if (activeColliders.Count == 0) {
            SetInactive();
            BlockObstacle();
        }
    }
    bool IsColliderStillInside(Collider col) {
        // Checks if collider is still inside this trigger's bounds
        if (!col.gameObject.activeInHierarchy) return false;

        return GetComponent<Collider>().bounds.Intersects(col.bounds);
    }
    void SetActive() {
        rend.material.color = Color.green;
        rend.material.EnableKeyword("_EMISSION");
        rend.material.SetColor("_EmissionColor", Color.green * 7f);
    }

    void SetInactive() {
        rend.material.color = Color.red;
        rend.material.SetColor("_EmissionColor", Color.red*7f);
    }

    void MoveObstacle() {
        Debug.Log("Move Obstacle");
    }

    void BlockObstacle() {
        Debug.Log("BlockWithObstacle");
    }
}
