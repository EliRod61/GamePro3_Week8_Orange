using UnityEngine;

public class MouseManager : MonoBehaviour
{
    // Update is called once per frame
    void Update()
    {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
    }
}
