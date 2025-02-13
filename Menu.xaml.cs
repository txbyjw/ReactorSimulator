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
        private int currentScenarioIndex = 0;

        private Reactor reactor;
        private Core core;
        private ControlRods controlRods;
        private Pressuriser pressuriser;
        private PrimaryCoolingLoop primaryLoop;
        private SecondaryCoolingLoop secondaryLoop;
        private PowerGeneration powerGeneration;
        private Simulation simulation;

        public int _scenarioIndex
        {
            get { return currentScenarioIndex; }
            set { currentScenarioIndex = value; }
        }

        public Menu(string user)
        {
            InitializeComponent();
            currentUser.Text = $"Currently logged in as {user}.";
            int scenarioIndex = currentScenarioIndex + 1;
            ScenarioData scenarioData = new ScenarioData();

            core = new Core(reactor, scenarioData, 0, 0, 0, 0, 0, 0); // Have to parse a double so hopefully resetting to 0 will work as it loads scenario anyway. Same for below.
            controlRods = new ControlRods(0);
            pressuriser = new Pressuriser(0, 0, 0, 0, false, false, false);
            primaryLoop = new PrimaryCoolingLoop(0, 0, 0);
            secondaryLoop = new SecondaryCoolingLoop(0, 0, 0);
            powerGeneration = new PowerGeneration(reactor, 0, 0);
            simulation = new Simulation();

            string scenarioFolder = "D:\\Computing\\ReactorSimulator\\data\\scenarios";
            loadScenarios(scenarioFolder);

            SettingsData settingsData = SettingsData.Load();
            SettingsManager.applySettings(settingsData, this, simulation);
        }

        private void loadScenarios(string path)
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

        private void previousScenario(object sender, RoutedEventArgs e)
        {
            currentScenarioIndex = (currentScenarioIndex == 0) ? scenarios.Count - 1 : currentScenarioIndex - 1;
            updateScenario();
        }

        private void nextScenario(object sender, RoutedEventArgs e)
        {
            currentScenarioIndex = (currentScenarioIndex == scenarios.Count - 1) ? 0 : currentScenarioIndex + 1;
            updateScenario();
        }

        private void updateScenario()
        {
            scenarioSelector.Text = scenarios[currentScenarioIndex];
            scenarioDescription.Text = scenarioDescriptions[scenarios[currentScenarioIndex]];
        }

        private void beginSimulation(object sender, RoutedEventArgs e)
        {
            Simulation simulationWindow = new Simulation();
            simulationWindow.Show();
            this.Close();
        }

        private void openSettings(object sender, RoutedEventArgs e)
        {
            Settings settingsWindow = new Settings(this);
            settingsWindow.ShowDialog();
        }

        private void openDocumentation(object sender, RoutedEventArgs e)
        {
            // Need to implement. Maybe a webpage? May be omitted idk if i cba
        }

        private void openEditor(object sender, RoutedEventArgs e)
        {
            // need to add - this will be easy (hopefully)
        }

        private void userSelection(object sender, RoutedEventArgs e)
        {
            Authenticator authenticatorWindow = new Authenticator();
            authenticatorWindow.Show();
            this.Close();
        }

        private void openStatistics(object sender, RoutedEventArgs e)
        {
            // again need to add.
        }

        private void quit(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }
    }
}