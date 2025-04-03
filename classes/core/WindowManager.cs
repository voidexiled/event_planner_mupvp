// classes/core/WindowManager.cs

namespace event_planner_mupvp.classes.core;

/// <summary>
///     Gestiona el estado de visibilidad de las diferentes ventanas de la UI.
///     Se usa PascalCase para los flags estáticos públicos según la convención estándar de C#.
/// </summary>
public static class WindowManager
{
    #region General State

    /// <summary>
    ///     Controla si todo el overlay (excepto quizás el botón de toggle) es visible.
    ///     Controlado por la tecla F10.
    /// </summary>
    public static bool IsOverlayVisible = true;

    #endregion

    #region Main Windows

    /// <summary>
    ///     Muestra la ventana de login/verificación de licencia.
    /// </summary>
    public static bool ShowLoginWindow  = true; // Inicia aquí

    /// <summary>
    ///     Muestra la ventana principal después del login.
    /// </summary>
    public static bool ShowMainWindow = false;

    /// <summary>
    ///     Muestra la notificación de actualización disponible.
    /// </summary>
    public static bool ShowUpdateWindow = false;

    #endregion

    #region Game Manager Windows

    /// <summary>
    ///     Muestra la ventana con la lista de todos los items del juego.
    /// </summary>
    public static bool ShowItemListWindow = false;

    /// <summary>
    ///     Muestra la ventana para gestionar y editar la lista de items de recompensa.
    /// </summary>
    public static bool ShowRewardListWindow = false;

    /// <summary>
    ///     Muestra la ventana con la lista de sets del juego.
    /// </summary>
    public static bool ShowSetListWindow = false;

    #endregion

    #region Tutor Windows

    /// <summary>
    ///     Muestra la ventana de administración de guías (crear, editar, buscar).
    /// </summary>
    public static bool ShowAdminGuidesWindow = false;

    /// <summary>
    ///     Muestra el formulario para agregar una nueva guía.
    /// </summary>
    public static bool ShowGuideEditorWindow = false; // Single flag for editor

    #endregion

    #region User Windows

    /// <summary>
    ///     Muestra la ventana del explorador de guías para usuarios.
    /// </summary>
    public static bool ShowGuideBrowserWindow = false;

    /// <summary>
    ///     Muestra la ventana de visualización de una guía seleccionada por el usuario.
    /// </summary>
    public static bool ShowSelectedGuideWindow = false;

    #endregion

    #region Owner Windows

    /// <summary>
    ///     Muestra la ventana de administración de usuarios.
    /// </summary>
    public static bool ShowUserAdminWindow = false;

    /// <summary>
    ///     Muestra el formulario para agregar un nuevo usuario.
    /// </summary>
    public static bool ShowNewUserWindow = false;

    #endregion

    #region Helper Methods

    /// <summary>
    ///     Establece todos los flags de ventanas (excepto Login y Update) a false.
    ///     Útil al cerrar sesión.
    /// </summary>
    public static void CloseAllWindows()
    {
        ShowMainWindow = false;
        ShowItemListWindow = false;
        ShowRewardListWindow = false;
        ShowSetListWindow = false;
        ShowAdminGuidesWindow = false;
        ShowGuideEditorWindow = false; // Close editor
        ShowGuideBrowserWindow = false;
        ShowSelectedGuideWindow = false;
        ShowUserAdminWindow = false;
        ShowNewUserWindow = false; // Ensure closed
    }

    #endregion
}