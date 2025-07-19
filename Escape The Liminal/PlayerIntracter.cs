using UnityEngine;

public abstract class PlayerIntracter : MonoBehaviour,IEntityComponent
{
    [SerializeField] private OverlapCaster intractCaster;
    [SerializeField] private LayerMask whatIsIntract;

    [SerializeField] protected Collider _colliders;
    private Player _player;

    public void Initialize(Entity entity)
    {
        _player = entity as Player;
        intractCaster.InitInPlayerCaster(_player);
    }

    public bool IntractKeyPressed()
    {
       _colliders = intractCaster.GetTargetInCaster(transform.position, whatIsIntract);
        return _colliders != null ? true : false;
    }

    public virtual void Interacting(Player player)
    {

    }
}
