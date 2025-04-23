using UnityEngine;
using static NodeTypes;

[RequireComponent(typeof(MapMaker2))]
public class MapAttributeEvent : MonoBehaviour
{

    [SerializeField] public int _probaCuisine = 2;
    [SerializeField] private int _probaIngredient = 2;
    [SerializeField] private int _probaCombat = 2;

    public static MapAttributeEvent Instance;

    private void Awake()
    {
        Instance = this;
    }

    public void MapMakingEvent()
    {
        int result = CalculProba(_probaCuisine, _probaIngredient, _probaCombat);

        if (result <= _probaCuisine)
        {
            MapMaker2.Instance._currentNode.EventName = NodesEventTypes.Cuisine;
            _probaCuisine = 0;
            _probaCombat += 1;
            _probaIngredient += 1;
            return;
        }
        if (result <= _probaCombat + _probaCuisine)
        {
            MapMaker2.Instance._currentNode.EventName = NodesEventTypes.Combat;
            _probaCuisine += 1;
            _probaCombat = 2;
            _probaIngredient += 1;
            return;
        }
        else
        {
            MapMaker2.Instance._currentNode.EventName = NodesEventTypes.Ingredient;
            _probaCuisine += 1;
            _probaCombat += 1;
            _probaIngredient = 2;
            return;
        }
    }

    public void MapMakingEventBeforeBoss()
    {
        if (MapMaker2.Instance._currentNode.Creator.EventName == NodesEventTypes.Cuisine) //Dans le cas ou une cuisine était juste avant
        {
            MapMaker2.Instance._currentNode.EventName = NodesEventTypes.Combat;
            return;
        }

        int result = CalculProba(_probaCuisine, 0, _probaCombat);

        if (result <= _probaCuisine)
        {
            MapMaker2.Instance._currentNode.EventName = NodesEventTypes.Cuisine;
            return;
        }
        else
        {
            MapMaker2.Instance._currentNode.EventName = NodesEventTypes.Combat;
            return;
        }
    }

    private int CalculProba(int Cuisine, int Ingredient, int Combat)
    {
        int Total = Cuisine + Ingredient + Combat;
        int result = Random.Range(1, Total + 1);
        return result;
    }
}
