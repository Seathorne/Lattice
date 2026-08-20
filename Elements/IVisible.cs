namespace Lattice.Elements;

public interface IVisible
{
    bool IsVisible { get; }

    void Hide();

    void Show();
}