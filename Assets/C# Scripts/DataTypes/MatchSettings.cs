


[System.Serializable]
public struct MatchSettings
{
    public int maxClients;
    public int maxTeams;
    public int maxClientsPerTeam;

    public bool allowUnfairTeams;


    public MatchSettings(bool _allowUnfairTeams)
    {
        maxClients = GameSettings.maxPlayers;
        maxTeams = GameSettings.maxTeams;
        maxClientsPerTeam = GameSettings.maxPlayersPerTeam;

        allowUnfairTeams = _allowUnfairTeams;
    }
}
