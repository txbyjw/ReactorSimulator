using System;
using System.IO;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Windows;
using System.Windows.Threading;
using System.ComponentModel;

namespace ReactorSimulator
{
    public partial class Simulation : Window
    {
        private DispatcherTimer timer;
        private Reactor reactor;
        private Menu menu;

        private Core core;
        private ControlRods controlRods;
        private Pressuriser pressuriser;
        private PrimaryCoolingLoop primaryLoop;
        private SecondaryCoolingLoop secondaryLoop;
        private PowerGeneration powerGeneration;

        ScenarioData scenarioData = new ScenarioData();

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

        private double primaryLoopTemperature;
        private double primaryLoopPressure;
        private double primaryLoopCoolantFlow;
        private bool primaryLoopLowFlow;
        private bool primaryLoopLowPressure;
        private bool primaryLoopHighTemperature;

        private double secondaryLoopTemperature;
        private double secondaryLoopPressure;
        private double secondaryLoopCoolantFlow;
        private bool secondaryLoopLowFlow;
        private bool secondaryLoopLowPressure;
        private bool secondaryLoopHighTemperature;

        private double powerGenerationPowerOutput;
        private double powerGenerationThermalPower;

        private int _scenarioIndex;

        public Simulation()
        {
            InitializeComponent();
            ScenarioData scenarioData = new ScenarioData();

            reactor = new Reactor(this, scenarioData);

            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromMilliseconds(1000); // Update every second
            // timer.Tick += updateUI;
            timer.Start();
        }

        public void loadScenario()
        {
            string filePath = $"D:\\Computing\\ReactorSimulator\\data\\scenarios\\scenario2.json"; // Temporary until it is all working then will add proper selection.

            if (File.Exists(filePath))
            {
                try
                {
                    string json = File.ReadAllText(filePath);
                    ScenarioData scenario = JsonSerializer.Deserialize<ScenarioData>(json);

                    DataContext = reactor;

                    coreTemperature = scenario.coreTemperature;
                    corePressure = scenario.corePressure;
                    coreCoolantFlow = scenario.coreCoolantFlow;
                    coreReactivity = scenario.coreReactivity;
                    coreIntegrity = scenario.coreIntegrity;
                    coreFuelIntegrity = scenario.coreFuelIntegrity;

                    controlRodsInsertionLevel = scenario.controlRodInsertionLevel;

                    pressuriserTemperature = scenario.pressuriserTemperature;
                    pressuriserPressure = scenario.pressuriserPressure;
                    pressuriserFillLevel = scenario.pressuriserFillLevel;
                    pressuriserHeatingPower = scenario.pressuriserHeatingPower;
                    pressuriserHeaterOn = scenario.pressuriserHeaterOn;
                    pressuriserReliefValveOpen = scenario.pressuriserReliefValveOpen;
                    pressuriserSprayNozzlesActive = scenario.pressuriserSprayNozzlesActive;

                    primaryLoopTemperature = scenario.primaryLoopTemperature;
                    primaryLoopPressure = scenario.primaryLoopPressure;
                    primaryLoopCoolantFlow = scenario.primaryLoopCoolantFlow;
                    primaryLoopLowFlow = scenario.primaryLoopLowFlow;
                    primaryLoopLowPressure = scenario.primaryLoopLowPressure;
                    primaryLoopHighTemperature = scenario.primaryLoopHighTemperature;

                    secondaryLoopTemperature = scenario.secondaryLoopTemperature;
                    secondaryLoopPressure = scenario.secondaryLoopPressure;
                    secondaryLoopCoolantFlow = scenario.secondaryLoopCoolantFlow;
                    secondaryLoopLowFlow = scenario.secondaryLoopLowFlow;
                    secondaryLoopLowPressure = scenario.secondaryLoopLowPressure;
                    secondaryLoopHighTemperature = scenario.secondaryLoopHighTemperature;

                    powerGenerationPowerOutput = scenario.powerGenerationPowerOutput;
                    powerGenerationThermalPower = scenario.powerGenerationThermalPower;

                    updateUI();
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

        public void updateUI(object sender, EventArgs e) // Updates the values on the UI with the variables received. F2 used to format to 2 dp for ease to the user.
        {
            CoreTemperature.Text = core.coreTemperature.ToString("F2");
            CorePressure.Text = core.corePressure.ToString("F2");
            CoreReactivity.Text = core.coreReactivity.ToString("F2");
            CoreCoolantFlow.Text = core.coreCoolantFlow.ToString("F2");

            ControlRodInsertion.Text = controlRods.ControlRodsInsertionLevel.ToString("F2");

            PressuriserTemperature.Text = pressuriser.pressuriserTemperature.ToString("F2");
            PressuriserPressure.Text = pressuriser.pressuriserPressure.ToString("F2");
            PressuriserWaterLevel.Text = pressuriser.pressuriserWaterLevel.ToString("F2");
            PressuriserHeatingPower.Text = pressuriser.pressuriserHeatingPower.ToString("F2");

            PrimaryCoolantTemp.Text = primaryLoop.primaryLoopCoolantTemperature.ToString("F2");
            PrimaryCoolantPressure.Text = primaryLoop.primaryLoopCoolantPressure.ToString("F2");
            PrimaryCoolantFlow.Text = primaryLoop.primaryLoopCoolantFlow.ToString("F2");

            SecondaryCoolantTemp.Text = secondaryLoop.secondaryLoopSteamTemperature.ToString("F2");
            SecondaryCoolantPressure.Text = secondaryLoop.secondaryLoopSteamPressure.ToString("F2");
            SecondaryCoolantFlow.Text = secondaryLoop.secondaryLoopSteamFlow.ToString("F2");

            PowerOutput.Text = powerGeneration.powerGenerationPowerOutput.ToString("F2");
            ThermalPower.Text = powerGeneration.powerGenerationThermalPower.ToString("F2");

            dangerIndicator.Text = primaryLoopLowFlow || primaryLoopLowPressure || primaryLoopHighTemperature || secondaryLoopLowFlow || secondaryLoopLowPressure || secondaryLoopHighTemperature ? "Danger: Alert!" : "Danger: Safe";
        }

        public void updateUI() // Have to have this duplicated parsing null otherwise I get an error.
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

        public double controlRodInsertionLevel { get; set; }

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
}