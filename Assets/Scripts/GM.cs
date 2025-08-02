using StarterAssets;
using System.Collections.Generic;
using UnityEngine;

public class GM : MonoBehaviour
{
    public GameObject ghostPrefab;
    public float recordInterval = 0.05f;
    public int maxframes = 2000;
    private float timer = 0f;
    public Transform playerTransform;
    [SerializeField]private Dataframe[] buffer;
    [SerializeField] private Dataframe[,] buffer1;
    private int writeIndex=0;
    public int columnIndex=-1;
    private int size=0;
    public Ghost[] ghosts=new Ghost[5];
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
        if (Input.GetKeyDown(KeyCode.Alpha1)) SpawnGhost(0);
        if (Input.GetKeyDown(KeyCode.Alpha2)) SpawnGhost(1);
        if (Input.GetKeyDown(KeyCode.Alpha3)) SpawnGhost(2);
        if (Input.GetKeyDown(KeyCode.Alpha4)) SpawnGhost(3);
        if (Input.GetKeyDown(KeyCode.Alpha5)) SpawnGhost(4);

        if (!recording||columnIndex>=5) return;
        timer += Time.deltaTime;
        if (timer > recordInterval) {
            //Debug.Log("isRecording");
            timer = 0;
            RecordFrame();
        }
    }
    void SpawnGhost(int index) {
        if (index > columnIndex) return;

        GameObject g = Instantiate(ghostPrefab,
            buffer1[index, 0].location,
            Quaternion.identity);

        Ghost ghostComponent = g.GetComponent<Ghost>();
        ghostComponent.replayData = new List<Dataframe>();

        for (int i = 0; i < size; i++) {
            ghostComponent.replayData.Add(buffer1[index, i]);
        }

        ghostComponent.enabled = true;
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
