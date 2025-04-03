using System.Diagnostics;
using System.Numerics;
using ClickableTransparentOverlay;
using event_planner_mupvp.classes.core;
using event_planner_mupvp.lib;
using ImGuiNET;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Memory;
using SixLabors.ImageSharp.PixelFormats;

namespace event_planner_mupvp.classes.UI.Renderers;

// classes/UI/Renderers/SelectedGuideWindowRenderer.cs
// For Process.Start
// Requires SixLabors.ImageSharp for image loading/display if using that method

public class SelectedGuideWindowRenderer : IWindowRenderer
{
    #region State

    private readonly Overlay _overlayInstance; // Guardar referencia al overlay principal
    
    private Guide? _selectedGuide;
    private readonly List<Guide> _guideHistory = new(); // For Back button
    private Vector2 _previousWindowPosition = Vector2.Zero; // Remember position
    private Vector2 _centerScreenPos; // Cache center screen pos

    // Image Loading State (Example using ClickableTransparentOverlay's helper)
    private readonly Dictionary<int, IntPtr> _guideImageHandles = new();
    private readonly Dictionary<int, Vector2> _guideImageSizes = new();
    private bool _isLoadingImage; // Prevent multiple load attempts

    // Dynamic Colors
    private float _dynamicRed = 1.0f;
    private float _dynamicGreen = 1.0f;
    private float _dynamicBlue;

    // Dependency (needed to get full Guide objects for related guides)
    // Assuming ApiManager holds the master list or can fetch by ID

    #endregion

    #region Constructor

    public SelectedGuideWindowRenderer(Overlay overlay)
    {
        _overlayInstance = overlay ?? throw new ArgumentNullException(nameof(overlay));
    }
    
    #endregion

    #region IWindowRenderer Implementation

    public void Render()
    {
        if (_selectedGuide == null)
        {
            // Ensure the window flag is false if no guide is selected
            if (WindowManager.ShowSelectedGuideWindow) WindowManager.ShowSelectedGuideWindow = false;
            return;
        }

        // Set position on first appearance or if reset
        if (_previousWindowPosition == Vector2.Zero)
        {
            _centerScreenPos = ImGui.GetMainViewport().GetCenter();
            ImGui.SetNextWindowPos(_centerScreenPos, ImGuiCond.Appearing, new Vector2(0.5f, 0.5f));
        }
        else
        {
            ImGui.SetNextWindowPos(_previousWindowPosition, ImGuiCond.Appearing);
        }

        // Set initial size
        ImGui.SetNextWindowSize(new Vector2(450, 600), ImGuiCond.Appearing);


        // Begin Window
        var windowTitle = $"Ver Guía: {_selectedGuide.Title} - {_selectedGuide.Author}";
        var flags = ImGuiWindowFlags.None; // Add flags as needed (e.g., AlwaysVerticalScrollbar)

        // Apply featured color to title bar maybe? (Tricky, requires pushing style vars)
        // Vector4 titleBgColor = _selectedGuide.IsFeatured ? new Vector4(_dynamicRed * 0.7f, _dynamicGreen * 0.7f, _dynamicBlue * 0.7f, 1f) : ImGui.GetStyle().Colors[(int)ImGuiCol.TitleBgActive];
        // ImGui.PushStyleColor(ImGuiCol.TitleBgActive, titleBgColor);

        if (ImGui.Begin(windowTitle, ref WindowManager.ShowSelectedGuideWindow, flags))
        {
            _previousWindowPosition = ImGui.GetWindowPos(); // Remember position

            // Apply featured color to text content
            var defaultColor = ImGui.GetStyle().Colors[(int)ImGuiCol.Text];
            var featuredColor = new Vector4(_dynamicRed, _dynamicGreen, _dynamicBlue, 1.0f);
            if (_selectedGuide.IsFeatured) ImGui.PushStyleColor(ImGuiCol.Text, featuredColor);

            RenderHeaderControls();
            ImGui.Separator();
            RenderGuideImage();
            RenderGuideBody();
            RenderLinksSection();
            RenderRelatedGuidesSection();

            if (_selectedGuide.IsFeatured) ImGui.PopStyleColor(); // Pop text color
        }
        // ImGui.PopStyleColor(); // Pop title color

        if (!WindowManager.ShowSelectedGuideWindow) OnClose(); // Call OnClose if window closed by 'X'
        ImGui.End();
    }

    public void OnClose()
    {
        _selectedGuide = null;
        _guideHistory.Clear();
        _previousWindowPosition = Vector2.Zero; // Reset position memory
        // Dispose image handles if necessary
        // foreach (var handle in _guideImageHandles.Values) { /* Dispose logic? Overlay might handle it */ }
        _guideImageHandles.Clear();
        _guideImageSizes.Clear();
        _isLoadingImage = false;
        Console.WriteLine("Selected Guide Window closed and state cleared.");
    }

    public void UpdateDynamicColors(float r, float g, float b)
    {
        _dynamicRed = r;
        _dynamicGreen = g;
        _dynamicBlue = b;
    }

    #endregion

    #region Public Methods

    /// <summary>
    ///     Sets the guide to be displayed and updates history.
    /// </summary>
    public void SetSelectedGuide(Guide guide)
    {
        if (guide == null) return;

        // Avoid adding the same guide consecutively to history
        if (_guideHistory.LastOrDefault() != guide) _guideHistory.Add(guide);
        // Optional: Limit history size
        // const int MAX_HISTORY = 10;
        // if (_guideHistory.Count > MAX_HISTORY) _guideHistory.RemoveAt(0);
        _selectedGuide = guide;
        _previousWindowPosition = Vector2.Zero; // Reset position for new guide centered view
        LoadGuideImageAsync(_selectedGuide); // Start loading image
        WindowManager.ShowSelectedGuideWindow = true; // Ensure flag is set
    }
    
    
    #endregion

    #region UI Rendering Sections

    private void RenderHeaderControls()
    {
        // Back Button
        if (_guideHistory.Count > 1)
        {
            if (ImGui.ArrowButton("##BackGuide", ImGuiDir.Left))
            {
                // Remove current guide, select previous one
                _guideHistory.RemoveAt(_guideHistory.Count - 1);
                var previousGuide = _guideHistory.Last();
                // Set the previous guide without adding it back to history here
                _selectedGuide = previousGuide;
                LoadGuideImageAsync(_selectedGuide); // Load image for previous guide
            }

            if (ImGui.IsItemHovered()) ImGui.SetTooltip("Volver a la guía anterior");
            ImGui.SameLine();
        }

        // Title (already in window title, maybe add tags here?)
        if (_selectedGuide != null && _selectedGuide.Tags.Any()) ImGui.TextDisabled($"Tags: {string.Join(", ", _selectedGuide.Tags)}");
    }

    private void RenderGuideImage()
    {
        if (_selectedGuide == null || string.IsNullOrWhiteSpace(_selectedGuide.Image)) return;

        ImGui.Spacing();

        if (_guideImageHandles.TryGetValue(_selectedGuide.Id, out var imageHandle))
        {
            // Image loaded, display it
            var imageSize = _guideImageSizes.GetValueOrDefault(_selectedGuide.Id, new Vector2(400, 280)); // Default size
            var windowWidth = ImGui.GetContentRegionAvail().X;
            // Scale image to fit window width, maintaining aspect ratio
            var aspectRatio = imageSize.Y / imageSize.X;
            var displaySize = new Vector2(windowWidth, windowWidth * aspectRatio);
            ImGui.Image(imageHandle, displaySize);
        }
        else if (_isLoadingImage)
        {
            ImGui.Text("Cargando imagen..."); // Show loading indicator
        }
        else
        {
            // Image not loaded and not currently loading (e.g., failed or first view)
            ImGui.TextDisabled("[Imagen no disponible]");
            // Optionally add a button to retry loading
            // if (ImGui.Button("Reintentar Cargar Imagen")) { LoadGuideImageAsync(_selectedGuide); }
        }

        ImGui.Spacing();
    }

    private void RenderGuideBody()
    {
        if (_selectedGuide == null) return;
        ImGui.Separator();
        foreach (var paragraph in _selectedGuide.Body)
        {
            // Use TextWrapped for automatic line breaking within the window width
            ImGui.TextWrapped(paragraph);
            ImGui.Spacing(); // Add some space between paragraphs
        }
    }

    private void RenderLinksSection()
    {
        if (_selectedGuide == null || string.IsNullOrWhiteSpace(_selectedGuide.Video)) return;

        ImGui.Separator();
        ImGui.SeparatorText("Enlaces");
        // Simple link rendering, could be improved (e.g., icons)
        ImGui.Text("Video:");
        ImGui.SameLine();
        // Make the video link selectable and react on click
        var linkColor = new Vector4(0.3f, 0.5f, 1.0f, 1.0f); // Blue link color
        ImGui.PushStyleColor(ImGuiCol.Text, linkColor);
        if (ImGui.Selectable(_selectedGuide.Video)) TryOpenUrl(_selectedGuide.Video);
        if (ImGui.IsItemHovered())
        {
            ImGui.SetTooltip("Abrir enlace de video en el navegador");
            ImGui.SetMouseCursor(ImGuiMouseCursor.Hand); // Indicate clickable
        }

        ImGui.PopStyleColor();
    }

    private void RenderRelatedGuidesSection()
    {
        if (_selectedGuide == null || !_selectedGuide.RelatedGuides.Any()) return;

        ImGui.Separator();
        ImGui.SeparatorText("Guías Relacionadas");

        // Fetch full guide objects for related IDs (assuming ApiManager has them)
        List<Guide> relatedGuidesFull = ApiManager.GetGuidesFromDB() // Inefficient: Ideally fetch only specific IDs
            .Where(g => _selectedGuide.RelatedGuides.Contains(g.Id))
            .ToList();

        if (!relatedGuidesFull.Any()) return;

        var linkColor = new Vector4(0.3f, 0.5f, 1.0f, 1.0f); // Blue link color
        ImGui.PushStyleColor(ImGuiCol.Text, linkColor);

        foreach (var relatedGuide in relatedGuidesFull)
        {
            var label = $"{relatedGuide.Title} - {relatedGuide.Author}";
            if (ImGui.Selectable(label))
                // Navigate to the related guide
                SetSelectedGuide(relatedGuide); // This updates history and loads the new guide
            if (ImGui.IsItemHovered())
            {
                ImGui.SetTooltip($"Ver la guía '{relatedGuide.Title}'");
                ImGui.SetMouseCursor(ImGuiMouseCursor.Hand);
            }
        }

        ImGui.PopStyleColor();
    }

    #endregion

    #region Helper Methods

    /// <summary>
    ///     Attempts to load an image from a URL asynchronously.
    ///     Uses ClickableTransparentOverlay's image loading helpers.
    /// </summary>
    private async void LoadGuideImageAsync(Guide? guide)
    {
        if (guide == null || string.IsNullOrWhiteSpace(guide.Image) || _guideImageHandles.ContainsKey(guide.Id) ||
            _isLoadingImage) return; // Skip if no image URL, already loaded, or currently loading

        _isLoadingImage = true;
        Console.WriteLine($"Loading image for guide {guide.Id}: {guide.Image}");
        // var handle = IntPtr.Zero;
        uint width = 400, height = 280;

        try
        {
            
            Console.WriteLine($"ImageBytes {GuideBrowserWindowRenderer.ImageBytes.Count}");
            GuideBrowserWindowRenderer.ImageBytes.TryGetValue(guide.Id, out var imgBytes);
            Console.WriteLine($"Bytes: {imgBytes.Length}");
            Image<Rgba32> img = Image.Load<Rgba32>(imgBytes);
            _overlayInstance.AddOrGetImagePointer($"{guide.Id}_guide_img", img, true, out var handle);

            Console.WriteLine($"Handle: {handle}");
            if (handle != IntPtr.Zero)
            {
                _guideImageHandles[guide.Id] = handle;
                _guideImageSizes[guide.Id] = new Vector2(width, height);
                Console.WriteLine($"Image loaded for guide {guide.Id}. Handle: {handle}, Size: {width}x{height}");
            }
            else
            {
                Console.WriteLine($"Failed to load image for guide {guide.Id}: Handle was zero.");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading image for guide {guide.Id} from {guide.Image}: {ex.Message}");
            // Optionally remove failed entry or mark as failed
            if (_guideImageHandles.ContainsKey(guide.Id)) _guideImageHandles.Remove(guide.Id);
            if (_guideImageSizes.ContainsKey(guide.Id)) _guideImageSizes.Remove(guide.Id);
        }
        finally
        {
            _isLoadingImage = false;
        }
    }

    /// <summary>
    ///     Tries to open a URL in the default system browser.
    /// </summary>
    private void TryOpenUrl(string url)
    {
        if (string.IsNullOrWhiteSpace(url)) return;
        try
        {
            // UseShellExecute = true is required on .NET Core/5+ to open URLs
            Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error opening URL '{url}': {ex.Message}");
            // Optionally show an error to the user
        }
    }

    #endregion
}