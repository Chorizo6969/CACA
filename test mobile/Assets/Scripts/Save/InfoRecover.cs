using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;

public class InfoRecover : MonoBehaviour
{
    [SerializeField] private TMP_Text m_Text;
    public int _saveLoad;

    void Start()
    {
        LoadEsential();
    }

    public void LoadEsential()
    {
        byte id = (byte)_saveLoad;
        string info = "";

        string path = Application.persistentDataPath + $"/MapSave{id}.json";
        if (File.Exists(path))
        {
            string json = File.ReadAllText(path);
            if (SaveManager.Instance.Encrypt)
            {
                json = SaveManager.Instance.EncryptDecrypt(json);
            }

            MapWrapper wrapper = JsonUtility.FromJson<MapWrapper>(json);

            Dictionary<Vector3Int, Node> tempDico = new();

            foreach (var item in wrapper.items)
            {
                if (Vector3Int.Distance(item.key, item.playerPosition) <= 1f)
                {
                    info = $"Save {id}\nNode : {item.position}";
                    Debug.Log(info);
                    break;
                }
            }
        }
        else
        {
            info = $"Save {id}\n(Empty)";
            Debug.Log(info);
        }

        m_Text.text = info;
    }
}
