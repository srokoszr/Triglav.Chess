using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using Triglav.Chess.Engine.Core;
using Triglav.Chess.Engine.Model.Serializable;
using Triglav.Chess.Engine.Persistence;
using Triglav.Chess.Model.Enumerations;

namespace Triglav.Chess.Engine.Notation
{
    public static class PgnWriter
    {
        public static IReadOnlyDictionary<String, String> Headers(GameDataSerializable game)
        {
            ArgumentNullException.ThrowIfNull(game);
            PositionFile.Validate(game.StartingPosition);
            if (game.Moves == null)
            {
                throw new PgnException(PgnDiagnosticCode.InvalidMetadata);
            }
            PgnMetadata metadata = game.Metadata ?? new PgnMetadata
            {
                White = game.WhitePlayer == PlayerType.Triglav ? "Triglav" : "?",
                Black = game.BlackPlayer == PlayerType.Triglav ? "Triglav" : "?",
            };
            if (!PgnProfile.ValidDate(metadata.Date) || metadata.ExtraTags == null)
            {
                throw new PgnException(PgnDiagnosticCode.InvalidMetadata);
            }
            Dictionary<String, String> tags = new(StringComparer.Ordinal)
            {
                ["Event"] = metadata.Event, ["Site"] = metadata.Site, ["Date"] = metadata.Date,
                ["Round"] = metadata.Round, ["White"] = metadata.White, ["Black"] = metadata.Black,
                ["Result"] = PgnProfile.Score(game.Result), ["Variant"] = PgnProfile.Variant,
                ["TriglavVariantVersion"] = PgnProfile.Version, ["TriglavResult"] = PgnProfile.Reason(game.Result),
                ["WhiteType"] = game.WhitePlayer == PlayerType.Triglav ? "program" : "human",
                ["BlackType"] = game.BlackPlayer == PlayerType.Triglav ? "program" : "human",
            };
            Position initial = Position.GetPosition(game.StartingPosition);
            if (PgnProfile.IsSetUp(initial))
            {
                tags.Add("SetUp", "1");
                tags.Add("FEN", initial.FEN_Notation);
            }
            tags.Add("PlyCount", game.Moves.Count.ToString(CultureInfo.InvariantCulture));
            foreach (KeyValuePair<String, String> extra in metadata.ExtraTags.OrderBy(static pair => pair.Key, StringComparer.Ordinal))
            {
                if (PgnProfile.ReservedTags.Contains(extra.Key))
                {
                    throw new PgnException(PgnDiagnosticCode.InvalidMetadata, token: extra.Key);
                }
                tags.Add(extra.Key, extra.Value);
            }
            foreach (KeyValuePair<String, String> tag in tags)
            {
                PgnProfile.ValidateTag(tag.Key, tag.Value);
            }
            return tags;
        }

        public static String Write(GameDataSerializable game)
        {
            IReadOnlyDictionary<String, String> headers = Headers(game);
            List<String> tokens = new();
            try
            {
                _ = GameReplay.Replay(game, (position, move, ply) =>
                {
                    String number = position.FullMoveCounter.ToString(CultureInfo.InvariantCulture);
                    if (position.OnMove == Colours.White)
                    {
                        tokens.Add(number + ".");
                    }
                    else if (ply == 0)
                    {
                        tokens.Add(number + "...");
                    }
                    tokens.Add(position.Clone().MoveDescriptive(move).AlgebraicNotation);
                });
            }
            catch (GameReplayException error)
            {
                PgnDiagnosticCode code = error.Error switch
                {
                    GameReplayError.IllegalMove => PgnDiagnosticCode.IllegalMove,
                    GameReplayError.MovesAfterEnd => PgnDiagnosticCode.MovesAfterEnd,
                    GameReplayError.CounterLimit => PgnDiagnosticCode.LimitExceeded,
                    _ => PgnDiagnosticCode.SnapshotMismatch,
                };
                throw new PgnException(code, ply: error.Ply);
            }
            tokens.Add(headers["Result"]);
            StringBuilder output = new();
            foreach (KeyValuePair<String, String> tag in headers)
            {
                output.Append(PgnProfile.Tag(tag.Key, tag.Value)).Append('\n');
            }
            output.Append('\n');
            Int32 width = 0;
            foreach (String token in tokens)
            {
                if (width > 0 && width + 1 + token.Length > 79)
                {
                    output.Append('\n');
                    width = 0;
                }
                if (width > 0)
                {
                    output.Append(' ');
                    width++;
                }
                output.Append(token);
                width += token.Length;
            }
            return output.Append("\n\n").ToString();
        }
    }
}
