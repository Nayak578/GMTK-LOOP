using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PressurePLate : MonoBehaviour {
    private Renderer rend;
    private HashSet<Collider> activeColliders = new HashSet<Collider>();
    [SerializeField] GameObject door;
    [SerializeField] Quaternion openRot;
    [SerializeField] Quaternion closeRot;
    private bool isPlateActive = false;
    private Coroutine rotateCoroutine;

    public Vector3 openEulerAngles = new Vector3(0, 90, 0); // Customize as needed
    public float rotationSpeed = 180f;

    void Start() {
        rend = GetComponent<Renderer>();
        closeRot = door.transform.rotation;
        openRot = Quaternion.Euler(openEulerAngles); // set via Inspector or hardcoded
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
        rend.material.EnableKeyword("_EMISSION");
        rend.material.SetColor("_EmissionColor", Color.green * 7f);
        isPlateActive = true;
    }

    void SetInactive() {
        rend.material.color = Color.red;
        rend.material.SetColor("_EmissionColor", Color.red * 7f);
        isPlateActive = false;
    }

    void MoveObstacle() {
        Debug.Log("Move Obstacle");

        if (rotateCoroutine != null) StopCoroutine(rotateCoroutine);
        rotateCoroutine = StartCoroutine(RotateDoor(openRot)); // or closeRot

    }

    void BlockObstacle() {
        Debug.Log("BlockWithObstacle");

        if (rotateCoroutine != null) StopCoroutine(rotateCoroutine);
        rotateCoroutine = StartCoroutine(RotateDoor(closeRot)); // or closeRot

    }


    IEnumerator RotateDoor(Quaternion targetRotation) {
        while (Quaternion.Angle(door.transform.rotation, targetRotation) > 0.1f) {
            door.transform.rotation = Quaternion.RotateTowards(
                door.transform.rotation,
                targetRotation,
                rotationSpeed * Time.deltaTime
            );
            yield return null;
        }
        door.transform.rotation = targetRotation;
    }



}
