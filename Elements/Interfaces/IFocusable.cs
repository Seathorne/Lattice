public interface IFocusable
{
    bool IsFocusable { get; set; }

    bool IsFocused { get; }

    void OnFocusGained();

    void OnFocusLost();
}