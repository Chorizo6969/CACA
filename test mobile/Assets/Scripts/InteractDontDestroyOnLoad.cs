using UnityEngine;

public class InteractDontDestroyOnLoad : MonoBehaviour
{
    public void ChangeSaveID(int id)
    {
        SaveManager.Instance.SaveID = (byte)id;
    }

    public void SaveFunction(int function)
    {
        switch (function)
        {
            case 0:
                SaveManager.Instance.SaveMap();
                break;
            case 1:
                SaveManager.Instance.LoadMap();
                break;
            case 2:
                SaveManager.Instance.DeleteMap();
                break;
        }
    }

    public void ChangeScene(string sceneName)
    {
        StartCoroutine(LoadScene.Instance.SceneLoadingOn(sceneName));
    }
}
