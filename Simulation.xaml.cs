using System;
using System.IO;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Windows;
using System.Windows.Threading;
using System.ComponentModel;
using System.Text.Json.Serialization;
using System.Windows.Data;
using System.Windows.Media;
using System.Globalization;

namespace ReactorSimulator
{
    public partial class Simulation : Window
    {
        private Reactor reactor;

        public Simulation(int index)
        {
            InitializeComponent();
            loadScenario(index);
            SettingsManager.applyUserSettings(null, null, this);
        }

        public void loadScenario(int scenarioIndex) // Method that loads the scenario data into the simulation and sets the default values.
        {
            string folder = "F:\\Computing\\ReactorSimulator\\data\\scenarios";
            string filePath = Path.Combine(folder, $"scenario{scenarioIndex + 1}.json");

            if (File.Exists(filePath))
            {
                try
                {
                    string json = File.ReadAllText(filePath);
                    ScenarioData scenario = JsonSerializer.Deserialize<ScenarioData>(json);

                    double coreTemperature = scenario.coreTemperature;
                    double corePressure = scenario.corePressure;
                    double coreReactivity = scenario.coreReactivity;
                    double coreNeutronFlux = scenario.coreNeutronFlux;
                    double coreIntegrity = scenario.coreIntegrity;
                    double coreFuelIntegrity = scenario.coreFuelIntegrity;

                    double controlRodsInsertionLevel = scenario.controlRodsInsertionLevel;

                    double pressuriserTemperature = scenario.pressuriserTemperature;
                    double pressuriserPressure = scenario.pressuriserPressure;
                    double pressuriserFillLevel = scenario.pressuriserFillLevel;
                    double pressuriserHeatingPower = scenario.pressuriserHeatingPower;
                    bool pressuriserHeaterOn = scenario.pressuriserHeaterOn;
                    bool pressuriserReliefValveOpen = scenario.pressuriserReliefValveOpen;
                    bool pressuriserSprayNozzlesActive = scenario.pressuriserSprayNozzlesActive;

                    double primaryLoopTemperature = scenario.primaryLoopTemperature;
                    double primaryLoopPressure = scenario.primaryLoopPressure;
                    double primaryLoopCoolantFlow = scenario.primaryLoopCoolantFlow;
                    bool primaryLoopLowFlow = scenario.primaryLoopLowFlow;
                    bool primaryLoopLowPressure = scenario.primaryLoopLowPressure;
                    bool primaryLoopHighTemperature = scenario.primaryLoopHighTemperature;

                    double secondaryLoopTemperature = scenario.secondaryLoopTemperature;
                    double secondaryLoopPressure = scenario.secondaryLoopPressure;
                    double secondaryLoopCoolantFlow = scenario.secondaryLoopCoolantFlow;
                    bool secondaryLoopLowFlow = scenario.secondaryLoopLowFlow;
                    bool secondaryLoopLowPressure = scenario.secondaryLoopLowPressure;
                    bool secondaryLoopHighTemperature = scenario.secondaryLoopHighTemperature;

                    double powerGenerationPowerOutput = scenario.powerGenerationPowerOutput;
                    double powerGenerationThermalPower = scenario.powerGenerationThermalPower;

                    reactor = new Reactor(this, scenario);
                    while (DataContext == null)
                    {
                        this.DataContext = reactor;
                    }

                    MessageBox.Show($"Successfully loaded scenario!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                }

                catch (Exception ex)
                {
                    MessageBox.Show($"Error loading chosen scenario: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else
            {
                MessageBox.Show("Could not find requested scenario file. Simulation may not initialise correctly.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }

    public class ScenarioData
    {
        public double coreTemperature { get; set; }
        public double corePressure { get; set; }
        public double coreCoolantFlow { get; set; }
        public double coreReactivity { get; set; }
        public double coreNeutronFlux { get; set; }
        public double coreIntegrity { get; set; }
        public double coreFuelIntegrity { get; set; }

        public double controlRodsInsertionLevel { get; set; }

        public double pressuriserTemperature { get; set; }
        public double pressuriserPressure { get; set; }
        public double pressuriserFillLevel { get; set; }
        public double pressuriserHeatingPower { get; set; }
        public bool pressuriserHeaterOn { get; set; }
        public bool pressuriserReliefValveOpen { get; set; }
        public bool pressuriserSprayNozzlesActive { get; set; }

        public double primaryLoopTemperature { get; set; }
        public double primaryLoopPressure { get; set; }
        public double primaryLoopCoolantFlow { get; set; }
        public bool primaryLoopLowFlow { get; set; }
        public bool primaryLoopLowPressure { get; set; }
        public bool primaryLoopHighTemperature { get; set; }

        public double secondaryLoopTemperature { get; set; }
        public double secondaryLoopPressure { get; set; }
        public double secondaryLoopCoolantFlow { get; set; }
        public bool secondaryLoopLowFlow { get; set; }
        public bool secondaryLoopLowPressure { get; set; }
        public bool secondaryLoopHighTemperature { get; set; }

        public double powerGenerationPowerOutput { get; set; }
        public double powerGenerationThermalPower { get; set; }
    }

    public class BoolToColourConverter : IValueConverter // This class changes bool values into colours for the indicators.
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool boolValue)
            {
                return boolValue ? Brushes.Green : Brushes.Red;
            }
            return Brushes.Gray;
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) // Have to have this otherwise this feature doesn't work but this is never used as it's never converted back.
        {
            throw new NotImplementedException();
        }
    }
}