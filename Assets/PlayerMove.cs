using UnityEngine;

public class PlayerMove : MonoBehaviour
{
    public float speed = 5f;

    void Update()
    {
        // Get keyboard input
        float moveX = Input.GetAxis("Horizontal"); // A/D or Left/Right keys
        float moveZ = Input.GetAxis("Vertical");   // W/S or Up/Down keys

        // Move the player
        // Vector3 move = Vector3.right * moveX + Vector3.forward * moveZ;
        // transform.position += move * speed * Time.deltaTime;
        Vector3 moveDirection = new Vector3(moveX, 0, moveZ);

        // Move the player position
        transform.position += moveDirection * speed * Time.deltaTime;
    }
}
