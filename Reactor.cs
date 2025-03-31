using System;
using System.IO;
using System.ComponentModel;
using System.Windows;
using System.Windows.Threading;
using Microsoft.Win32;
using System.Diagnostics;

namespace ReactorSimulator
{
    public class Reactor : INotifyPropertyChanged
    {
        private double timeElapsed = 0;
        private string filePath = "F:\\Computing\\ReactorSimulator\\data\\log.txt";

        private DispatcherTimer timer;
        public event PropertyChangedEventHandler PropertyChanged;

        public Core core { get; private set; }
        public ControlRods controlRods { get; private set; }
        public Pressuriser pressuriser { get; private set; }
        public PrimaryCoolingLoop primaryLoop { get; private set; }
        public SecondaryCoolingLoop secondaryLoop { get; private set; }
        public PowerGeneration powerGeneration { get; private set; }
        public Simulation simulationWindow;

        public Reactor(Simulation window, ScenarioData scenarioData) // Constructor
        {
            simulationWindow = window;
            bool isInitialised = false;

            if (scenarioData == null)
            {
                MessageBox.Show("ScenarioData is not being initialised.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // Initialising all of the components and passing the data from scenario data to them.
            core = new Core(scenarioData.coreTemperature, scenarioData.corePressure, scenarioData.coreReactivity, scenarioData.coreNeutronFlux, scenarioData.coreCoolantFlow, scenarioData.coreIntegrity, scenarioData.coreFuelIntegrity);
            controlRods = new ControlRods(scenarioData.controlRodsInsertionLevel);
            pressuriser = new Pressuriser(scenarioData.pressuriserTemperature, scenarioData.pressuriserPressure, scenarioData.pressuriserFillLevel, scenarioData.pressuriserHeatingPower, scenarioData.pressuriserHeaterOn, scenarioData.pressuriserReliefValveOpen, scenarioData.pressuriserSprayNozzlesActive);
            primaryLoop = new PrimaryCoolingLoop(scenarioData.primaryLoopTemperature, scenarioData.primaryLoopPressure, scenarioData.primaryLoopCoolantFlow);
            secondaryLoop = new SecondaryCoolingLoop(scenarioData.secondaryLoopTemperature, scenarioData.secondaryLoopPressure, scenarioData.secondaryLoopCoolantFlow);
            powerGeneration = new PowerGeneration(scenarioData.powerGenerationPowerOutput, scenarioData.powerGenerationThermalPower);

            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(1); // Temporarily, this will be user-set later.
            timer.Tick += (sender, e) => updateSimulation(isInitialised);
            timer.Start();

            if (File.Exists(filePath)) // Makes a new log every time the simulation is started.
            {
                try
                {
                    File.Delete(filePath);
                }
                catch (IOException ex)
                {
                    MessageBox.Show($"Could not delete the log file: {ex.Message}", "Log Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }

            try
            {
                using (File.Create(filePath)) { }
            }
            catch (IOException ex)
            {
                MessageBox.Show($"Could not create a log file: {ex.Message}", "Log Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void updateSimulation(bool isInitialised) // The main method which calls all of the individual update calls and the log.
        {
            if (!isInitialised)
            {
                await Task.Delay(1000);  // Waits a second to allow everything to load properly, added as a debug for the DataBinding but decided to leave as this could prevent any future issues anyway.
            }

            if (core == null || controlRods == null || pressuriser == null || primaryLoop == null || secondaryLoop == null || powerGeneration == null) // Stops the simulation if it doesn't initialise correctly, may try to change this to try again a few times before closing.
            {
                MessageBox.Show("A component is null and the program cannot initialise.\nTerminating...", "Simulation Error", MessageBoxButton.OK, MessageBoxImage.Error);
                await Task.Delay(3000);
                Application.Current.Shutdown();
            }
            else
            {
                isInitialised = true;
            }

            timeElapsed += 1;

            core.updateCore(controlRods.controlRodsInsertionLevel, pressuriser.pressuriserTemperature, pressuriser.pressuriserPressure, primaryLoop.primaryLoopCoolantTemperature, primaryLoop.primaryLoopCoolantPressure, primaryLoop.primaryLoopCoolantFlow, powerGeneration.powerGenerationThermalPower);
            controlRods.updateControlRods(core.coreNeutronFlux);
            pressuriser.updatePressuriser(core.corePressure, primaryLoop.primaryLoopCoolantTemperature, primaryLoop.primaryLoopCoolantFlow);
            primaryLoop.updatePrimaryCoolingLoop(core.coreTemperature, core.corePressure, pressuriser.pressuriserPressure, powerGeneration.powerGenerationThermalPower);
            secondaryLoop.updateSecondaryCoolingLoop(primaryLoop.primaryLoopCoolantTemperature);
            powerGeneration.updatePowerGeneration(core.coreTemperature, core.corePressure, core.coreReactivity, primaryLoop.primaryLoopCoolantTemperature, primaryLoop.primaryLoopCoolantPressure, primaryLoop.primaryLoopCoolantPressure, secondaryLoop.secondaryLoopSteamTemperature, secondaryLoop.secondaryLoopSteamPressure, secondaryLoop.secondaryLoopSteamFlow);
            updateLog();
        }

        private void updateLog() // Method to write all values to a text file every cycle, using this for debugging calculations but might end up keeping.
        {
            using (StreamWriter writer = new StreamWriter(filePath, true))
            {
                writer.WriteLine($"New update: {timeElapsed}s");
                writer.WriteLine($"Core Temperature: {core.coreTemperature}°C");
                writer.WriteLine($"Core Pressure: {core.corePressure} bar");
                writer.WriteLine($"Core Reactivity: {core.coreReactivity}%");
                writer.WriteLine($"Core Neutron Flux: {core.coreNeutronFlux}");
                writer.WriteLine($"Core Integrity: {core.coreIntegrity}%");
                writer.WriteLine($"Core Fuel Integrity: {core.coreFuelIntegrity}%");

                writer.WriteLine($"Control Rods Insertion Level: {controlRods.controlRodsInsertionLevel}%");

                writer.WriteLine($"Pressuriser Temperature: {pressuriser.pressuriserTemperature}°C");
                writer.WriteLine($"Pressuriser Pressure: {pressuriser.pressuriserPressure} bar");
                writer.WriteLine($"Pressuriser Water Level: {pressuriser.pressuriserWaterLevel}%");
                writer.WriteLine($"Pressuriser Heating Power: {pressuriser.pressuriserHeatingPower}KW");
                writer.WriteLine($"Pressuriser Heater On: {pressuriser.pressuriserHeaterOn}");
                writer.WriteLine($"Pressuriser Relief Valve Open: {pressuriser.pressuriserReliefValveOpen}");
                writer.WriteLine($"Pressuriser Spray Nozzles Active: {pressuriser.pressuriserSprayNozzlesActive}");

                writer.WriteLine($"Primary Loop Temperature: {primaryLoop.primaryLoopCoolantTemperature}°C");
                writer.WriteLine($"Primary Loop Pressure: {primaryLoop.primaryLoopCoolantPressure} bar");
                writer.WriteLine($"Primary Loop Coolant Flow: {primaryLoop.primaryLoopCoolantFlow} l/min");

                writer.WriteLine($"Secondary Loop Temperature: {secondaryLoop.secondaryLoopSteamTemperature}°C");
                writer.WriteLine($"Secondary Loop Pressure: {secondaryLoop.secondaryLoopSteamPressure} bar");
                writer.WriteLine($"Secondary Loop Coolant Flow: {secondaryLoop.secondaryLoopSteamFlow} l/min");

                writer.WriteLine($"Power Generation Power Output: {powerGeneration.powerGenerationPowerOutput} MW");
                writer.WriteLine($"Power Generation Thermal Power: {powerGeneration.powerGenerationThermalPower} MW");
            }
        }

        protected void updateUI(string propertyName) // Method that updates UI with the new values.
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class Core : INotifyPropertyChanged
    {
        // Properties
        private double temperature;
        private double pressure;
        private double reactivity;
        private double neutronFlux;
        private double integrity;
        private double fuelIntegrity;
        public double heatTransferred;

        // Getters
        public double coreTemperature
        {
            get => temperature;
            set
            {
                if (temperature != value)
                {
                    temperature = value;
                    updateUI(nameof(coreTemperature));
                }
            }
        }
        public double corePressure
        {
            get => pressure;
            set
            {
                if (pressure != value)
                {
                    pressure = value;
                    updateUI(nameof(corePressure));
                }
            }
        }
        public double coreReactivity
        {
            get => reactivity;
            set
            {
                if (reactivity != value)
                {
                    reactivity = value;
                    updateUI(nameof(coreReactivity));
                }
            }
        }
        public double coreNeutronFlux
        {
            get => neutronFlux;
            set
            {
                if (neutronFlux != value)
                {
                    neutronFlux = value;
                    updateUI(nameof(coreNeutronFlux));
                }
            }
        }
        public double coreIntegrity
        {
            get => integrity;
            set
            {
                if (integrity != value)
                {
                    integrity = value;
                    updateUI(nameof(coreIntegrity));
                }
            }
        }
        public double coreFuelIntegrity
        {
            get => fuelIntegrity;
            set
            {
                if (fuelIntegrity != value)
                {
                    fuelIntegrity = value;
                    updateUI(nameof(coreFuelIntegrity));
                }
            }
        }

        /* Indicators and Switches - Not used so commenting out, need to implement in future versions
        private bool operatingPowerIndicator = true;
        private bool dangerIndicator = false;
        private bool overheatingIndicator = false;
        private bool criticalOverheatingIndicator = false;
        private bool criticalMassIndicator = true;
        private bool reactiveIndicator = true;
        private bool emergencyShutdownSwitch = false;

        public bool isOperatingPowerIndicator() => operatingPowerIndicator;
        public bool isDangerIndicator() => dangerIndicator;
        public bool isCriticalMassIndicator() => criticalMassIndicator;
        public bool isReactiveIndicator() => reactiveIndicator;
        public bool isEmergencyShutdownSwitch() => emergencyShutdownSwitch;*/

        public event PropertyChangedEventHandler PropertyChanged;

        // Constructor class
        public Core(double temperature, double pressure, double reactivity, double neutronFlux, double flowRate, double integrity, double fuelIntegrity)
        {
            coreTemperature = temperature;
            corePressure = pressure;
            coreReactivity = reactivity;
            coreNeutronFlux = neutronFlux;
            coreIntegrity = integrity;
            coreFuelIntegrity = fuelIntegrity;
        }

        public void updateCore(double controlRodsInsertionLevel, double pressuriserTemperature, double pressuriserPressure, double primaryLoopTemperature, double primaryLoopPressure, double primaryLoopFlowRate, double thermalPower)
        {
            coreTemperature = calculateTemperature(controlRodsInsertionLevel, pressuriserTemperature, primaryLoopTemperature, primaryLoopFlowRate, thermalPower);
            corePressure = calculatePressure(primaryLoopFlowRate, pressuriserPressure);
            coreReactivity = calculateReactivity(controlRodsInsertionLevel);
            coreNeutronFlux = calculateNeutronFlux(thermalPower);
            coreIntegrity = calculateIntegrity();
            coreFuelIntegrity = calculateFuelIntegrity(thermalPower);
        }

        private double calculateTemperature(double rodsInsertionLevel, double pressuriserTemperature, double primaryLoopTemperature, double primaryLoopFlowRate, double thermalPower) // Calculates the core temperature and returns a clamped value.
        {
            double baseTemperature = pressuriserTemperature;
            double temperatureDifference = temperature - primaryLoopTemperature;
            double heatCapacity = 3500;

            double heatGenerated = 0.005 * thermalPower * (1 + reactivity / 200) * (1 - rodsInsertionLevel / 100);
            double heatTransferred = 0.98 * (temperatureDifference * primaryLoopFlowRate / 500);

            double newTemperature = baseTemperature + ((heatGenerated - heatTransferred) / heatCapacity);

            return Math.Clamp(newTemperature, 0, 450);
        }

        private double calculatePressure(double primaryLoopFlowRate, double pressuriserPressure) // Calculates the pressure inside the core and returns a clamped value.
        {
            double thermalExpansion = (temperature - 330) * 0.001;
            double coolantDensityEffect = -(temperature - 330) * 0.0005;
            double pressuriserEffect = (pressuriserPressure - pressure) * 0.5;

            double newPressure = pressuriserPressure + thermalExpansion + coolantDensityEffect + pressuriserEffect;

            return Math.Clamp(newPressure, 0, 175);
        }

        private double calculateReactivity(double rodsInsertionLevel) // Calculates the reactivity and clamps the value.
        {
            double controlRodEffect = -(rodsInsertionLevel / 100) * 1.5;
            double fuelEffect = (fuelIntegrity / 100);
            double temperatureFeedback = -(coreTemperature - 330) * 0.005;

            double newReactivity = reactivity + controlRodEffect + fuelEffect + temperatureFeedback;

            return Math.Clamp(newReactivity, 0, 100);
        }

        private double calculateNeutronFlux(double thermalPower) // Calculates 
        {
            double baseFlux = 1e13;

            double reactivityEffect = reactivity * 1e11;
            double powerEffect = thermalPower * 1e9;

            double newFlux = baseFlux + reactivityEffect + powerEffect;

            return Math.Max(0, newFlux);
        }

        private double calculateIntegrity()
        {
            double temperatureEffect = Math.Max(0, (temperature - 350) * 0.0005);
            double pressureEffect = Math.Max(0, (corePressure - 165) * 0.01);
            double fuelEffect = (100 - fuelIntegrity) * 0.00025;

            double newIntegrity = integrity - (temperatureEffect + pressureEffect + fuelEffect);

            return Math.Clamp(newIntegrity, 0, 100);
        }

        private double calculateFuelIntegrity(double thermalPower)
        {
            double temperatureEffect = Math.Max(0, (temperature - 400) * 0.00025);
            double powerEffect = thermalPower * 0.00005;

            double newIntegrity = fuelIntegrity - (temperatureEffect + powerEffect);

            return Math.Clamp(newIntegrity , 0, 100);
        }

        public void updateUI(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

    }

    public class ControlRods : INotifyPropertyChanged
    {
        // Properties
        private double insertionLevel;
        private double neutronAbsorbtionRate;

        // Getters
        public double controlRodsInsertionLevel
        {
            get => insertionLevel;
            set
            {
                if (insertionLevel != value)
                { 
                    insertionLevel = value;
                    updateUI(nameof(controlRodsInsertionLevel));
                }
            }
        }
        public double controlRodsNeutronAbsorbtionRate
        {
            get => neutronAbsorbtionRate;
            set
            {
                if (neutronAbsorbtionRate != value)
                {
                    neutronAbsorbtionRate = value;
                    updateUI(nameof(controlRodsNeutronAbsorbtionRate));
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public ControlRods(double insertionLevel)
        {
            controlRodsInsertionLevel = insertionLevel;
        }

        public void updateControlRods(double neutronFlux)
        {
            controlRodsNeutronAbsorbtionRate = calculateNeutronAbsorbtionRate(neutronFlux);
        }

        private double calculateNeutronAbsorbtionRate(double neutronFlux)
        {
            double baseRate = 1e11;

            double insertionEffect = insertionLevel / 100 * baseRate;
            double fluxEffect = neutronFlux * 1e-11;

            double newRate = insertionEffect + fluxEffect;

            return Math.Max(0, newRate);
        }

        protected void updateUI(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class Pressuriser : INotifyPropertyChanged
    {
        // Properties
        private double temperature;
        private double pressure;
        private double waterLevel;
        private double heatingPower;
        private bool heaterOn;
        private bool reliefValveOpen;
        private bool sprayNozzlesActive;

        // Getters
        public double pressuriserTemperature
        {
            get => temperature;
            set
            {
                if (temperature != value)
                {
                    temperature = value;
                    updateUI(nameof(pressuriserTemperature));
                }
            }
        }
        public double pressuriserPressure
        {
            get => pressure;
            set
            {
                if (pressure != value)
                {
                    pressure = value;
                    updateUI(nameof(pressuriserPressure));
                }
            }
        }
        public double pressuriserWaterLevel
        {
            get => waterLevel;
            set
            {
                if (waterLevel != value)
                {
                    waterLevel = value;
                    updateUI(nameof(pressuriserWaterLevel));
                }
            }
        }
        public double pressuriserHeatingPower
        {
            get => heatingPower;
            set
            {
                if (heatingPower != value)
                {
                    heatingPower = value;
                    updateUI(nameof(pressuriserHeatingPower));
                }
            }
        }
        public bool pressuriserHeaterOn
        {
            get => heaterOn;
            set
            {
                if (heaterOn != value)
                {
                    heaterOn = value;
                    updateUI(nameof(pressuriserHeaterOn));
                }
            }
        }
        public bool pressuriserReliefValveOpen
        {
            get => reliefValveOpen;
            set
            {
                if (reliefValveOpen != value)
                {
                    reliefValveOpen = value;
                    updateUI(nameof(pressuriserReliefValveOpen));
                }
            }
        }
        public bool pressuriserSprayNozzlesActive
        {
            get => sprayNozzlesActive;
            set
            {
                if (sprayNozzlesActive != value)
                {
                    sprayNozzlesActive = value;
                    updateUI(nameof(pressuriserSprayNozzlesActive));
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public Pressuriser(double temperature, double pressure, double fillLevel, double heatingPower, bool heaterOn, bool reliefValveOpen, bool sprayNozzlesActive)
        {
            this.temperature = temperature;
            this.pressure = pressure;
            this.waterLevel = fillLevel;
            this.heatingPower = heatingPower;
            this.heaterOn = heaterOn;
            this.reliefValveOpen = reliefValveOpen;
            this.sprayNozzlesActive = sprayNozzlesActive;
        }

        public void updatePressuriser(double corePressure, double primaryLoopTemperature, double primaryLoopFlow) // Update method
        {
            pressuriserTemperature = calculateTemperature(primaryLoopTemperature);
            pressuriserPressure = calculatePressure();
            pressuriserWaterLevel = calculateVolume(corePressure, primaryLoopFlow);
            pressuriserHeaterOn = heaterActive();
            pressuriserSprayNozzlesActive = sprayActive();
            pressuriserReliefValveOpen = ventActive();
        }

        private double calculateTemperature(double primaryLoopTemperature)
        {
            double heatingEffect = heaterOn ? heatingPower * 0.05 : 0;
            double coolantEffect = sprayNozzlesActive ? -(temperature - primaryLoopTemperature) * 1 : 0;
            double volumeEffect = (waterLevel / 1000);

            double newTemperature = temperature + heatingEffect + coolantEffect + volumeEffect;

            return Math.Clamp(newTemperature, 0, 450);
        }

        private double calculatePressure()
        {
            double basePressure = 155;
            double pressureDifference = basePressure - pressure;
            double temperatureEffect = (temperature - 330) * 0.03;
            double volumeEffect = (waterLevel - 80) * 0.2;
            double ventEffect = reliefValveOpen ? (pressure - basePressure) * 0.1 : 0;

            double newPressure = pressure + (pressureDifference * 0.5) + temperatureEffect + volumeEffect - ventEffect;

            return Math.Clamp(newPressure, 0, 175);
        }

        private double calculateVolume(double corePressure, double primaryLoopFlow)
        {
            double pressureEffect = (corePressure - 155) * 0.01;
            double floweffect = (primaryLoopFlow - 250) * 0.0005;

            double newVolume = waterLevel + floweffect + pressureEffect;

            return Math.Clamp(newVolume, 0, 100);
        }

        // The following methods will be user-controlled if I have time
        private bool heaterActive()
        {
            if (temperature <= 328 && waterLevel <=80)
            {
                return true;
            }
            else if (pressure >= 156 || waterLevel >= 85)
            {
                return false;
            }
            return heaterOn;
        }

        private bool sprayActive()
        {
            if (temperature >= 332 || pressure >= 157)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private bool ventActive()
        {
            if (pressure >= 158)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        protected void updateUI(string propertyName) // Method that updates UI with the new values.
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class PrimaryCoolingLoop : INotifyPropertyChanged
    {
        // Properties
        private double temperature;
        private double pressure;
        private double flowRate;
        private bool lowFlowIndicator;
        private bool highTemperatureIndicator;
        private bool lowPressureIndicator;

        public double primaryLoopCoolantTemperature
        {
            get => temperature;
            set
            {
                if (temperature != value)
                {
                    temperature = value;
                    updateUI(nameof(primaryLoopCoolantTemperature));
                }
            }
        }
        public double primaryLoopCoolantPressure
        {
            get => pressure;
            set
            {
                if (pressure != value)
                {
                    pressure = value;
                    updateUI(nameof(primaryLoopCoolantPressure));
                }
            }
        }
        public double primaryLoopCoolantFlow
        {
            get => flowRate;
            set
            {
                if (flowRate != value)
                {
                    flowRate = value;
                    updateUI(nameof(primaryLoopCoolantFlow));
                }
            }
        }
        public bool primaryLoopLowFlowIndicator
        {
            get => lowFlowIndicator;
            set
            {
                if (lowFlowIndicator != value)
                {
                    lowFlowIndicator = value;
                    updateUI(nameof(primaryLoopLowFlowIndicator));
                }
            }
        }
        public bool primaryLoopHighTemperatureIndicator
        {
            get => highTemperatureIndicator;
            set
            {
                if (highTemperatureIndicator != value)
                {
                    highTemperatureIndicator = value;
                    updateUI(nameof(primaryLoopHighTemperatureIndicator));
                }
            }
        }
        public bool primaryLoopLowPressureIndicator
        {
            get => lowPressureIndicator;
            set
            {
                if (lowPressureIndicator != value)
                {
                    lowPressureIndicator = value;
                    updateUI(nameof(primaryLoopLowPressureIndicator));
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public PrimaryCoolingLoop(double temperature, double pressure, double flowRate)
        {
            this.temperature = temperature;
            this.pressure = pressure;
            this.flowRate = flowRate;
        }

        public void updatePrimaryCoolingLoop(double coreTemperature, double corePressure, double pressuriserPressure, double thermalPower) // Update method
        {
            primaryLoopCoolantTemperature = calculateTemperature(coreTemperature, thermalPower);
            primaryLoopCoolantPressure = calculatePressure(coreTemperature, corePressure, pressuriserPressure);
        }

        private double calculateTemperature(double coreTemperature, double thermalPower) // Calculates the primary loop temperature.
        {
            double coreEffect = (coreTemperature - temperature) * 0.1;
            double powerEffect = thermalPower * 0.0005;
            double flowEffect = -(flowRate - 250) * 0.002;

            double newTemperature = temperature + coreEffect + powerEffect + flowEffect;

            return Math.Clamp(newTemperature, 0, 450);
        }

        private double calculatePressure(double coreTemperature, double corePressure, double pressuriserPressure) // Calculates the primary loop pressure.
        {
            double coreEffect = (corePressure - pressure) * 0.1;
            double temperatureEffect = (temperature - coreTemperature) * 0.01;
            double pressuriserEffect = -(pressuriserPressure - pressure) * 0.01;

            double newPressure = pressure + coreEffect + temperatureEffect + pressuriserEffect;

            return Math.Clamp(newPressure, 0, 175);
        }

        protected void updateUI(string propertyName) // Method that updates UI with the new values.
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class SecondaryCoolingLoop : INotifyPropertyChanged
    {
        // Properties
        private double temperature;
        private double pressure;
        private double flowRate;
        private bool lowFlowIndicator;
        private bool highTemperatureIndicator;
        private bool lowPressureIndicator;

        // Getters
        public double secondaryLoopSteamTemperature
        {
            get => temperature;
            set
            {
                if (temperature != value)
                {
                    temperature = value;
                    updateUI(nameof(secondaryLoopSteamTemperature));
                }
            }
        }
        public double secondaryLoopSteamPressure
        {
            get => pressure;
            set
            {
                if (pressure != value)
                {
                    pressure = value;
                    updateUI(nameof(secondaryLoopSteamPressure));
                }
            }
        }
        public double secondaryLoopSteamFlow
        {
            get => flowRate;
            set
            {
                if (flowRate != value)
                {
                    flowRate = value;
                    updateUI(nameof(secondaryLoopSteamFlow));
                }
            }
        }
        public bool secondaryLoopLowFlowIndicator
        {
            get => lowFlowIndicator;
            set
            {
                if (lowFlowIndicator != value)
                {
                    lowFlowIndicator = value;
                    updateUI(nameof(secondaryLoopLowFlowIndicator));
                }
            }
        }
        public bool secondaryLoopHighTemperatureIndicator
        {
            get => highTemperatureIndicator;
            set
            {
                if (highTemperatureIndicator != value)
                {
                    highTemperatureIndicator = value;
                    updateUI(nameof(secondaryLoopHighTemperatureIndicator));
                }
            }
        }
        public bool secondaryLoopLowPressureIndicator
        {
            get => lowPressureIndicator;
            set
            {
                if (lowPressureIndicator != value)
                {
                    lowPressureIndicator = value;
                    updateUI(nameof(secondaryLoopLowPressureIndicator));
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public SecondaryCoolingLoop(double temperature, double pressure, double flowRate) // Constructor
        {
            this.temperature = temperature;
            this.pressure = pressure;
            this.flowRate = flowRate;
        }

        public void updateSecondaryCoolingLoop(double primaryLoopTemperature) // Method to call the methods for secondary loop calculations.
        {
            secondaryLoopSteamTemperature = calculateTemperature(primaryLoopTemperature);
            secondaryLoopSteamPressure = calculatePressure();
        }

        private double calculateTemperature(double primaryLoopTemperature) // Method that calculates the steam temperature.
        {
            double heatTransferred = (primaryLoopTemperature - temperature) * flowRate * 0.001;

            double newTemperature = temperature + heatTransferred;

            return Math.Clamp(newTemperature, 0, 150);
        }

        private double calculatePressure() // Method to calculate the steam pressure
        {
            double basePressure = temperature > 100 ? 50 + (temperature - 100) * 0.2 : 0;

            double flowEffect = flowRate * 0.01;

            double newPressure = basePressure + flowEffect;

            return Math.Clamp(newPressure, 0, 80);
        }

        protected void updateUI(string propertyName) // Method that updates UI with the new values.
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class PowerGeneration : INotifyPropertyChanged
    {
        // Properties
        private double powerOutput;
        private double thermalPower;

        // Getters
        public double powerGenerationPowerOutput
        {
            get => powerOutput;
            set
            {
                if (powerOutput != value)
                {
                    powerOutput = value;
                    updateUI(nameof(powerGenerationPowerOutput));
                }
            }
        }
        public double powerGenerationThermalPower
        {
            get => thermalPower;
            set
            {
                if (thermalPower != value)
                {
                    thermalPower = value;
                    updateUI(nameof(powerGenerationThermalPower));
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public PowerGeneration(double realPower, double thermalPower) // Constructor
        {
            this.powerOutput = realPower;
            this.thermalPower = thermalPower;
        }

        public void updatePowerGeneration(double coreTemperature, double corePressure, double reactivity, double primaryLoopTemperature, double primaryLoopPressure, double primaryLoopFlow, double secondaryLoopTemperature, double secondaryLoopPressure, double secondaryLoopFlow) // Method to call the methods for power generation calculations.
        {
            powerGenerationPowerOutput = calculateElectricalPower(secondaryLoopTemperature, secondaryLoopPressure, secondaryLoopFlow);
            powerGenerationThermalPower = calculateThermalPower(coreTemperature, corePressure, reactivity, primaryLoopFlow);
        }

        private double calculateElectricalPower(double secondaryLoopTemperature, double secondaryLoopPressure, double secondaryLoopFlow) // Method to calculate the electrical power output.
        {
            double efficiency = 0.33;
            double temperatureEfficiency = secondaryLoopTemperature / 300;
            double pressureEfficiency = secondaryLoopPressure / 60;
            double flowEfficiency = secondaryLoopFlow / 300;
            double overallEfficency = efficiency * temperatureEfficiency * pressureEfficiency * flowEfficiency;

            double newPower = thermalPower * overallEfficency;

            return Math.Clamp(newPower, 0, 1500);
        }

        private double calculateThermalPower(double coreTemperature, double corePressure, double reactivity, double primaryLoopFlow) // Method to calculate the raw thermal power output.
        {
            double temperatureEffect = coreTemperature * 0.1;
            double pressureEffect = corePressure * 0.05;
            double reactivityEffect = reactivity * 10;
            double flowEffect = primaryLoopFlow * 0.02;

            double newPower = (temperatureEffect + reactivityEffect + pressureEffect + flowEffect) * 3; // temporary fix need to re-look at this equation.

            return Math.Clamp(newPower, 0, 4000);
        }

        protected void updateUI(string propertyName) // Method that updates UI with the new values.
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}