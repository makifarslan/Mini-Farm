using UnityEngine;

public class LookAtCamera : MonoBehaviour
{
    void Start()
    {
        transform.rotation = Camera.main.transform.rotation;
    }
}