// classes/UI/Renderers/MainWindowRenderer.cs
using ImGuiNET;
using System;
using System.Numerics;
using event_planner_mupvp.classes.core;
using event_planner_mupvp.lib;


namespace event_planner_mupvp.classes.UI.Renderers;

/// <summary>
/// Renders the main application window shown after successful login.
/// Contains user info, role-based tool buttons, and general options.
/// </summary>
public class MainWindowRenderer : IWindowRenderer
{
    #region State
    // Dynamic colors passed from RenderClass
    private float _dynamicRed = 1.0f;
    private float _dynamicGreen = 1.0f;
    private float _dynamicBlue = 0.0f;
    private readonly string _appVersion; // Store app version passed from RenderClass
    #endregion

    #region Constructor
    // Accept app version and potentially references to other renderers if needed
    public MainWindowRenderer(string appVersion /*, other renderer refs */)
    {
        _appVersion = appVersion;
    }
    #endregion

    #region IWindowRenderer Implementation
    public void UpdateDynamicColors(float r, float g, float b)
    {
        _dynamicRed = r;
        _dynamicGreen = g;
        _dynamicBlue = b;
    }

    public void Render()
    {
        // --- Window Setup ---
        ImGui.SetNextWindowSize(new Vector2(280, 480), ImGuiCond.FirstUseEver); // Slightly taller
        ImGui.SetNextWindowPos(new Vector2(50, 50), ImGuiCond.FirstUseEver); // Default position

        ImGuiWindowFlags windowFlags = ImGuiWindowFlags.NoCollapse; // Allow closing, no collapsing

        // --- Render Window ---
        if (ImGui.Begin("MU PVP ONLINE - STAFF HELPER", ref WindowManager.ShowMainWindow, windowFlags))
        {
            RenderHeader();
            RenderUserInfo();
            ImGui.Separator();
            RenderToolsBasedOnRole(); // Buttons for role-specific windows
            RenderOtherOptions();     // Logout, Exit
        }
        ImGui.End(); // IMPORTANT: Always call End()

        // Handle window close via 'X' button
        if (!WindowManager.ShowMainWindow)
        {
            OnClose();
            // Decide if closing main window should exit app or just hide it
            // Environment.Exit(0);
        }
    }

    public void OnClose()
    {
        Console.WriteLine("Main Window closed.");
        // Reset any specific state if needed, though usually not necessary for main window
    }
    #endregion

    #region UI Rendering Sections
    private void RenderHeader()
    {
        // Can use ImGui.PushFont / PopFont here for different sizes
        ImGui.Text("Desarrollado por Void Exiled (GM Kxacez)");
        ImGui.Text($"Versión: {_appVersion}");
    }

    private void RenderUserInfo()
    {
        ImGui.SeparatorText("Info Usuario");
        if (SessionManager.currentUser != null)
        {
            ImGui.Text("Rango:");
            ImGui.SameLine();
            string rankString = SessionManager.currentUser.GetRankAsString(); // Assumes method exists
            // Use ColorManager (or ColorUtils) for consistent coloring
            Vector4 userColor = ColorManager.GetUserColor(SessionManager.currentUser, _dynamicRed, _dynamicGreen, _dynamicBlue);
            ImGui.TextColored(userColor, rankString);

            // Optionally display license key or username
            // ImGui.Text($"Key: {SessionManager.currentUser.Key}");
        }
        else
        {
            ImGui.TextColored(new Vector4(1f, 0f, 0f, 1f), "Error: Datos de usuario no disponibles.");
        }
        ImGui.Spacing();
    }

    private void RenderToolsBasedOnRole()
    {
        // Render buttons conditionally based on user role from SessionManager
        // Important: Check higher roles first OR allow fall-through if roles are hierarchical

        // Owner specific tools
        if (SessionManager.UserIsOwner()) RenderOwnerToolButtons();

        // Admin specific tools (Owners also see these if hierarchical)
        if (SessionManager.UserIsAdministrator()) RenderAdministratorToolButtons();

        // CM specific tools (Admins/Owners also see these if hierarchical)
        if (SessionManager.UserIsCommunityManager()) RenderCommunityManagerToolButtons();

        // GM specific tools (Admins/Owners also see these if hierarchical)
        if (SessionManager.UserIsGameManager()) RenderGameManagerToolButtons();

        // Tutor specific tools (GMs/Admins/Owners also see these if hierarchical)
        if (SessionManager.UserIsTutor()) RenderTutorToolButtons();

        // User tools (Everyone logged in sees these)
        if (SessionManager.loggedIn) RenderUserToolButtons(); // Check loggedIn instead of UserIsUser

    }

    private void RenderOtherOptions()
    {
        ImGui.SeparatorText("Otras Opciones");
        ImGui.Spacing();

        // --- Logout Button ---
        // Use red color style for logout/exit
        ImGui.PushStyleColor(ImGuiCol.Button, new Vector4(0.8f, 0.1f, 0.15f, 1f));
        ImGui.PushStyleColor(ImGuiCol.ButtonHovered, new Vector4(0.9f, 0.2f, 0.2f, 1f));
        ImGui.PushStyleColor(ImGuiCol.ButtonActive, new Vector4(0.7f, 0.1f, 0.1f, 1f));

        if (ImGui.Button("Cerrar Sesión", new Vector2(-1, 25))) // Full width
        {
            SessionManager.DeleteSession();
            WindowManager.CloseAllWindows(); // Close all tool windows
            WindowManager.ShowMainWindow = false; // Hide this window
            WindowManager.ShowLoginWindow = true;  // Show login window
        }

        ImGui.Spacing();

        // --- Exit Button ---
        if (ImGui.Button("Salir de la Aplicación", new Vector2(-1, 25)))
        {
            Environment.Exit(0); // Close the entire application
        }

        ImGui.PopStyleColor(3); // Pop the 3 red button colors
    }
    #endregion

    #region Role-Specific Button Sections
    // These methods contain ONLY the buttons that toggle WindowManager flags

    private void RenderOwnerToolButtons()
    {
        ImGui.SeparatorText("Owner Tools");
        RenderToggleButton("Admin Usuarios", ref WindowManager.ShowUserAdminWindow);
        ImGui.Spacing();
    }

    private void RenderAdministratorToolButtons()
    {
        ImGui.SeparatorText("Admin Tools");
        if (ImGui.Button("Admin Staff (N/A)", new Vector2(-1, 25))) { /* Placeholder */ }
        ImGui.Spacing();
    }

    private void RenderCommunityManagerToolButtons()
    {
        ImGui.SeparatorText("CM Tools");
        if (ImGui.Button("Lista Comandos (N/A)", new Vector2(-1, 25))) { /* Placeholder */ }
        ImGui.Spacing();
    }

    private void RenderGameManagerToolButtons()
    {
        ImGui.SeparatorText("GM Tools");
        RenderToggleButton("Lista Items", ref WindowManager.ShowItemListWindow);
        RenderToggleButton("Lista Recompensas", ref WindowManager.ShowRewardListWindow);
        RenderToggleButton("Lista Sets", ref WindowManager.ShowSetListWindow);
        ImGui.Spacing();
    }

    private void RenderTutorToolButtons()
    {
        ImGui.SeparatorText("Tutor Tools");
        RenderToggleButton("Admin Guías", ref WindowManager.ShowAdminGuidesWindow);
        ImGui.Spacing();
    }

    private void RenderUserToolButtons()
    {
        ImGui.SeparatorText("Herramientas"); // General section for users
        if (RenderToggleButton("Guías", ref WindowManager.ShowGuideBrowserWindow)) // If button was clicked
        {
             // If user just closed the browser, also close the selected guide viewer
             if (!WindowManager.ShowGuideBrowserWindow)
             {
                 WindowManager.ShowSelectedGuideWindow = false;
             }
        }
        ImGui.Spacing();
    }

    /// <summary>
    /// Helper to render a standard toggle button for showing/hiding a window.
    /// </summary>
    /// <param name="label">Base label for the window.</param>
    /// <param name="windowFlag">The boolean flag in WindowManager controlling visibility.</param>
    /// <returns>True if the button was clicked in this frame, false otherwise.</returns>
    private bool RenderToggleButton(string label, ref bool windowFlag)
    {
        string buttonLabel = $"{(windowFlag ? "Cerrar" : "Abrir")} {label}";
        bool clicked = ImGui.Button(buttonLabel, new Vector2(-1, 25)); // Full width
        if (clicked)
        {
            windowFlag = !windowFlag; // Toggle the flag

            // Call OnOpen/OnClose if the corresponding renderer exists and implements it
            // This requires passing renderer instances or using a central registry.
             if (windowFlag)
             {
                 // FindRenderer(ref windowFlag)?.OnOpen(); // Example placeholder
             }
             else
             {
                 // FindRenderer(ref windowFlag)?.OnClose(); // Example placeholder
             }
        }
        return clicked;
    }
    #endregion
}
