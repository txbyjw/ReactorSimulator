using System;
using System.IO;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Windows;
using System.Windows.Threading;

namespace ReactorSimulator
{
    public partial class Simulation : Window
    {
        private DispatcherTimer timer;
        private Reactor reactor;
        private Variables variables;

        private double coreTemperature;
        private double corePressure;
        private double coreCoolantFlow;
        private double coreReactivity;
        private double coreIntegrity;
        private double coreFuelIntegrity;

        private double controlRodsInsertionLevel;

        private double pressuriserTemperature;
        private double pressuriserPressure;
        private double pressuriserFillLevel;
        private double pressuriserHeatingPower;
        private bool pressuriserHeaterOn;
        private bool pressuriserReliefValveOpen;
        private bool pressuriserSprayNozzlesActive;

        private double pclTemperature;
        private double pclPressure;
        private double pclCoolantFlow;
        private bool pclLowFlow;
        private bool pclLowPressure;
        private bool pclHighTemperature;

        private double sclTemperature;
        private double sclPressure;
        private double sclCoolantFlow;
        private bool sclLowFlow;
        private bool sclLowPressure;
        private bool sclHighTemperature;

        private double pgPowerOutput;
        private double pgThermalPower;

        public Simulation(Core core, ControlRods controlRods, Pressuriser pressuriser, PrimaryCoolingLoop primaryCoolingLoop, SecondaryCoolingLoop secondaryCoolingLoop, PowerGeneration powerGeneration)
        {
            InitializeComponent();
            InitialiseSimulation();

            reactor = new Reactor(this);

            variables = new Variables(core, controlRods, pressuriser, primaryCoolingLoop, secondaryCoolingLoop, powerGeneration);
            DataContext = variables;

            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromMilliseconds(500); // Update every second
            timer.Tick += updateUI;
            timer.Start();
        }

        private void InitialiseSimulation()
        {
            string filePath = "F:\\Computing\\A-Level Computer Science NEA\\data\\scenarios\\testScenario.json"; // Temporary until it is all working then will add proper selection.

            if (File.Exists(filePath))
            {
                try
                {
                    string json = File.ReadAllText(filePath);
                    ScenarioData scenario = JsonSerializer.Deserialize<ScenarioData>(json);

                    variables = new Variables(scenario);
                    DataContext = variables;

                    coreTemperature = scenario.coreTemperature;
                    corePressure = scenario.corePressure;
                    coreCoolantFlow = scenario.coreCoolantFlow;
                    coreReactivity = scenario.coreReactivity;
                    coreIntegrity = scenario.coreIntegrity;
                    coreFuelIntegrity = scenario.coreFuelIntegrity;

                    controlRodsInsertionLevel = scenario.controlRodsInsertionLevel;

                    pressuriserTemperature = scenario.pressuriserTemperature;
                    pressuriserPressure = scenario.pressuriserPressure;
                    pressuriserFillLevel = scenario.pressuriserFillLevel;
                    pressuriserHeatingPower = scenario.pressuriserHeatingPower;
                    pressuriserHeaterOn = scenario.pressuriserHeaterOn;
                    pressuriserReliefValveOpen = scenario.pressuriserReliefValveOpen;
                    pressuriserSprayNozzlesActive = scenario.pressuriserSprayNozzlesActive;

                    pclTemperature = scenario.pclTemperature;
                    pclPressure = scenario.pclPressure;
                    pclCoolantFlow = scenario.pclCoolantFlow;
                    pclLowFlow = scenario.pclLowFlow;
                    pclLowPressure = scenario.pclLowPressure;
                    pclHighTemperature = scenario.pclHighTemperature;

                    sclTemperature = scenario.sclTemperature;
                    sclPressure = scenario.sclPressure;
                    sclCoolantFlow = scenario.sclCoolantFlow;
                    sclLowFlow = scenario.sclLowFlow;
                    sclLowPressure = scenario.sclLowPressure;
                    sclHighTemperature = scenario.sclHighTemperature;

                    pgPowerOutput = scenario.pgPowerOutput;
                    pgThermalPower = scenario.pgThermalPower;

                    updateUI();
                    MessageBox.Show($"Loaded scenario!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                }

                catch (Exception ex)
                {
                    MessageBox.Show($"Error loading chosen scenario: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else
            {
                MessageBox.Show("Could not find requested scenario file.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public void updateUI(object sender, EventArgs e)
        {
            CoreTemperature.Text = $"Temperature: {coreTemperature}°C";
            CorePressure.Text = $"Pressure: {corePressure} bar";
            CoreReactivity.Text = $"Reactivity: {coreReactivity}%";

            PressuriserPressure.Text = $"Pressure: {pressuriserPressure} bar";
            PressuriserTemperature.Text = $"Temperature: {pressuriserTemperature}°C";
            PressuriserWaterLevel.Text = $"Water Level: {pressuriserFillLevel}%";

            PrimaryCoolantTemp.Text = $"Primary Coolant: {pclTemperature}°C";
            SecondaryCoolantTemp.Text = $"Secondary Coolant: {sclTemperature}°C";

            PowerOutput.Text = $"Power Output: {pgPowerOutput}MW";
            ThermalPower.Text = $"Thermal Power Output: {pgThermalPower}MW";

            dangerIndicator.Text = pclLowFlow || pclLowPressure || pclHighTemperature || sclLowFlow || sclLowPressure || sclHighTemperature ? "Danger: Alert!" : "Danger: Safe";
        }

        public void updateUI()
        {
            updateUI(null, null);
        }
    }

    public class ScenarioData
    {
        public double coreTemperature { get; set; }
        public double corePressure { get; set; }
        public double coreCoolantFlow { get; set; }
        public double coreReactivity { get; set; }
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

        public double pclTemperature { get; set; }
        public double pclPressure { get; set; }
        public double pclCoolantFlow { get; set; }
        public bool pclLowFlow { get; set; }
        public bool pclLowPressure { get; set; }
        public bool pclHighTemperature { get; set; }

        public double sclTemperature { get; set; }
        public double sclPressure { get; set; }
        public double sclCoolantFlow { get; set; }
        public bool sclLowFlow { get; set; }
        public bool sclLowPressure { get; set; }
        public bool sclHighTemperature { get; set; }

        public double pgPowerOutput { get; set; }
        public double pgThermalPower { get; set; }
    }
}