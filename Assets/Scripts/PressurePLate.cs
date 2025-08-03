using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PressurePLate : MonoBehaviour {
    private Renderer rend;
    private HashSet<Collider> activeColliders = new HashSet<Collider>();
    [SerializeField] GameObject door;

    private bool isPlateActive = false;
    private Coroutine moveCoroutine;

    public Vector3 moveOffset = new Vector3(0, 3f, 0); // How high the door moves
    public float moveSpeed = 3f; // Units per second

    private Vector3 closePos;
    private Vector3 openPos;

    void Start() {
        rend = GetComponent<Renderer>();
        closePos = door.transform.position;
        openPos = closePos + moveOffset;
    }

    void Update() {
        activeColliders.RemoveWhere(c => c == null || !IsColliderStillInside(c));

        if (activeColliders.Count > 0 && !isPlateActive) {
            SetActive();
            MoveObstacle();
        } else if (activeColliders.Count == 0 && isPlateActive) {
            SetInactive();
            BlockObstacle();
        }
    }

    private void OnTriggerEnter(Collider other) {
        activeColliders.Add(other);
    }

    private void OnTriggerExit(Collider other) {
        activeColliders.Remove(other);
    }

    bool IsColliderStillInside(Collider col) {
        if (!col.gameObject.activeInHierarchy) return false;
        return GetComponent<Collider>().bounds.Intersects(col.bounds);
    }

    void SetActive() {
        rend.material.color = Color.green;
        //rend.material.EnableKeyword("_EMISSION");
        //rend.material.SetColor("_EmissionColor", Color.green * 7f);
        isPlateActive = true;
    }

    void SetInactive() {
        rend.material.color = Color.red;
        //rend.material.SetColor("_EmissionColor", Color.red * 7f);
        isPlateActive = false;
    }

    void MoveObstacle() {
        Debug.Log("Move Obstacle");
        if (moveCoroutine != null) StopCoroutine(moveCoroutine);
        moveCoroutine = StartCoroutine(MoveDoor(openPos));
    }

    void BlockObstacle() {
        Debug.Log("BlockWithObstacle");
        if (moveCoroutine != null) StopCoroutine(moveCoroutine);
        moveCoroutine = StartCoroutine(MoveDoor(closePos));
    }

    IEnumerator MoveDoor(Vector3 targetPosition) {
        while (Vector3.Distance(door.transform.position, targetPosition) > 0.01f) {
            door.transform.position = Vector3.MoveTowards(
                door.transform.position,
                targetPosition,
                moveSpeed * Time.deltaTime
            );
            yield return null;
        }
        door.transform.position = targetPosition;
    }
}
