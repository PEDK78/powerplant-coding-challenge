using Engie.PCC.Api.Models;
using Engie.PCC.Api.Services.Calculators;
using FluentAssertions;
using Xunit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Engie.PCC.Api.Tests.Tests
{
    public class CalcultatorTests
    {
        [Fact]
        public void TestGasFiredCalculator_ExpectValid()
        {
            var fuels = Fuels.Create(15, 24, 60, 20);
            var powerplant = Powerplant.Create("g1", PowerPlantType.GasFired, 0.5m, 50, 100);
            var caculator = new PowerplantCalculatorGasfired();
            var costPerUnit = caculator.ComputeCostPerMWH(fuels, powerplant);
            costPerUnit.Should().Be(36);

            var minPower = caculator.ComputeMinimumDeliverablePower(fuels, powerplant);
            minPower.Should().Be(50);

            var maxPower = caculator.ComputeMaximumDeliverablePower(fuels, powerplant);
            maxPower.Should().Be(100);
        }

        [Fact]
        public void TestGasFiredCalculator_UseInvalidPowerplantType_ExpectError()
        {
            var fuels = Fuels.Create(15, 24, 60, 20);
            var powerplant = Powerplant.Create("g1", PowerPlantType.Turbojet, 0.5m, 50, 100);

            // Act.
            Action act = () => {
                var caculator = new PowerplantCalculatorGasfired();
                caculator.ComputeCostPerMWH(fuels, powerplant);
            };

            act.Should().Throw<PowerplantCalculatorException>();
        }

        [Fact]
        public void TestTurbojetCalculator_ExpectValid()
        {
            var fuels = Fuels.Create(15, 24, 60, 20);
            var powerplant = Powerplant.Create("g1", PowerPlantType.Turbojet, 0.5m, 0, 75);
            var caculator = new PowerplantCalculatorTurbojet();
            var costPerUnit = caculator.ComputeCostPerMWH(fuels, powerplant);
            costPerUnit.Should().Be(48);

            var minPower = caculator.ComputeMinimumDeliverablePower(fuels, powerplant);
            minPower.Should().Be(0);

            var maxPower = caculator.ComputeMaximumDeliverablePower(fuels, powerplant);
            maxPower.Should().Be(75);
        }


        [Fact]
        public void TestTurbojetCalculator_UseInvalidPowerplantType_ExpectError()
        {
            var fuels = Fuels.Create(15, 24, 60, 20);
            var powerplant = Powerplant.Create("g1", PowerPlantType.GasFired, 0.5m, 50, 100);

            // Act.
            Action act = () => {
                var caculator = new PowerplantCalculatorTurbojet();
                caculator.ComputeCostPerMWH(fuels, powerplant);
            };

            act.Should().Throw<PowerplantCalculatorException>();
        }

        [Fact]
        public void TestWindTurbineCalculator_ExpectValid()
        {
            var fuels = Fuels.Create(15, 24, 60, 20);
            var powerplant = Powerplant.Create("g1", PowerPlantType.WindTurbine, 1, 0, 75);
            var caculator = new PowerplantCalculatorWindTurbine();
            var costPerUnit = caculator.ComputeCostPerMWH(fuels, powerplant);
            costPerUnit.Should().Be(0);

            var minPower = caculator.ComputeMinimumDeliverablePower(fuels, powerplant);
            minPower.Should().Be(0);

            var maxPower = caculator.ComputeMaximumDeliverablePower(fuels, powerplant);
            maxPower.Should().Be(45);
        }

        [Fact]
        public void TestWindTurbineCalculator_UseInvalidPowerplantType_ExpectError()
        {
            var fuels = Fuels.Create(15, 24, 60, 20);
            var powerplant = Powerplant.Create("g1", PowerPlantType.GasFired, 0.5m, 50, 100);

            // Act.
            Action act = () => {
                var caculator = new PowerplantCalculatorWindTurbine();
                caculator.ComputeCostPerMWH(fuels, powerplant);
            };

            act.Should().Throw<PowerplantCalculatorException>();
        }
    }
}
