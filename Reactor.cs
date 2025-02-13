using System;
using System.Numerics;
using System.Windows;
using System.Windows.Navigation;
using System.Windows.Threading;
using Microsoft.Win32;
using ReactorSimulator;
using System.ComponentModel;
using System.Diagnostics;

namespace ReactorSimulator
{
    // Holds the "clock" to repeatedly call updates.
    public class Reactor
    {
        private DispatcherTimer timer;
        public double timeElapsed = 0;
        public event Action dangerStateChanged;

        public Core core;
        public ControlRods controlRods;
        public Pressuriser pressuriser;
        public PrimaryCoolingLoop primaryLoop;
        public SecondaryCoolingLoop secondaryLoop;
        public PowerGeneration powerGeneration;
        public Simulation simulationWindow;
        private ScenarioData scenarioData;

        public Reactor(Simulation window, ScenarioData scenarioData)
        {
            simulationWindow = window;

            core = new Core(this, scenarioData, scenarioData.coreTemperature, scenarioData.corePressure, scenarioData.coreReactivity, scenarioData.coreCoolantFlow, scenarioData.coreIntegrity, scenarioData.coreFuelIntegrity);
            controlRods = new ControlRods(scenarioData.controlRodInsertionLevel);
            pressuriser = new Pressuriser(scenarioData.pressuriserTemperature, scenarioData.pressuriserPressure, scenarioData.pressuriserFillLevel, scenarioData.pressuriserHeatingPower, scenarioData.pressuriserHeaterOn, scenarioData.pressuriserReliefValveOpen, scenarioData.pressuriserSprayNozzlesActive);
            primaryLoop = new PrimaryCoolingLoop(scenarioData.primaryLoopTemperature, scenarioData.primaryLoopPressure, scenarioData.primaryLoopCoolantFlow);
            secondaryLoop = new SecondaryCoolingLoop(scenarioData.secondaryLoopTemperature, scenarioData.secondaryLoopPressure, scenarioData.secondaryLoopCoolantFlow);
            powerGeneration = new PowerGeneration(this, scenarioData.powerGenerationPowerOutput, scenarioData.powerGenerationThermalPower);

            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromMilliseconds(1000); // Temporarily, this will be user-set later.
            // timer.Tick += (sender, e) => updateSimulation();
            timer.Start();
        }

        private void updateSimulation() // Called every second to update each class and their attributes.
        {
            if (core == null || controlRods == null || pressuriser == null || primaryLoop == null || secondaryLoop == null || powerGeneration == null)
            {
                return;
            }

            core.updateCore(timeElapsed);
            controlRods.updateControlRods(50); // temporary until i sort user control out whenever
            pressuriser.updatePressuriser(timeElapsed);
            primaryLoop.updatePrimaryCoolingLoop(core.coreTemperature, timeElapsed);
            secondaryLoop.updateSecondaryCoolingLoop(primaryLoop.primaryLoopCoolantTemperature, timeElapsed);
            powerGeneration.updatePowerGeneration(timeElapsed);
            checkIndicators();

            timeElapsed += 1;
        }

        private void checkIndicators()
        {
            bool previousState = core.coreTemperature > 350 || core.corePressure > 165 || core.coreCoolantFlow < 250 || core.coreReactivity > 95 || core.coreIntegrity < 30; // need to make this more efficient so i can add more without it being 5 miles long lol
            dangerStateChanged?.Invoke();
        }
    }

    // Main class, contains most of the calculations.
    public class Core : INotifyPropertyChanged
    {
        // Properties (Encapsulated)
        private double temperature;
        private double pressure;
        private double coolantFlow;
        private double reactivity;
        private double integrity;
        private double fuelIntegrity;

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
        public double coreCoolantFlow
        {
            get => coolantFlow;
            set
            {
                if (coolantFlow != value)
                {
                    coolantFlow = value;
                    updateUI(nameof(coreCoolantFlow));
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

        // Indicators and Switches
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
        public bool isEmergencyShutdownSwitch() => emergencyShutdownSwitch;

        // Constants
        private const double pressureTemperatureCoefficient = 0.5; // This is the (approximate) pressure increase per deg c above 300 degrees. Or at least google says so.

        private Reactor reactor;
        private ControlRods controlRods;
        private Pressuriser pressuriser;
        private PrimaryCoolingLoop primaryLoop;
        private SecondaryCoolingLoop secondaryLoop;
        private PowerGeneration powerGeneration;
        private ScenarioData scenarioData;

        public event PropertyChangedEventHandler PropertyChanged;

        // Constructor class
        public Core(Reactor reactor, ScenarioData scenarioData, double temperature, double pressure, double reactivity, double flowRate, double integrity, double fuelIntegrity)
        {
            this.reactor = reactor;
            this.temperature = temperature;
            this.pressure = pressure;
            this.reactivity = reactivity;
            this.coolantFlow = flowRate;
            this.integrity = integrity;
            this.fuelIntegrity = fuelIntegrity;

            controlRods = new ControlRods(scenarioData.controlRodInsertionLevel);
            pressuriser = new Pressuriser(temperature, pressure, scenarioData.pressuriserFillLevel, scenarioData.pressuriserHeatingPower, scenarioData.pressuriserHeaterOn, scenarioData.pressuriserReliefValveOpen, scenarioData.pressuriserSprayNozzlesActive);
            primaryLoop = new PrimaryCoolingLoop(scenarioData.primaryLoopTemperature, scenarioData.primaryLoopPressure, scenarioData.primaryLoopCoolantFlow);
            secondaryLoop = new SecondaryCoolingLoop(scenarioData.secondaryLoopTemperature, scenarioData.secondaryLoopPressure, scenarioData.secondaryLoopCoolantFlow);
            powerGeneration = new PowerGeneration(reactor, scenarioData.powerGenerationPowerOutput, scenarioData.powerGenerationThermalPower);
        }

        public Pressuriser GetPressuriser() => pressuriser;

        public void updateCore(double timeElapsed)
        {
            if (reactor == null) // Just a check to stop the calculations if the reactor isn't yet initialised.
            {
                return;
            }

            calculateReactivity(controlRods.ControlRodsNeutronAbsorbtionRate, primaryLoop.primaryLoopHeatTransferEfficiency, coreFuelIntegrity, coreTemperature, timeElapsed);
            calculateTemperature(coreReactivity, primaryLoop.primaryLoopCoolantFlow, primaryLoop.primaryLoopCoolantTemperature, powerGeneration.powerGenerationThermalEfficiency, powerGeneration.powerGenerationPowerOutput);
            calculatePressure(coreTemperature, primaryLoop.primaryLoopCoolantFlow, powerGeneration.powerGenerationPowerOutput, pressureTemperatureCoefficient, timeElapsed);
            calculateIntegrity(coreTemperature, corePressure, timeElapsed);
        }

        public void calculateReactivity(double neutronAbsorbtionRate, double coolantModerationFactor, double fuelIntegrity, double temperature, double timeElapsed) // I cannot get this to calculate right and it is SO PAINFUL.
        {
            double controlRodEffect = Math.Max(0, 1 - (neutronAbsorbtionRate / 100)); // Stops the value being bigger than 1.
            double coolantEffect = Math.Max(0, coolantModerationFactor * (1 - 0.01 * (temperature - 300))); // Keeps positive to prevent stupid negative values I was always getting.
            double fuelEffect = Math.Max(0, fuelIntegrity * (1 - 0.005 * timeElapsed)); // Prevents negative fuel integrity because that would be stupid.

            double tempReactivity = reactivity * controlRodEffect * coolantEffect * fuelEffect; // 166250%? seems right bro idek if thats better than -3000%

            if (tempReactivity < 0 || tempReactivity > 100)
            {
                MessageBox.Show($"Reactivity out of range. Calculated reactivity was {tempReactivity}%, clamped value to be within range. (0% - 100%)", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
            }

            reactivity = Math.Clamp(tempReactivity, 0, 100);

            reactiveIndicator = reactivity > 0;
            criticalMassIndicator = reactivity >= 70;

            if (!dangerIndicator) // Checks to see if danger indicator is already lit, don't want it to override with a false value if there is danger. WORK ON WHEN CAN.
            {
                dangerIndicator = reactivity >= 98;
            }
        }

        public void calculateTemperature(double reactivity, double coolantFlow, double coolantTemperature, double thermalConductivity, double powerOutput)
        {
            double heatGenerated = powerOutput * reactivity / 100;
            double coolantEffect = coolantFlow * (coolantTemperature - 300);
            double heatTransferred = thermalConductivity * (heatGenerated - coolantEffect);
            double tempTemperature = temperature + (heatGenerated - heatTransferred) * 0.01;

            if (tempTemperature < 15 || tempTemperature > 500)
            {
                MessageBox.Show($"Temperature out of range. Calculated temperature was {tempTemperature}°C, clamped value to be within range (15°C - 500°C)", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
            }

            temperature = Math.Clamp(tempTemperature, 15, 500);

            if (!dangerIndicator)
            {
                dangerIndicator = temperature >= 340;
            }

            overheatingIndicator = temperature >= 340;
            criticalOverheatingIndicator = temperature >= 400;
        }

        public void calculatePressure(double temperature, double coolantFlow, double powerOutput, double pressureTemperatureCoefficient, double timeElapsed)
        {
            pressuriser.updatePressuriser(timeElapsed);
            double tempPressure = pressuriser.pressuriserPressure;

            tempPressure += (temperature - 300) * pressureTemperatureCoefficient;

            double powerEffect = powerOutput * 0.05;
            double coolantEffect = coolantFlow * 0.01;

            tempPressure += powerEffect;
            tempPressure -= coolantEffect;

            if (tempPressure < 1 || tempPressure > 180)
            {
                MessageBox.Show($"Pressure out of range. Calculated pressure was {tempPressure} bar, clamped value to be within range (1 bar - 180 bar)", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
            }

            pressure = Math.Clamp(tempPressure, 1, 180);

            if (!dangerIndicator)
            {
                dangerIndicator = pressure > 160;
            }
        }

        public void calculateIntegrity(double temperature, double pressure, double timeElapsed)
        {
            double tempIntegrity = coreIntegrity;

            if (temperature >= 350 || pressure >= 165) // 10 degrees / bar over where warnings start
            {
                double overTemperature = temperature - 350;
                double overPressure = pressure - 165;

                dangerIndicator = true;

                if (overTemperature < 0)
                {
                    tempIntegrity -= 0.01 * overTemperature;
                }
                else if (overPressure < 0)
                {
                    tempIntegrity -= 0.1 * overPressure;
                }
            }
            else
            {
                tempIntegrity -= 0.005 * timeElapsed; // Natural degradation, takes just over 5.5 hours to decay to 0% naturally so should be good. May reduce though.

                if (!dangerIndicator)
                {
                    dangerIndicator = tempIntegrity < 20;
                }
            }

            if (tempIntegrity <= 20)
            {
                dangerIndicator = true;
            }

            if (tempIntegrity < 0 || tempIntegrity > 100)
            {
                MessageBox.Show($"Integrity out of range. Calculated integrity was {tempIntegrity} bar, clamped value to be within range (1% - 100%)", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
            }

            integrity = Math.Clamp(tempIntegrity, 0, 100);
        }

        public void calculateFuelEfficiency(double timeElapsed)
        {
            double tempFuelEfficiency = coreFuelIntegrity;
        }

        protected void updateUI(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

    }

    public class ControlRods : INotifyPropertyChanged
    {
        private double insertionLevel = 50;
        private double neutronAbsorbtionRate = 50;

        public double ControlRodsInsertionLevel
        {
            get => insertionLevel;
            set
            {
                if (insertionLevel != value)
                { 
                    insertionLevel = value;
                    updateUI(nameof(ControlRodsInsertionLevel));
                }
            }
        }
        public double ControlRodsNeutronAbsorbtionRate
        {
            get => neutronAbsorbtionRate;
            set
            {
                if (neutronAbsorbtionRate != value)
                {
                    neutronAbsorbtionRate = value;
                    updateUI(nameof(ControlRodsNeutronAbsorbtionRate));
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public ControlRods(double insertionLevel)
        {
            this.insertionLevel = insertionLevel;
        }

        public void updateControlRods(double adjustment)
        {
            adjustInsertion(adjustment);
        }

        public void adjustInsertion(double adjustment)
        {
            insertionLevel = Math.Clamp(insertionLevel + adjustment, 0, 100);
            neutronAbsorbtionRate = Math.Pow(insertionLevel / 100, 2) * 100;
        }

        public void calculateNeutronAbsorbtionRate()
        {
            // Need to do.
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

        // Constants
        private const double minPressure = 145.0;
        private const double maxPressure = 165.0;
        private const double minWaterLevel = 20.0;
        private const double maxWaterLevel = 100.0;

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

        public void updatePressuriser(double timeElapsed)
        {
            double tempTemperature = temperature;
            double tempPressure = pressure;

            if (heaterOn)
            {
                tempTemperature += heatingPower * 0.01 * timeElapsed;
                tempPressure += heatingPower * 0.005 * timeElapsed;
            }

            if (reliefValveOpen)
            {
                tempPressure -= 0.1 * timeElapsed;
                reliefValveOpen = tempPressure > maxPressure;
            }

            pressure = Math.Clamp(tempPressure, 0, maxPressure + 15);
            temperature = Math.Clamp(tempTemperature, 15, 500);
        }

        public void toggleHeater(bool status)
        {
            heaterOn = status;
            heatingPower = status ? 500 : 0;
        }

        public void toggleSprayNozzles()
        {
            sprayNozzlesActive = true;
            pressure -= 1.0;
            sprayNozzlesActive = false;
        }

        public void toggleReliefValve()
        {
            if (pressure > maxPressure)
            {
                reliefValveOpen = true;
                pressure -= 2.0;
            }
            else if (pressure < minPressure)
            {
                toggleHeater(true);
            }

            if (waterLevel > maxWaterLevel || waterLevel < minWaterLevel)
            {
                MessageBox.Show($"Water level out of range. Calculated water level was {waterLevel} bar, clamped value to be within range (0% - 100%)", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        protected void updateUI(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class PrimaryCoolingLoop : INotifyPropertyChanged
    {
        // Properties
        private double coolantTemperature;
        private double coolantPressure;
        private double coolantFlow;
        private double heatTransferEfficiency;
        private bool lowFlowIndicator;
        private bool highTemperatureIndicator;
        private bool lowPressureIndicator;

        public double primaryLoopCoolantTemperature
        {
            get => coolantTemperature;
            set
            {
                if (coolantTemperature != value)
                {
                    coolantTemperature = value;
                    updateUI(nameof(primaryLoopCoolantTemperature));
                }
            }
        }
        public double primaryLoopCoolantPressure
        {
            get => coolantPressure;
            set
            {
                if (coolantPressure != value)
                {
                    coolantPressure = value;
                    updateUI(nameof(primaryLoopCoolantPressure));
                }
            }
        }
        public double primaryLoopCoolantFlow
        {
            get => coolantFlow;
            set
            {
                if (coolantFlow != value)
                {
                    coolantFlow = value;
                    updateUI(nameof(primaryLoopCoolantFlow));
                }
            }
        }
        public double primaryLoopHeatTransferEfficiency
        {
            get => heatTransferEfficiency;
            set
            {
                if (heatTransferEfficiency != value)
                {
                    heatTransferEfficiency = value;
                    updateUI(nameof(primaryLoopHeatTransferEfficiency));
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
            this.coolantTemperature = temperature;
            this.coolantPressure = pressure;
            this.coolantFlow = flowRate;
        }

        public void updatePrimaryCoolingLoop(double reactorTemperature, double timeElapsed)
        {
            calculateHeatTransfer(reactorTemperature);
            updateIndicators();
        }

        private void calculateHeatTransfer(double reactorTemperature)
        {
            double tempCoolantTemperature = coolantTemperature;
            double heatRemoved = reactorTemperature * (coolantFlow / 500.0) * (heatTransferEfficiency / 100.0);
            tempCoolantTemperature += (reactorTemperature - heatRemoved) * 0.01;

            if (tempCoolantTemperature < 15 || tempCoolantTemperature > 500)
            {
                MessageBox.Show($"Temperature out of range. Calculated temperature was {tempCoolantTemperature}°C, clamped value to be within range (15°C - 500°C)", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
            }

            coolantTemperature = Math.Clamp(tempCoolantTemperature, 15, 500);
        }

        public void adjustFlow(double newFlow) // Validates that newFlow is within range and updates private variable.
        {
            if (newFlow < 0 || newFlow > 500)
            {
                MessageBox.Show($"Coolant flow rate is out of range. The flow rate was calculated to be {newFlow} m3/min. Clamped value to be within correct range (0 m3/min  - 500 m3/min)", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            coolantFlow = Math.Clamp(newFlow, 0, 500);
        }

        private void updateIndicators() // Method to change the indicators if required.
        {
            lowFlowIndicator = coolantFlow < 100.0;
            highTemperatureIndicator = coolantTemperature >= 340.0;
            lowPressureIndicator = coolantPressure <= 145.0;
        }

        protected void updateUI(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class SecondaryCoolingLoop : INotifyPropertyChanged
    {
        // Properties (Encapsulated)
        private double steamTemperature = 120.0;
        private double steamPressure = 10.0;
        private double steamFlow = 300.0;
        private double heatTransferEfficiency = 90.0;
        private bool lowFlowIndicator = false;
        private bool highTemperatureIndicator = false;
        private bool lowPressureIndicator = false;

        public double secondaryLoopSteamTemperature
        {
            get => steamTemperature;
            set
            {
                if (steamTemperature != value)
                {
                    steamTemperature = value;
                    updateUI(nameof(secondaryLoopSteamTemperature));
                }
            }
        }

        public double secondaryLoopSteamPressure
        {
            get => steamPressure;
            set
            {
                if (steamPressure != value)
                {
                    steamPressure = value;
                    updateUI(nameof(secondaryLoopSteamPressure));
                }
            }
        }

        public double secondaryLoopSteamFlow
        {
            get => steamFlow;
            set
            {
                if (steamFlow != value)
                {
                    steamFlow = value;
                    updateUI(nameof(secondaryLoopSteamFlow));
                }
            }
        }

        public double secondaryLoopHeatTransferEfficiency
        {
            get => heatTransferEfficiency;
            set
            {
                if (heatTransferEfficiency != value)
                {
                    heatTransferEfficiency = value;
                    updateUI(nameof(secondaryLoopHeatTransferEfficiency));
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

        public SecondaryCoolingLoop(double temperature, double pressure, double flowRate)
        {
            this.steamTemperature = temperature;
            this.steamPressure = pressure;
            this.steamFlow = flowRate;
        }

        public void updateSecondaryCoolingLoop(double primaryLoopCoolantTemperature, double timeElapsed) // Method to call the methods for calculations. Runs every "clock" cycle from Reactor class.
        {
            calculateHeatTransfer(primaryLoopCoolantTemperature);
            updateIndicators();
        }

        private void calculateHeatTransfer(double primaryLoopCoolantTemperature) // Method to calculate the steam temperature. Also returns steam pressure.
        {
            double tempSteamTemperature = steamTemperature;
            double heatTransferred = primaryLoopCoolantTemperature * (steamFlow / 500.0) * (heatTransferEfficiency / 100.0);
            tempSteamTemperature += (heatTransferred - steamTemperature) * 0.02;

            if (tempSteamTemperature < 100 || tempSteamTemperature > 400)
            {
                MessageBox.Show($"Temperature out of range. Calculated temperature was {tempSteamTemperature}°C, clamped value to be within range (100°C - 400°C)", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
            }

            steamTemperature = Math.Clamp(tempSteamTemperature, 100, 400);
            steamPressure = calculateSteamPressure(steamTemperature);
        }

        private double calculateSteamPressure(double temperature) // Method to calculate the steam pressure. Simple equation.
        {
            double tempPressure = (temperature - 100) * 0.4 + 5;

            if (tempPressure > 60 || tempPressure < 5)
            {
                MessageBox.Show($"Steam pressure is out of range. The pressure was calculated to be {tempPressure} bar. Clamped value to be within correct range (5 bar - 60 bar)", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
            }

            return Math.Clamp(tempPressure, 5, 60); // Returning the calculated pressure back to "calculateHeatTransfer()".
        }

        public void adjustSteamFlow(double newFlow) // Double checks newFlow is within range and updates once validated.
        {
            if (newFlow < 0 || newFlow > 500)
            {
                MessageBox.Show($"Steam flow rate is out of range. The flow rate was calculated to be {newFlow}m3/min. Clamped value to be within correct range (0m3/min  - 500m3/min)", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            steamFlow = Math.Clamp(newFlow, 0, 500);
        }

        private void updateIndicators()
        {
            bool lowFlowIndicator = steamFlow < 100;
            bool highTemperatureIndicator = steamTemperature > 270;
            bool lowPressureIndicator = steamPressure < 10;
        }

        protected void updateUI(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class PowerGeneration : INotifyPropertyChanged
    {
        // Properties
        private double powerOutput = 0.0; // MW
        private double thermalPower = 0.0; // MW
        private double thermalEfficiency = 0.33; // %

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
        public double powerGenerationThermalEfficiency
        {
            get => thermalEfficiency;
            set
            {
                if (thermalEfficiency != value)
                {
                    thermalEfficiency = value;
                    updateUI(nameof(powerGenerationThermalEfficiency));
                }
            }
        }

        private Reactor reactor;
        public event PropertyChangedEventHandler PropertyChanged;

        public PowerGeneration(Reactor reactor, double realPower, double thermalPower)
        {
            this.reactor = reactor;
            this.powerOutput = realPower;
            this.thermalPower = thermalPower;
        }

        public void updatePowerGeneration(double timeElapsed)
        {
            double temperature = reactor.core.coreTemperature;
            double pressure = reactor.core.corePressure;
            double coolantFlow = reactor.core.coreCoolantFlow;
            double reactivity = reactor.core.coreReactivity;

            double thermalPower = calculateThermalPower(temperature, pressure, coolantFlow, reactivity);
            double tempPowerOutput = thermalPower * thermalEfficiency;

            if (tempPowerOutput < 0)
            {
                MessageBox.Show($"Power output is out of range. The power output was calculated to be {tempPowerOutput}MW. Clamped value to be within correct range (0MW  - 1000MW)", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                tempPowerOutput = 0;
            }

            powerOutput = Math.Clamp(tempPowerOutput, 0, 1000);

            updatePowerIndicators();
        }

        private double calculateThermalPower(double temperature, double pressure, double coolantFlow, double reactivity) // Calculates the thermal power of the reactor, not the final power output.
        {
            double power = (temperature - 300) * 0.1 * (pressure / 150) * (coolantFlow * 400) * (reactivity * 100);
            return Math.Clamp(power, 0, 1100);
        }

        private void updatePowerIndicators() // Fix
        {
            bool dangerIndicator = reactor.core.isDangerIndicator();

            if (powerOutput > 1000)
            {
                dangerIndicator = true;
            }
        }

        protected void updateUI(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        // Need a method to check what is causing danger indicator. E.g. if high power output causes a danger indicator, it should switch off only if no other danger is present when the power goes back down to a safe level.
    }
}