using System;
using System.Data;
using System.Collections.Generic;
using System.Linq;

namespace Check_carasi_DF_ContextClearing.Tests
{
    /// <summary>
    /// Mock data generator for creating realistic test data for the application
    /// </summary>
    public static class MockDataGenerator
    {
        private static readonly Random _random = new Random();

        // Sample interface names for different automotive domains
        private static readonly string[] _interfaceNamePrefixes = 
        {
            "ENG_", "TRANS_", "BRAKE_", "STEER_", "SUSP_", "HVAC_", "LIGHT_", "DOOR_",
            "BATTERY_", "MOTOR_", "INV_", "CHARGE_", "COOL_", "HEAT_", "AIRBAG_", "ABS_"
        };

        private static readonly string[] _interfaceNameSuffixes = 
        {
            "Temp", "Press", "Speed", "Torque", "Volt", "Curr", "Pos", "Stat",
            "Req", "Resp", "Cmd", "Sts", "Diag", "Err", "Warn", "Info"
        };

        private static readonly string[] _units = 
        {
            "V", "A", "°C", "bar", "rpm", "Nm", "kg", "m/s", "m/s²", "°", "%", "Hz", "W", "Ω"
        };

        private static readonly string[] _swTypes = 
        {
            "UINT8", "UINT16", "UINT32", "SINT8", "SINT16", "SINT32", "BOOLEAN", "FLOAT32", "FLOAT64"
        };

        private static readonly string[] _mmTypes = 
        {
            "CAN", "LIN", "FlexRay", "Ethernet", "I2C", "SPI", "PWM", "ADC", "DIO"
        };

        private static readonly string[] _descriptions = 
        {
            "Engine temperature sensor value",
            "Battery voltage measurement",
            "Motor speed feedback",
            "Brake pedal position",
            "Steering wheel angle",
            "HVAC blower speed command",
            "Door lock status indication",
            "Charging current request",
            "Inverter temperature warning",
            "Suspension damping coefficient",
            "Headlight intensity control",
            "Airbag deployment status",
            "ABS activation flag",
            "Transmission gear position"
        };

        /// <summary>
        /// Generates a realistic Carasi interface structure
        /// </summary>
        /// <param name="interfaceName">Optional specific interface name</param>
        /// <returns>Populated Carasi_Interface structure</returns>
        public static Carasi_Interface GenerateCarasiInterface(string interfaceName = null)
        {
            if (string.IsNullOrEmpty(interfaceName))
            {
                interfaceName = GenerateInterfaceName();
            }

            var swType = _swTypes[_random.Next(_swTypes.Length)];
            var unit = _units[_random.Next(_units.Length)];
            
            return new Carasi_Interface
            {
                name = interfaceName,
                input = GenerateInputList(),
                output = GenerateOutputName(interfaceName),
                description = _descriptions[_random.Next(_descriptions.Length)],
                unit = unit,
                minValue = GenerateMinValue(swType, unit),
                maxValue = GenerateMaxValue(swType, unit),
                resolution = GenerateResolution(swType),
                initialisation = GenerateInitialValue(swType),
                swType = swType,
                mmType = _mmTypes[_random.Next(_mmTypes.Length)],
                conversion = GenerateConversion(unit),
                comments = $"Generated test interface for {interfaceName}",
                computeDetails = $"Computed using standard {swType} arithmetic"
            };
        }

        /// <summary>
        /// Generates a realistic Dataflow interface structure
        /// </summary>
        /// <param name="interfaceName">Optional specific interface name</param>
        /// <returns>Populated Dataflow_Interface structure</returns>
        public static Dataflow_Interface GenerateDataflowInterface(string interfaceName = null)
        {
            if (string.IsNullOrEmpty(interfaceName))
            {
                interfaceName = GenerateInterfaceName();
            }

            var rteDirection = GetRandomRteDirection();
            var mappingType = GetRandomMappingType();
            var status = GetRandomStatus();

            return new Dataflow_Interface
            {
                status = status,
                description = $"Dataflow for {interfaceName}",
                Rte_Direction = rteDirection,
                MappingType = mappingType,
                Pseudo_code = GeneratePseudoCode(interfaceName, rteDirection),
                System_constant = GenerateSystemConstant(interfaceName),
                FC_Name = GenerateFunctionName(interfaceName),
                Producer = GenerateProducerName(interfaceName),
                Consumers = GenerateConsumerList(interfaceName),
                
                // PSA Properties
                PSAname = $"PSA_{interfaceName}",
                PSAswType = _swTypes[_random.Next(_swTypes.Length)],
                PSAunit = _units[_random.Next(_units.Length)],
                PSAconversion = "1.0",
                PSAresolution = "0.1",
                PSAminValue = "0",
                PSAmaxValue = "100",
                PSAoffset = "0",
                PSAinitialisation = "0",
                
                // Bosch Properties
                Boschname = $"BOSCH_{interfaceName}",
                BoschswType = _swTypes[_random.Next(_swTypes.Length)],
                Boschunit = _units[_random.Next(_units.Length)],
                Boschconversion = "1.0",
                Boschresolution = "0.1",
                BoschminValue = "0",
                BoschmaxValue = "100",
                Boschoffset = "0",
                Boschinitialisation = "0"
            };
        }

        /// <summary>
        /// Generates a DataTable with multiple Carasi interfaces
        /// </summary>
        /// <param name="count">Number of interfaces to generate</param>
        /// <returns>DataTable with Carasi interface data</returns>
        public static DataTable GenerateCarasiDataTable(int count = 10)
        {
            var template = TestDataHelper.CreateTemplateDataTable();
            
            for (int i = 0; i < count; i++)
            {
                var carasiInterface = GenerateCarasiInterface();
                var row = template.NewRow();
                
                row["Interface Name"] = carasiInterface.name;
                row["Input"] = string.Join(";", carasiInterface.input ?? new string[0]);
                row["Output"] = carasiInterface.output;
                row["Description"] = carasiInterface.description;
                row["Unit"] = carasiInterface.unit;
                row["Min Value"] = carasiInterface.minValue;
                row["Max Value"] = carasiInterface.maxValue;
                row["Resolution"] = carasiInterface.resolution;
                row["Initialisation"] = carasiInterface.initialisation;
                row["SW Type"] = carasiInterface.swType;
                row["MM Type"] = carasiInterface.mmType;
                row["Conversion"] = carasiInterface.conversion;
                row["Comments"] = carasiInterface.comments;
                row["Compute Details"] = carasiInterface.computeDetails;
                
                template.Rows.Add(row);
            }
            
            return template;
        }

        /// <summary>
        /// Generates a DataTable with multiple Dataflow interfaces
        /// </summary>
        /// <param name="count">Number of interfaces to generate</param>
        /// <returns>DataTable with Dataflow interface data</returns>
        public static DataTable GenerateDataflowDataTable(int count = 10)
        {
            var dataTable = new DataTable();
            dataTable.Columns.Add("Status", typeof(string));
            dataTable.Columns.Add("Description", typeof(string));
            dataTable.Columns.Add("RTE Direction", typeof(string));
            dataTable.Columns.Add("Mapping Type", typeof(string));
            dataTable.Columns.Add("Pseudo Code", typeof(string));
            dataTable.Columns.Add("System Constant", typeof(string));
            dataTable.Columns.Add("FC Name", typeof(string));
            dataTable.Columns.Add("Producer", typeof(string));
            dataTable.Columns.Add("Consumers", typeof(string));
            
            for (int i = 0; i < count; i++)
            {
                var dataflowInterface = GenerateDataflowInterface();
                var row = dataTable.NewRow();
                
                row["Status"] = dataflowInterface.status;
                row["Description"] = dataflowInterface.description;
                row["RTE Direction"] = dataflowInterface.Rte_Direction;
                row["Mapping Type"] = dataflowInterface.MappingType;
                row["Pseudo Code"] = dataflowInterface.Pseudo_code;
                row["System Constant"] = dataflowInterface.System_constant;
                row["FC Name"] = dataflowInterface.FC_Name;
                row["Producer"] = dataflowInterface.Producer;
                row["Consumers"] = dataflowInterface.Consumers;
                
                dataTable.Rows.Add(row);
            }
            
            return dataTable;
        }

        /// <summary>
        /// Generates a list of interface names for batch testing
        /// </summary>
        /// <param name="count">Number of interface names to generate</param>
        /// <returns>List of unique interface names</returns>
        public static List<string> GenerateInterfaceNames(int count = 20)
        {
            var names = new HashSet<string>();
            
            while (names.Count < count)
            {
                names.Add(GenerateInterfaceName());
            }
            
            return names.ToList();
        }

        /// <summary>
        /// Generates a realistic A2L measurement entry
        /// </summary>
        /// <param name="interfaceName">Interface name for the measurement</param>
        /// <returns>A2L measurement string</returns>
        public static string GenerateA2LMeasurement(string interfaceName)
        {
            var swType = _swTypes[_random.Next(_swTypes.Length)];
            var address = $"0x{_random.Next(0x1000, 0x9999):X4}";
            var (minVal, maxVal) = GetTypeRange(swType);
            
            return $"        MEASUREMENT {interfaceName} \"Generated measurement for {interfaceName}\"\n" +
                   $"        {swType} {address} 1 {minVal} {maxVal}";
        }

        #region Private Helper Methods

        private static string GenerateInterfaceName()
        {
            var prefix = _interfaceNamePrefixes[_random.Next(_interfaceNamePrefixes.Length)];
            var suffix = _interfaceNameSuffixes[_random.Next(_interfaceNameSuffixes.Length)];
            var number = _random.Next(1, 999).ToString("D3");
            return $"{prefix}{suffix}_{number}";
        }

        private static string[] GenerateInputList()
        {
            var inputCount = _random.Next(0, 4); // 0-3 inputs
            var inputs = new string[inputCount];
            
            for (int i = 0; i < inputCount; i++)
            {
                inputs[i] = $"Input_{i + 1}_{GenerateInterfaceName()}";
            }
            
            return inputs;
        }

        private static string GenerateOutputName(string interfaceName)
        {
            return $"Output_{interfaceName}";
        }

        private static string GenerateMinValue(string swType, string unit)
        {
            var (minVal, _) = GetTypeRange(swType);
            
            // Apply some unit-specific logic
            switch (unit)
            {
                case "°C":
                    return "-40";
                case "%":
                case "V":
                case "A":
                    return "0";
                default:
                    return minVal.ToString();
            }
        }

        private static string GenerateMaxValue(string swType, string unit)
        {
            var (_, maxVal) = GetTypeRange(swType);
            
            // Apply some unit-specific logic
            switch (unit)
            {
                case "°C":
                    return "150";
                case "%":
                    return "100";
                case "V":
                    return "12";
                case "A":
                    return "100";
                default:
                    return maxVal.ToString();
            }
        }

        private static string GenerateResolution(string swType)
        {
            switch (swType)
            {
                case "UINT8":
                case "SINT8":
                case "BOOLEAN":
                    return "1";
                case "FLOAT32":
                case "FLOAT64":
                    return "0.001";
                default:
                    return "0.1";
            }
        }

        private static string GenerateInitialValue(string swType)
        {
            switch (swType)
            {
                case "BOOLEAN":
                    return _random.Next(0, 2).ToString();
                case "FLOAT32":
                case "FLOAT64":
                    return "0.0";
                default:
                    return "0";
            }
        }

        private static string GenerateConversion(string unit)
        {
            switch (unit)
            {
                case "°C":
                    return "RAW * 0.1 - 40";
                case "%":
                    return "RAW * 0.1";
                case "V":
                    return "RAW * 0.001";
                case "A":
                    return "RAW * 0.01";
                default:
                    return "RAW * 1.0";
            }
        }

        private static string GetRandomRteDirection()
        {
            var directions = new[] { "Input", "Output", "Bidirectional" };
            return directions[_random.Next(directions.Length)];
        }

        private static string GetRandomMappingType()
        {
            var types = new[] { "Direct", "Indirect", "Calculated", "Table", "Formula" };
            return types[_random.Next(types.Length)];
        }

        private static string GetRandomStatus()
        {
            var statuses = new[] { "Active", "Inactive", "Deprecated", "Development", "Testing" };
            return statuses[_random.Next(statuses.Length)];
        }

        private static string GeneratePseudoCode(string interfaceName, string direction)
        {
            switch (direction)
            {
                case "Input":
                    return $"IF {interfaceName}_Valid THEN Read_{interfaceName}() ENDIF";
                case "Output":
                    return $"IF Calculation_Ready THEN Write_{interfaceName}(value) ENDIF";
                default:
                    return $"Process_{interfaceName}(input, output)";
            }
        }

        private static string GenerateSystemConstant(string interfaceName)
        {
            return $"K_{interfaceName.ToUpper()}_CONST";
        }

        private static string GenerateFunctionName(string interfaceName)
        {
            return $"FC_{interfaceName}_Process";
        }

        private static string GenerateProducerName(string interfaceName)
        {
            var modules = new[] { "ENGINE_MGT", "TRANS_CTRL", "BRAKE_CTRL", "HVAC_CTRL", "BATTERY_MGT" };
            return modules[_random.Next(modules.Length)];
        }

        private static string GenerateConsumerList(string interfaceName)
        {
            var consumerCount = _random.Next(1, 4);
            var consumers = new List<string>();
            var modules = new[] { "DIAG_MGT", "HMI_CTRL", "GATEWAY", "DATA_LOG", "SAFETY_MGT" };
            
            for (int i = 0; i < consumerCount; i++)
            {
                consumers.Add(modules[_random.Next(modules.Length)]);
            }
            
            return string.Join(";", consumers.Distinct());
        }

        private static (long min, long max) GetTypeRange(string swType)
        {
            switch (swType)
            {
                case "UINT8":
                    return (0, 255);
                case "SINT8":
                    return (-128, 127);
                case "UINT16":
                    return (0, 65535);
                case "SINT16":
                    return (-32768, 32767);
                case "UINT32":
                    return (0, 4294967295);
                case "SINT32":
                    return (-2147483648, 2147483647);
                case "BOOLEAN":
                    return (0, 1);
                case "FLOAT32":
                    return (-3402823, 3402823); // Simplified for testing
                case "FLOAT64":
                    return (-1798693, 1798693); // Simplified for testing
                default:
                    return (0, 65535);
            }
        }

        #endregion
    }
}
