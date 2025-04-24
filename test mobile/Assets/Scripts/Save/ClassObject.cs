using static NodeTypes;
using System;
using UnityEngine;
using System.Collections.Generic;

[Serializable]
public class SerializableNode
{
    public Vector3Int key;
    public int position;
    public int hauteur;
    public NodesEventTypes eventName;
    public bool onYReviendra;
}

[Serializable]
public class MapWrapper
{
    public List<SerializableNode> items = new();
}
