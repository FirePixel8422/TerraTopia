using DG.Tweening;
using Unity.Netcode;
using UnityEngine;



public class UnitBase : TileObject
{
    private int ownerPlayerGameId;
    public int unitId;

    public Renderer colorRenderer;
    public Transform headTransform;

    [Header("Unit Stats")]

    [Header("Health based variables")]
    [SerializeField] private int _health;
    [SerializeField] private int _maxHealth;

    [Header("Movement based variables")]
    [SerializeField] private int _movementRange;
    [SerializeField] private int _detectionRange;

    [Header("Combat based variables")]
    [SerializeField] private int _attackDamage;

    [Header("Achievements")]
    [SerializeField] private int _unitsKilled;
    [SerializeField] private int tilesVisited;


    public void TakeDamage(int damageToTake)
    {
        _health -= damageToTake;
    }

    public void OnClick()
    {

    }

    public void OnDifferentClickableClicked(GameObject newlyClickedObject)
    {
        if (newlyClickedObject.TryGetComponent(out TileBase TB))
        {
            //Moves towards the newly selected tile(Only if it is within reach(decided by the "MovementRange factor" )
            if (IsTileWithinReach(newlyClickedObject.transform.position))
            {
                print("Unit is within range");
                transform.DOMove(newlyClickedObject.transform.position, 1f);
            }
        }
    }

    Vector3 unitPos => new Vector3(transform.position.x, 0, transform.position.z);
    public bool IsTileWithinReach(Vector3 tilePos)
    {
        tilePos.y = 0f;
        print((int)Vector3.Distance(unitPos, tilePos));
        if ((int)Vector3.Distance(unitPos, tilePos) <= _movementRange) { return true; }
        return false;
    }


    public void OnSpawnUnit_OnServer(ulong clientNetworkId, int playerGameId)
    {
        NetworkObject.SpawnWithOwnership(clientNetworkId, true);
    }


    public override void OnNetworkSpawn()
    {

        ownerPlayerGameId = ClientManager.GetClientGameIdFromNetworkId(NetworkObject.OwnerClientId);
        if (IsServer)
        {
            OnSpawn_ClientRPC(ownerPlayerGameId);
        }
    }


    [ClientRpc(RequireOwnership = false)]
    private void OnSpawn_ClientRPC(int ownerPlayerGameId)
    {
        colorRenderer.material = UnitSpawnHandler.GetTeamColorMaterial_OnServer(ownerPlayerGameId, unitId);
    }
}
