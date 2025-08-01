using StarterAssets;
using System.Collections.Generic;
using UnityEngine;

public class GM : MonoBehaviour
{
    public float recordInterval = 0.05f;
    public int maxframes = 2000;
    private float timer = 0f;
    public Transform playerTransform;
    [SerializeField]private Dataframe[] buffer;
    [SerializeField] private Dataframe[,] buffer1;
    private int writeIndex=0;
    public int columnIndex=-1;
    private int size=0;
    [SerializeField]private ThirdPersonController tpc;
    private bool recording = false;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        tpc = playerTransform.GetComponent<ThirdPersonController>();
        buffer = new Dataframe[maxframes];
        buffer1 = new Dataframe[5,maxframes];
    }

    // Update is called once per frame
    void Update() {
        if (Input.GetKeyDown(KeyCode.G)) {
            columnIndex++;
            writeIndex = 0;
            size = 0;
            recording = true;
        }

        if (!recording||columnIndex>=5) return;
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
            jumped = Input.GetKeyDown(KeyCode.Space),
            animationBlend = tpc.GetAnimationBlend(),
            inputMagnitude = tpc.GetInputMagnitude()
        };
        
        //Debug.Log("Buffer Recording");
        buffer[writeIndex] = data;
        buffer1[columnIndex,writeIndex] = data;
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
            result.Add(buffer1[columnIndex,index]);
        }
        return result;
    }


}
