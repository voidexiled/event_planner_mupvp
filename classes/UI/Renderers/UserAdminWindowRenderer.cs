using System.Numerics;
using event_planner_mupvp.classes.core;
using ImGuiNET;

namespace event_planner_mupvp.classes.UI.Renderers;

// classes/UI/Renderers/UserAdminWindowRenderer.cs

public class UserAdminWindowRenderer : IWindowRenderer
{
    #region State

    // TODO: Add state for displaying list of users (needs fetching logic)
    private readonly List<User> _userList = new(); // Placeholder
    private string _userSearch = "";
    private bool _needsUserRefresh = true;

    // Dependency
    private readonly NewUserWindowRenderer _newUserRenderer;

    #endregion

    #region Constructor

    public UserAdminWindowRenderer(NewUserWindowRenderer newUserRenderer)
    {
        _newUserRenderer = newUserRenderer;
    }

    #endregion

    #region IWindowRenderer Implementation

    public void Render()
    {
        // TODO: Implement user list fetching if _needsUserRefresh is true

        ImGui.SetNextWindowSize(new Vector2(400, 500), ImGuiCond.FirstUseEver);
        if (ImGui.Begin("Administración de Usuarios", ref WindowManager.ShowUserAdminWindow))
        {
            // Button to open the "Add New User" window
            if (ImGui.Button("Agregar Nuevo Usuario", new Vector2(-1, 25))) // Full width
            {
                // Reset the state of the NewUser window before opening
                _newUserRenderer.PrepareForNewUser();
                WindowManager.ShowNewUserWindow = true;
            }

            ImGui.Separator();

            // TODO: Implement User List Display and Search/Filtering
            ImGui.InputTextWithHint("##UserSearch", "Buscar Usuario...", ref _userSearch, 100);
            ImGui.SameLine();
            if (ImGui.Button("Refrescar Lista")) _needsUserRefresh = true;

            ImGui.BeginChild("UserListChild", Vector2.Zero, ImGuiChildFlags.Borders);
            if (_needsUserRefresh)
                ImGui.Text("Cargando usuarios...");
            else if (_userList.Count == 0)
                ImGui.Text("No hay usuarios para mostrar.");
            else
                // Render the filtered user list here
                // Example: foreach (var user in filteredUserList) { ImGui.Text(user.Key + " - " + user.UserType); /* Add Edit/Delete buttons */ }
                ImGui.Text("IMPLEMENTACIÓN DE LISTA DE USUARIOS PENDIENTE");
            ImGui.EndChild();
        }

        if (!WindowManager.ShowUserAdminWindow) OnClose();
        ImGui.End();
    }

    public void OnOpen()
    {
        _needsUserRefresh = true; // Reload user list when opened
        Console.WriteLine("User Admin Window opened.");
        // Ensure New User window is closed initially when this opens
        WindowManager.ShowNewUserWindow = false;
    }

    public void OnClose()
    {
        _userSearch = "";
        _userList.Clear(); // Clear cache on close
        Console.WriteLine("User Admin Window closed.");
        // Do not automatically close the New User window if it was opened from here
    }

    #endregion

    // Add methods for fetching/filtering user list from ApiManager if needed
}