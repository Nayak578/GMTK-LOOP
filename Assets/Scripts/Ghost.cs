using System.Collections.Generic;
using UnityEngine;

public class Ghost : MonoBehaviour
{
    public List<Dataframe> replayData;
    private int currentFrame = 0;
    public float replayInterval = 0.05f;

    private float timer = 0f;
    [SerializeField]Animator animator;
    private int _animIDSpeed;
    private int _animIDMotionSpeed;
    private void Start() {
        if (animator) {
            _animIDSpeed = Animator.StringToHash("Speed");
            _animIDMotionSpeed = Animator.StringToHash("MotionSpeed");
        }
    }
    void Update() {
        if (replayData == null || currentFrame >= replayData.Count) {
            if (animator) {
               animator.SetFloat(_animIDMotionSpeed, 0);
            }
            return;
        }
        
        timer += Time.deltaTime;
        if (timer >= replayInterval) {
            Dataframe frame = replayData[currentFrame];
            transform.position = frame.location;
            transform.rotation = frame.rotation;

            if (animator) {
                animator.SetFloat(_animIDSpeed, frame.animationBlend, 0.1f, Time.deltaTime);
                animator.SetFloat(_animIDMotionSpeed, frame.inputMagnitude, 0.1f, Time.deltaTime);
            }
            if (frame.interacts) {
                InteractWithNearbyObject();
            }
            // Optional: animation handling here
            // animator.SetTrigger("Jump") if frame.jumped etc.

            currentFrame++;
            timer = 0f;
        }
    }
    void InteractWithNearbyObject() {
        Collider[] hits = Physics.OverlapSphere(transform.position, 5f); // radius of interaction
        foreach (var hit in hits) {
            Interactable interactable = hit.GetComponent<Interactable>();
            if (interactable != null) {
                interactable.Interact();
                break;
            }
        }
    }

}
