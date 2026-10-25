using System;
using Triglav.Chess.App.GUI.Model;
using Triglav.Chess.Engine.Core;
using Triglav.Chess.Engine.Model.Serializable;

namespace Triglav.Chess.App.GUI.Interface
{
    public interface IGameEngine : IBoardClickHandler, IDisposable
    {
        event EvaluationChangedHandler OnEvaluationChanged;

        event EvaluationChangedHandler OnMinMaxEvaluationChanged;

        event EvaluationChangedHandler OnWinChanceEvaluationChanged;

        EvaluationWrapper CurrentEaluation { get; }

        EvaluationWrapper MinMaxEaluation { get; }

        GameDataSerializable GameData { get; }

        String GameFilePath { get; }

        Position GetPositionSnapshot();

        Boolean SaveGame(String path = null);

        Boolean ExportPgn(String path);

        IGameForm MainForm { get; set; }

        void Resign();

        void DrawOffer();
    }
}
