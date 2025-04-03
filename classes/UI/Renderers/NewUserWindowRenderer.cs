using System.Numerics;
using event_planner_mupvp.classes.core;
using event_planner_mupvp.lib;
using ImGuiNET;

namespace event_planner_mupvp.classes.UI.Renderers;

// classes/UI/Renderers/NewUserWindowRenderer.cs

public class NewUserWindowRenderer : IWindowRenderer
{
    #region State

    private string _keyInput = "";
    private string _selectedRankString = UserTypes.USUARIO.ToString(); // Default selection
    private UserTypes _selectedRank = UserTypes.USUARIO;

    #endregion

    #region Constructor

    #endregion

    #region IWindowRenderer Implementation

    public void Render()
    {
        // Set position relative to User Admin window or center? Let's center.
        ImGui.SetNextWindowSize(new Vector2(350, 280), ImGuiCond.Appearing);
        ImGui.SetNextWindowPos(ImGui.GetMainViewport().GetCenter(), ImGuiCond.Appearing, new Vector2(0.5f, 0.5f));

        // Use BeginPopupModal if making it a modal, or Begin for a regular window
        if (ImGui.Begin("Agregar Nuevo Usuario", ref WindowManager.ShowNewUserWindow, ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoCollapse))
        {
            RenderFields();
            ImGui.Separator();
            RenderActions();
            RenderApiMessage();
        }

        if (!WindowManager.ShowNewUserWindow) OnClose();
        ImGui.End();
    }

    public void OnOpen()
    {
        // State is prepared by PrepareForNewUser() before setting flag
        Console.WriteLine("New User Window opened.");
    }

    public void OnClose()
    {
        // Clear fields when closed by 'X' or cancel
        PrepareForNewUser();
        Console.WriteLine("New User Window closed.");
    }

    #endregion

    #region Public Control

    /// <summary>
    ///     Resets the input fields and messages, called before opening the window.
    /// </summary>
    public void PrepareForNewUser()
    {
        _keyInput = "";
        _selectedRank = UserTypes.USUARIO;
        _selectedRankString = UserTypes.USUARIO.ToString();
        ApiManager.adminMessage = ""; // Clear any previous API message
    }

    #endregion

    #region UI Rendering

    private void RenderFields()
    {
        // License Key Input
        ImGui.Text("Llave de Licencia:");
        ImGui.InputText("##LicenseKeyInput", ref _keyInput, 50); // Max 50 chars
        ImGui.SameLine();
        if (ImGui.Button("Generar Nueva Llave")) _keyInput = Guid.NewGuid().ToString(); // Generate a new GUID

        ImGui.Spacing();

        // Rank Selection Combo Box
        ImGui.Text("Selecciona el Rango:");
        if (ImGui.BeginCombo("##RankCombo", _selectedRankString))
        {
            foreach (UserTypes rank in Enum.GetValues(typeof(UserTypes)))
            {
                // Optional: Exclude certain ranks if needed (e.g., Creador)
                // if (rank == UserTypes.CREADOR) continue;

                var isSelected = rank == _selectedRank;
                if (ImGui.Selectable(rank.ToString(), isSelected))
                {
                    _selectedRank = rank;
                    _selectedRankString = rank.ToString(); // Update display string
                }

                if (isSelected) ImGui.SetItemDefaultFocus(); // Keep selected item highlighted
            }

            ImGui.EndCombo();
        }
    }

    private void RenderActions()
    {
        var buttonWidth = (ImGui.GetContentRegionAvail().X - ImGui.GetStyle().ItemSpacing.X * 1) * 0.5f; // Adjust spacing if needed
        if (ImGui.Button("Agregar Usuario", new Vector2(buttonWidth, 30))) AddUser();
        ImGui.SameLine();
        if (ImGui.Button("Cancelar", new Vector2(buttonWidth, 30))) WindowManager.ShowNewUserWindow = false; // Just close the window
    }

    private void RenderApiMessage()
    {
        // Display message from ApiManager (success or error)
        if (!string.IsNullOrEmpty(ApiManager.adminMessage))
        {
            ImGui.Spacing();
            ImGui.TextColored(ApiManager.messageColor, ApiManager.adminMessage);
        }
    }

    #endregion

    #region Logic

    private void AddUser()
    {
        // Basic Validation
        if (string.IsNullOrWhiteSpace(_keyInput))
        {
            ApiManager.adminMessage = "Error: La Llave de Licencia no puede estar vacía.";
            ApiManager.messageColor = new Vector4(1, 0, 0, 1);
            return;
        }

        // Create User object
        var newUser = new User
        {
            Key = _keyInput.Trim(), // Trim whitespace
            UserType = _selectedRank
        };

        Console.WriteLine($"Intentando agregar usuario: Key={newUser.Key}, Rank={newUser.UserType}");

        // Call ApiManager to register (assuming it handles messages)
        ApiManager.RegisterNewUser(newUser); // This method should set adminMessage and messageColor

        // Optionally clear fields only on success? ApiManager should indicate success/failure.
        // if (/* check ApiManager success flag/message */) {
        //    PrepareForNewUser(); // Clear for next entry after success
        // }
    }

    #endregion
}