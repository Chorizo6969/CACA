using NUnit.Framework.Interfaces;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(MapMaker2))]
public class MapBuildingTools : MonoBehaviour
{
    [SerializeField] [Tooltip("Image qui sera dupliqué pour faire les chemins entre les nodes")] private Image _origineImage;
    [SerializeField][Tooltip("GameObject parent des chemins, si null alors c'est le porteur du script le parent")] private GameObject _parent;
    private List<Image> _trailList = new();
    public bool FirstTimeDraw = true;

    #region Singleton
    public static MapBuildingTools Instance;
    private void Awake()
    {
        Instance = this;
        if (_parent == null)
        {
            _parent = gameObject;
        }
        for (int i = 0; i <= 50; i++)
        {
            Image NewNode = Instantiate(_origineImage, _parent.transform);
            NewNode.gameObject.SetActive(false);
            _trailList.Add(NewNode);
        }
    }
    #endregion

    /// <summary>
    /// Fonction qui trace un trait entre le point A et B
    /// </summary>
    /// <param name="PointA">Point A (_fatherOfNode)</param>
    /// <param name="PointB">Point B (CurrentNode)</param>
    /// <param name="Drawing">Booléen qui permet de dessiner ou non</param>
    public void TraceTonTrait(Node PointA, Node PointB)
    {
        if (FirstTimeDraw)
        {
            // Sprite entre father et current
            Image CurrentTrail = _trailList[0];
            _trailList.RemoveAt(0);

            CurrentTrail.gameObject.SetActive(true);

            Vector3 trailPos = (PointA.transform.localPosition + PointB.transform.localPosition) / 2f; //au milleu des 2
            CurrentTrail.transform.localPosition = trailPos;

            // Rotation du sprite
            Vector3 dir = PointB.transform.localPosition - PointA.transform.localPosition;
            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            CurrentTrail.transform.rotation = Quaternion.Euler(0, 0, angle);
        }
        else
        {
            FirstTimeDraw = true;
        }
    }

    public bool Intersection(int mapRangeCurrentIndex, int probaIntersection)
    {
        if (mapRangeCurrentIndex >= 4)
        {
            int result = Random.Range(1, 11);
            if (result <= probaIntersection) //Intersection si true
            {
                return true;
            }
            return false;
        }
        else
        {
            return false;
        }

    }
}
