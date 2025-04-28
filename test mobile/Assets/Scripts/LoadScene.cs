using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadScene : MonoBehaviour
{
    [SerializeField] private GameObject _panelLoading;
    [SerializeField] private GameObject _logo;
    [SerializeField] private Animator _animator;
    private bool _loading;

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

    public IEnumerator SceneLoadingOn(string sceneName)
    {
        _panelLoading.SetActive(true);
        yield return new WaitForSeconds(1);
        _logo.SetActive(true);
        _animator.SetBool("Activate", true);
        yield return new WaitForSeconds(0.1f);

        SceneManager.LoadSceneAsync(sceneName);
        _loading = true;
        StartCoroutine(SceneLoadingOff());
    }

    public IEnumerator SceneLoadingOff()
    {
        if (!_loading) yield return null;

        yield return new WaitForSeconds(4.9f);
        _logo.SetActive(false);
        _animator.SetBool("Activate", false);
        yield return new WaitForSeconds(1);
        _panelLoading.SetActive(false);
    }
}
