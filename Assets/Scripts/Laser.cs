using System.Collections;
using UnityEngine;

public class Laser : MonoBehaviour
{
    public float disableDuration = 2f;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Renderer rend = GetComponent<Renderer>();
        if (rend != null) {
            rend.material.EnableKeyword("_EMISSION");
            rend.material.color = Color.red;
            rend.material.SetColor("_EmissionColor", Color.red * 7f);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    private void OnTriggerEnter(Collider other) {
        if (other.CompareTag("Player")) {
            Debug.Log("PlayerCollided");
            //StartCoroutine(DisableTemporarily());
            other.GetComponent<PlayerMovement>().OnDeath();
        }
        if (other.CompareTag("Echo")) {
            other.GetComponent<Ghost>().OnDeath();
            StartCoroutine(DisableTemporarily());
        }
    }
    private IEnumerator DisableTemporarily() {
        Collider col = GetComponent<Collider>();
        Renderer rend = GetComponent<Renderer>();
        
        col.enabled = false;
        if (rend != null) {
            rend.material.EnableKeyword("_EMISSION");
            rend.material.color = Color.green;
            rend.material.SetColor("_EmissionColor", Color.green * 7f);
        }
        yield return new WaitForSeconds(disableDuration);

        col.enabled = true;
        if (rend != null) {
            rend.material.EnableKeyword("_EMISSION");
            rend.material.color = Color.red;
            rend.material.SetColor("_EmissionColor", Color.red * 7f);
        }
    }

}
