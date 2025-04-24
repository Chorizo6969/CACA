using UnityEngine;
using System.IO;
using System.Collections.Generic;
using System;

public class SaveManager : MonoBehaviour
{
    [SerializeField] private bool _encrypt;
    [SerializeField] private string _keyEncrypt;

    public void SaveMap(int id)
    {
        MapWrapper wrapper = new MapWrapper();

        foreach (var kvp in MapMaker2.Instance._dicoNode)
        {
            Node node = kvp.Value;

            SerializableNode snode = new SerializableNode
            {
                key = kvp.Key,
                position = node.Position,
                hauteur = node.Hauteur,
                eventName = node.EventName,
                onYReviendra = node.OnYReviendra
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
            MapMaker2.Instance._dicoNode.Clear();

            foreach (var item in wrapper.items)
            {
                Node node = Instantiate(MapMaker2.Instance._nodePrefab, MapMaker2.Instance.transform);
                node.transform.localPosition = item.key;
                node.Position = item.position;
                node.Hauteur = item.hauteur;
                node.EventName = item.eventName;
                node.OnYReviendra = item.onYReviendra;

                MapMaker2.Instance._dicoNode[item.key] = node;
                node.gameObject.SetActive(true);
            }

            Node.TriggerMapCompleted(); // Redéclenche l'affichage des sprites
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
            result += (char)(json[i] ^ _keyEncrypt[i % _keyEncrypt.Length]);
        }

        return result;
    }

}
