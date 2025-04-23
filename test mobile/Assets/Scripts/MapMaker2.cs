using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static NodeTypes;

[RequireComponent(typeof(MapAttributeEvent))]
public class MapMaker2 : MonoBehaviour
{
    public static MapMaker2 Instance;

    [Header("Map Adjusting")]
    [SerializeField] [Tooltip("Le nombre de node minimum entre le Node de départ et le boss")] private int _mapRange;
    [SerializeField] [Tooltip("Distance à laquelle le node va spawn sur l'axe X")] private int _distanceSpawnX = 200;
    [SerializeField] [Tooltip("Distance à laquelle le node va spawn sur l'axe Y")] private int DistanceSpawnY = 0;

    [Header("Probality")]
    [SerializeField] [Tooltip("Probabiltité à chaque node d'avoir une intersection (0 = impossible)")] [Range(0,10)] private int _probaIntersection = 3;

    [Header("Other ne pas toucher sauf code")]
    [SerializeField] private Node _nodePrefab;

    [SerializeField] private Node _fatherOfNode;

    public Node _currentNode { get; private set; }

    private Queue<Node> _nodeList = new();
    private List<Node> _nodeAvecUnPetitTrucEnPlus = new();
    private Dictionary<Vector3Int, Node> _dicoNode = new();
    private int _currentHauteur = 3;


    private void Awake()
    {
        Instance = this;
        for (int i = 0; i <= _mapRange * 4; i++)
        {
            Node NewNode = Instantiate(_nodePrefab, gameObject.transform);
            NewNode.transform.localPosition = _fatherOfNode.transform.localPosition;
            _nodeList.Enqueue(NewNode);
        }
    }

    void Start()
    {
        Vector3Int startPos = new Vector3Int(-1045, 0, 0); // Enregistre le node de départ
        _fatherOfNode.transform.localPosition = startPos;
        _dicoNode.Add(startPos, _fatherOfNode);

        MapMaking(1);  //Je crée la map
        List<Node> Pourforeach = _nodeAvecUnPetitTrucEnPlus;
        foreach (Node node in Pourforeach)
        {
            _currentHauteur = node.Hauteur;
            _fatherOfNode = node;
            _probaIntersection = 0;
            MapMaking(node.Position);
        }
        Node.TriggerMapCompleted();
    }

    public void MapMaking(int StartPosition)
    {
        //D'abord on fait un chemin du début vers le boss. SI un choix arrive on crée les 2 nodes et on mets le 2ème en stand by. Une fois fini on reviens dessus puis on continue jusqu'a
        //ce que c'est co nous ammène au boss et ainsi de suite pour chaque nodes avec le bool en true

        for (int i = StartPosition; i <= _mapRange; i++)
        {
            _currentNode = _nodeList.Dequeue();
            _currentNode.gameObject.SetActive(true);
            if (_fatherOfNode.Position >= _mapRange - 2) //zone faut revenir au centre
            {
                if (_currentHauteur != 3)
                {
                    if (_currentHauteur > 3)
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
                MapAttributeEvent.Instance._probaCuisine = 0;
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

            _fatherOfNode = _currentNode;
        }
    }

    public void CreateBranch(int tourboucle)
    {
        if (_currentHauteur + 1 <= 5 && _currentHauteur - 1 >= 1) //Si on peut monter et descendre
        {
            DistanceSpawnY = 140;
            _currentHauteur++;
            _currentNode.OnYReviendra = true;
            _nodeAvecUnPetitTrucEnPlus.Add(_currentNode);
            ToutDroit(tourboucle);
            _currentNode = _nodeList.Dequeue();
            _currentNode.gameObject.SetActive(true);
            DistanceSpawnY = -140;
            _currentHauteur -= 2;
            ToutDroit(tourboucle);
        }
        else if (_currentHauteur + 1 <= 5) //Si on peut monter
        {
            DistanceSpawnY = 140;
            _currentHauteur++;
            _currentNode.OnYReviendra = true;
            _nodeAvecUnPetitTrucEnPlus.Add(_currentNode);
            ToutDroit(tourboucle);
        }
        else if (_currentHauteur - 1 >= 1) //Si on peut descendre
        {
            DistanceSpawnY = -140;
            _currentHauteur -= 2;
            ToutDroit(tourboucle);
        }
        DistanceSpawnY = 0;
    }

    public void ToutDroit(int tourboucle)
    {
        //On le place et on arrondit
        Vector3Int newPosition = new Vector3Int((_distanceSpawnX * tourboucle) + Mathf.RoundToInt(_currentNode.transform.localPosition.x), Mathf.RoundToInt(_fatherOfNode.transform.localPosition.y) + DistanceSpawnY, Mathf.RoundToInt(_currentNode.transform.localPosition.z));
        _currentNode.transform.localPosition = newPosition;

        if (_dicoNode.ContainsKey(newPosition))
        {
            Node existingValue = _dicoNode[newPosition];
            print("Un node est déja présent ici" + existingValue);
            Destroy(_currentNode.gameObject);
            DrawLineMap.Instance.TraceTonTrait(_fatherOfNode, existingValue, true);
        }
        else
        {
            _dicoNode.Add(newPosition, _currentNode);
            DrawLineMap.Instance.TraceTonTrait(_fatherOfNode, _currentNode, true);//vvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvv
            _currentNode.Creator = _fatherOfNode;
            _currentNode.Hauteur = _currentHauteur;
            _currentNode.Position = tourboucle;
        }
    }

    public void CreateBranch(int tourboucle, bool Up)
    {
        if (Up) //Si on peut monter
        {
            DistanceSpawnY = 140;
            _currentHauteur++;
            ToutDroit(tourboucle);
        }
        else //Si on peut descendre
        {
            DistanceSpawnY = -140;
            _currentHauteur -= 2;
            ToutDroit(tourboucle);
        }
        DistanceSpawnY = 0;
    }

    /// <summary>
    /// Retourne un bool si la proba de l'intersection est passé ou non
    /// </summary>
    /// <returns>Bool de la propa de crée une intersection</returns>
    public bool Intersection()
    {
        int result = UnityEngine.Random.Range(1, 11);
        if (result <= _probaIntersection) //Intersection si true
        {
            return true;
        }
        return false;
    }
}