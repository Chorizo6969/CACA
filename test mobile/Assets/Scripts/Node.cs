using UnityEngine;
using System;
using System.Collections.Generic;
using UnityEngine.UI;
using static NodeTypes;

public class Node : MonoBehaviour
{
    public static event Action OnMapCompleted;
    public int Position;
    public int Hauteur;
    public Node Creator;
    public NodesEventTypes EventName;
    public bool OnYReviendra;
    [SerializeField] private Image _componentImageRef;
    [SerializeField] private List<Sprite> _listOfSprite = new();

    public static void TriggerMapCompleted()
    {
        OnMapCompleted?.Invoke();
    }

    public void InteractPlayer()
    {
        PlayerMap.Instance.SetupTarget(gameObject.transform.position);
    }

    private void OnEnable()
    {
        OnMapCompleted += SetupSprite;
    }

    private void OnDisable()
    {
        OnMapCompleted -= SetupSprite;
    }

    /// <summary>
    /// Fonction qui peut setup un sprite pour le node selon son rôle
    /// </summary>
    private void SetupSprite()
    {
        switch (EventName)
        {
            case NodesEventTypes.Cuisine:
                _componentImageRef.sprite = _listOfSprite[0];
                break;
            case NodesEventTypes.Combat:
                _componentImageRef.sprite = _listOfSprite[1];
                break;
            case NodesEventTypes.Ingredient:
                _componentImageRef.sprite = _listOfSprite[2];
                break;
            case NodesEventTypes.Heal:
                _componentImageRef.sprite = _listOfSprite[3];
                break;
            case NodesEventTypes.Boss:
                _componentImageRef.sprite = _listOfSprite[4];
                break;
            case NodesEventTypes.Start:
                _componentImageRef.sprite = _listOfSprite[5];
                break;
        }
    }
}
