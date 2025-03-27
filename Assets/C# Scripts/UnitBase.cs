using DG.Tweening;
using Unity.Netcode;
using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System;

[RequireComponent(typeof(UnitNetworkStateMachine))]
public class UnitBase : TileObject
{
    private int ownerPlayerGameId;
    public int unitId;

    public Renderer colorRenderer;
    public Transform headTransform;

    private UnitNetworkStateMachine animationStateMachine;

    [Header("Unit Stats")]
    [Header("Health based variables")]
    [SerializeField] private int _health;
    [SerializeField] private int _maxHealth;

    [Header("Movement based variables")]
    [SerializeField] private int _movementRange;
    [SerializeField] private TileLayer _allowedTileLayers;
    [SerializeField] private GameObject _markerPrefab;
    [SerializeField] private float _stepHeight = 0.5f;

    [Header("Combat based variables")]
    [SerializeField] private int _attackDamage;
    [SerializeField] private int _attackRange;
    [SerializeField] private GameObject _attackMarkerPrefab;

    [Header("Achievements")]
    [SerializeField] private int _unitsKilled;
    [SerializeField] private int tilesVisited;

    public TileBase CurrentTile;

    private List<GameObject> _tileMarkers = new List<GameObject>();

    [Tooltip("The amount of turn actions the unit has left, in this case being MOVEMENT STEPS")]
    public int TurnActionsLeft { get; set; }

    [Tooltip("The amount of alternative turn actions the unit has left, in this case being ATTACK MOVES")]
    public int AltTurnActionsLeft { get; set; }

    public void Awake()
    {
        TurnManager.OnMyTurnStarted += OnTurnChange;
    }
    private void Start()
    {
        animationStateMachine = GetComponent<UnitNetworkStateMachine>();
    }

    #region Interfaces
    [ServerRpc(RequireOwnership = false)]
    public void TakeDamage_ServerRPC(int damageToTake)
    {
        _health -= damageToTake;
        if (_health <= 0)
        {
            animationStateMachine.GetHurt();

            CurrentTile.DeAssignUnit_ClientRPC(this);
            NetworkObject.Despawn(true);
        }
        else
        {
            animationStateMachine.Die();
        }
    }

    public override void OnClick(int playerId)
    {
        //Make sure the player is the owner of the unit, otherwise returns
        if (playerId != ownerPlayerGameId) { return; }

        if (_markerPrefab)
        {
            PlaceMarkersOnReachableTiles();
        }
    }

    public override void OnDifferentClickableClicked(GameObject newlyClickedObject)
    {
        if (TurnActionsLeft >= 1)
        {
            if (newlyClickedObject.TryGetComponent(out TileBase TB))
            {
                //Checks whether the tile holds an unit or not
                if (TB.CurrentHeldUnit != null)
                {
                    //Checks if the unit has any alternative actions left(in this case being attack moves)
                    if (AltTurnActionsLeft >= 1)
                    {
                        //Checks if the unit is within the attack range
                        if (Vector3.Distance(newlyClickedObject.transform.position, currentTilePos) <= _attackRange)
                        {
                            if (AltTurnActionsLeft <= 0) { return; }
                            TB.CurrentHeldUnit.TakeDamage_ServerRPC(_attackDamage);
                            AltTurnActionsLeft--;
                            return;
                        }

                    }
                }
                //If the tile does not hold an unit, the unit will try to move to the tile
                else
                {
                    //Checks whether or not the tile is able to be moved to
                    if (IsTileWithinReach(newlyClickedObject.transform.position) & CanMoveToTile(TB))
                    {
                        MoveToTile(newlyClickedObject);
                    }
                }
            }
        }
        ClearMarkers();
    }
    public void OnTurnChange()
    {
        TurnActionsLeft = 1;
        AltTurnActionsLeft = 1;
    }
    #endregion
    private Vector3 currentTilePos
    {
        get
        {
            return CurrentTile != null ? new Vector3(CurrentTile.transform.position.x, CurrentTile.transform.position.y, CurrentTile.transform.position.z) : transform.position;
        }
    }
    #region Movement
    protected virtual bool IsTileWithinReach(Vector3 tilePos)
    {
        tilePos.y = 0f;
        int xDistance = Mathf.Abs((int)(currentTilePos.x - tilePos.x));
        int zDistance = Mathf.Abs((int)(currentTilePos.z - tilePos.z));
        return Mathf.Max(xDistance, zDistance) <= _movementRange;
    }
    protected virtual bool CanMoveToTile(TileBase TB)
    {
        if(TB.CurrentHeldUnit != null) { return false; }
        return true;
    }
    protected virtual void ClearMarkers()
    {
        foreach (var marker in _tileMarkers)
        {
            Destroy(marker.gameObject);
        }
        _tileMarkers.Clear();
    }

    protected virtual void PlaceMarkersOnReachableTiles()
    {
        ClearMarkers();
        Vector3 startTilePos = currentTilePos;

        for (int x = -_movementRange; x <= _movementRange; x++)
        {
            for (int z = -_movementRange; z <= _movementRange; z++)
            {
                int xDistance = Mathf.Abs(x);
                int zDistance = Mathf.Abs(z);

                if (xDistance + zDistance <= _movementRange)
                {
                    Vector3 targetPos = currentTilePos + new Vector3(x, 0f, z);

                    //Prevent searching the starting tile
                    if (targetPos.ToVector2() == startTilePos.ToVector2()) { continue; }

                    if (GridManager.TryGetTileByPos(targetPos.ToVector2(), out TileBase tb))
                    {
                        //Break incase of an unit already inhabiting the "selected" tile
                        if (tb.CurrentHeldUnit != null)
                        {
                            //Checks whether or not the unit has any alternative actions left(in this case being attack moves)
                            if (AltTurnActionsLeft <= 0) { continue; }

                            //Checks if the unit has the same owner as this one, if so it will continue the loop. 
                            if (tb.CurrentHeldUnit.ownerPlayerGameId == ownerPlayerGameId) { continue; }

                            //Checks if the unit is within attack range
                            if (Vector3.Distance(targetPos, currentTilePos) <= _attackRange)
                            {
                                targetPos.y = tb.transform.position.y;
                                GameObject marker = Instantiate(_attackMarkerPrefab, targetPos, Quaternion.identity);
                                _tileMarkers.Add(marker);
                            }
                            continue;

                        }

                        if (TurnActionsLeft <= 0) { print("No actions left"); continue; }

                        //Skips incase of the newly "selected" tile being the starting tile of the unit
                        if (targetPos == currentTilePos) { continue; }

                        //Checks if the Tile has the correct tags allowing the unit to walk on it
                        if (tb.tileLayer.HasFlag(_allowedTileLayers))
                        {
                            float heightDifference = Mathf.Abs(transform.position.y - tb.transform.position.y);
                            if (heightDifference <= _stepHeight)
                            {
                                targetPos.y = tb.transform.position.y;
                                GameObject marker = Instantiate(_markerPrefab, targetPos, Quaternion.identity);
                                _tileMarkers.Add(marker);
                            }
                        }
                    }
                }
            }
        }
    }

    protected virtual void MoveToTile(GameObject targetTile)
    {
        if (targetTile == null) return;
        List<GameObject> path = FindPathToTile(targetTile);

        if (path != null && path.Count > 0)
        {
            MoveAlongPath(path);
        }
    }

    protected virtual void MoveAlongPath(List<GameObject> path)
    {
        if (path == null || path.Count == 0) return;

        StartCoroutine(MoveThroughTiles(path));
    }

    // Uses BFS (Breadth First Search)
    protected virtual List<GameObject> FindPathToTile(GameObject targetTile)
    {
        List<GameObject> path = new List<GameObject>();
        Vector3 startPos = currentTilePos;
        Vector3 targetPos = targetTile.transform.position;

        Queue<Vector3> queue = new Queue<Vector3>();
        Dictionary<Vector3, Vector3> parentTiles = new Dictionary<Vector3, Vector3>();
        HashSet<Vector3> visitedTiles = new HashSet<Vector3>();

        queue.Enqueue(startPos);
        visitedTiles.Add(startPos);

        Vector3[] directions = new Vector3[] {
        Vector3.right, Vector3.left, Vector3.forward, Vector3.back
    };

        // Debug: Log initial positions
        Debug.Log($"Starting pathfinding from {startPos} to {targetPos}");

        while (queue.Count > 0)
        {
            Vector3 currentTile = queue.Dequeue();

            if (Vector3.Distance(currentTile, targetPos) < _stepHeight)
            {
                while (currentTile != startPos)
                {
                    if (GridManager.TryGetTileByPos(currentTile.ToVector2(), out GameObject tileAtIterator))
                    {
                        if (tileAtIterator != null)
                        {
                            path.Insert(0, tileAtIterator);
                            Debug.Log($"Path: Added {tileAtIterator.transform.position}");
                        }
                    }
                    currentTile = parentTiles[currentTile];
                }

                if (GridManager.TryGetTileByPos(currentTile.ToVector2(), out GameObject startTile))
                {
                    path.Insert(0, startTile);
                    Debug.Log($"Path: Added start tile {startTile.transform.position}");
                }
                break;
            }

            foreach (var dir in directions)
            {
                Vector3 neighbor = currentTile + dir;

                Debug.Log($"Checking neighbor: {neighbor} from current tile: {currentTile}");

                if (!visitedTiles.Contains(neighbor) && IsTileWithinReach(neighbor))
                {
                    if (GridManager.TryGetTileByPos(neighbor.ToVector2(), out GameObject nextTile))
                    {
                        if (nextTile != null)
                        {
                            TileBase nextTileBase = nextTile.GetComponent<TileBase>();

                            if (nextTileBase != null && _allowedTileLayers.HasFlag(nextTileBase.tileLayer))
                            {
                                float heightDifference = Mathf.Abs(currentTile.y - nextTile.transform.position.y);
                                Debug.Log($"Height difference to {neighbor}: {heightDifference}");

                                // If height difference is lower than step height, it is considered valid
                                if (heightDifference <= _stepHeight)
                                {
                                    queue.Enqueue(neighbor);
                                    visitedTiles.Add(neighbor);
                                    parentTiles[neighbor] = currentTile;
                                }
                            }
                        }
                    }
                }
            }
        }

        if (path.Count == 0)
        {
            Debug.LogWarning($"No path found from {startPos} to {targetPos}!");
        }

        return path;
    }

    protected virtual IEnumerator MoveThroughTiles(List<GameObject> path)
    {
        animationStateMachine.Run();

        if (path.Count == 1)
        {
            Debug.Log("Clicked on the unit's starting tile, nothing happens");
            yield return null;
        }

        for (int i = 0; i < path.Count; i++)
        {
            var tile = path[i];
            if (tile == null)
            {
                continue;
            }

            if (tile.transform == null)
            {
                continue;
            }

            ///
            /// Make sure the unit can move before taking off the turn action
            ///
            TurnActionsLeft--;


            Vector3 targetPosition = new Vector3(tile.transform.position.x, tile.transform.position.y, tile.transform.position.z);

            float moveDuration = 0.5f;

            transform.DOMove(targetPosition, moveDuration)
                .SetEase(Ease.Linear)
                .OnStart(() =>
                {
                    if (tile.TryGetComponent(out TileBase tileBase))
                    {
                        CurrentTile.AssignUnit_ClientRPC(this);
                        CurrentTile = tileBase;
                        CurrentTile.DeAssignUnit_ClientRPC(this);
                    }

                    Vector3 direction = (targetPosition - transform.position).normalized;
                    float targetYRotation = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg;
                    transform.rotation = Quaternion.Euler(0, targetYRotation, 0);
                })
                .OnComplete(() =>
                {
                    transform.rotation = Quaternion.Euler(0, transform.rotation.eulerAngles.y, 0);

                    GridManager.Discover3X3(currentTilePos.ToVector2());
                });

            yield return new WaitForSeconds(moveDuration);
        }

        // Ensure the unit reaches the last tile
        var lastTile = path[path.Count - 1];
        if (lastTile != null && lastTile.transform != null)
        {
            Vector3 finalPosition = new Vector3(lastTile.transform.position.x, lastTile.transform.position.y, lastTile.transform.position.z);
            transform.position = finalPosition;
            Debug.Log($"Unit reached the final tile at position {finalPosition}");
        }

        animationStateMachine.Idle();
    }
    #endregion

    public void OnSpawnUnit_OnServer(ulong clientNetworkId, int playerGameId)
    {
        NetworkObject.SpawnWithOwnership(clientNetworkId, true);
    }

    public override void OnNetworkSpawn()
    {
        ownerPlayerGameId = ClientManager.GetClientGameId(NetworkObject.OwnerClientId);
        if (IsServer)
        {
            OnSpawn_ClientRPC(ownerPlayerGameId);
        }
    }

    [ClientRpc(RequireOwnership = false)]
    private void OnSpawn_ClientRPC(int ownerPlayerGameId)
    {
        colorRenderer.material = UnitSpawnHandler.GetTeamColorMaterial_OnServer(ownerPlayerGameId, unitId);

        UnitSpawnHandler.SpawnUnitHead(headTransform, ownerPlayerGameId, unitId);
    }
}

