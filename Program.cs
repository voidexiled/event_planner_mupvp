// Program.cs (en la raíz)

#region Usings

using System.Runtime.InteropServices;
using event_planner_mupvp.lib;
using SixLabors.ImageSharp.Memory;
// Para SessionManager, ItemManager, ApiManager

// Para RenderClass

#endregion

namespace event_planner_mupvp;

// O tu namespace principal
/// <summary>
///     Punto de entrada principal de la aplicación.
/// </summary>
public static class Program // Clase estática es común para Main
{
    #region DllImports (Console Window Handling)

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern IntPtr GetConsoleWindow();

    [DllImport("user32.dll")]
    private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

    private const int SW_HIDE = 0;
    private const int SW_SHOW = 5;

    #endregion

    /// <summary>
    ///     Método principal de la aplicación.
    /// </summary>
    [STAThread] // Recomendado para aplicaciones con UI o COM
    public static async Task Main(string[] args)
    {
        // 1. Ocultar la ventana de consola (Opcional)
        HideConsoleWindow();

        try
        {
            // 2. Inicializar Managers Esenciales (Antes de crear el overlay)
            MemoryAllocator.Default.Allocate<byte>(0, AllocationOptions.None);
            Console.WriteLine("Initializing Managers...");
            SessionManager.LoadSession();
            ItemManager.PopulateItems(); // Asegúrate que esto cargue los datos necesarios
            // ApiManager.Initialize();
            // Inicializar otros managers si existen (ColorManager, ExcManager no necesitan si son estáticos puros)
            Console.WriteLine("Managers Initialized.");

            // 3. Crear e Iniciar el Overlay
            Console.WriteLine("Starting Overlay...");
            using var overlay = new RenderClass(); // 'using' asegura Dispose al salir
            await overlay.Run(); // Ejecuta el bucle principal de ClickableTransparentOverlay

            // 4. Código después de cerrar el overlay (si es necesario)
            Console.WriteLine("Overlay has been closed.");
        }
        catch (Exception ex)
        {
            // Capturar cualquier error fatal durante la inicialización o ejecución
            Console.WriteLine($"FATAL ERROR: {ex.Message}\n{ex.StackTrace}");
            // Podrías mostrar un MessageBox aquí antes de salir
            // MessageBox.Show($"An unexpected error occurred: {ex.Message}", "Fatal Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        finally
        {
            // Mostrar la consola al final si se ocultó (para ver logs de errores)
            // ShowConsoleWindow();
            Console.WriteLine("Application exiting.");
        }
    }

    #region Console Window Helpers

    /// <summary>
    ///     Oculta la ventana de la consola asociada a esta aplicación.
    /// </summary>
    private static void HideConsoleWindow()
    {
        try
        {
            var handle = GetConsoleWindow();
            if (handle != IntPtr.Zero) ShowWindow(handle, SW_HIDE);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error hiding console: {ex.Message}");
        }
    }

    /// <summary>
    ///     Muestra la ventana de la consola asociada a esta aplicación.
    /// </summary>
    private static void ShowConsoleWindow()
    {
        try
        {
            var handle = GetConsoleWindow();
            if (handle != IntPtr.Zero) ShowWindow(handle, SW_SHOW);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error showing console: {ex.Message}");
        }
    }

    #endregion
}