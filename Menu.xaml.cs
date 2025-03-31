using System.Windows;
using System.Windows.Input;
using System.Windows.Controls;
using System.IO;
using System.Text.Json;
using System.Security.Cryptography.X509Certificates;

namespace ReactorSimulator
{
    public partial class Menu : Window
    {
        private List<int> scenarioIndex;
        private List<string> scenarios;
        private Dictionary<string, string> scenarioDescriptions;
        private Settings settingsWindow;
        private Simulation simulationWindow;
        private int currentScenarioIndex = 0;

        public int _scenarioIndex
        {
            get { return currentScenarioIndex; }
            set { currentScenarioIndex = value; }
        }

        public Menu(string username, string userID) // Constructor
        {
            InitializeComponent();
            currentUser.Text = $"Currently logged in as {username}.";
            SettingsManager.loadUserSettings(userID, this, null, null);
            SettingsManager.applyUserSettings(this, null, null);
            int scenarioIndex = currentScenarioIndex + 1;
            ScenarioData scenarioData = new ScenarioData();

            string scenarioFolder = "F:\\Computing\\ReactorSimulator\\data\\scenarios";
            loadScenarios(scenarioFolder);
        }

        private void loadScenarios(string path) // Method that loads all of the scenarios in the scenario folder.
        {
            scenarioIndex = new List<int>();
            scenarios = new List<string>();
            scenarioDescriptions = new Dictionary<string, string>();

            if (!Directory.Exists(path))
            {
                MessageBox.Show("Could not find scenario path. Scenarios cannot be loaded.", "Scenario Loader", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            foreach (string file in Directory.GetFiles(path, "*.json"))
            {
                try
                {
                    string content = File.ReadAllText(file);
                    using JsonDocument doc = JsonDocument.Parse(content);

                    var metadata = doc.RootElement.GetProperty("metadata");
                    int number = metadata.GetProperty("scenario").GetInt16();
                    string name = metadata.GetProperty("name").GetString();
                    string description = metadata.GetProperty("description").GetString();

                    scenarioIndex.Add(number);
                    scenarios.Add(name);
                    scenarioDescriptions[name] = description;
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error whilst reading {file} at {path}: {ex.Message}", "Scenario Loader", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }

            if (scenarios.Count > 0)
            {
                updateScenario();
            }
        }

        private void previousScenario(object sender, RoutedEventArgs e) // Moves to the previous displayed scenario.
        {
            currentScenarioIndex = (currentScenarioIndex == 0) ? scenarios.Count - 1 : currentScenarioIndex - 1;
            updateScenario();
        }

        private void nextScenario(object sender, RoutedEventArgs e) // Moves to the next displayed scenario.
        {
            currentScenarioIndex = (currentScenarioIndex == scenarios.Count - 1) ? 0 : currentScenarioIndex + 1;
            updateScenario();
        }

        private void updateScenario() // Method that actually updates the displayed scenario from the index.
        {
            scenarioSelector.Text = scenarios[currentScenarioIndex];
            scenarioDescription.Text = scenarioDescriptions[scenarios[currentScenarioIndex]];
        }

        private void beginSimulation(object sender, RoutedEventArgs e) // Method called from the begin button, starts the simulation.
        {
            simulationWindow = new Simulation(currentScenarioIndex);
            SettingsManager.applyUserSettings(this, settingsWindow, simulationWindow);
            simulationWindow.Show();
            this.Close();
        }

        private void openSettings(object sender, RoutedEventArgs e) // MEthod called from the settings button, opens the settings window.
        {
            settingsWindow = new Settings(this);
            settingsWindow.Show();
        }

        private void openDocumentation(object sender, RoutedEventArgs e)
        {
            // Need to implement. Maybe a webpage? May be omitted idk if i cba
        }

        private void openEditor(object sender, RoutedEventArgs e)
        {
            // need to add - this will be easy (hopefully)
        }

        private void userSelection(object sender, RoutedEventArgs e) // Called from the change user button, goes back to the authenticator.
        {
            SettingsManager.unloadUserSettings();
            Authenticator authenticatorWindow = new Authenticator();
            authenticatorWindow.Show();
            this.Close();
        }

        private void openStatistics(object sender, RoutedEventArgs e)
        {
            // again need to add.
        }

        private void quit(object sender, RoutedEventArgs e) // Needs no explanation really.
        {
            Application.Current.Shutdown();
        }
    }
}