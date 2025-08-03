using UnityEngine;

public class ChangePos : MonoBehaviour
{
    public PlayerMovement player;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    private void OnTriggerEnter(Collider other) {
        player.respawn = new Vector3(-3.05671954f, 0.0149998069f, -3.4345057f);
    }
}
