


[System.Serializable]
public struct MatchSettings
{
    public int maxPlayers;
    public int maxTeams;
    public int maxPlayersPerTeam;

    public bool allowUnfairTeams;


    public MatchSettings(bool _allowUnfairTeams)
    {
        maxPlayers = 4;
        maxTeams = 4;
        maxPlayersPerTeam = 4;
        allowUnfairTeams = _allowUnfairTeams;
    }
}
