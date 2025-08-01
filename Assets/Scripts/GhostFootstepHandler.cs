using UnityEngine;

public class GhostFootstepHandler : MonoBehaviour {
    // This will silently absorb the animation event
    public void OnFootstep(AnimationEvent animationEvent) { }
    public void OnLand(AnimationEvent animationEvent) { }
}

