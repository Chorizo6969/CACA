using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadScene : MonoBehaviour
{
    #region Singleton
    public static LoadScene Instance;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(this.gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(this.gameObject);
    }
    #endregion

    public void Scene(string sceneName)
    {
        StartCoroutine(SceneLoading(sceneName));
    }

    IEnumerator SceneLoading(string sceneName)
    {
        AsyncOperation chargement = SceneManager.LoadSceneAsync(sceneName);

        while (!chargement.isDone)
        {
            float progression = Mathf.Clamp01(chargement.progress / 0.9f);
            Debug.Log("Progression : " + (progression * 100f) + "%");
            yield return null;
        }
    }
}
