using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public GameObject secretDoor;

    // Update is called once per frame
    void Update()
    {
        if (PlayerController.numberOfOranges >= 24)
        {
            secretDoor.SetActive(false); 
        }
}

    public void LoadScene()
    {
        SceneManager.LoadScene("OrangeRun");
    }
}
