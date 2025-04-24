using UnityEngine;
using System.IO;
using System.Collections.Generic;

public class SaveManager : MonoBehaviour
{
    [SerializeField] private bool _encrypt;
    [SerializeField] private string _encryptKey;

    public void SaveMap(int id)
    {
        MapWrapper wrapper = new MapWrapper();

        foreach (var kvp in MapMakerTest352.Instance._dicoNode)
        {
            Node node = kvp.Value;

            SerializableNode snode = new SerializableNode
            {
                key = kvp.Key,
                position = node.Position,
                hauteur = node.Hauteur,
                eventName = node.EventName,
                onYReviendra = node.OnYReviendra,
                
                // Sauvegarde la clé du créateur (ou Vector3Int.zero si null)
                creatorKey = node.Creator != null ? MapMakerTest352.Instance.GetKeyFromNode(node.Creator) : Vector3Int.zero
            };

            wrapper.items.Add(snode);
        }

        string json = JsonUtility.ToJson(wrapper, true);
        string path = Application.persistentDataPath + $"/MapSave{id}.json";
        if (_encrypt)
        {
            string encryptedJson = EncryptDecrypt(json);
            File.WriteAllText(path, encryptedJson);
        }
        else
        {
            File.WriteAllText(path, json);
        }

        Debug.Log("Carte sauvegardée à : " + path);
    }

    public void LoadMap(int id)
    {
        string path = Application.persistentDataPath + $"/MapSave{id}.json";
        if (File.Exists(path))
        {
            string json = File.ReadAllText(path);
            if (_encrypt)
            {
                json = EncryptDecrypt(json);
            }

            MapWrapper wrapper = JsonUtility.FromJson<MapWrapper>(json);
            MapMakerTest352.Instance._dicoNode.Clear();

            Dictionary<Vector3Int, Node> tempDico = new();

            foreach (var item in wrapper.items)
            {
                Node node = Instantiate(MapMakerTest352.Instance._nodePrefab, MapMakerTest352.Instance.transform);
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

            MapMakerTest352.Instance._dicoNode = tempDico;
            Node.TriggerMapCompleted(); // Redéclenche l'affichage des sprites

            // Redessiner les traits entre les nodes
            if (DrawLineMap.Instance != null)
            {
                DrawLineMap.Instance.FirstTimeDraw = true;
                foreach (var node in tempDico.Values)
                {
                    if (node.Creator != null)
                    {
                        DrawLineMap.Instance.TraceTonTrait(node.Creator, node);
                    }
                }
            }
        }
        else
        {
            Debug.LogWarning("Fichier introuvable. Génération d’une nouvelle carte !");
            SaveMap(id);
        }
    }

    public void DeleteMap(int id)
    {
        string path = Application.persistentDataPath + $"/MapSave{id}.json";
        if (File.Exists(path))
        {
            File.Delete(path);
            Debug.Log("Carte supprim?e ? : " + path);
        }
    }

    private string EncryptDecrypt(string json)
    {
        string result = "";

        for (int i = 0; i < json.Length; i++)
        {
            result += (char)(json[i] ^ _encryptKey[i % _encryptKey.Length]);
        }

        return result;
    }
}
