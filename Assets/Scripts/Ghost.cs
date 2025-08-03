using System.Collections.Generic;
using UnityEngine;

public class Ghost : MonoBehaviour {
    public List<Dataframe> replayData;
    private int currentFrame = 0;
    public float replayInterval = 0.05f;

    private float timer = 0f;
    [SerializeField] Animator animator;
    private int _animIDSpeed;
    private int _animIDMotionSpeed;

    private Dataframe current;
    private Dataframe next;
    
    private void Start() {
        if (animator) {
            _animIDSpeed = Animator.StringToHash("Speed");
            _animIDMotionSpeed = Animator.StringToHash("MotionSpeed");
        }

        if (replayData != null && replayData.Count > 1) {
            current = replayData[0];
            next = replayData[1];
        }
    }

    void Update() {
        if (currentFrame >= replayData.Count - 1) {
            gameObject.SetActive(false);
        }
        if (replayData == null || currentFrame >= replayData.Count - 1) {
            if (animator) {
                animator.SetFloat(_animIDMotionSpeed, 0);
            }
            return;
        }

        timer += Time.deltaTime;

        float t = timer / replayInterval;
        t = Mathf.Clamp01(t);

        //Smooth interpolation
        transform.position = Vector3.Lerp(current.location, next.location, t);
        transform.rotation = Quaternion.Slerp(current.rotation, next.rotation, t);

        // Smooth animation blending
        if (animator) {
            float blendedSpeed = Mathf.Lerp(current.animationBlend, next.animationBlend, t);
            float blendedMagnitude = Mathf.Lerp(current.inputMagnitude, next.inputMagnitude, t);
            animator.SetFloat(_animIDSpeed, blendedSpeed, 0.1f, Time.deltaTime);
            animator.SetFloat(_animIDMotionSpeed, blendedMagnitude, 0.1f, Time.deltaTime);
        }

        if (t >= 1f) {
            currentFrame++;
            timer = 0f;

            if (currentFrame < replayData.Count - 1) {
                current = replayData[currentFrame];
                next = replayData[currentFrame + 1];

                if (current.interacts) {
                    InteractWithNearbyObject();
                }
            }
        }
    }

    void InteractWithNearbyObject() {
        Collider[] hits = Physics.OverlapSphere(transform.position, 5f);
        foreach (var hit in hits) {
            Interactable interactable = hit.GetComponent<Interactable>();
            if (interactable != null) {
                interactable.Interact();
                break;
            }
        }
    }
    public void OnDeath() {
        Debug.Log("Echo death");
    }
}
