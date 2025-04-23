using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UI;
using static NodeTypes;

public class MapMaker : MonoBehaviour
{
    public int MapRange;
    public int DistanceSpawnX = 200;
    public int DistanceSpawnY = 140;


    public static MapMaker Instance;

    [SerializeField] private Node _prefab;
    [SerializeField] private Node _fatherOfNode;
    [SerializeField] private Queue<Node> _nodeList = new();
    [SerializeField] private Queue<Node> _allNode = new(); //Liste de tout les nodes
    [SerializeField] private List<Image> _trailList = new();
    [SerializeField] private int _probaIntersection = 3;
    private Dictionary<Vector3Int, Node> _dicoNode = new();

    public Node _currentNode;

    /*private void Awake()
    {
        Instance = this;
        for (int i = 0; i <= MapRange * 2; i++)
        {
            Node NewNode = Instantiate(_prefab, gameObject.transform);
            NewNode.transform.localPosition = _fatherOfNode.transform.localPosition;
            _nodeList.Enqueue(NewNode);
        }
        _allNode = _nodeList;
    }

    void Start()
    {
        MapMaking();  //Je crée la map
    }*/


    public void MapMaking()
    {
        // Position de départ au centre
        Vector3Int startPos = new Vector3Int(-1045, 0, 0);
        _fatherOfNode.transform.localPosition = startPos;
        _dicoNode.Add(startPos, _fatherOfNode);

        Vector3Int currentPos = startPos;

        for (int i = 1; i <= MapRange; i++)
        {
            DistanceSpawnY = 0;
            _currentNode = _nodeList.Dequeue();
            _currentNode.gameObject.SetActive(true);

            Vector3Int nextPos = currentPos + Vector3Int.right; // Avancer vers la droite

            if (Intersection())
            {
                // Tentative croisement
                bool goUp = UnityEngine.Random.Range(0, 2) == 0;
                Vector3Int offset = goUp ? Vector3Int.up : Vector3Int.down;

                Vector3Int testPos = currentPos + Vector3Int.right + offset;

                // Vérifie si la case est libre et dans la grille 0-4 en hauteur
                if (testPos.y >= 0 && testPos.y <= 4 && !_dicoNode.ContainsKey(testPos))
                {
                    nextPos = testPos;
                }

                DistanceSpawnY = 140;
                _currentNode.OnYReviendra = true;
                // sinon reste tout droit
            }

            // Place le Node
            Vector3 newPosition = new Vector3((DistanceSpawnX * i) + nextPos.x, DistanceSpawnY + nextPos.y, 0);

            _currentNode.transform.localPosition = newPosition;

            // Trail entre father et current
            Image CurrentTrail = _trailList[0];
            _trailList.RemoveAt(0);

            CurrentTrail.gameObject.SetActive(true);

            Vector3 trailPos = (_fatherOfNode.transform.localPosition + _currentNode.transform.localPosition) / 2f;
            CurrentTrail.transform.localPosition = trailPos;

            // Rotation du trail
            Vector3 dir = _currentNode.transform.localPosition - _fatherOfNode.transform.localPosition;
            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            CurrentTrail.transform.rotation = Quaternion.Euler(0, 0, angle);

            // MAJ du père
            _currentNode.Creator = _fatherOfNode;
            _fatherOfNode = _currentNode;
            currentPos = nextPos;
            _dicoNode.Add(currentPos, _currentNode);

            // Event attribution
            _currentNode.Position = i;
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
        }

        Node.TriggerMapCompleted();
        foreach (Vector3Int vector in _dicoNode.Keys)
        {
            print(vector); //C'est pas les bonnes values
        }
    }


    public bool Intersection()
    {
        int result = UnityEngine.Random.Range(0, 11);
        if (result <= _probaIntersection) //Intersection si true
        {
            return true;
        }
        return false;
    }
}