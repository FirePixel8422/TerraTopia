using DG.Tweening;
using Unity.Netcode;
using UnityEngine;
using System.Collections.Generic;
using System.Collections;
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
    [SerializeField] private TileLayer _allowedTileLayers;
    [SerializeField] private GameObject _markerPrefab;
    [SerializeField] private float _stepHeight = 0.5f;

    [Header("Combat based variables")]
    [SerializeField] private int _attackDamage;

    [Header("Achievements")]
    [SerializeField] private int _unitsKilled;
    [SerializeField] private int tilesVisited;

    public TileBase CurrentTile;

    private List<GameObject> _tileMarkers = new List<GameObject>();

    public void TakeDamage(int damageToTake)
    {
        _health -= damageToTake;
        if (_health <= 0)
        {
            Die();
        }
    }

    public override void OnClick()
    {
        if (_markerPrefab)
        {
            PlaceMarkersOnReachableTiles();
        }
    }

    public override void OnDifferentClickableClicked(GameObject newlyClickedObject)
    {
        if (newlyClickedObject.TryGetComponent(out TileBase TB))
        {
            if (IsTileWithinReach(newlyClickedObject.transform.position))
            {
                MoveToTile(newlyClickedObject);
            }
        }
        ClearMarkers();
    }

    Vector3 unitPos => new Vector3(transform.position.x, 0, transform.position.z);
    private Vector3 currentTilePos
    {
        get
        {
            return CurrentTile != null ? new Vector3(CurrentTile.transform.position.x, CurrentTile.transform.position.y, CurrentTile.transform.position.z) : transform.position;
        }
    }

    public bool IsTileWithinReach(Vector3 tilePos)
    {
        tilePos.y = 0f;
        int xDistance = Mathf.Abs((int)(currentTilePos.x - tilePos.x));
        int zDistance = Mathf.Abs((int)(currentTilePos.z - tilePos.z));
        return Mathf.Max(xDistance, zDistance) <= _movementRange;
    }

    private void ClearMarkers()
    {
        foreach (var marker in _tileMarkers)
        {
            Destroy(marker.gameObject);
        }
        _tileMarkers.Clear();
    }

    private void PlaceMarkersOnReachableTiles()
    {
        ClearMarkers();

        for (int x = -_movementRange; x <= _movementRange; x++)
        {
            for (int z = -_movementRange; z <= _movementRange; z++)
            {
                int xDistance = Mathf.Abs(x);
                int zDistance = Mathf.Abs(z);

                if (xDistance + zDistance <= _movementRange)
                {
                    Vector3 targetPos = unitPos + new Vector3(x, 0f, z);

                    if (GridManager.TryGetTileByPos(targetPos.ToVector2(), out TileBase tb))
                    {
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

    private void MoveToTile(GameObject targetTile)
    {
        if (targetTile == null) return;
        List<GameObject> path = FindPathToTile(targetTile);

        if (path != null && path.Count > 0)
        {
            MoveAlongPath(path);
        }
    }

    private void MoveAlongPath(List<GameObject> path)
    {
        if (path == null || path.Count == 0) return;

        StartCoroutine(MoveThroughTiles(path));
    }

    private IEnumerator MoveThroughTiles(List<GameObject> path)
    {
        foreach (var tile in path)
        {
            if (tile == null)
            {
                continue;
            }

            if (tile.transform == null)
            {
                continue;
            }

            Vector3 targetPosition = new Vector3(tile.transform.position.x, tile.transform.position.y, tile.transform.position.z);

            float moveDuration = 0.5f;

            transform.DOMove(targetPosition, moveDuration)
                .SetEase(Ease.Linear)
                .OnStart(() =>
                {
                    Vector3 direction = (targetPosition - transform.position).normalized;
                    float targetYRotation = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg;
                    transform.rotation = Quaternion.Euler(0, targetYRotation, 0);
                })
                .OnComplete(() =>
                {
                    if (tile.TryGetComponent(out TileBase tileBase))
                    {
                        CurrentTile.DeAssignUnit(this);
                        CurrentTile = tileBase;
                        CurrentTile.AssignUnit(this);
                    }

                    transform.rotation = Quaternion.Euler(0, transform.rotation.eulerAngles.y, 0);

                    GridManager.Discover3X3(currentTilePos.ToVector2());
                });
            yield return new WaitForSeconds(moveDuration);

        }
    }

    //Uses BFS(Breadth First Search)
    private List<GameObject> FindPathToTile(GameObject targetTile)
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

                                // If height difference is lower thein stepheight it is considered valid
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
    }

    private void Die()
    {
        CurrentTile.DeAssignUnit(this);
        Destroy(gameObject);
    }
}
