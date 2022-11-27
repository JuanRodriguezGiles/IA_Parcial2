using UnityEngine;

public class CameraMove : MonoBehaviour
{
    public float speed = 5;
    private void LateUpdate()
    {
        float x = Input.GetAxis("Horizontal");
        float y = Input.GetAxis("Vertical");

        Vector3 movement = new Vector3(speed * x, speed * y, 0);
        movement *= Time.deltaTime;

        transform.Translate(movement);

        if (Input.GetKey(KeyCode.Q))
        {
            Vector3 pos = transform.position;
            pos.z -= 0.5f;
            transform.position = pos;
        }
        if (Input.GetKey(KeyCode.E))
        {
            Vector3 pos = transform.position;
            pos.z += 0.5f;
            transform.position = pos;
        }
    }
}