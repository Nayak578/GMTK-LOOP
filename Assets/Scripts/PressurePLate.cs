using UnityEngine;

public class PressurePLate : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    Renderer rend;
    void Start()
    {
        rend = GetComponent<Renderer>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    
    private void OnTriggerEnter(Collider other) {
        rend.material.color = Color.green;
        MoveObstacle();
        rend.material.EnableKeyword("_EMISSION");
        rend.material.SetColor("_EmissionColor", Color.green *7f);
    }
    private void OnTriggerExit(Collider other) {
        BlockObstacle();
        rend.material.color = Color.red;
        rend.material.SetColor("_EmissionColor", Color.red * 7f);

    }
    void MoveObstacle() {
        Debug.Log("Move Obstacle");
        //use lerp to show moving in update set current and final pos
        //set final and current pos here
    }
    void BlockObstacle() {
        Debug.Log("BlockWithObstacle");

    }
}
