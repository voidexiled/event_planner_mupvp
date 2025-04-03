// RenderClass.cs (en la raíz del proyecto)

#region Usings

using System.Numerics;
using System.Runtime.InteropServices;
using ClickableTransparentOverlay;
using event_planner_mupvp.classes.core;
using event_planner_mupvp.classes.UI.Renderers;
using event_planner_mupvp.lib;
using ImGuiNET;
using MUPVPUI.Classes.UI.Styles;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

#endregion

namespace event_planner_mupvp;

// Namespace raíz
/// <summary>
///     Clase principal del overlay, hereda de ClickableTransparentOverlay.
///     Orquesta el renderizado de las diferentes ventanas/componentes de la UI.
/// </summary>
public class RenderClass : Overlay
{
    
    #region Structs
    
    [StructLayout(LayoutKind.Sequential)]
    struct ScreenSize
    {
        public int Width;
        public int Height;
    }
    
    #endregion
    
    #region Constants and Static Readonly

    /// <summary>
    ///     Versión actual de la aplicación.
    /// </summary>
    public static readonly string AppVersion = "0.0.0.4"; // Usar readonly static

    private const string UpdateCheckUrl = "https://raw.githubusercontent.com/voidexiled/event_planner_mupvp/main/version.txt";
    private const int VK_F10 = 0x79; // Código de tecla virtual para F10

    #endregion

    #region DllImports

    [DllImport("user32.dll")]
    private static extern int GetSystemMetrics(int nIndex);

    private const int SM_CXSCREEN = 0;
    private const int SM_CYSCREEN = 1;

    [DllImport("user32.dll")]
    private static extern short GetAsyncKeyState(int nKey);

    #endregion

    #region Fields

    // --- Overlay State ---
    private bool _isOutdated;
    private bool _firstFrame = true;
    private bool _assetsLoaded; // Flag para carga única de assets
    private int _screenWidth;
    private int _screenHeight;

    // --- Dynamic Colors ---
    private double _colorTime;
    private const double _colorSpeed = 3.0; // Velocidad del ciclo de color
    private float _dynamicRed = 1.0f;
    private float _dynamicGreen = 1.0f;
    private float _dynamicBlue;

    // --- Hotkey State ---
    private readonly Dictionary<int, bool> _wasKeyDown = new();

    // --- Renderers ---
    // Guardamos instancias para llamar a sus métodos Render() y pasar dependencias
    private readonly List<IWindowRenderer> _activeRenderers = new(); // Lista para fácil iteración (opcional)
    private LoginWindowRenderer? _loginRenderer;
    private UpdateNotificationWindowRenderer? _updateRenderer;
    private MainWindowRenderer? _mainRenderer;
    private ItemListWindowRenderer? _itemListRenderer;
    private RewardListWindowRenderer? _rewardListRenderer;
    private SetListWindowRenderer? _setListRenderer;
    private GuideBrowserWindowRenderer? _guideBrowserRenderer;
    private SelectedGuideWindowRenderer? _selectedGuideRenderer;
    private AdminGuidesWindowRenderer? _adminGuidesRenderer;
    private GuideEditorWindowRenderer? _guideEditorRenderer; // Renombrado para claridad
    private UserAdminWindowRenderer? _userAdminRenderer;
    private NewUserWindowRenderer? _newUserRenderer;
    // Añade variables para los renderers que faltan si los implementas

    #endregion

    #region Constructor

    public RenderClass() : base("MU PVP Tool", GetSystemMetrics(SM_CXSCREEN), GetSystemMetrics(SM_CYSCREEN)) // Título de la ventana del overlay
    {
        // 1. Verificar Actualizaciones (Síncrono por simplicidad aquí, idealmente asíncrono)
        CheckForUpdates();

        // 2. Inicializar Renderers (Crear instancias y pasar dependencias)
        InitializeRenderers();

        
    }

    #endregion

    #region Initialization Methods

    /// <summary>
    ///     Comprueba si hay una nueva versión comparando con la URL.
    /// </summary>
    private void CheckForUpdates()
    {
        var urlVersion = "";
        try
        {
            // Considera usar async/await si el constructor puede ser async
            // o mover esto a PostInitialized
            using HttpClient client = new();
            client.Timeout = TimeSpan.FromSeconds(5); // Evitar bloqueo largo
            urlVersion = client.GetStringAsync(UpdateCheckUrl).Result;

            var tempGithubVersion = urlVersion.Replace(".", "").Trim();
            var tempLocalVersion = AppVersion.Replace(".", "").Trim();

            if (int.TryParse(tempGithubVersion, out var githubVersion) &&
                int.TryParse(tempLocalVersion, out var localVersion))
            {
                if (githubVersion > localVersion)
                {
                    _isOutdated = true;
                    WindowManager.ShowUpdateWindow = true;
                    WindowManager.ShowLoginWindow = false; // No mostrar login si hay update
                }
            }
            else
            {
                Console.WriteLine("Error: No se pudieron parsear las versiones.");
            }
        }
        catch (Exception e)
        {
            Console.WriteLine($"Error al verificar versión: {e.GetType().Name} - {e.Message}");
            _isOutdated = false; // Asumir que no está desactualizado si falla la comprobación
        }

        Console.WriteLine($"CheckForUpdates: Local={AppVersion}, Remote={urlVersion}, Outdated={_isOutdated}");
    }

    /// <summary>
    ///     Crea instancias de todas las clases Renderer y establece sus dependencias.
    /// </summary>
    private void InitializeRenderers()
    {
        Console.WriteLine("Inicializando Renderers...");
        // Crear en orden de dependencia si es necesario

        // Core / Notificaciones
        _loginRenderer = new LoginWindowRenderer();
        _updateRenderer = new UpdateNotificationWindowRenderer(this); // Pasa 'this' para poder llamar a Close()

        // --- GM Tools ---
        _rewardListRenderer = new RewardListWindowRenderer(); // Necesario para ItemList y SetList
        _itemListRenderer = new ItemListWindowRenderer(_rewardListRenderer); // Pasa RewardList
        _setListRenderer = new SetListWindowRenderer(_rewardListRenderer); // Pasa RewardList

        // --- User Tools ---
        _selectedGuideRenderer = new SelectedGuideWindowRenderer(this); // Necesita instancia para GuideBrowser
        _guideBrowserRenderer = new GuideBrowserWindowRenderer(_selectedGuideRenderer); // Pasa SelectedGuide

        // --- Tutor Tools ---
        _guideEditorRenderer = new GuideEditorWindowRenderer(); // Instancia necesaria para AdminGuides
        _adminGuidesRenderer = new AdminGuidesWindowRenderer(_guideEditorRenderer); // Pasa GuideEditor

        // --- Owner Tools ---
        _newUserRenderer = new NewUserWindowRenderer(); // Instancia necesaria para UserAdmin
        _userAdminRenderer = new UserAdminWindowRenderer(_newUserRenderer); // Pasa NewUser

        // --- Main Window (Depende de varios para pasar info o referencias si fuera necesario) ---
        _mainRenderer = new MainWindowRenderer(AppVersion /*, pasa otros renderers si main necesita interactuar directamente */);


        // Opcional: Añadir a lista genérica para iterar (no usado actualmente)
        // _activeRenderers.AddRange(new IWindowRenderer[] { _loginRenderer, _updateRenderer, ... });
        Console.WriteLine("Renderers inicializados.");
    }

    /// <summary>
    ///     Se llama después de que ImGui se inicializa. Ideal para cargar fuentes, estilos y assets.
    /// </summary>
    protected override Task PostInitialized()
    {
        Console.WriteLine("PostInitialized: Aplicando estilo y cargando assets...");
        try
        {
            // Aplicar el estilo principal
            MainMenuStyle.ApplyStyle();
            // O usar estilo alternativo:
            // MainMenuStyle.ApplyDarkStyleWithRounding();

            // Cargar fuentes (Asegúrate que las rutas sean correctas)
            ReplaceFont("C:\\Windows\\Fonts\\segoeui.ttf", 16, FontGlyphRangeType.English); // Fuente base
            // this.AddFontFromFileTTF("fonts/poorstory.ttf", 18, FontGlyphRangeType.English); // Fuente adicional

            // Cargar otros assets (ej: imágenes) de forma asíncrona si es posible
            LoadAssetsAsync(); // Lanza la carga asíncrona

            Console.WriteLine("Estilo aplicado, carga de assets iniciada.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error en PostInitialized: {ex.Message}");
            // Considerar mostrar un error al usuario aquí
        }

        return Task.CompletedTask;
    }

    
    /// <summary>
    ///     Carga assets como imágenes de forma asíncrona.
    /// </summary>
    private async void LoadAssetsAsync() // async void es aceptable para "fire and forget" en UI
    {
        // TODO: Implementar carga de imágenes para guías si es necesario
        await _guideBrowserRenderer.LoadAllGuideImagesAsync();
        // await Task.Delay(100); // Simular carga
        _assetsLoaded = true;
        Console.WriteLine("Assets cargados.");
    }

    #endregion

    #region Core Render Loop

    /// <summary>
    ///     Método principal llamado en cada frame para dibujar la UI.
    /// </summary>
    protected override void Render()
    {
        
        // 3. Calcular tamaño inicial pantalla
        this.Size = new(GetSystemMetrics(SM_CXSCREEN), GetSystemMetrics(SM_CYSCREEN));
        this.Position = new(0, 0); // Resetear posición si cambia tamaño
        
        if (_firstFrame)
        {
            VSync = true; // Activar VSync
            _firstFrame = false;
        }

        // 1. Procesar Entradas (Hotkeys)
        HandleHotkeys();

        // 2. Actualizar Estado Global (Colores, Tamaño)
        UpdateDynamicColors();
        ConfigureOverlaySize(); // Revisa si el tamaño de pantalla cambió

        // 3. Renderizado Condicional
        // 3a. Ventana de Actualización (Prioritaria)
        if (WindowManager.ShowUpdateWindow)
        {
            _updateRenderer?.Render();
            return; // Bloquea el resto de la UI
        }

        // 3b. Botón global para mostrar/ocultar (siempre presente pero discreto)
        RenderOverlayToggleButton();
        
        ShowDebugWindow();

        // 3c. Si el overlay está oculto, no renderizar nada más
        if (!WindowManager.IsOverlayVisible) return;

        // 4. Renderizar Ventanas según Estado (Login o Principal + Secundarias)
        try
        {
            if (!SessionManager.loggedIn)
            {
                // Estado Deslogueado: Mostrar Login
                if (WindowManager.ShowLoginWindow) _loginRenderer?.Render();
            }
            else
            {
                // Estado Logueado: Mostrar Principal y las activas
                if (WindowManager.ShowLoginWindow) WindowManager.ShowLoginWindow = false; // Asegurar que login se oculte
                WindowManager.ShowMainWindow = true; // Asegurar que main window se muestre

                // Pasar colores dinámicos a los renderers que los usen
                PassDynamicColorsToRenderers();

                // Renderizar ventanas activas según WindowManager flags
                if (WindowManager.ShowMainWindow) _mainRenderer?.Render();
                if (WindowManager.ShowItemListWindow) _itemListRenderer?.Render();
                if (WindowManager.ShowRewardListWindow) _rewardListRenderer?.Render();
                if (WindowManager.ShowSetListWindow) _setListRenderer?.Render();
                if (WindowManager.ShowGuideBrowserWindow) _guideBrowserRenderer?.Render();
                if (WindowManager.ShowSelectedGuideWindow) _selectedGuideRenderer?.Render();
                if (WindowManager.ShowAdminGuidesWindow) _adminGuidesRenderer?.Render();
                if (WindowManager.ShowGuideEditorWindow) _guideEditorRenderer?.Render(); // Ventana de edición/nueva guía
                if (WindowManager.ShowUserAdminWindow) _userAdminRenderer?.Render();
                if (WindowManager.ShowNewUserWindow) _newUserRenderer?.Render();
                // Añadir llamadas a Render() para cualquier otro renderer
            }
        }
        catch (Exception ex)
        {
            // Capturar errores durante el renderizado para no crashear el overlay
            Console.WriteLine($"!!! ERROR EN RENDER LOOP: {ex.Message}\n{ex.StackTrace}");
            // Podrías dibujar un mensaje de error en pantalla
            // ImGui.TextColored(new Vector4(1,0,0,1), $"Render Error: {ex.Message}");
        }


        // --- Debugging ---
        // ShowDebugWindow(); // Descomentar para ver info de debug
    }

    #endregion

    #region Update and Input Methods

    /// <summary>
    ///     Procesa las teclas presionadas para acciones globales como mostrar/ocultar.
    /// </summary>
    private void HandleHotkeys()
    {
        if (IsKeyPressed(VK_F10)) // F10
        {
            WindowManager.IsOverlayVisible = !WindowManager.IsOverlayVisible;
            Console.WriteLine($"Overlay Visible Toggled: {WindowManager.IsOverlayVisible}");
        }
        // Añadir más hotkeys si es necesario
    }

    /// <summary>
    ///     Detecta si una tecla fue presionada en este frame (transición de arriba a abajo).
    /// </summary>
    private bool IsKeyPressed(int keyCode)
    {
        var isKeyDown = (GetAsyncKeyState(keyCode) & 0x8000) != 0;
        _wasKeyDown.TryGetValue(keyCode, out var wasDown); // Obtiene estado anterior o false
        _wasKeyDown[keyCode] = isKeyDown; // Actualiza estado actual
        return isKeyDown && !wasDown; // Devuelve true solo en la transición
    }

    /// <summary>
    ///     Actualiza los valores de los colores dinámicos (arcoíris).
    /// </summary>
    private void UpdateDynamicColors()
    {
        _colorTime += ImGui.GetIO().DeltaTime * _colorSpeed;
        _dynamicRed = (float)(Math.Sin(_colorTime) * 0.5 + 0.5);
        _dynamicGreen = (float)(Math.Sin(_colorTime + 2.0 * Math.PI / 3.0) * 0.5 + 0.5);
        _dynamicBlue = (float)(Math.Sin(_colorTime + 4.0 * Math.PI / 3.0) * 0.5 + 0.5);
    }

    /// <summary>
    ///     Pasa los colores dinámicos actualizados a los renderers que los necesiten.
    /// </summary>
    private void PassDynamicColorsToRenderers()
    {
        // Solo los renderers que muestran rangos especiales necesitan los colores
        _mainRenderer?.UpdateDynamicColors(_dynamicRed, _dynamicGreen, _dynamicBlue);
        _rewardListRenderer?.UpdateDynamicColors(_dynamicRed, _dynamicGreen, _dynamicBlue);
        _adminGuidesRenderer?.UpdateDynamicColors(_dynamicRed, _dynamicGreen, _dynamicBlue);
        // _guideEditorRenderer?.UpdateDynamicColors(_dynamicRed, _dynamicGreen, _dynamicBlue);
        _guideBrowserRenderer?.UpdateDynamicColors(_dynamicRed, _dynamicGreen, _dynamicBlue);
        _selectedGuideRenderer?.UpdateDynamicColors(_dynamicRed, _dynamicGreen, _dynamicBlue);
        // Añadir otros si es necesario
    }


    /// <summary>
    ///     Ajusta el tamaño del overlay si la resolución de pantalla cambia.
    /// </summary>
    private void ConfigureOverlaySize()
    {
        var currentWidth = GetSystemMetrics(SM_CXSCREEN);
        var currentHeight = GetSystemMetrics(SM_CYSCREEN);

        // if (_screenWidth != currentWidth || _screenHeight != currentHeight)
        // {
        //     this.Size.Width = currentWidth;
        //     this.Size.Height = currentHeight;
        //     this._screenWidth = currentWidth;
        //     this._screenHeight = currentHeight;
        //     // this.Position = Vector2.Zero; // Resetear posición si cambia tamaño
        //     Console.WriteLine($"Screen size changed: {_screenWidth}x{_screenHeight}. Overlay resized.");
        // }
    }

    #endregion

    #region UI Helper Methods

    /// <summary>
    ///     Renderiza un botón pequeño y discreto para mostrar/ocultar el overlay.
    /// </summary>
    private void RenderOverlayToggleButton()
    {
        var flags = ImGuiWindowFlags.NoDecoration | // Sin título, borde, etc.
                    ImGuiWindowFlags.AlwaysAutoResize |
                    ImGuiWindowFlags.NoSavedSettings |
                    ImGuiWindowFlags.NoFocusOnAppearing |
                    ImGuiWindowFlags.NoNav |
                    ImGuiWindowFlags.NoMove; // Evitar que se mueva

        // Posición fija (esquina superior derecha)
        const float padding = 10f;
        var position = new Vector2(ImGui.GetIO().DisplaySize.X - padding, padding);
        ImGui.SetNextWindowPos(position, ImGuiCond.Always, new Vector2(1.0f, 0.0f)); // Pivot arriba-derecha

        // Fondo semi-transparente
        ImGui.SetNextWindowBgAlpha(0.5f);

        if (ImGui.Begin("OverlayToggle", flags))
        {
            var btnText = WindowManager.IsOverlayVisible ? "Ocultar (F10)" : "Mostrar (F10)";
            if (ImGui.SmallButton(btnText)) // Botón más pequeño
                WindowManager.IsOverlayVisible = !WindowManager.IsOverlayVisible;
            if (ImGui.IsItemHovered()) ImGui.SetTooltip("Click o F10 para mostrar/ocultar");
        }

        ImGui.End();
    }

    /// <summary>
    ///     Muestra una ventana con información útil para depuración.
    /// </summary>
    private void ShowDebugWindow()
    {
        ImGui.SetNextWindowSize(new Vector2(400, 400), ImGuiCond.FirstUseEver);
        if (ImGui.Begin("Debug Info"))
        {
            ImGui.Text($"Version: {AppVersion}");
            ImGui.Text($"Outdated: {_isOutdated}");
            ImGui.Text($"Assets Loaded: {_assetsLoaded}");
            ImGui.Text($"Screen Size: {_screenWidth}x{_screenHeight}");
            ImGui.Text($"Overlay Visible: {WindowManager.IsOverlayVisible}");
            ImGui.Text($"Logged In: {SessionManager.loggedIn}");
            ImGui.Text($"Current User: {SessionManager.currentUser?.Key} ({SessionManager.currentUser?.UserType})");
            ImGui.Separator();
            ImGui.Text("Window Visibility:");
            ImGui.Checkbox("Login", ref WindowManager.ShowLoginWindow);
            ImGui.Checkbox("Main", ref WindowManager.ShowMainWindow);
            ImGui.Checkbox("Update", ref WindowManager.ShowUpdateWindow);
            ImGui.Checkbox("Item List", ref WindowManager.ShowItemListWindow);
            ImGui.Checkbox("Reward List", ref WindowManager.ShowRewardListWindow);
            ImGui.Checkbox("Set List", ref WindowManager.ShowSetListWindow);
            ImGui.Checkbox("Guide Browser", ref WindowManager.ShowGuideBrowserWindow);
            ImGui.Checkbox("Selected Guide", ref WindowManager.ShowSelectedGuideWindow);
            ImGui.Checkbox("Admin Guides", ref WindowManager.ShowAdminGuidesWindow);
            ImGui.Checkbox("Guide Editor", ref WindowManager.ShowGuideEditorWindow);
            ImGui.Checkbox("User Admin", ref WindowManager.ShowUserAdminWindow);
            ImGui.Checkbox("New User", ref WindowManager.ShowNewUserWindow);
            ImGui.Separator();
            ImGui.Text($"Mouse Pos: {ImGui.GetMousePos()}");
            ImGui.Text($"Delta Time: {ImGui.GetIO().DeltaTime:F4}");
            ImGui.Text($"FPS: {ImGui.GetIO().Framerate:F1}");
        }

        ImGui.End();
        ImGui.ShowDemoWindow();
        
    }

    #endregion
    
}