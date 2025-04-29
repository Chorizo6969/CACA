using System;
using System.Collections.Generic;
using UnityEngine;
using static NodeTypes;

public class MapAttributeEvent : MonoBehaviour
{
    private Dictionary<Vector3Int, Node> _dico = new();
    private int _nombreNodeAttribue;
    private int _posX;

    [SerializeField] private int _probaCuisine = 17;
    [SerializeField] private int _probaIngredient = 17;
    [SerializeField] private int _probaCombat = 50;
    [SerializeField] private int _probaHeal = 16;

    private int çafait2foisnon = 0;

    #region Singleton
    public static MapAttributeEvent Instance;

    private void Awake()
    {
        Instance = this;
    }
    #endregion

    public void MapMakingEvent()
    {
        _dico = MapMaker2.Instance._dicoNode;
        for (int i = 4; i <= MapMaker2.Instance._mapRange - 2; i++)
        {
            _nombreNodeAttribue = 0;
            foreach (KeyValuePair<Vector3Int, Node> KeyAndValues in _dico)
            {
                if (KeyAndValues.Value.Position == i)
                {
                    _posX = KeyAndValues.Key.x;
                }
            }
            int nombreTourDeBoucle = CountNodesWithX(_posX);
            
            foreach (KeyValuePair<Vector3Int, Node> KeyAndValues in _dico)
            {
                if (_nombreNodeAttribue < nombreTourDeBoucle)
                {
                    if (KeyAndValues.Value.Position == i)
                    {
                        switch(KeyAndValues.Value.Creator.EventName)
                        {
                            case NodesEventTypes.Combat:
                                if (çafait2foisnon >= 2) { _probaCombat = 0; }
                                break;
                            case NodesEventTypes.Cuisine:
                                _probaCuisine = 0;
                                break;
                            case NodesEventTypes.Heal:
                                _probaHeal = 0;
                                break;
                            case NodesEventTypes.Ingredient:
                                _probaIngredient = 0;
                                break;
                        }
                        AttributeEventNode(KeyAndValues.Value, _probaCuisine, _probaCombat, _probaIngredient, _probaHeal);
                        _nombreNodeAttribue++;
                    }
                }
            }
        }

    }

    public int CountNodesWithX(int indexX)
    {
        int count = 0;
        foreach (var entry in _dico)
        {
            if (entry.Key.x == indexX)
            {
                count++;
            }
        }
        return count;
    }


    public void AttributeEventNode(Node node, int proba1, int proba2, int proba3, int proba4)
    {
        int result = CalculProba(proba1, proba2, proba3, proba4);

        if (result <= _probaCuisine)
        {
            node.EventName = NodesEventTypes.Cuisine;
            _probaCuisine = 0;
            _probaCombat = 57;
            _probaIngredient = 22;
            _probaHeal = 21;
            çafait2foisnon = 0;
        }
        else if (result <= _probaCombat + _probaCuisine)
        {
            node.EventName = NodesEventTypes.Combat;
            _probaCuisine = 34;
            _probaCombat = 0;
            _probaIngredient = 33;
            _probaHeal = 33;
            çafait2foisnon++;
        }
        else if (result <= _probaIngredient + _probaCombat + _probaCuisine)
        {
            node.EventName = NodesEventTypes.Ingredient;
            _probaCuisine = 22;
            _probaCombat = 57;
            _probaIngredient = 0;
            _probaHeal = 21;
            çafait2foisnon = 0;
        }
        else
        {
            node.EventName = NodesEventTypes.Heal;
            _probaCuisine = 22;
            _probaCombat = 57;
            _probaIngredient = 21;
            _probaHeal = 0;
            çafait2foisnon = 0;
        }
    }

    /// <summary>
    /// Fonction qui va set les nodes pour leur attribué un event de case entre "Cuisine et Combat" 
    /// </summary>
    public void MapMakingEventBeforeBoss()
    {
        if (MapMaker2.Instance._currentNode.Creator.EventName == NodesEventTypes.Cuisine) //Dans le cas ou une cuisine était juste avant
        {
            MapMaker2.Instance._currentNode.EventName = NodesEventTypes.Combat;
            return;
        }

        int result = CalculProba(_probaCuisine, 0, 0, _probaHeal);

        if (result <= _probaCuisine)
        {
            MapMaker2.Instance._currentNode.EventName = NodesEventTypes.Cuisine;
            return;
        }
        else
        {
            MapMaker2.Instance._currentNode.EventName = NodesEventTypes.Heal;
            return;
        }
    }

    /// <summary>
    /// Set la probabilité d'avoir une cuisine à 0
    /// </summary>
    public void SetCuisineProbaToNull() { _probaCuisine = 0; }

    private int CalculProba(int Cuisine, int Ingredient, int Combat, int Heal)
    {
        int Total = Cuisine + Ingredient + Combat + Heal;
        int result = UnityEngine.Random.Range(1, Total + 1);
        return result;
    }
}
