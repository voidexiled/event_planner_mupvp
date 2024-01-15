using IMGUITEST;
using System.Runtime.InteropServices;
using ImGuiNET;
namespace Main
{
    public class Program
    {



        [DllImport("kernel32.dll")]
        static extern IntPtr GetConsoleWindow();

        [DllImport("user32.dll")]
        static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        const int SW_HIDE = 0;
        const int SW_SHOW = 5;


        public static void Main(string[] args)
        {

            //var defFont = ImGui.GetIO().Fonts.AddFontFromFileTTF("C:\\Windows\\Fonts\\Arial.ttf", 13, null, ImGui.GetIO().Fonts.GetGlyphRangesDefault());



            var handle = GetConsoleWindow();

            SessionManager.LoadSession();

            RenderClass ova = new RenderClass();
            ItemManager.PopulateItems();
            ova.Start().Wait();
            // Hide
            //ShowWindow(handle, SW_SHOW);




        }
        //#pragma comment(linker, "/SUBSYSTEM:windows /ENTRY:mainCRTStartup")

    }
}