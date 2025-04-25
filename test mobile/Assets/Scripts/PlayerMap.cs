using UnityEngine;

public class PlayerMap : MonoBehaviour
{
    private Vector3 _target;
    public float speed = 5f;
    public static PlayerMap Instance;

    private void Awake()
    {
        Instance = this;
        _target = transform.position;
    }

    public void SetupTarget(Vector3 target)
    {
        _target = target;
    }

    private void Update()
    {
        if (Vector3.Distance(transform.position, _target) > 0.1f)
        {
            transform.position = Vector3.MoveTowards(transform.position, _target, speed * Time.deltaTime);
        }
    }
}
