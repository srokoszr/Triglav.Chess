# GUI languages

Application-owned UI text comes from `App/Triglav.Chess.App.GUI/Properties/Resources.resx` (English) and `Resources.pl.resx` (Polish). Both catalogues have the same 131 nonempty string keys. The GUI declares English as its neutral language; Polish UI cultures, including `pl-PL`, load the Polish satellite resources. Other languages fall back to English.

The GUI follows the process UI culture inherited from the system when forms are created. There is no in-app language selector. File-format extensions, chess notation, folder names and internal control identifiers are language-independent.

Game import summaries, native replay diagnostics and counter-reconstruction warnings, generic win results, and all structured PGN diagnostics are included. Resource coverage includes form titles and designer labels, editor controls, engine levels, menus, save/open dialog titles and filters, board coordinates, status text, evaluations, game results and application-owned error messages. The GUI formats result enums through localized text instead of displaying their code names. File and engine errors show localized messages; full exception details go to the diagnostic trace.

When adding a key, provide both English and Polish values, preserve format placeholders, and refresh the strongly typed resource accessors in `Resources.Designer.cs`. `GuiResourceTests` checks key parity, nonempty values, placeholders, encoding and culture fallback. A compiled-GUI resource audit also verified all 131 accessors and every game-result mapping in English and Polish. Layout at different display scales and longer translated labels still needs an interactive visual check.
