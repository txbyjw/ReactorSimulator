using System.Windows;

namespace ReactorSimulator
{
    public static class ResolutionManager // Class for the resolution manager.
    {
        public static void applyResolution(string resolution, bool fullscreen, Window window1, Window window2) // Method to update resolution of the window.
        {
            if (window1 == null  || window2 == null || string.IsNullOrEmpty(resolution)) // Checks it can actually provide valid data to prevent an error.
            {
                return;
            }

            if (fullscreen) // If fullscreen is on, resolution doesn't apply so no need for it.
            {
                window1.WindowStyle = WindowStyle.None;
                window1.ResizeMode = ResizeMode.NoResize;
                window1.WindowState = WindowState.Maximized;
                window2.WindowStyle = WindowStyle.None;
                window2.ResizeMode = ResizeMode.NoResize;
                window2.WindowState = WindowState.Maximized;
                return; // Stops the next part which reapplies the borders and resolution.
            }

            window1.WindowStyle = WindowStyle.SingleBorderWindow;
            window1.ResizeMode = ResizeMode.CanResize;
            window1.WindowState = WindowState.Normal;
            window2.WindowStyle = WindowStyle.SingleBorderWindow;
            window2.ResizeMode = ResizeMode.CanResize;
            window2.WindowState = WindowState.Normal;

            switch (resolution) // Changes the resolution of the target window based on the resolution parsed.
            {
                case "2560x1440":
                    window1.Width = 2560;
                    window1.Height = 1440;
                    window2.Width = 2560;
                    window2.Height = 1440;
                    break;
                case "1920x1080":
                    window1.Width = 1920;
                    window1.Height = 1080;
                    window2.Width = 1920;
                    window2.Height = 1080;
                    break;
                case "1366x768":
                    window1.Width = 1366;
                    window1.Height = 768;
                    window2.Width = 1366;
                    window2.Height = 768;
                    break;
                case "1280x1024":
                    window1.Width = 1280;
                    window1.Height = 1024;
                    window2.Width = 1280;
                    window2.Height = 1024;
                    break;
                case "1440x900":
                    window1.Width = 1440;
                    window1.Height = 900;
                    window2.Width = 1440;
                    window2.Height = 900;
                    break;
                case "1600x900":
                    window1.Width = 1600;
                    window1.Height = 900;
                    window2.Width = 1600;
                    window2.Height = 900;
                    break;
                case "1680x1050":
                    window1.Width = 1680;
                    window1.Height = 1050;
                    window2.Width = 1680;
                    window2.Height = 1050;
                    break;
                case "1280x800":
                    window1.Width = 1280;
                    window1.Height = 800;
                    window2.Width = 1280;
                    window2.Height = 800;
                    break;
                case "1024x768":
                    window1.Width = 1024;
                    window1.Height = 768;
                    window2.Width = 1024;
                    window2.Height = 768;
                    break;
                default:
                    break;
            }
        }
    }
}
