using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Triglav.Chess.App.GUI.Control;
using Triglav.Chess.App.GUI.Interface;
using Triglav.Chess.App.GUI.Model;
using Triglav.Chess.App.GUI.Properties;
using Triglav.Chess.Engine.Core;
using Triglav.Chess.Engine.Model.Serializable;
using Triglav.Chess.Engine.Persistence;
using Triglav.Chess.Model.Enumerations;
using Triglav.Chess.Model.Structs;
using Triglav.Chess.Model.Structs.Interoperable;

namespace Triglav.Chess.App.GUI
{
    public class StandardGameEngine : IGameEngine
    {
        public StandardGameEngine(GameOptions options, BoardControl boardControl, EvaluationBar evaluationBar, IGameForm gameForm, String gameFilePath, Position positionToPlay = null, GameDataSerializable gameData = null)
        {
            this.PositionEvaluator = PositionEvaluator.GetDefault();
            this.Options = options;
            this.BoardControl = boardControl;
            this.EvaluationBar = evaluationBar;
            this.MainForm = gameForm;
            this.GameFilePath = gameFilePath;
            positionToPlay ??= Position.GetStartingPosition();
            this.PositionStack = PositionStackWrapper.New(positionToPlay.OnMove);
            this.Position = positionToPlay;
            this.UpdateBoardOrientation();
            this.IsPlayerMove = options.IsHuman(positionToPlay.OnMove);
            this.BoardControl.SetState(this.Position);

            this.GameData = gameData ?? GameDataSerializable.Create(this.Position,
                options.IsHuman(Colours.White) ? PlayerType.Player : PlayerType.Triglav,
                options.IsHuman(Colours.Black) ? PlayerType.Player : PlayerType.Triglav);

            EvaluationWrapper initialTerminal = this.PositionEvaluator.EvaluateTerminalPosition(this.Position, this.PositionStack, 0);
            if (initialTerminal.IsTerminal)
            {
                this.CompleteGame(initialTerminal);
            }
            else
            {
                this.SaveRecoveryPgn();
                if (!this.IsPlayerMove)
                {
                    this.EngineMove();
                }
            }
        }

        private volatile bool isEndOfGame = false;

        public PositionEvaluator PositionEvaluator
        {
            get;
        }

        public GameOptions Options
        {
            get;
        }

        public BoardControl BoardControl
        {
            get;
        }

        public EvaluationBar EvaluationBar
        {
            get;
        }

        public IGameForm MainForm
        {
            get; set;
        }

        public PositionStackWrapper PositionStack
        {
            get;
        }

        public Position Position
        {
            get; private set;
        }

        private EvaluationWrapper _currentEaluation;

        public EvaluationWrapper CurrentEaluation
        {
            get
            {
                return this._currentEaluation;
            }
            set
            {
                this._currentEaluation = value;
                this.OnEvaluationChanged?.Invoke();
            }
        }

        private EvaluationWrapper _minMaxEaluation;

        public EvaluationWrapper MinMaxEaluation
        {
            get
            {
                return this._minMaxEaluation;
            }
            set
            {
                this._minMaxEaluation = value;
                this.OnMinMaxEvaluationChanged();
                this.OnWinChanceEvaluationChanged();
            }
        }

        public event EvaluationChangedHandler OnEvaluationChanged;

        public event EvaluationChangedHandler OnMinMaxEvaluationChanged;

        public event EvaluationChangedHandler OnWinChanceEvaluationChanged;

        private volatile bool isPlayerMove;

        public bool IsPlayerMove
        {
            get
            {
                return this.isPlayerMove;
            }
            private set
            {
                if (this.isEndOfGame)
                {
                    this.MainForm.SetLabelText(Resources.GameFinished);
                }
                else if (this.Options.PlayWithFriend)
                {
                    this.MainForm.SetLabelText(this.Position.OnMove == Colours.White ? Resources.AnalysisWhiteToMove : Resources.AnalysisBlackToMove);
                }
                else
                {
                    this.MainForm.SetLabelText(value ? Resources.YourMove : Resources.EngineIsThinking);
                }
                this.isPlayerMove = value;
            }
        }

        private MoveStruct? savedLastMove;

        private Coordinates? lastClick;

        private Coordinates? LastClick
        {
            get
            {
                return this.lastClick;
            }
            set
            {
                if (value != null)
                {
                    var moves = this.Position.GetMoves().Where(x => x.MoveStruct.Move.From == value.Value.SingleCoordinate).Select(x => x.MoveStruct.Move).ToArray();
                    this.BoardControl.SetState(this.Position, this.savedLastMove, value.Value, moves);
                }
                else
                {
                    if (this.savedLastMove.HasValue)
                    {
                        this.BoardControl.SetState(this.Position, this.savedLastMove.Value);
                    }
                    else
                    {
                        this.BoardControl.SetState(this.Position);
                    }
                }
                this.lastClick = value;
            }
        }

        private readonly PgnRecoveryFile recoveryPgn = new(AppContext.BaseDirectory);

        public String GameFilePath
        {
            get; private set;
        }

        public GameDataSerializable GameData
        {
            get; private set;
        }

        // Game state is committed on the UI thread, including engine moves.
        public Position GetPositionSnapshot() => this.Position.Clone();

        public Boolean SaveGame(String path = null)
        {
            Boolean saved = false;
            try
            {
                String destination = path ?? this.GameFilePath;
                GameFile.Save(destination, this.GameData);
                this.GameFilePath = destination;
                saved = true;
            }
            catch (Exception error) when (PositionFileDialogs.IsFileError(error))
            {
                this.MainForm.ShowPersistenceError(error);
            }
            // Attempt recovery even when the chosen binary destination is unavailable.
            this.SaveRecoveryPgn();
            return saved;
        }

        private void SaveRecoveryPgn()
        {
            try
            {
                this.recoveryPgn.Save(this.GameData);
            }
            catch (Exception error) when (PositionFileDialogs.IsFileError(error))
            {
                this.MainForm.ShowPersistenceError(error, this.recoveryPgn.FilePath);
            }
        }

        public Boolean ExportPgn(String path)
        {
            try
            {
                GameFile.ExportPgn(path, this.GameData);
                return true;
            }
            catch (Exception error) when (PositionFileDialogs.IsFileError(error))
            {
                this.MainForm.ShowPersistenceError(error);
                return false;
            }
        }

        public void ClickMade(Coordinates coordinates)
        {
            if (!this.IsPlayerMove)
            {
                return;
            }

            var moves = this.Position.GetMoves();
            if (this.LastClick.HasValue)
            {
                var move = new MoveStruct(this.LastClick.Value.SingleCoordinate, coordinates.SingleCoordinate);
                if (moves.Any(x => x.MoveStruct.Move == move))
                {
                    this.LastClick = null;
                    this.IsPlayerMove = false;

                    if (this.MakeMove(move))
                    {
                        if (this.Options.PlayWithFriend)
                        {
                            this.IsPlayerMove = true;
                        }
                        else
                        {
                            this.EngineMove();
                        }
                    }
                }
                else
                {
                    if (moves.Any(x => x.MoveStruct.Move.From == coordinates.SingleCoordinate) && this.LastClick.Value != coordinates)
                    {
                        this.LastClick = coordinates;
                    }
                    else
                    {
                        this.LastClick = null;
                    }
                }
            }
            else
            {
                if (moves.Any(x => x.MoveStruct.Move.From == coordinates.SingleCoordinate))
                {
                    this.LastClick = coordinates;
                }
            }
        }

        public void EngineMove()
        {
            if (this.Options.PlayWithFriend || this.isEndOfGame || this.disposedValue)
            {
                return;
            }
            EvaluationWrapper terminal = this.PositionEvaluator.EvaluateTerminalPosition(this.Position, this.PositionStack, 0);
            if (terminal.IsTerminal)
            {
                this.CompleteGame(terminal);
                return;
            }
            var thread = new Thread(() => this.EngineMoveThread());
            thread.Start();
        }

        private void EngineMoveThread()
        {
            try
            {
                var algorithm = new Algorithm(this.Options.Level, this.Options.HumanMode);
                var (evaluation, move) = algorithm.GetMove(this.Position, this.PositionStack);
                if (this.disposedValue)
                {
                    return;
                }

                this.BoardControl.Invoke(() =>
                {
                    if (this.disposedValue || this.isEndOfGame)
                    {
                        return;
                    }
                    if (this.MakeMove(move.Move))
                    {
                        this.MinMaxEaluation = evaluation;
                        this.IsPlayerMove = true;
                    }
                });
            }
            catch (Exception error)
            {
                if (!this.disposedValue)
                {
                    this.BoardControl.Invoke(() => this.MainForm.ShowEngineError(error));
                }
            }
        }

        private void UpdateBoardOrientation()
        {
            Boolean flip = (this.Options.PlayWithFriend ? this.Position.OnMove : this.Options.ColourToPlay) == Colours.Black;
            this.BoardControl.FlipBoard = flip;
            this.EvaluationBar.FlipBoard = flip;
        }

        /// <summary>
        /// Makes move on the board
        /// </summary>
        /// <param name="move">move to be made</param>
        /// <returns>true if game continues, false when the move ends the game</returns>
        private bool MakeMove(MoveStruct move)
        {
            if (this.isEndOfGame)
            {
                return false;
            }
            EvaluationWrapper beforeMove = this.PositionEvaluator.EvaluateTerminalPosition(this.Position, this.PositionStack, 0);
            if (beforeMove.IsTerminal)
            {
                this.CompleteGame(beforeMove);
                return false;
            }
            if (!move.IsValid || !this.Position.GetMoves().Any(candidate => candidate.MoveStruct.Move == move))
            {
                throw new InvalidOperationException(Resources.InvalidEngineMove);
            }
            this.savedLastMove = move;
            this.PositionStack.Push(this.Position.State);
            this.Position.Move(move);
            this.UpdateBoardOrientation();
            this.BoardControl.SetState(this.Position, move);

            this.GameData.AddMove(move, this.Position.PositionDataSerializable);

            var terminal = this.PositionEvaluator.EvaluateTerminalPosition(this.Position, this.PositionStack, (Byte)Math.Min(this.PositionStack.Count, Byte.MaxValue));
            if (terminal.IsTerminal)
            {
                this.CompleteGame(terminal);
                return false;
            }

            var nonTerminal = this.PositionEvaluator.EvaluateNonTerminalPosition(this.Position, (byte)this.PositionStack.Count);
            this.SaveGame();
            this.CurrentEaluation = nonTerminal;

            return true;
        }

        private void CompleteGame(EvaluationWrapper terminal)
        {
            this.isEndOfGame = true;
            this.isPlayerMove = false;
            this.LastClick = null;
            this.MainForm.SetLabelText(Resources.GameFinished);

            switch (terminal.Result)
            {
                case EvaluationResult.DrawOrUknown:
                    {
                        if (!this.PositionStack.IsThreefoldRepetition(this.Position))
                        {
                            this.GameData.Result = GameResult.Draw;
                        }
                        else
                        {
                            this.GameData.Result = GameResult.DrawThreefoldRepetition;
                        }
                        break;
                    }
                case EvaluationResult.WhiteWinStalemate:
                    {
                        this.GameData.Result = GameResult.WhiteWinStalemate;
                        break;
                    }
                case EvaluationResult.WhiteWinNakedMate:
                    {
                        this.GameData.Result = GameResult.WhiteWinNakedMate;
                        break;
                    }
                case EvaluationResult.WhiteWinMate:
                    {
                        this.GameData.Result = GameResult.WhiteWinMate;
                        break;
                    }
                case EvaluationResult.BlackWinStalemate:
                    {
                        this.GameData.Result = GameResult.BlackWinStalemate;
                        break;
                    }
                case EvaluationResult.BlackWinNakedMate:
                    {
                        this.GameData.Result = GameResult.BlackWinNakedMate;
                        break;
                    }
                case EvaluationResult.BlackWinMate:
                    {
                        this.GameData.Result = GameResult.BlackWinMate;
                        break;
                    }
            }

            _ = this.SaveGame();
            this.CurrentEaluation = terminal;
        }

        public void Resign()
        {
            if (this.isEndOfGame)
            {
                return;
            }
            if ((this.Options.PlayWithFriend ? this.Position.OnMove : this.Options.ColourToPlay) == Colours.White)
            {
                this.GameData.Result = GameResult.BlackWinResign;
            }
            else
            {
                this.GameData.Result = GameResult.WhiteWinResign;
            }
            this.isEndOfGame = true;
            this.IsPlayerMove = false;
            this.SaveGame();
        }

        public void DrawOffer() => throw new NotImplementedException();

        private bool disposedValue;

        protected virtual void Dispose(bool disposing)
        {
            this.disposedValue = true;
        }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            this.Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
