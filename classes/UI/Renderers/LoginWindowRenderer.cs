using System.Numerics;
using event_planner_mupvp.classes.core;
using event_planner_mupvp.lib;
using ImGuiNET;

namespace event_planner_mupvp.classes.UI.Renderers;

// classes/UI/Renderers/LoginWindowRenderer.cs

/// <summary>
///     Renders the initial login window for license key verification.
/// </summary>
public class LoginWindowRenderer : IWindowRenderer
{
    #region State

    private string _sessionKey = ""; // Input buffer for the license key

    #endregion

    #region Constructor

    // No dependencies needed if SessionManager is static

    #endregion

    #region IWindowRenderer Implementation

    public void Render()
    {
        // If already logged in, ensure this window is hidden and main window is shown
        if (SessionManager.loggedIn)
        {
            if (WindowManager.ShowLoginWindow) WindowManager.ShowLoginWindow = false;
            if (!WindowManager.ShowMainWindow) WindowManager.ShowMainWindow = true; // Ensure main window shows if login succeeds silently
            return;
        }

        // --- Window Setup ---
        ImGui.SetNextWindowSize(new Vector2(300, 190), ImGuiCond.Always); // Fixed size is often better for login
        ImGui.SetNextWindowPos(ImGui.GetMainViewport().GetCenter(), ImGuiCond.Appearing, new Vector2(0.5f, 0.5f)); // Center on appear

        var windowFlags = ImGuiWindowFlags.NoCollapse |
                          ImGuiWindowFlags.NoResize |
                          ImGuiWindowFlags.NoMove |
                          ImGuiWindowFlags.NoSavedSettings; // Don't save position/size

        // --- Render Window ---
        // Use 'ref' flag so closing via 'X' updates WindowManager (though maybe undesirable for login)
        if (ImGui.Begin("Verificar Clave de Licencia", ref WindowManager.ShowLoginWindow, windowFlags))
        {
            ImGui.SeparatorText("Ingresa tu clave");
            ImGui.Spacing();

            // Input Field
            ImGui.PushItemWidth(-1); // Full width
            // Set initial focus on the input field when window appears
            if (ImGui.IsWindowAppearing()) ImGui.SetKeyboardFocusHere(0);
            ImGui.InputText("##LicenseKeyInput", ref _sessionKey, 50); // Max 50 chars
            ImGui.PopItemWidth();
            ImGui.Spacing();

            // Error Message Area
            RenderErrorMessage();
            ImGui.Spacing();
            ImGui.Separator();
            ImGui.Spacing();

            // Action Buttons
            RenderActionButtons();
        }

        ImGui.End();

        // If closed via 'X', decide action (e.g., exit app?)
        if (!WindowManager.ShowLoginWindow) Console.WriteLine("Login window closed by user 'X'. Exiting.");
        // Environment.Exit(0); // Uncomment to exit if login is mandatory
    }

    // OnClose and OnOpen are not strictly necessary here unless resetting specific state

    #endregion

    #region UI Rendering Helpers

    private void RenderErrorMessage()
    {
        if (!string.IsNullOrEmpty(SessionManager.errorMessage))
        {
            // Wrap text in case of long error messages
            ImGui.PushTextWrapPos(ImGui.GetContentRegionAvail().X);
            ImGui.TextColored(new Vector4(1.0f, 0.2f, 0.2f, 1.0f), SessionManager.errorMessage);
            ImGui.PopTextWrapPos();
        }
        else
        {
            // Keep space consistent even when no error
            ImGui.Text(""); // Placeholder for spacing
        }
    }

    private void RenderActionButtons()
    {
        var buttonWidth = (ImGui.GetContentRegionAvail().X - ImGui.GetStyle().ItemSpacing.X) * 0.5f;
        var verifyClicked = ImGui.Button("Verificar", new Vector2(buttonWidth, 30));
        // Trigger verification also on Enter key press within the window
        var enterPressed = ImGui.IsKeyPressed(ImGuiKey.Enter) && ImGui.IsWindowFocused(ImGuiFocusedFlags.RootAndChildWindows);

        if (verifyClicked || enterPressed) VerifyLicenseKey();

        ImGui.SameLine();

        if (ImGui.Button("Salir", new Vector2(buttonWidth, 30))) Environment.Exit(0); // Exit application
    }

    #endregion

    #region Logic

    private void VerifyLicenseKey()
    {
        // SessionManager.SaveSession handles validation and updates state
        bool success = SessionManager.SaveSession(_sessionKey);
        if (success)
        {
            Console.WriteLine("Login successful.");
            WindowManager.ShowLoginWindow = false; // Hide login window
            WindowManager.ShowMainWindow = true; // Show main window
            // SessionManager.errorMessage should be cleared by SaveSession on success
        }
        else
        {
            Console.WriteLine($"Login failed: {SessionManager.errorMessage}");
            // Error message is already set by SessionManager, will display on next frame
            _sessionKey = ""; // Optionally clear input on failure
        }
    }

    #endregion
}