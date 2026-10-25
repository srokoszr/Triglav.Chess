namespace Triglav.Chess.Model.Enumerations
{
    public enum EvaluationResult : sbyte
    {
        DrawOrUknown = 0,

        WhiteWinStalemate = 1,
        WhiteWinNakedMate = 2,
        WhiteWinMate = 3,

        BlackWinStalemate = -1,
        BlackWinNakedMate = -2,
        BlackWinMate = -3,
    }
}
