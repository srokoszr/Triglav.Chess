using Triglav.Chess.Model.Structs;

namespace Triglav.Chess.App.GUI.Interface
{
    public interface IBoardClickHandler
    {
        void ClickMade(Coordinates coordinates);
    }
}
