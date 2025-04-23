using System.Collections.Generic;
using UnityEngine;
using static NodeTypes;

/// <summary>
/// Script qui construit la carte du jeu
/// </summary>
[RequireComponent(typeof(MapAttributeEvent))]
public class MapMaker2 : MonoBehaviour
{
    #region Variables
    public static MapMaker2 Instance;

    [Header("Map Adjusting")]
    [SerializeField] [Tooltip("Le nombre de node minimum entre le Node de départ et le boss")] private int _mapRange;
    [SerializeField] [Tooltip("Distance à laquelle le node va spawn sur l'axe X")] private int _distanceSpawnX = 200;
    [SerializeField] [Tooltip("Distance à laquelle le node va spawn sur l'axe Y")] private int _distanceSpawnY = 0;
    [SerializeField][Tooltip("Position en X à laquelle le 1er Node spawn (Le mieux : -1045)")] private int _firstNodePosition = -1045;

    [Header("Probality")]
    [SerializeField] [Tooltip("Probabiltité à chaque node d'avoir une intersection (0 = impossible)")] [Range(0,10)] private int _probaIntersection = 3;

    [Header("Other ne pas toucher sauf code")]
    [SerializeField] private Node _nodePrefab;
    [SerializeField] private Node _parentNode;

    public Node _currentNode { get; private set; }

    private Queue<Node> _nodeList = new();
    private List<Node> _intersection = new(); //Liste des nodes qui vont devoir continuer à crée un chemin à partir d'eux
    public Dictionary<Vector3Int, Node> _dicoNode = new(); //ToDo :Faire en sorte qu'il soit privé sauf pour la save.
    private int _currentHeight = 3;
    #endregion

    private void Awake()
    {
        Instance = this;
        for (int i = 0; i <= _mapRange * 4; i++) //Création de plein de node que on placera plus tard
        {
            Node NewNode = Instantiate(_nodePrefab, gameObject.transform);
            NewNode.transform.localPosition = _parentNode.transform.localPosition;
            _nodeList.Enqueue(NewNode);
        }
    }

    void Start()
    {
        Vector3Int startPos = new Vector3Int(_firstNodePosition, 0, 0); // Enregistre le node de départ
        _parentNode.transform.localPosition = startPos;
        _dicoNode.Add(startPos, _parentNode);

        MapMaking(1);  //Je crée la map

        foreach (Node node in _intersection)
        {
            _currentHeight = node.Hauteur;
            _parentNode = node;
            _probaIntersection = 0;
            MapMaking(node.Position);
        }
        Node.TriggerMapCompleted();
    }

    public void MapMaking(int StartPosition)
    {
        for (int i = StartPosition; i <= _mapRange; i++)
        {
            _currentNode = _nodeList.Dequeue();
            _currentNode.gameObject.SetActive(true);
            if (_parentNode.Position >= _mapRange - 2) //zone faut revenir au Boss
            {
                if (_currentHeight != 3)
                {
                    if (_currentHeight > 3)
                    {
                        CreateBranch(i, false);
                    }
                    else
                    {
                        CreateBranch(i, true);
                    }
                }
                else { ToutDroit(i); }
            }
            else if (Intersection())
            {
                CreateBranch(i);
            }
            else { ToutDroit(i); }

            #region AttributeEvent
            if (_currentNode.Position == 1)
            {
                _currentNode.EventName = NodesEventTypes.Cuisine;
                MapAttributeEvent.Instance.SetCuisineProbaToNull();
            }
            else if (_currentNode.Position + 1 == _mapRange)
            {
                MapAttributeEvent.Instance.MapMakingEventBeforeBoss();
            }
            else if (_currentNode.Position == _mapRange)
            {
                _currentNode.EventName = NodesEventTypes.Boss;
            }
            else
            {
                MapAttributeEvent.Instance.MapMakingEvent();
            }
            #endregion

            _parentNode = _currentNode;
        }
    }

    public void CreateBranch(int tourboucle)
    {
        if (_currentHeight + 1 <= 5 && _currentHeight - 1 >= 1) //Si on peut monter et descendre
        {
            _distanceSpawnY = 140;
            _currentHeight++;
            _currentNode.OnYReviendra = true;
            _intersection.Add(_currentNode);
            ToutDroit(tourboucle);
            _currentNode = _nodeList.Dequeue();
            _currentNode.gameObject.SetActive(true);
            _distanceSpawnY = -140;
            _currentHeight -= 2;
            ToutDroit(tourboucle);
        }
        else if (_currentHeight + 1 <= 5) //Si on peut monter
        {
            _distanceSpawnY = 140;
            _currentHeight++;
            _currentNode.OnYReviendra = true;
            _intersection.Add(_currentNode);
            ToutDroit(tourboucle);
        }
        else if (_currentHeight - 1 >= 1) //Si on peut descendre
        {
            _distanceSpawnY = -140;
            _currentHeight -= 2;
            ToutDroit(tourboucle);
        }
        _distanceSpawnY = 0;
    }

    public void ToutDroit(int tourboucle)
    {
        //On le place et on arrondit pour le dicooo
        Vector3Int newPosition = new Vector3Int((_distanceSpawnX * tourboucle) + Mathf.RoundToInt(_currentNode.transform.localPosition.x), Mathf.RoundToInt(_parentNode.transform.localPosition.y) + _distanceSpawnY, Mathf.RoundToInt(_currentNode.transform.localPosition.z));
        _currentNode.transform.localPosition = newPosition;

        if (_dicoNode.ContainsKey(newPosition))
        {
            Node existingValue = _dicoNode[newPosition];
            print("Un node est déja présent ici" + existingValue);
            Destroy(_currentNode.gameObject);
            DrawLineMap.Instance.TraceTonTrait(_parentNode, existingValue, true);
        }
        else
        {
            _dicoNode.Add(newPosition, _currentNode);
            DrawLineMap.Instance.TraceTonTrait(_parentNode, _currentNode, true);
            _currentNode.Creator = _parentNode;
            _currentNode.Hauteur = _currentHeight;
            _currentNode.Position = tourboucle;
        }
    }

    public void CreateBranch(int tourboucle, bool Up)
    {
        if (Up) //Si on peut monter
        {
            _distanceSpawnY = 140;
            _currentHeight++;
            ToutDroit(tourboucle);
        }
        else //Si on peut descendre
        {
            _distanceSpawnY = -140;
            _currentHeight -= 2;
            ToutDroit(tourboucle);
        }
        _distanceSpawnY = 0;
    }

    /// <summary>
    /// Retourne un bool si la proba de l'intersection est passé ou non
    /// </summary>
    /// <returns>Bool de la propa de crée une intersection</returns>
    public bool Intersection()
    {
        int result = Random.Range(1, 11);
        if (result <= _probaIntersection) //Intersection si true
        {
            return true;
        }
        return false;
    }
}