using System.Buffers;
using Triglav.Chess.Model.Structs;

namespace Triglav.Chess.Engine.KnownPieceLocations
{
    public static class KnownBishopPositions
    {
        public static readonly SearchValues<byte> WhiteLightSquareBishopPositions = SearchValues.Create(new byte[]
        {
            new Coordinates("c1").SingleCoordinate,
            new Coordinates("g1").SingleCoordinate,
            new Coordinates("a3").SingleCoordinate,
            new Coordinates("e3").SingleCoordinate,
            new Coordinates("c5").SingleCoordinate,
            new Coordinates("g5").SingleCoordinate,
            new Coordinates("a7").SingleCoordinate,
            new Coordinates("e7").SingleCoordinate,
        });

        public static readonly SearchValues<byte> WhiteDarkSquareBishopPositions = SearchValues.Create(new byte[]
        {
            new Coordinates("f1").SingleCoordinate,
            new Coordinates("b1").SingleCoordinate,
            new Coordinates("d3").SingleCoordinate,
            new Coordinates("h3").SingleCoordinate,
            new Coordinates("b5").SingleCoordinate,
            new Coordinates("f5").SingleCoordinate,
            new Coordinates("d7").SingleCoordinate,
            new Coordinates("h7").SingleCoordinate,
        });

        public static readonly SearchValues<byte> BlackLightSquareBishopPositions = SearchValues.Create(new byte[]
        {
            new Coordinates("c8").SingleCoordinate,
            new Coordinates("g8").SingleCoordinate,
            new Coordinates("a6").SingleCoordinate,
            new Coordinates("e6").SingleCoordinate,
            new Coordinates("c4").SingleCoordinate,
            new Coordinates("g4").SingleCoordinate,
            new Coordinates("a2").SingleCoordinate,
            new Coordinates("e2").SingleCoordinate,
        });

        public static readonly SearchValues<byte> BlackDarkSquareBishopPositions = SearchValues.Create(new byte[]
        {
            new Coordinates("f8").SingleCoordinate,
            new Coordinates("b8").SingleCoordinate,
            new Coordinates("d6").SingleCoordinate,
            new Coordinates("h6").SingleCoordinate,
            new Coordinates("b4").SingleCoordinate,
            new Coordinates("f4").SingleCoordinate,
            new Coordinates("d2").SingleCoordinate,
            new Coordinates("h2").SingleCoordinate,
        });
    }
}
