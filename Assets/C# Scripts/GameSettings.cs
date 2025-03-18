



/// <summary>
/// static class that holds settings that are unchangable by players playing the game (Constants "const")
/// </summary>
public static class GameSettings
{
    public const int maxPlayers = 4;
    public const int maxTeams = 4;
    public const int maxPlayersPerTeam = 2;

    public const ulong UnAssignedPlayerId = maxPlayers + 1;



    public const int updateManagerListPreSizeCapacity = 4;

    //foods
    public const int wheatFoodSize = 4;
    public const int fishFoodSize = 3;
}
