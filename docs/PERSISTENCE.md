# Position and game files

The editor and active game's **File → Save Position…** share the same persistence service. The main window's **Load Position** opens a successfully loaded position in the editor. A failed load leaves the current editor position unchanged.

## Positions

- `.TgPosition` is the binary protobuf position format. It stores pieces, side to move, king/queen first-move state, halfmove clock and full-move counter. Existing binary files without counters load with defaults of `0` and `1`.
- `.FEN` is UTF-8 text using six medieval FEN fields. File extensions are case-insensitive; unsupported extensions are rejected.
- Older editor versions accidentally wrote FEN text into `.TgPosition` files. These files still load, including UTF-8 files with a BOM. Saving them as `.TgPosition` writes the binary format.

### Medieval FEN

The third field records unused special first moves: `K`/`k` for the White/Black king and `Q`/`q` for the White/Black queen on its starting square. These letters describe medieval first-move rights.

Medieval chess has no en passant, so the fourth field stores a comma-separated list of **queen squares whose special first move has been used**, excluding each side's starting queen square. An unlisted queen away from its starting square retains its special first move, including a newly promoted pawn. Use `-` when no additional queen state is needed. Starting-square queen rights remain in the third field.

For example:

```text
3qk3/8/1q1Q4/8/8/1Q1q4/8/3QK3 w KQ b6,b3 17 42
```

The queens on `b6` and `b3` have moved; the queens on `d6` and `d3` retain their special first moves. White's starting king and queen retain their rights; Black's do not. The position remains six fields long. A nonempty fourth field is a medieval variant extension and should not be interpreted as a standard-chess en passant target.

Historical FEN files with `-` in the fourth field remain readable, but queen state previously omitted from those files cannot be recovered. Loading validates structure, kings, counters and state fields; it does not prove that the position can arise in a legal game.

## Games

Starting a game asks for a `.TgGame` destination, initially suggesting `Documents/Triglav Chess/Games`. The initial position is saved before the game starts. Each committed move, terminal result and resignation triggers another save. **Save Game** writes to the current destination; **Save Game As…** switches the destination after a successful save.

Closing saves again. An unfinished game's result remains `Unknown`; normal closure does not turn it into `Error`. If saving fails, the game remains open and shows the error, allowing another attempt or a different destination through Save Game As.

`.TgGame` is the primary saved game, with its initial position and successive move snapshots. The GUI also automatically maintains a recovery PGN beside its executable (`AppContext.BaseDirectory`), named `game-<timestamp>-<unique-id>.PGN`. It writes the initial game, then replaces that same file after each committed move, final result, resignation and explicit save/close. Each update is flushed before replacement, so an unexpected app crash leaves the last completed save available. Files remain after normal closure, and each new game gets a unique name. Save Game As does not change the recovery path.

The binary and recovery PGN saves are attempted independently. A failure shows an error including the recovery path when applicable; a failed recovery PGN does not prevent binary saving or closing after a successful binary save. The GUI directory must be writable for recovery saves. **Export PGN…** remains available for choosing an additional export destination. A PGN export failure does not invalidate or prevent the binary save. The shared PGN writer now preserves metadata, uses ASCII results and handles Black-to-move/custom-counter starts. **Import Game…** accepts both native `.TgGame` and `.PGN` files through the same summary and final-position preview. See [PGN format](PGN_FORMAT.md) for the supported subset and explicit legacy mode. There is no game-resume GUI yet.

### Importing games

**Import Game…** selects either supported format, validates the game, and shows names (when available), result, recorded plies and warnings. Both formats replay legal moves, reject moves after termination, reconstruct prior-position history and make the final position available in the editor. Playing that position starts a new game. A failed import leaves the previous preview intact; choosing a different file clears it.

Native import checks each saved snapshot against replay. Older native games without stored counters reconstruct them from the starting position and moves, with a warning; the source file is unchanged. Native metadata retains its own format's capabilities and is not converted through PGN. The PGN compatibility options apply only when a PGN file is selected. Input defaults to a 16 MiB byte limit and 20,000 plies for both formats, configurable through `GameImportOptions` on `GameFile.Import`.

## Threefold repetition

The live game checks the current position against the prior-position stack after each committed move and before starting another engine search. The current position is not stored in that stack: each move pushes its **pre-move** position exactly once. Comparing every second prior position preserves the side to move, while the piece bitfields preserve king and queen first-move rights. The third occurrence ends the game and saves `DrawThreefoldRepetition` without appending another move.

The regression fixture `BlackRc5Threefold.PGN` replays the complete 124-ply game where the final position occurs after `58...Rc5`, `60...Rc5`, and `62...Rc5`. It verifies the populated history, a legal search result after the second occurrence, termination at the third, and binary/recovery PGN saving with all 124 valid moves.

## Write failures and verification

All three formats are written to a unique temporary file in the destination directory. The temporary file is flushed and closed before replacing the destination. A serialization failure or locked destination leaves the previous file intact; temporary files are cleaned up when possible. This protects individual saves against partial writes, without promising recovery from every filesystem or power failure.

The persistence tests cover both sides to move, asymmetric first-move rights, multiple queens, promotion before and after its first move, counters, legacy formats, malformed input, repeated game saves, interrupted serialization and locked files. The GUI builds, but save/load dialogs and menu layout still need an interactive smoke test.
