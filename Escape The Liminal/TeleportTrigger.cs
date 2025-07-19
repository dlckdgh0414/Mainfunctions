using UnityEngine;

public class TeleportTrigger : MonoBehaviour
{
    private Vector3 _lastPos;
    private Transform _otherSidecorridorTrm;

    private BoxCollider _teleportCollider;

    [SerializeField] private GameObject corridor;

    [SerializeField] private GameEventChannelSO teleportEventChannel;

    TeleportEvent teleportEvent = TeleportEventChannel.TeleportEvent;

    [field : SerializeField] public Transform teleportPos;
    private void OnTriggerEnter(Collider other)
    {
        if(other.TryGetComponent(out Player player))
            _lastPos = player.transform.position;
    }

    private void OnTriggerExit(Collider other)
    {
        if(other.TryGetComponent(out Player player))
        {
            Vector3 currentPlayerPos = player.transform.position;

            Vector3 dir = currentPlayerPos - _lastPos;     

            if(Vector3.Dot(transform.forward, dir) > 0)
            {
                _otherSidecorridorTrm = teleportEvent.Teleport.TelportTrm(corridor.gameObject);

                player.transform.position = teleportEvent.Teleport.TeleportPos();

                _teleportCollider = teleportEvent.Teleport.TeleportBoxCollider();
                teleportEvent.IsTeleported = true;
                teleportEvent.TeleportTrigger = this;
                teleportEventChannel.RaiseEvent(teleportEvent);

                if(_teleportCollider == null)
                {
                    return;
                }
                _teleportCollider.enabled = false;
            }
            _lastPos = currentPlayerPos;
        }
    }

    public void Restoration()
    {
        teleportEvent.IsTeleported = false;
        teleportEvent.TeleportTrigger = null;
        if (_teleportCollider == null)
        {
            return;
        }
        _teleportCollider.enabled = true;

        teleportEventChannel.RaiseEvent(teleportEvent);
    }
}
