using UnityEngine;
using static NodeTypes;

[RequireComponent(typeof(MapMakerTest352))]
public class MapAttributeEventTest : MonoBehaviour
{

    [SerializeField] private int _probaCuisine = 2;
    [SerializeField] private int _probaIngredient = 2;
    [SerializeField] private int _probaCombat = 2;

    #region Singleton
    public static MapAttributeEventTest Instance;

    private void Awake()
    {
        Instance = this;
    }
    #endregion

    public void MapMakingEvent()
    {
        int result = CalculProba(_probaCuisine, _probaIngredient, _probaCombat);

        if (result <= _probaCuisine)
        {
            MapMakerTest352.Instance._currentNode.EventName = NodesEventTypes.Cuisine;
            _probaCuisine = 0;
            _probaCombat += 1;
            _probaIngredient += 1;
            return;
        }
        if (result <= _probaCombat + _probaCuisine)
        {
            MapMakerTest352.Instance._currentNode.EventName = NodesEventTypes.Combat;
            _probaCuisine += 1;
            _probaCombat = 2;
            _probaIngredient += 1;
            return;
        }
        else
        {
            MapMakerTest352.Instance._currentNode.EventName = NodesEventTypes.Ingredient;
            _probaCuisine += 1;
            _probaCombat += 1;
            _probaIngredient = 2;
            return;
        }
    }

    /// <summary>
    /// Fonction qui va set les nodes pour leur attribué un event de case entre "Cuisine et Combat" 
    /// </summary>
    public void MapMakingEventBeforeBoss()
    {
        if (MapMakerTest352.Instance._currentNode.Creator.EventName == NodesEventTypes.Cuisine) //Dans le cas ou une cuisine était juste avant
        {
            MapMakerTest352.Instance._currentNode.EventName = NodesEventTypes.Combat;
            return;
        }

        int result = CalculProba(_probaCuisine, 0, _probaCombat);

        if (result <= _probaCuisine)
        {
            MapMakerTest352.Instance._currentNode.EventName = NodesEventTypes.Cuisine;
            return;
        }
        else
        {
            MapMakerTest352.Instance._currentNode.EventName = NodesEventTypes.Combat;
            return;
        }
    }

    /// <summary>
    /// Set la probabilité d'avoir une cuisine à 0
    /// </summary>
    public void SetCuisineProbaToNull()
    {
        _probaCuisine = 0;
    }

    private int CalculProba(int Cuisine, int Ingredient, int Combat)
    {
        int Total = Cuisine + Ingredient + Combat;
        int result = Random.Range(1, Total + 1);
        return result;
    }
}