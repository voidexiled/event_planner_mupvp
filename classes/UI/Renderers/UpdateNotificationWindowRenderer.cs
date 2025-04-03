using System.Diagnostics;
using System.Numerics;
using ClickableTransparentOverlay;
using ImGuiNET;

namespace event_planner_mupvp.classes.UI.Renderers;

public class UpdateNotificationWindowRenderer : IWindowRenderer
{
    private readonly Overlay _overlayInstance;

    public UpdateNotificationWindowRenderer(Overlay overlayInstance)
    {
        _overlayInstance = overlayInstance;
    }

    public void Render()
    {
        ImGui.SetNextWindowSize(new Vector2(280, 110), ImGuiCond.Always); // Tamaño fijo
        ImGui.SetNextWindowPos(ImGui.GetMainViewport().GetCenter(), ImGuiCond.Appearing, new Vector2(0.5f, 0.5f));

        // No permitir cerrar con 'X'
        if (ImGui.Begin("Actualización Disponible",
                ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoMove))
        {
            ImGui.TextWrapped("Hay una nueva versión disponible para descargar."); // Wrapped
            ImGui.Spacing();
            ImGui.Separator();
            ImGui.Spacing();
            if (ImGui.Button("Descargar y Salir", new Vector2(-1, 30)))
            {
                try
                {
                    // Asegúrate que Updater.exe esté en la ruta correcta o especifícala
                    ProcessStartInfo startInfo = new ProcessStartInfo("Updater.exe")
                    {
                        WorkingDirectory = AppDomain.CurrentDomain.BaseDirectory // Ejecutar desde el directorio de la app
                    };
                    Process.Start(startInfo);
                    _overlayInstance.Close(); // Cierra el overlay
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error al iniciar Updater.exe: {ex.Message}");
                    // TODO: Mostrar mensaje de error al usuario en la UI si falla
                }
            }
        }
        ImGui.End();
    }
}