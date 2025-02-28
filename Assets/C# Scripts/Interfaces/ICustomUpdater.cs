using Unity.Burst;

/// <summary>
/// Dont forget to call "CustomUpdateManager.AddUpdater(this)"
/// </summary>
public interface ICustomUpdater
{
    public bool RequireUpdate { get; }

    [BurstCompile]
    public void OnUpdate();
}
