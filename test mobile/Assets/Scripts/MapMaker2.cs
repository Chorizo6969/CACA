using System.Collections.Generic;
using UnityEngine;
using static NodeTypes;

/// <summary>
/// Script qui construit la carte du jeu
/// </summary>
[RequireComponent(typeof(MapAttributeEvent))]
[RequireComponent(typeof(MapBuildingTools))]
public class MapMaker2 : MonoBehaviour
{
    #region Variables
    public static MapMaker2 Instance;

    [Header("Map Adjusting")]
    [SerializeField][Range(4, 15)][Tooltip("Le nombre de node minimum entre le Node de départ et le boss")] private int _mapRange;
    [SerializeField][Tooltip("Distance à laquelle le node va spawn sur l'axe X")] private int _distanceSpawnX = 200;
    [SerializeField][Tooltip("Distance à laquelle le node va spawn sur l'axe Y")] private int _distanceSpawnY = 0;
    [SerializeField][Tooltip("Position en X à laquelle le 1er Node spawn (Le mieux : -1045)")] private int _firstNodePosition = -1045;

    [Header("Probality")]
    [SerializeField][Tooltip("Probabiltité à chaque node d'avoir une intersection (0 = impossible)")][Range(0, 10)] private int _probaIntersection = 3;

    [Header("Other ne pas toucher sauf code")]
    [field: SerializeField] public Node _nodePrefab { get; private set; }
    [SerializeField] private Node _parentNode;
    public Node _currentNode { get; private set; }

    /// <summary>
    /// Queue de node crée au début du jeu (environ 40)
    /// </summary>
    private Queue<Node> _nodeList = new();
    public List<Node> _intersection { get; private set; } = new();  //Liste des nodes qui vont devoir continuer à crée un chemin à partir d'eux
    public Dictionary<Vector3Int, Node> _dicoNode { get; set; } = new(); //ToDo :Faire en sorte qu'il soit privé sauf pour la save.
    private int _currentHeight = 3;
    private Node _existingValue;
    #endregion
    private void Awake()
    {
        Instance = this;
        for (int i = 0; i <= 40; i++) //Création de plein de node que on placera plus tard
        {
            Node NewNode = Instantiate(_nodePrefab, gameObject.transform);
            NewNode.transform.localPosition = _parentNode.transform.localPosition;
            _nodeList.Enqueue(NewNode);
        }
    }

    private void Start()
    {
        MapMaking(1);
        ConstructionSecondaireGraph();
        Node.TriggerMapCompleted(); //Attribution des rôles
    }

    public void MapMaking(int StartPosition)
    {
        Vector3Int startPos = new Vector3Int(_firstNodePosition, 0, 0); // Enregistre le node de départ
        _parentNode.transform.localPosition = startPos;
        _dicoNode.Add(startPos, _parentNode);

        for (int i = StartPosition; i <= _mapRange; i++)
        {
            _currentHeight = _parentNode.Hauteur;
            _currentNode = _nodeList.Dequeue();
            #region BossVerif
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
                else { ToutDroit(i, _parentNode); }
            }
            else if (MapBuildingTools.Instance.Intersection(i, _probaIntersection))
            {
                CreateBranch(i);
            }
            else { ToutDroit(i, _parentNode); }
            #endregion
            #region AttributeEvent
            if (_currentNode.Position + 1 == _mapRange) { MapAttributeEvent.Instance.MapMakingEventBeforeBoss(); }
            else if (_currentNode.Position == _mapRange) { _currentNode.EventName = NodesEventTypes.Boss; }
            else { MapAttributeEvent.Instance.MapMakingEvent(); }
            switch (_currentNode.Position)
            {
                case 1:
                    _currentNode.EventName = NodesEventTypes.Ingredient;
                    break;
                case 2:
                    _currentNode.EventName = NodesEventTypes.Cuisine;
                    MapAttributeEvent.Instance.SetCuisineProbaToNull();
                    break;
                case 3:
                    _currentNode.EventName = NodesEventTypes.Combat;
                    break;
            }
            #endregion

            _parentNode = _currentNode;
        }
    }

    public void CreateBranch(int tourboucle)
    {
        if (_currentHeight + 1 <= 4 && _currentHeight - 1 >= 2) //Si on peut monter et descendre
        {
            _distanceSpawnY = 200;
            _currentNode.OnYReviendra = true;
            _intersection.Add(_currentNode);
            ToutDroit(tourboucle, _parentNode);
            _currentNode.Hauteur = _currentHeight + 1;
            _currentNode = _nodeList.Dequeue();
            _distanceSpawnY = -200;
            ToutDroit(tourboucle, _parentNode);
            _currentNode.Hauteur = _currentHeight - 1;
        }
        else if (_currentHeight + 1 <= 4) //Si on peut monter
        {
            _distanceSpawnY = 200;
            _currentNode.OnYReviendra = true;
            _intersection.Add(_currentNode);
            ToutDroit(tourboucle, _parentNode);
            _currentNode.Hauteur = _currentHeight + 1;
        }
        else if (_currentHeight - 1 >= 2) //Si on peut descendre
        {
            _distanceSpawnY = -200;
            ToutDroit(tourboucle, _parentNode);
            _currentNode.Hauteur = _currentHeight - 1;
        }
        _distanceSpawnY = 0;
    }
    public void ToutDroit(int tourboucle, Node NodeForY)
    {
        //On le place et on arrondit pour le dicooo
        Vector3Int newPosition = new Vector3Int((_distanceSpawnX * tourboucle) + Mathf.RoundToInt(_currentNode.transform.localPosition.x), Mathf.RoundToInt(NodeForY.transform.localPosition.y) + _distanceSpawnY, Mathf.RoundToInt(_currentNode.transform.localPosition.z));
        if (_dicoNode.ContainsKey(newPosition))
        {
            _existingValue = _dicoNode[newPosition];
            print("Un node est déja présent ici" + _existingValue);
            _nodeList.Enqueue(_currentNode);
        }
        else
        {
            _currentNode.transform.localPosition = newPosition;
            _currentNode.gameObject.SetActive(true);
            _currentNode.Hauteur = _currentHeight;
            _dicoNode.Add(newPosition, _currentNode);
            MapBuildingTools.Instance.TraceTonTrait(_parentNode, _currentNode);
            _currentNode.Creator = _parentNode;
            _currentNode.Position = tourboucle;
        }
    }

    public void CreateBranch(int tourboucle, bool Up)
    {
        if (Up) //Si on peut monter
        {
            _distanceSpawnY = 200;
            ToutDroit(tourboucle, _parentNode);
            _currentNode.Hauteur = _currentHeight + 1;
        }
        else //Si on peut descendre
        {
            _distanceSpawnY = -200;
            ToutDroit(tourboucle, _parentNode);
            _currentNode.Hauteur = _currentHeight - 1;
        }
        _distanceSpawnY = 0;
    }

    public void ConstructionSecondaireGraph()
    {
        foreach (Node node in _intersection)
        {
            _parentNode = node;
            while (true)
            {
                _currentHeight = _parentNode.Hauteur;
                _currentNode = _nodeList.Dequeue();
                int tour = _parentNode.Position + 1;

                // Calcul de la position potentielle
                Vector3Int nextPosition = new Vector3Int((_distanceSpawnX * tour) + Mathf.RoundToInt(_currentNode.transform.localPosition.x), Mathf.RoundToInt(_parentNode.transform.localPosition.y),Mathf.RoundToInt(_currentNode.transform.localPosition.z));

                if (_dicoNode.ContainsKey(nextPosition))
                {
                    Node nodeExistant = _dicoNode[nextPosition];

                    MapBuildingTools.Instance.TraceTonTrait(_parentNode, nodeExistant);// Trace une ligne entre le parent actuel et le node déjà existant
                    nodeExistant.Creator = _parentNode;

                    _nodeList.Enqueue(_currentNode);
                    break;
                }
                if (_parentNode.Position >= _mapRange - 2)
                {
                    if (_currentHeight != 3)
                    {
                        if (_currentHeight > 3)
                        {
                            CreateBranch(tour, false); // descendre
                            MapBuildingTools.Instance.TraceTonTrait(_parentNode, _existingValue);
                        }
                        else
                        {
                            CreateBranch(tour, true); // monter
                            MapBuildingTools.Instance.TraceTonTrait(_parentNode, _existingValue);
                        }
                    }
                }
                else
                {
                    ToutDroit(tour, _parentNode);
                }
                _parentNode = _currentNode;
            }
        }
    }

    public Vector3Int GetKeyFromNode(Node node)
    {
        foreach (var kvp in _dicoNode)
        {
            if (kvp.Value == node)
                return kvp.Key;
        }
        return Vector3Int.zero; // par défaut si pas trouvé
    }
}