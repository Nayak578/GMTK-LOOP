using System.Collections.Generic;
using UnityEngine;

public class Ghost : MonoBehaviour
{
    public List<Dataframe> replayData;
    private int currentFrame = 0;
    public float replayInterval = 0.05f;

    private float timer = 0f;

    void Update() {
        if (replayData == null || currentFrame >= replayData.Count) return;

        timer += Time.deltaTime;
        if (timer >= replayInterval) {
            Dataframe frame = replayData[currentFrame];
            transform.position = frame.location+new Vector3(0,1,0);
            transform.rotation = frame.rotation;
            

            // Optional: animation handling here
            // animator.SetTrigger("Jump") if frame.jumped etc.

            currentFrame++;
            timer = 0f;
        }
    }
}
