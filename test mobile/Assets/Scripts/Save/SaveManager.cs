using UnityEngine;
using System.IO;
using System.Collections.Generic;

/// <summary>
/// Script qui gère la sauvegarde la map
/// </summary>
public class SaveManager : MonoBehaviour
{
    [Tooltip("Sauvegarde crypter ou non")] public bool Encrypt;
    [Tooltip("Numéro de fichier de sauvegarde")] public byte SaveID;
    const string ENCRYPT_KEY = "Tr0mp1ne7te";

    #region Singleton
    public static SaveManager Instance;

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

    public void SaveMap()
    {
        MapWrapper wrapper = new();

        foreach (var kvp in MapMaker2.Instance._dicoNode)
        {
            Node node = kvp.Value;

            SerializableNode snode = new()
            {
                key = kvp.Key,
                position = node.Position,
                hauteur = node.Hauteur,
                eventName = node.EventName,
                onYReviendra = node.OnYReviendra,
                playerPosition = Vector3Int.RoundToInt(PlayerMap.Instance.transform.localPosition),

                // Sauvegarde la clé du créateur (ou Vector3Int.zero si null)
                creatorKey = node.Creator != null ? MapBuildingTools.Instance.GetKeyFromNode(node.Creator) : Vector3Int.zero
            };

            wrapper.items.Add(snode);
        }

        string json = JsonUtility.ToJson(wrapper, true);
        string path = Application.persistentDataPath + $"/MapSave{SaveID}.json";

        if (Encrypt)
        {
            string encryptedJson = EncryptDecrypt(json);
            File.WriteAllText(path, encryptedJson);
        }
        else
        {
            File.WriteAllText(path, json);
        }
    }

    public void LoadMap()
    {
        string path = Application.persistentDataPath + $"/MapSave{SaveID}.json";

        if (File.Exists(path))
        {
            string json = File.ReadAllText(path);
            if (Encrypt)
            {
                json = EncryptDecrypt(json);
            }

            MapWrapper wrapper = JsonUtility.FromJson<MapWrapper>(json);
            MapMaker2.Instance._dicoNode.Clear();

            Dictionary<Vector3Int, Node> tempDico = new();

            foreach (var item in wrapper.items)
            {
                Node node = Instantiate(MapMaker2.Instance._nodePrefab, MapMaker2.Instance.transform);
                node.transform.localPosition = item.key;
                node.Position = item.position;
                node.Hauteur = item.hauteur;
                node.EventName = item.eventName;
                node.OnYReviendra = item.onYReviendra;

                tempDico[item.key] = node;
                node.gameObject.SetActive(true);
            }

            // Relie les créateurs une fois que tous les nodes sont instanciés
            foreach (var item in wrapper.items)
            {
                if (tempDico.ContainsKey(item.key))
                {
                    Node node = tempDico[item.key];
                    if (item.creatorKey != Vector3Int.zero && tempDico.ContainsKey(item.creatorKey))
                    {
                        node.Creator = tempDico[item.creatorKey];
                    }
                }
            }

            MapMaker2.Instance._dicoNode = tempDico;
            Node.TriggerMapCompleted(); // Redéclenche l'affichage des sprites

            // Redessiner les traits entre les nodes
            if (MapBuildingTools.Instance != null)
            {
                MapBuildingTools.Instance.FirstTimeDraw = true;
                foreach (var node in tempDico.Values)
                {
                    if (node.Creator != null)
                    {
                        MapBuildingTools.Instance.TraceTonTrait(node.Creator, node);
                    }
                }
            }
        }
        else
        {
            SaveMap();
        }
    }

    public void DeleteMap()
    {
        string path = Application.persistentDataPath + $"/MapSave{SaveID}.json";
        if (File.Exists(path))
        {
            File.Delete(path);
        }
    }

    public string EncryptDecrypt(string json)
    {
        string result = "";

        for (int i = 0; i < json.Length; i++)
        {
            result += (char)(json[i] ^ ENCRYPT_KEY[i % ENCRYPT_KEY.Length]);
        }

        return result;
    }
}
