using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Drive : MonoBehaviour
{
    public float speed = 10.0f;
    public float rotationSpeed = 100.0f;

    Vector2 rotation = Vector2.zero;

    void Update()
    {
        // Get the horizontal and vertical axis.
        // By default they are mapped to the arrow keys.
        // The value is in the range -1 to 1
        float translation = Input.GetAxis("Vertical") * speed;
        rotation.y += Input.GetAxis("Mouse X");
        rotation.x -= Input.GetAxis("Mouse Y");
        rotation.x = Mathf.Clamp(rotation.x, -15f, 15f);

        // Make it move 10 meters per second instead of 10 meters per frame...
        translation *= Time.deltaTime;

        // Move translation along the object's z-axis
        transform.Translate(0, 0, translation);

        //transform.eulerAngles = new Vector2(0, rotation.y);
        transform.localRotation = Quaternion.Euler(rotation.x * rotationSpeed, rotation.y * rotationSpeed, 0);

    }
}
