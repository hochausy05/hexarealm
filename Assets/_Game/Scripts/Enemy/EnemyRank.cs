namespace HexaRealm.Enemy
{
    /// <summary>
    /// Authoring rank for regular enemies, ordered from the lowest rank to the highest.
    /// Power-budget code validates this declared value; it never derives or mutates it.
    /// </summary>
    public enum EnemyRank
    {
        F = 0,
        E = 1,
        D = 2,
        C = 3,
        B = 4,
        A = 5,
        S = 6
    }
}
