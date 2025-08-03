using UnityEngine;
using UnityEngine.UI;

public class stry : MonoBehaviour {
    public Image targetImage; // The UI Image component
    public Sprite[] sprites;  // List of sprites to switch between
    private int currentIndex = 0;

    // Call this method on button click
    void Update() {
        if (Input.GetKeyDown(KeyCode.N)) {
            currentIndex++;
            if (currentIndex > 2) {
                gameObject.SetActive(false);
                return;
            }
            if (sprites.Length == 0 || targetImage == null) return;

            // Loop through sprites
            targetImage.sprite = sprites[currentIndex];
            
            
        }
    }
}
