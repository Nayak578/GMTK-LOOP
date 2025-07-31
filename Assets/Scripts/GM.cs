using System.Collections.Generic;
using UnityEngine;

public class GM : MonoBehaviour
{
    public float recordInterval = 0.05f;
    public int maxframes = 2000;
    private float timer = 0f;
    public Transform playerTransform;
    [SerializeField]private Dataframe[] buffer;
    private int writeIndex=0;
    private int size=0;
    private bool recording = true;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        buffer = new Dataframe[maxframes];
    }

    // Update is called once per frame
    void Update()
    {
        if (!recording) return;
        timer += Time.deltaTime;
        if (timer > recordInterval) {
            //Debug.Log("isRecording");
            timer = 0;
            RecordFrame();
        }
    }
    void RecordFrame() {
        Dataframe data = new Dataframe {
            location = playerTransform.position,
            rotation = playerTransform.rotation,
            jumped = Input.GetKeyDown(KeyCode.Space)
        };
        
        //Debug.Log("Buffer Recording");
        buffer[writeIndex] = data;
        Debug.Log( buffer[writeIndex].location);
        writeIndex =(writeIndex+1)%maxframes;
        size = Mathf.Min(size + 1, maxframes);
    }
    public void stopRecording() {
        recording = false;
    }
    public List<Dataframe> GetReplay() {
        List<Dataframe> result = new List<Dataframe>(size);
        int startIndex = (writeIndex - size + maxframes) % maxframes;

        for (int i = 0; i < size; i++) {
            int index = (startIndex + i) % maxframes;
            result.Add(buffer[index]);
        }
        return result;
    }


}
