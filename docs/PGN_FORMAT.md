# Triglav medieval PGN profile, version 1

This is the implemented F03 interchange profile. It records this project's medieval rules using PGN-style headers and SAN. It is not a standard-chess importer, and conventional chess tools may reject the moves or the medieval FEN fields.

## Export and metadata

Manual export and automatic recovery saving use the same `PgnWriter`. Output is UTF-8 without a BOM, with LF line endings, deterministic tag order, and movetext wrapped between tokens at 79 characters. Empty games also end with a result marker. Header strings escape quotes and backslashes; control characters, invalid Unicode, invalid tag names and values longer than 255 UTF-16 code units are rejected.

The writer emits Event, Site, Date, Round, White, Black and Result, followed by project/setup/player-type information, PlyCount and sorted supplemental headers. Unknown fields use `?`; the unknown date is `????.??.??`. Known game dates are captured once when a GUI game is created, not on every autosave. Valid partial dates, such as `2026.??.??`, are preserved. Engine names are recorded as `Triglav`; unknown human names remain unknown. Player type is stored as `human` or `program`.

All score tokens, tag names, project codes and SAN are independent of the UI language. Names may contain Polish characters. The baseline notation reference is the [PGN specification](https://www.saremba.de/chessgml/standards/pgn/pgn-complete.htm); the UTF-8 and medieval conventions below define this project's additional profile.

## Variant and initial position

Exports always identify the rules:

```text
[Variant "Triglav-Medieval"]
[TriglavVariantVersion "1"]
```

Import requires that variant, or an explicit option to interpret an unmarked file as Triglav medieval chess. An explicitly different variant is rejected even when that option is selected. An omitted version defaults to 1 on import; an explicitly unsupported version is rejected.

`SetUp "1"` and `FEN` are emitted whenever the board, first-move rights, side to move, halfmove clock or full-move counter differs from the default starting position. The FEN format is described in [PERSISTENCE.md](PERSISTENCE.md). Its third field describes king/queen first-move rights. Its fourth field records moved extra queens instead of en passant. This remains a six-field format.

On import, FEN requires SetUp=1 and SetUp=1 requires FEN. SetUp=0 is allowed without FEN. The initial full-move number is respected, including a Black start, for example:

```text
12... Nf6 13. Nf3 *
```

Each recorded move contributes one to PlyCount, regardless of its starting move number. SAN is resolved against the engine's legal moves. Capture notation, first-move leaps, queen-only promotion (`=Q`), disambiguation, and check/checkmate suffixes must agree with the generated notation. Queens with different first-move rights are still competitors for SAN disambiguation. `#` denotes actual checkmate, not every terminal position that happens to involve check.

## Results

The Result header and movetext marker use the same ASCII score. `*` means an unknown/unfinished or interrupted outcome; it is not itself a claim that the board is nonterminal. A result without an identified reason stays generic unless replay proves a specific terminal result. The importer never turns an otherwise unexplained win into resignation or checkmate.

`TriglavResult` preserves the more specific local outcome:

| Code | PGN score | Meaning |
| --- | --- | --- |
| `unfinished` | `*` | Unknown/unfinished outcome. |
| `error` | `*` | Interrupted by an error. |
| `draw` | `1/2-1/2` | General draw, including double bare king. |
| `draw-agreed` | `1/2-1/2` | Agreed draw. |
| `draw-repetition` | `1/2-1/2` | Threefold repetition. |
| `white-win`, `black-win` | `1-0`, `0-1` | Winner known; reason unspecified. |
| `white-checkmate`, `black-checkmate` | `1-0`, `0-1` | Checkmate. |
| `white-stalemate`, `black-stalemate` | `1-0`, `0-1` | Stalemate victory under the medieval rules. |
| `white-bare-king`, `black-bare-king` | `1-0`, `0-1` | Victory by the bare-king rule. |
| `white-resignation`, `black-resignation` | `1-0`, `0-1` | Victory by resignation. |

Unknown reason codes and inconsistent header/body scores are errors. A claimed checkmate, stalemate, bare-king victory or repetition must be supported by the final position/history. When replay proves a terminal outcome, an explicit decisive/draw score must agree with it. No moves are accepted after an already terminal position.

The exporter verifies legal moves and their recorded snapshots on a temporary position. It does not mutate or adjudicate the supplied game result. Import performs the result/history consistency checks described above.

## Supported import subset

Use **Import Game…** in the main window, which accepts native `.TgGame` and `.PGN` equally. Select a PGN file, choose either explicit compatibility option if needed, and read the game. PGN compatibility options are disabled for native files. The summary displays names, result, recorded plies and warnings. The final position can be opened in the editor. Playing it starts a new game; this screen does not resume the imported game or provide game analysis.

The reader supports one game at a time, canonical medieval SAN, move numbers, quoted/escaped headers and results. Move numbers may be omitted; supplied numbers and one/three-dot indications must match the side and counter. Optional metadata headers may be omitted and receive unknown values. A strict import requires the final movetext marker; a supplied Result header must match it. UTF-8 with an optional BOM is accepted, as are CRLF/CR line endings and ordinary whitespace.

Brace and semicolon comments are consumed, with an explicit warning that they are discarded. Unknown supplemental headers are preserved. Computed/reserved headers cannot be overridden through supplemental metadata, and incorrectly cased reserved names are rejected.

Variations, NAGs, `!`/`?` annotations, castling, en passant, underpromotion, percent escape lines and multiple-game files are outside this subset. Unsupported constructs and illegal/ambiguous moves produce an error rather than a partially applied game. Other legacy character encodings are not guessed.

Default resource limits are 16 MiB of input, 20,000 plies, 256 headers and 255 UTF-16 code units per quoted value or token. Byte/ply limits are configurable in `PgnImportOptions`. Moves that would overflow the position counters are rejected. Structured diagnostics carry a code, source line/column and ply/token when applicable; zero indicates a file-level or unavailable location. The GUI translates all error/warning descriptions into English and Polish.

## Explicit legacy Triglav mode

The legacy checkbox enables only known historical repairs:

- Accept an unmarked file as this variant, with a warning.
- Read unquoted/empty legacy Result headers; normalize en-dash score separators.
- Convert the historical unknown date `?` into `????.??.??`.
- Accept a missing final marker. Use a supplied valid score, otherwise derive a provable terminal result or retain `*`.
- Repair a historical PlyCount offset, with a warning.

Warnings identify the repairs. Contradictory scores, unsupported rules and invalid/duplicated moves are still errors. The mode does not repair the old Black-first writer's duplicated move.

The unchanged `BlackRc5Threefold.PGN` regression fixture is accepted in legacy mode. Replay reconstructs all 124 plies, proves repetition after `62...Rc5`, and produces canonical PGN that strict mode can read again.

## Binary compatibility and history ownership

`GameDataSerializable` adds optional metadata at protobuf field 6; existing fields 1–5 retain their meanings. Older files without metadata still load, retaining unknown dates. Games predating counter persistence, identified by absent metadata and default counters throughout, can export by reconstructing counters through replay. A previously missing starting offset cannot be recovered.

Generic White/Black wins use new enum values 12 and 13; existing numeric outcomes are unchanged. Older applications are not guaranteed to understand these new outcomes or metadata. Current applications preserve them through binary saves and PGN round trips.

`PgnImportResult` contains managed game data, a final position and warnings. `CreateHistory()` allocates a native stack owned by its caller and must be disposed. For N moves, it contains the initial position plus the first N−1 resulting positions; the final position is excluded. For zero moves, it is empty. This matches live play's push-before-move convention and retains the starting colour in history enumeration.

## Verification and limits

`PgnDocumentTests` provides explicit expected documents/movetext and semantic round trips for results, dates/names, Black starts, custom counters, promotions, mixed queen rights, capture/check/mate notation, legacy repairs, malformed input and resource limits. File-failure tests verify that previous exports survive errors. The existing full-game repetition regression remains separate.

GUI and CLI builds and all 313 selected tests pass, including 73 focused PGN cases, 10 shared/native import cases and 4 localization tests. The import form populates native and PGN previews in both languages without displaying a window; a terminal initial GUI game also saves its result without requesting a move. No external standard-chess application compatibility or interactive GUI smoke test is claimed. Comments and annotations are not preserved, and imported-game continuation remains separate work.
