using System;
using System.IO;
using System.Windows;
using System.Text.Json;
using System.Windows.Controls;
using System.Windows.Automation;

// Need to add clock interval, option to make custom scenarios, warning volume (if i add sounds).

namespace ReactorSimulator
{
    public class SettingsData // Class to hold application settings, serialisable.
    {
        public string resolution { get; set; } = "1920x1080";
        public bool fullscreen { get; set; } = false;

        public void save(string userID) // Method which saves settings to the json file.
        {
            string folderPath = "F:\\Computing\\ReactorSimulator\\data\\options";

            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }

            string filePath = Path.Combine(folderPath, $"{userID}.json");

            try
            {
                var options = new JsonSerializerOptions { WriteIndented = true };
                string json = JsonSerializer.Serialize(this, options);
                File.WriteAllText(filePath, json);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving settings: {ex.Message}\n{ex.StackTrace}.", "SettingsData Save Method Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public static SettingsData load(string userID) // Method to load settings from the json.
        {
            string folderPath = "F:\\Computing\\ReactorSimulator\\data\\options";
            string filePath = Path.Combine(folderPath, $"{userID}.json");

            if (!File.Exists(filePath))
            {
                var defaultSettings = new SettingsData();
                defaultSettings.save(userID);
                return defaultSettings;
            }

            try
            {
                string json = File.ReadAllText(filePath);
                return JsonSerializer.Deserialize<SettingsData>(json) ?? new SettingsData();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading settings: {ex.Message}\n{ex.StackTrace}.", "SettingsData Load Method Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return new SettingsData();
            }
        }
    }

    public static class SettingsManager
    {
        public static SettingsData currentUserSettings;
        private static string currentUserID;

        public static void loadUserSettings(string userID, Window menuWindow, Window settingsWindow, Window simulationWindow) // Method that loads the user ID and passes it to the load method in SettingsData, then applies the settings (done on launch)
        {
            currentUserID = userID;
            currentUserSettings = SettingsData.load(currentUserID);
            applyUserSettings(menuWindow, settingsWindow, simulationWindow);
        }

        public static void saveUserSettings() // Calls the save method in SettingsData.
        {
            if (currentUserSettings != null && currentUserID != null)
            {
                currentUserSettings.save(currentUserID);
            }
        }

        public static void applyUserSettings(Window menuWindow, Window settingsWindow, Window simulationWindow) // Method to apply the setings to the open window(s).
        {
            if (currentUserSettings != null)
            {
                if (menuWindow != null)
                {
                    ResolutionManager.applyResolution(currentUserSettings.resolution, currentUserSettings.fullscreen, menuWindow);
                }

                if (simulationWindow != null)
                {
                    ResolutionManager.applyResolution(currentUserSettings.resolution, currentUserSettings.fullscreen, simulationWindow);
                }

                if (settingsWindow != null)
                {
                    string[] parts = currentUserSettings.resolution.Split("x");

                    if (parts.Length == 2 && int.TryParse(parts[0], out int width) && int.TryParse(parts[1], out int height))
                    {
                        settingsWindow.Width = Math.Clamp(width * 0.4, 300, 500);
                        settingsWindow.Height = Math.Clamp(height * 0.5, 500, 600);
                    }
                }
            }
        }

        public static void unloadUserSettings() // Method called when the user logs out to unload the settings.
        {
            currentUserSettings = null;
            currentUserID = null;
        }

        public static string getUserID() // Same as the getUserID method in the Authenticator.
        {
            string filePath = "F:\\Computing\\ReactorSimulator\\data\\options";
            string[] files = Directory.GetFiles(filePath);

            if (files.Length == 0)
            {
                return "1";
            }

            int lastItem = 0;
            foreach (string file in files)
            {
                string fileName = Path.GetFileNameWithoutExtension(file);
                if (int.TryParse(fileName, out int number))
                {
                    lastItem = Math.Max(lastItem, number);
                }
            }
            return (lastItem + 1).ToString();
        }
    }

    public partial class Settings : Window
    {
        private Menu menu;
        private SettingsData settingsData;

        public Settings(Menu menu)
        {
            InitializeComponent();
            this.menu = menu;
            this.Topmost = true;

            settingsData = SettingsManager.currentUserSettings;

            if (settingsData != null)
            {
                foreach (ComboBoxItem item in resolutionOption.Items)
                {
                    if (item.Content.ToString() == settingsData.resolution)
                    {
                        resolutionOption.SelectedItem = item;
                        break;
                    }
                }
                fullscreenCheckbox.IsChecked = settingsData.fullscreen;
            }

            SettingsManager.applyUserSettings(null, this, null);
        }

        private void saveSettings() // Does what it says on the tin
        {
            if (settingsData != null)
            {
                settingsData.resolution = (resolutionOption.SelectedItem as ComboBoxItem)?.Content.ToString() ?? "1920x1080";
                settingsData.fullscreen = fullscreenCheckbox.IsChecked ?? false;
            }
        }

        private void fullscreenChecked(object sender, RoutedEventArgs e) // Method called from the checkbox, calls the save method.
        {
            if (settingsData != null)
            {
                settingsData.fullscreen = fullscreenCheckbox.IsChecked ?? false;
            }

            saveSettings();
            SettingsManager.saveUserSettings();
        }

        private void applyResolution(object sender, EventArgs e) // Method that applies the resolution, called from the "apply" button.
        {
            saveSettings();
            SettingsManager.saveUserSettings();
            SettingsManager.applyUserSettings(menu, this, null);
            this.Close();
        }
    }
}