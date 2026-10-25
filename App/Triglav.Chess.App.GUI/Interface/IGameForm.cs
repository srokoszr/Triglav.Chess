using System;

namespace Triglav.Chess.App.GUI.Interface
{
    public interface IGameForm
    {
        void SetLabelText(String yourMove);

        void ShowPersistenceError(Exception error, String path = null);

        void ShowEngineError(Exception error);
    }
}
