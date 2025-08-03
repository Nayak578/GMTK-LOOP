using StarterAssets;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GM : MonoBehaviour {
    public GameObject ghostPrefab;
    public float recordInterval = 0.05f;
    public int maxframes = 500;
    private float timer = 0f;
    public Transform playerTransform;
    [SerializeField] private Dataframe[] buffer;
    [SerializeField] private Dataframe[,] buffer1;
    private int writeIndex = 0;
    public int columnIndex = -1;
    private int size = 0;
    public Ghost[] ghosts = new Ghost[5];
    [SerializeField] private ThirdPersonController tpc;
    public bool recording = false;
    private bool latch = false;
    private bool jumpLatch = false;
    public GameObject[] images = new GameObject[5];
    public GameObject isRecording;
    public GameObject overflow;
    private bool[] spawned = new bool[5]; // Track if each ghost has been spawned
    private bool[] recorded = new bool[5]; // Track if each ghost has been recorded

    void Start() {
        tpc = playerTransform.GetComponent<ThirdPersonController>();
        buffer = new Dataframe[maxframes];
        buffer1 = new Dataframe[5, maxframes];

        // Lock the cursor
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }


    void Update() {
        if (Input.GetKeyDown(KeyCode.G)) {
            if (Input.GetKeyDown(KeyCode.Backspace)) {
                SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
            }

            if (recording) {
                Debug.LogWarning("Stop the current recording before starting a new one.");
                return;
            }

            if (columnIndex >= 4) {
                Debug.LogWarning("Maximum number of recordings reached.");
                return;
            }

            columnIndex++;
            isRecording.SetActive(true);
            writeIndex = 0;
            size = 0;
            recording = true;
            Debug.Log($"Recording started: {columnIndex + 1}/5");
        }

        if (Input.GetKeyDown(KeyCode.E)) latch = true;
        if (Input.GetKeyDown(KeyCode.Space)) jumpLatch = true;

        if (Input.GetKeyDown(KeyCode.Alpha1)) TrySpawnGhost(0);
        if (Input.GetKeyDown(KeyCode.Alpha2)) TrySpawnGhost(1);
        if (Input.GetKeyDown(KeyCode.Alpha3)) TrySpawnGhost(2);
        if (Input.GetKeyDown(KeyCode.Alpha4)) TrySpawnGhost(3);
        if (Input.GetKeyDown(KeyCode.Alpha5)) TrySpawnGhost(4);

        if (!recording || columnIndex >= 5) return;

        timer += Time.deltaTime;
        if (timer > recordInterval) {
            timer = 0;
            RecordFrame();
            jumpLatch = false;
            latch = false;
        }
    }

    void TrySpawnGhost(int index) {
        if (!recorded[index]) {
            Debug.LogWarning($"Ghost {index + 1} has not been recorded yet.");
            return;
        }

        if (spawned[index]) {
            Debug.LogWarning($"Ghost {index + 1} has already been spawned.");
            return;
        }

        if (ghostPrefab == null) {
            Debug.LogError("Ghost prefab not assigned.");
            return;
        }

        GameObject g = Instantiate(ghostPrefab, buffer1[index, 0].location, Quaternion.identity);

        Ghost ghostComponent = g.GetComponent<Ghost>();
        ghostComponent.replayData = new List<Dataframe>();

        for (int i = 0; i < size; i++) {
            ghostComponent.replayData.Add(buffer1[index, i]);
        }

        ghostComponent.enabled = true;
        spawned[index] = true;

        if (images[index] != null) {
            images[index].SetActive(false);
        }
    }

    void RecordFrame() {
        Vector3 pos = playerTransform.position; // No Y override

        Dataframe data = new Dataframe {
            location = pos,
            rotation = playerTransform.rotation,
            jumped = jumpLatch,
            animationBlend = tpc.GetAnimationBlend(),
            inputMagnitude = tpc.GetInputMagnitude(),
            interacts = latch
        };
        if (data.jumped) Debug.Log("Jumping");
        buffer[writeIndex] = data;
        buffer1[columnIndex, writeIndex] = data;

        if (writeIndex == maxframes - 1) {
            StartCoroutine(ActivateTemporarily());
            writeIndex = 0;
            size = 0;
            return;
        }

        writeIndex = (writeIndex + 1) % maxframes;
        size = Mathf.Min(size + 1, maxframes);
    }

    public void stopRecording() {
        if (columnIndex >= 0 && columnIndex < 5) {
            // Manually record the last frame before stopping
            RecordLastFrame();

            isRecording.SetActive(false);
            if (images[columnIndex] != null)
                images[columnIndex].SetActive(true);
            recording = false;
            recorded[columnIndex] = true;
            Debug.Log($"Recording {columnIndex + 1} stopped.");
        }
    }


    public List<Dataframe> GetReplay() {
        List<Dataframe> result = new List<Dataframe>(size);
        int startIndex = (writeIndex - size + maxframes) % maxframes;

        for (int i = 0; i < size; i++) {
            int index = (startIndex + i) % maxframes;
            result.Add(buffer1[columnIndex, index]);
        }

        return result;
    }

    IEnumerator ActivateTemporarily() {
        overflow.SetActive(true);
        yield return new WaitForSeconds(5);
        overflow.SetActive(false);
    }
    void RecordLastFrame() {
        Vector3 pos = playerTransform.position;

        Dataframe data = new Dataframe {
            location = pos,
            rotation = playerTransform.rotation,
            jumped = Input.GetKey(KeyCode.Space), // optional: recent jump state
            animationBlend = tpc.GetAnimationBlend(),
            inputMagnitude = tpc.GetInputMagnitude(),
            interacts = Input.GetKey(KeyCode.E) // optional: recent interact
        };
        if (data.jumped) Debug.Log("Jumping");
        buffer[writeIndex] = data;
        buffer1[columnIndex, writeIndex] = data;

        writeIndex = (writeIndex + 1) % maxframes;
        size = Mathf.Min(size + 1, maxframes);
    }

}
