# Analysis example

`AnalysisExample.TgGame` is a byte-for-byte copy of the user's supplied
`game-2026-10-18_19-26-19-7c7ecf281906477e9b8d3b6188ffae3d.TgGame`.
It ships with the GUI so **Analysis → Game analysis → Load example game** works
without depending on a machine-specific Documents path. Tests use this same file
to verify every forward/backward replay position and source-file preservation.
It contains 61 plies (ending after White's 31st move), with White winning by checkmate.

This is a UI/replay example. No engine evaluation or game annotation is generated.
