using UnityEngine;
using UnityEngine.SceneManagement; // for scene loading

public class LevelEndTrigger : MonoBehaviour
{
    // Optional: assign in Inspector if you want to show a message
    public string nextSceneName;

    private void OnTriggerEnter(Collider other)
    {
        // Check if the player touched the trigger
        if (other.CompareTag("Player"))
        {
            Debug.Log("Level Completed!");

            // Example 1: Load another scene
            if (!string.IsNullOrEmpty(nextSceneName))
            {
                SceneManager.LoadScene(nextSceneName);
            }

            // Example 2: Call a method to show a victory screen
            // GameManager.Instance.EndLevel();
        }
    }
}