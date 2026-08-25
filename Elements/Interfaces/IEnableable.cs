public interface IEnableable
{
    bool IsEnableable { get; set; }

    bool IsEnabled { get; }

    void OnEnabled();

    void OnDisabled();
}