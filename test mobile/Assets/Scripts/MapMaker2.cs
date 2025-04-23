using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UI;
using static NodeTypes;

public class MapMaker2 : MonoBehaviour
{
    public int MapRange;
    public int DistanceSpawnX = 200;
    public int DistanceSpawnY = 0;
    public int CurrentHauteur;

    public static MapMaker2 Instance;

    [SerializeField] private Node _prefab;
    [SerializeField] private Node _fatherOfNode;
    [SerializeField] private Queue<Node> _nodeList = new();
    [SerializeField] private List<Node> _nodeAvecUnPetitTrucEnPlus = new();
    [SerializeField] private List<Image> _trailList = new();
    [SerializeField] private int _probaIntersection = 3;
    [SerializeField] private bool test;
    private Dictionary<Vector3Int, Node> _dicoNode = new();

    public Node _currentNode;

    private void Awake()
    {
        Instance = this;
        for (int i = 0; i <= MapRange * 4; i++)
        {
            Node NewNode = Instantiate(_prefab, gameObject.transform);
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
            CurrentHauteur = node.Hauteur;
            _fatherOfNode = node;
            _probaIntersection = 0;
            test = true;
            MapMaking(node.Position);
        }
        Node.TriggerMapCompleted();
    }

    public void MapMaking(int StartPosition)
    {
        //D'abord on fait un chemin du début vers le boss. SI un choix arrive on crée les 2 nodes et on mets le 2ème en stand by. Une fois fini on reviens dessus puis on continue jusqu'a
        //ce que c'est co nous ammène au boss et ainsi de suite pour chaque nodes avec le bool en true

        for (int i = StartPosition; i <= MapRange; i++)
        {
            _currentNode = _nodeList.Dequeue();
            _currentNode.gameObject.SetActive(true);
            if (_fatherOfNode.Position >= MapRange - 2) //zone faut revenir au centre
            {
                if (CurrentHauteur != 3)
                {
                    if (CurrentHauteur > 3)
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

            if (_currentNode.Position == 1)
            {
                _currentNode.EventName = NodesEventTypes.Cuisine;
                MapAttributeEvent.Instance._probaCuisine = 0;
            }
            else if (_currentNode.Position + 1 == MapRange)
            {
                MapAttributeEvent.Instance.MapMakingEventBeforeBoss();
            }
            else if (_currentNode.Position == MapRange)
            {
                _currentNode.EventName = NodesEventTypes.Boss;
            }
            else
            {
                MapAttributeEvent.Instance.MapMakingEvent();
            }

            _fatherOfNode = _currentNode;
        }
    }

    public void CreateBranch(int tourboucle)
    {
        if (CurrentHauteur + 1 <= 5 && CurrentHauteur - 1 >= 1) //Si on peut monter et descendre
        {
            DistanceSpawnY = 140;
            CurrentHauteur++;
            _currentNode.OnYReviendra = true;
            _nodeAvecUnPetitTrucEnPlus.Add(_currentNode);
            ToutDroit(tourboucle);
            _currentNode = _nodeList.Dequeue();
            _currentNode.gameObject.SetActive(true);
            DistanceSpawnY = -140;
            CurrentHauteur -= 2;
            ToutDroit(tourboucle);
        }
        else if (CurrentHauteur + 1 <= 5) //Si on peut monter
        {
            DistanceSpawnY = 140;
            CurrentHauteur++;
            _currentNode.OnYReviendra = true;
            _nodeAvecUnPetitTrucEnPlus.Add(_currentNode);
            ToutDroit(tourboucle);
        }
        else if (CurrentHauteur - 1 >= 1) //Si on peut descendre
        {
            DistanceSpawnY = -140;
            CurrentHauteur -= 2;
            ToutDroit(tourboucle);
        }
        DistanceSpawnY = 0;
    }

    public void ToutDroit(int tourboucle)
    {
        //On le place et on arrondit
        Vector3Int newPosition = new Vector3Int((DistanceSpawnX * tourboucle) + Mathf.RoundToInt(_currentNode.transform.localPosition.x), Mathf.RoundToInt(_fatherOfNode.transform.localPosition.y) + DistanceSpawnY, Mathf.RoundToInt(_currentNode.transform.localPosition.z));
        _currentNode.transform.localPosition = newPosition;

        if (_dicoNode.ContainsKey(newPosition))
        {
            Node existingValue = _dicoNode[newPosition];
            print("Un node est déja présent ici" + existingValue);
            Destroy(_currentNode.gameObject);
            TraceTonTrait(existingValue);
        }
        else
        {
            _dicoNode.Add(newPosition, _currentNode);
            TraceTonTrait(_currentNode);
            _currentNode.Creator = _fatherOfNode;
            _currentNode.Hauteur = CurrentHauteur;
            _currentNode.Position = tourboucle;
        }
    }

    public void TraceTonTrait(Node PointB)
    {
        if(!test)
        {
            // Sprite entre father et current
            Image CurrentTrail = _trailList[0];
            _trailList.RemoveAt(0);

            CurrentTrail.gameObject.SetActive(true);

            Vector3 trailPos = (_fatherOfNode.transform.localPosition + PointB.transform.localPosition) / 2f; //au milleu des 2
            CurrentTrail.transform.localPosition = trailPos;

            // Rotation du sprite
            Vector3 dir = PointB.transform.localPosition - _fatherOfNode.transform.localPosition;
            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            CurrentTrail.transform.rotation = Quaternion.Euler(0, 0, angle);
        }
        else { test = false; }

    }

    public void CreateBranch(int tourboucle, bool Up)
    {
        if (Up) //Si on peut monter
        {
            DistanceSpawnY = 140;
            CurrentHauteur++;
            ToutDroit(tourboucle);
        }
        else //Si on peut descendre
        {
            DistanceSpawnY = -140;
            CurrentHauteur -= 2;
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