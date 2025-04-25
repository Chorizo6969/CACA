using UnityEngine;

public class PlayerOnMap : MonoBehaviour
{
    #region Singleton
    public static PlayerOnMap Instance;

    private void Awake()
    {
        Instance = this;
    }
    #endregion
}
