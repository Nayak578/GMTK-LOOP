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
    private int _animIDJump;
    private int _animIDGrounded;

    private bool hasJumped = false;

    private Dataframe current;
    private Dataframe next;

    private void Start() {
        if (animator) {
            _animIDSpeed = Animator.StringToHash("Speed");
            _animIDMotionSpeed = Animator.StringToHash("MotionSpeed");
            _animIDJump = Animator.StringToHash("Jump");
            _animIDGrounded = Animator.StringToHash("Grounded");
        }

        if (replayData != null && replayData.Count > 1) {
            current = replayData[0];
            next = replayData[1];
        }
    }

    void Update() {
        if (replayData == null || currentFrame >= replayData.Count - 1) {
            if (animator) {
                animator.SetFloat(_animIDMotionSpeed, 0);
            }
            gameObject.SetActive(false);
            return;
        }

        timer += Time.deltaTime;
        float t = Mathf.Clamp01(timer / replayInterval);

        // Smooth position interpolation
        Vector3 lerpedPos = Vector3.Lerp(current.location, next.location, t);

        // Fix stutter: don't interpolate Y during takeoff frame
        if (current.jumped && !hasJumped) {
            lerpedPos.y = current.location.y;
        }

        transform.position = lerpedPos;
        transform.rotation = Quaternion.Slerp(current.rotation, next.rotation, t);

        // Animation blending
        if (animator) {
            float blendedSpeed = Mathf.Lerp(current.animationBlend, next.animationBlend, t);
            float blendedMagnitude = Mathf.Lerp(current.inputMagnitude, next.inputMagnitude, t);
            animator.SetFloat(_animIDSpeed, blendedSpeed, 0.1f, Time.deltaTime);
            animator.SetFloat(_animIDMotionSpeed, blendedMagnitude, 0.1f, Time.deltaTime);
            
            if (current.jumped && !hasJumped) {
                animator.SetTrigger(_animIDJump);
                animator.SetBool(_animIDGrounded, false);
                hasJumped = true;
            }
        }

        if (t >= 1f) {
            currentFrame++;
            timer = 0f;

            if (animator) {
                animator.ResetTrigger(_animIDJump); // Reset here
                animator.SetBool(_animIDGrounded, true);
            }

            hasJumped = false;

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
        Destroy(gameObject);
    }
}
