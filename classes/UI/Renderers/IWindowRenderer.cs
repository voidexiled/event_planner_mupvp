// classes/UI/Renderers/IWindowRenderer.cs

namespace event_planner_mupvp.classes.UI.Renderers;

/// <summary>
///     Interface for classes responsible for rendering specific UI windows or components.
/// </summary>
public interface IWindowRenderer
{
    /// <summary>
    ///     Renders the UI elements for this window/component.
    /// </summary>
    void Render();

    /// <summary>
    ///     Called when the window is explicitly or implicitly closed.
    ///     Use this to reset internal state if necessary.
    /// </summary>
    void OnClose()
    {
    } // Default empty implementation

    /// <summary>
    ///     Optional: Called when the window is opened.
    ///     Use this to load data or prepare state.
    /// </summary>
    void OnOpen()
    {
    } // Default empty implementation

    /// <summary>
    ///     Optional: Passes dynamic color values for UI elements like admin text.
    /// </summary>
    void UpdateDynamicColors(float r, float g, float b)
    {
    } // Default empty implementation
}