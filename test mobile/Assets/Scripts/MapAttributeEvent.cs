using UnityEngine;
using static NodeTypes;

[RequireComponent(typeof(MapMaker2))]
public class MapAttributeEvent : MonoBehaviour
{

    [SerializeField] private int _probaCuisine = 2;
    [SerializeField] private int _probaIngredient = 2;
    [SerializeField] private int _probaCombat = 2;
    [SerializeField] private int _probaHeal = 2;

    #region Singleton
    public static MapAttributeEvent Instance;

    private void Awake()
    {
        Instance = this;
    }
    #endregion

    public void MapMakingEvent()
    {
        int result = CalculProba(_probaCuisine, _probaIngredient, _probaCombat, _probaHeal);

        if (result <= _probaCuisine)
        {
            MapMaker2.Instance._currentNode.EventName = NodesEventTypes.Cuisine;
            _probaCuisine = 0;
            _probaCombat += 1;
            _probaIngredient += 1;
            _probaHeal += 1;
            return;
        }
        else if (result <= _probaCombat + _probaCuisine)
        {
            MapMaker2.Instance._currentNode.EventName = NodesEventTypes.Combat;
            _probaCuisine += 1;
            _probaCombat = 2;
            _probaIngredient += 1;
            _probaHeal += 1;
            return;
        }
        else if (result <= _probaIngredient + _probaCombat + _probaCuisine)
        {
            MapMaker2.Instance._currentNode.EventName = NodesEventTypes.Ingredient;
            _probaCuisine += 1;
            _probaCombat += 1;
            _probaIngredient = 2;
            _probaHeal += 1;
        }
        else
        {
            MapMaker2.Instance._currentNode.EventName = NodesEventTypes.Heal;
            _probaHeal = 2;
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
    public void SetCuisineProbaToNull()
    {
        _probaCuisine = 0;
    }

    private int CalculProba(int Cuisine, int Ingredient, int Combat, int Heal)
    {
        int Total = Cuisine + Ingredient + Combat + Heal;
        int result = Random.Range(1, Total + 1);
        return result;
    }
}