using System.Windows;

namespace ReactorSimulator
{
    public static class ResolutionManager
    {
        public static void applyResolution(string resolution, bool fullscreen, Window window) // Method to update the resolution when applied.
        {
            string[] parts = resolution.Split('x');
            int width = 0;
            int height = 0;

            if (window == null) // Checks it can actually provide valid windows to prevent an error.
            {
                MessageBox.Show("No window given to ResolutionManager.", "Resolution Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (parts.Length != 2 || !int.TryParse(parts[0], out width) || !int.TryParse(parts[1], out height)) // Cheks the resolution is valid, if not sets it to the default and tells the user.
            {
                MessageBox.Show("The resolution given to ResolutionManager is not valid. Defaulting to 1920x1080.", "Resolution Error", MessageBoxButton.OK, MessageBoxImage.Information);
                width = 1920;
                height = 1080;
            }

            if (fullscreen) // If fullscreen is on, resolution doesn't apply so no need for it.
            {
                window.WindowStyle = WindowStyle.None;
                window.ResizeMode = ResizeMode.NoResize;
                window.WindowState = WindowState.Maximized;
                return;
            }

            window.WindowStyle = WindowStyle.SingleBorderWindow;
            window.ResizeMode = ResizeMode.CanResize;
            window.WindowState = WindowState.Normal;
            window.Width = width;
            window.Height = height;

            switch (resolution) // Changes the resolution of the target window based on the resolution parsed.
            {
                case "2560x1440":
                    window.Width = 2560;
                    window.Height = 1440;
                    break;
                case "1920x1080":
                    window.Width = 1920;
                    window.Height = 1080;
                    break;
                case "1366x768":
                    window.Width = 1366;
                    window.Height = 768;
                    break;
                case "1280x1024":
                    window.Width = 1280;
                    window.Height = 1024;
                    break;
                case "1440x900":
                    window.Width = 1440;
                    window.Height = 900;
                    break;
                case "1600x900":
                    window.Width = 1600;
                    window.Height = 900;
                    break;
                case "1680x1050":
                    window.Width = 1680;
                    window.Height = 1050;
                    break;
                case "1280x800":
                    window.Width = 1280;
                    window.Height = 800;
                    break;
                case "1024x768":
                    window.Width = 1024;
                    window.Height = 768;
                    break;
                default:
                    break;
            }
        }
    }
}
