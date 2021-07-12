using Engie.PCC.Api.Models;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Xunit;
using System;
using Engie.PCC.Api.Services;

namespace Engie.PCC.Api.Tests.Tests
{
    public class ServiceTests : TestBase
    {
        [Fact]
        public void LoadIsLowerThanCombinedGeneration_OneTypeSameEfficiency()
        {
            var load = 230;
            var fuels = Fuels.Create(12.5m, 50.8m, 60, 20);
            var plants = new List<Powerplant>
            {
                Powerplant.Create("g1", PowerPlantType.GasFired, 0.51m, 10, 100),
                Powerplant.Create("g2", PowerPlantType.GasFired, 0.75m, 10, 200)
            };

            // Act.
            var service = GetProductionPlanService();
            var result = service.Calculate(load, fuels, plants).Result;

            // Assert.
            result.Sum(p => p.Power).Should().Be(load);

            var expectedResult = new List<PowerplantResult>
            {
                PowerplantResult.Create("g2", 200),
                PowerplantResult.Create("g1", 30)
            };

            result.Should().BeEquivalentTo(expectedResult);
        }

        [Fact]
        public void LoadIsLowerThanCombinedGeneration_OneTypeDifferentEfficiencies()
        {
            var load = 230;
            var fuels = Fuels.Create(12.5m, 50.8m, 60, 20);
            var plants = new List<Powerplant>
            {
                Powerplant.Create("g1", PowerPlantType.GasFired, 0.75m, 10, 100),
                Powerplant.Create("g2", PowerPlantType.GasFired, 0.51m, 10, 200)
            };

            // Act.
            var service = GetProductionPlanService();
            var result = service.Calculate(load, fuels, plants).Result;

            // Assert.
            result.Sum(p => p.Power).Should().Be(load);

            var expectedResult = new List<PowerplantResult>
            {
                PowerplantResult.Create("g1", 100),
                PowerplantResult.Create("g2", 130)
            };

            result.Should().BeEquivalentTo(expectedResult);
        }

        [Fact]
        public void LoadIsLowerThanCombinedGeneration_TwoTypes()
        {
            var load = 230;
            var fuels = Fuels.Create(12.5m, 50.8m, 60, 20);
            var plants = new List<Powerplant>
            {
                Powerplant.Create("g1", PowerPlantType.GasFired, 0.51m, 10, 100),
                Powerplant.Create("g2", PowerPlantType.GasFired, 0.75m, 10, 200),
                Powerplant.Create("w1", PowerPlantType.WindTurbine, 1, 0, 25)
            };

            // Act.
            var service = GetProductionPlanService();
            var result = service.Calculate(load, fuels, plants).Result;

            // Assert.
            result.Sum(p => p.Power).Should().Be(load);

            var expectedResult = new List<PowerplantResult>
            {
                PowerplantResult.Create("w1", (plants.First(x => x.Name == "w1").MaxPower) * fuels.Wind / 100),
                PowerplantResult.Create("g2", 200),
                PowerplantResult.Create("g1", 15)
            };

            result.Should().BeEquivalentTo(expectedResult);
        }

        [Fact]
        public void LoadIsLowerThanCombinedGeneration_TwoTypes_ExpectOneGasFiredDown()
        {
            var load = 230;
            var fuels = Fuels.Create(12.5m, 50.8m, 60, 20);
            var plants = new List<Powerplant>
            {
                Powerplant.Create("g1", PowerPlantType.GasFired, 0.51m, 10, 100),
                Powerplant.Create("g2", PowerPlantType.GasFired, 0.75m, 10, 200),
                Powerplant.Create("w1", PowerPlantType.WindTurbine, 1, 0, 25),
                Powerplant.Create("w2", PowerPlantType.WindTurbine, 1, 0, 25)
            };

            // Act.
            var service = GetProductionPlanService();
            var result = service.Calculate(load, fuels, plants).Result;

            // Assert.
            result.Sum(p => p.Power).Should().Be(load);

            var expectedResult = new List<PowerplantResult>
            {
                PowerplantResult.Create("w1", (plants.First(x => x.Name == "w1").MaxPower) * fuels.Wind / 100),
                PowerplantResult.Create("w2", (plants.First(x => x.Name == "w2").MaxPower) * fuels.Wind / 100),
                PowerplantResult.Create("g2", 200),
                PowerplantResult.Create("g1", 0)
            };

            result.Should().BeEquivalentTo(expectedResult);
        }

        [Fact]
        public void LoadIsLowerThanCombinedGeneration_TwoTypes_ExpectLastGasFiredAtMinimum()
        {
            var load = 232;
            var fuels = Fuels.Create(12.5m, 50.8m, 60, 20);
            var plants = new List<Powerplant>
            {
                Powerplant.Create("g1", PowerPlantType.GasFired, 0.51m, 10, 100),
                Powerplant.Create("g2", PowerPlantType.GasFired, 0.75m, 10, 200),
                Powerplant.Create("w1", PowerPlantType.WindTurbine, 1, 0, 25),
                Powerplant.Create("w2", PowerPlantType.WindTurbine, 1, 0, 25)
            };

            // Act.
            var service = GetProductionPlanService();
            var result = service.Calculate(load, fuels, plants).Result;

            // Assert.
            result.Sum(p => p.Power).Should().Be(load);

            var expectedResult = new List<PowerplantResult>
            {
                PowerplantResult.Create("w1", (plants.First(x => x.Name == "w1").MaxPower) * fuels.Wind / 100),
                PowerplantResult.Create("w2", (plants.First(x => x.Name == "w2").MaxPower) * fuels.Wind / 100),
                PowerplantResult.Create("g2", 192),
                PowerplantResult.Create("g1", 10)
            };

            result.Should().BeEquivalentTo(expectedResult);
        }

        [Fact]
        public void LoadIsLowerThanCombinedGeneration_TwoTypes_DifferentWindTurbine_ExpectLastGasFiredAtMinimum()
        {
            var load = 245;
            var fuels = Fuels.Create(12.5m, 50.8m, 60, 20);
            var plants = new List<Powerplant>
            {
                Powerplant.Create("g1", PowerPlantType.GasFired, 0.51m, 10, 100),
                Powerplant.Create("g2", PowerPlantType.GasFired, 0.75m, 10, 200),
                Powerplant.Create("w1", PowerPlantType.WindTurbine, 1, 0, 25),
                Powerplant.Create("w2", PowerPlantType.WindTurbine, 1, 0, 48)
            };

            // Act.
            var service = GetProductionPlanService();
            var result = service.Calculate(load, fuels, plants).Result;

            // Assert.
            result.Sum(p => p.Power).Should().Be(load);

            var expectedResult = new List<PowerplantResult>
            {
                PowerplantResult.Create("w2", (plants.First(x => x.Name == "w2").MaxPower) * fuels.Wind / 100),
                PowerplantResult.Create("w1", (plants.First(x => x.Name == "w1").MaxPower) * fuels.Wind / 100),
                PowerplantResult.Create("g2", 191.2m),
                PowerplantResult.Create("g1", 10)
            };

            result.Should().BeEquivalentTo(expectedResult);
        }

        [Fact]
        public void LoadIsLowerThanCombinedGeneration_ThreeTypes_ExpectTurbojetDown()
        {
            var load = 300;
            var fuels = Fuels.Create(12.5m, 50.8m, 60, 20);
            var plants = new List<Powerplant>
            {
                Powerplant.Create("g1", PowerPlantType.GasFired, 0.51m, 10, 100),
                Powerplant.Create("g2", PowerPlantType.GasFired, 0.75m, 10, 200),
                Powerplant.Create("w1", PowerPlantType.WindTurbine, 1, 0, 25),
                Powerplant.Create("w2", PowerPlantType.WindTurbine, 1, 0, 48),
                Powerplant.Create("t1", PowerPlantType.Turbojet, 0.8m, 0, 16)
            };

            // Act.
            var service = GetProductionPlanService();
            var result = service.Calculate(load, fuels, plants).Result;

            // Assert.
            result.Sum(p => p.Power).Should().Be(load);

            var expectedResult = new List<PowerplantResult>
            {
                PowerplantResult.Create("w2", (plants.First(x => x.Name == "w2").MaxPower) * fuels.Wind / 100),
                PowerplantResult.Create("w1", (plants.First(x => x.Name == "w1").MaxPower) * fuels.Wind / 100),
                PowerplantResult.Create("g2", 200),
                PowerplantResult.Create("g1", 56.2m),
                PowerplantResult.Create("t1", 0),
            };

            result.Should().BeEquivalentTo(expectedResult);
        }

        [Fact]
        public void LoadIsLowerThanCombinedGeneration_ThreeTypes_TurbjetAttractive_ExpectTurbojetAtMax()
        {
            var load = 300;
            var fuels = Fuels.Create(12.5m, 12.4m, 60, 20);
            var plants = new List<Powerplant>
            {
                Powerplant.Create("g1", PowerPlantType.GasFired, 0.51m, 10, 100),
                Powerplant.Create("g2", PowerPlantType.GasFired, 0.75m, 10, 200),
                Powerplant.Create("w1", PowerPlantType.WindTurbine, 1, 0, 25),
                Powerplant.Create("w2", PowerPlantType.WindTurbine, 1, 0, 48),
                Powerplant.Create("t1", PowerPlantType.Turbojet, 0.8m, 0, 16)
            };

            // Act.
            var service = GetProductionPlanService();
            var result = service.Calculate(load, fuels, plants).Result;

            // Assert.
            result.Sum(p => p.Power).Should().Be(load);

            var expectedResult = new List<PowerplantResult>
            {
                PowerplantResult.Create("w2", (plants.First(x => x.Name == "w2").MaxPower) * fuels.Wind / 100),
                PowerplantResult.Create("w1", (plants.First(x => x.Name == "w1").MaxPower) * fuels.Wind / 100),
                PowerplantResult.Create("t1", 16),
                PowerplantResult.Create("g2", 200),
                PowerplantResult.Create("g1", 40.2m)
            };

            result.Should().BeEquivalentTo(expectedResult);
        }

        [Fact]
        public void LoadIsLowerThanCombinedGeneration_ThreeTypes_TurbjetAttractive_ExpectTurbojetAtMax_ExpectLastGasFiredDown()
        {
            var load = 255;
            var fuels = Fuels.Create(12.5m, 12.4m, 60, 20);
            var plants = new List<Powerplant>
            {
                Powerplant.Create("g1", PowerPlantType.GasFired, 0.51m, 10, 100),
                Powerplant.Create("g2", PowerPlantType.GasFired, 0.75m, 10, 200),
                Powerplant.Create("w1", PowerPlantType.WindTurbine, 1, 0, 25),
                Powerplant.Create("w2", PowerPlantType.WindTurbine, 1, 0, 48),
                Powerplant.Create("t1", PowerPlantType.Turbojet, 0.8m, 0, 16)
            };

            // Act.
            var service = GetProductionPlanService();
            var result = service.Calculate(load, fuels, plants).Result;

            // Assert.
            result.Sum(p => p.Power).Should().Be(load);

            var expectedResult = new List<PowerplantResult>
            {
                PowerplantResult.Create("w2", (plants.First(x => x.Name == "w2").MaxPower) * fuels.Wind / 100),
                PowerplantResult.Create("w1", (plants.First(x => x.Name == "w1").MaxPower) * fuels.Wind / 100),
                PowerplantResult.Create("t1", 16),
                PowerplantResult.Create("g2", 195.2m),
                PowerplantResult.Create("g1", 0)
            };

            result.Should().BeEquivalentTo(expectedResult);
        }

        [Fact]
        public void LoadIsHigherThanCombinedGeneration_ExpectExceptionThrown()
        {
            var load = 1500;
            var fuels = Fuels.Create(12.5m, 12.4m, 60, 20);
            var plants = new List<Powerplant>
            {
                Powerplant.Create("g1", PowerPlantType.GasFired, 0.51m, 10, 100),
                Powerplant.Create("g2", PowerPlantType.GasFired, 0.75m, 10, 200),
                Powerplant.Create("w1", PowerPlantType.WindTurbine, 1, 0, 25),
                Powerplant.Create("w2", PowerPlantType.WindTurbine, 1, 0, 48),
                Powerplant.Create("t1", PowerPlantType.Turbojet, 0.8m, 0, 16)
            };

            // Act.
            Action act = () => {
                var service = GetProductionPlanService();
                service.Calculate(load, fuels, plants).Wait();
            };

            act.Should().Throw<ProductionPlanServiceException>();
        }

        [Fact]
        public void LoadIsLowerThanMinimalDeliverable_ExpectExceptionThrown()
        {
            var load = 40;
            var fuels = Fuels.Create(12.5m, 12.4m, 60, 20);
            var plants = new List<Powerplant>
            {
                Powerplant.Create("g1", PowerPlantType.GasFired, 0.51m, 50, 100),
                Powerplant.Create("g2", PowerPlantType.GasFired, 0.75m, 50, 200),
                Powerplant.Create("w1", PowerPlantType.WindTurbine, 1, 0, 25)
            };

            // Act.
            Action act = () => {
                var service = GetProductionPlanService();
                service.Calculate(load, fuels, plants).Wait();
            };

            act.Should().Throw<ProductionPlanServiceException>();
        }

        [Fact]
        public void LoadIsLowerThanCombinedGeneration_ThreeTypes_WithOtherAlternatives_ExpectGasFiredDown()
        {
            var load = 35;
            var fuels = Fuels.Create(12.5m, 33, 60, 20);
            var plants = new List<Powerplant>
            {
                Powerplant.Create("g1", PowerPlantType.GasFired, 0.5m, 10, 100),
                Powerplant.Create("g2", PowerPlantType.GasFired, 0.5m, 10, 200),
                Powerplant.Create("w1", PowerPlantType.WindTurbine, 1, 0, 25),
                Powerplant.Create("w2", PowerPlantType.WindTurbine, 1, 0, 25),
                Powerplant.Create("t1", PowerPlantType.Turbojet, 0.8m, 0, 16)
            };

            // Act.
            var service = GetProductionPlanService();
            var result = service.Calculate(load, fuels, plants).Result;

            // Assert.
            result.Sum(p => p.Power).Should().Be(load);

            var expectedResult = new List<PowerplantResult>
            {
                PowerplantResult.Create("w1", 15),
                PowerplantResult.Create("w2", 15),
                PowerplantResult.Create("g2", 0),
                PowerplantResult.Create("g1", 0),
                PowerplantResult.Create("t1", 5)
            };

            result.Should().BeEquivalentTo(expectedResult);
        }



        [Fact]
        //Test values from dpettens
        public void CalculateUnitCommitment_ProductionPlanWithDifferentEfficiency_EfficiencyIsRespected()
        {
            var load = 350;
            var fuels = Fuels.Create(15, 50, 80);
            var plants = new List<Powerplant>
            {
                Powerplant.Create("Turbojet", PowerPlantType.Turbojet, 0.3m, 0, 25),
                Powerplant.Create("Wind", PowerPlantType.WindTurbine, 1, 0, 100),
                Powerplant.Create("Gas", PowerPlantType.GasFired, 0.5m, 100, 300)
            };

            // Act.
            var service = GetProductionPlanService();
            var result = service.Calculate(load, fuels, plants).Result;

            var expectedResult = new List<PowerplantResult>
            {
                PowerplantResult.Create("Wind", 80),
                PowerplantResult.Create("Gas", 270),
                PowerplantResult.Create("Turbojet", 0)
            };

            result.Sum(p => p.Power).Should().Be(load);
            result.Should().BeEquivalentTo(expectedResult);
        }

        [Fact]
        //Test values from dpettens
        public void CalculateUnitCommitment_LoadIsADecimalNumber_SumOfPowerPlantsIsEqualToLoad()
        {
            var load = 350.5m;
            var fuels = Fuels.Create(15, 50, 80);
            var plants = new List<Powerplant>
            {
                Powerplant.Create("Turbojet", PowerPlantType.Turbojet, 0.3m, 0, 25),
                Powerplant.Create("Wind", PowerPlantType.WindTurbine, 1, 0, 100),
                Powerplant.Create("Gas", PowerPlantType.GasFired, 0.5m, 100, 300)
            };

            // Act.
            var service = GetProductionPlanService();
            var result = service.Calculate(load, fuels, plants).Result;

            var expectedResult = new List<PowerplantResult>
            {
                PowerplantResult.Create("Wind", 80),
                PowerplantResult.Create("Gas", 270.5m),
                PowerplantResult.Create("Turbojet", 0)
            };

            result.Sum(p => p.Power).Should().Be(load);
            result.Should().BeEquivalentTo(expectedResult);
        }

        [Fact]
        //Test values from dpettens
        public void CalculateUnitCommitment_ExamplePayload1_ReturnsExpectedResult()
        {
            var load = 480;
            var fuels = Fuels.Create(13.4m, 50.8m, 60, 20);
            var plants = new List<Powerplant>
            {
                Powerplant.Create("gasfiredbig1", PowerPlantType.GasFired, 0.53m, 100, 460),
                Powerplant.Create("gasfiredbig2", PowerPlantType.GasFired, 0.53m, 100, 460),
                Powerplant.Create("gasfiredsomewhatsmaller", PowerPlantType.GasFired, 0.37m, 40, 210),
                Powerplant.Create("tj1", PowerPlantType.Turbojet, 0.3m, 0, 16),
                Powerplant.Create("windpark1", PowerPlantType.WindTurbine, 1, 0, 150),
                Powerplant.Create("windpark2", PowerPlantType.WindTurbine, 1, 0, 36)
            };

            // Act.
            var service = GetProductionPlanService();
            var result = service.Calculate(load, fuels, plants).Result;

            var expectedResult = new List<PowerplantResult>
            {
                PowerplantResult.Create("windpark1", 90),
                PowerplantResult.Create("windpark2", 21.6m),
                PowerplantResult.Create("gasfiredbig1", 368.4m),
                PowerplantResult.Create("gasfiredbig2", 0),
                PowerplantResult.Create("gasfiredsomewhatsmaller", 0),
                PowerplantResult.Create("tj1", 0),
            };

            result.Sum(p => p.Power).Should().Be(load);
            result.Should().BeEquivalentTo(expectedResult);
        }

        [Fact]
        //Test values from dpettens
        public void CalculateUnitCommitment_ExamplePayload2_ReturnsExpectedResult()
        {
            var load = 480;
            var fuels = Fuels.Create(13.4m, 50.8m, 0, 20);
            var plants = new List<Powerplant>
            {
                Powerplant.Create("gasfiredbig1", PowerPlantType.GasFired, 0.53m, 100, 460),
                Powerplant.Create("gasfiredbig2", PowerPlantType.GasFired, 0.53m, 100, 460),
                Powerplant.Create("gasfiredsomewhatsmaller", PowerPlantType.GasFired, 0.37m, 40, 210),
                Powerplant.Create("tj1", PowerPlantType.Turbojet, 0.3m, 0, 16),
                Powerplant.Create("windpark1", PowerPlantType.WindTurbine, 1, 0, 150),
                Powerplant.Create("windpark2", PowerPlantType.WindTurbine, 1, 0, 36)
            };

            // Act.
            var service = GetProductionPlanService();
            var result = service.Calculate(load, fuels, plants).Result;

            var expectedResult = new List<PowerplantResult>
            {
                PowerplantResult.Create("windpark1", 0),
                PowerplantResult.Create("windpark2", 0),
                PowerplantResult.Create("gasfiredbig1", 380),
                PowerplantResult.Create("gasfiredbig2", 100),
                PowerplantResult.Create("gasfiredsomewhatsmaller", 0),
                PowerplantResult.Create("tj1", 0)
            };

            result.Sum(p => p.Power).Should().Be(load);
            result.Should().BeEquivalentTo(expectedResult);
        }

        [Fact]
        //Test values from dpettens
        public void CalculateUnitCommitment_ExamplePayload3_ReturnsExpectedResult()
        {
            var load = 910;
            var fuels = Fuels.Create(13.4m, 50.8m, 60, 20);
            var plants = new List<Powerplant>
            {
                Powerplant.Create("gasfiredbig1", PowerPlantType.GasFired, 0.53m, 100, 460),
                Powerplant.Create("gasfiredbig2", PowerPlantType.GasFired, 0.53m, 100, 460),
                Powerplant.Create("gasfiredsomewhatsmaller", PowerPlantType.GasFired, 0.37m, 40, 210),
                Powerplant.Create("tj1", PowerPlantType.Turbojet, 0.3m, 0, 16),
                Powerplant.Create("windpark1", PowerPlantType.WindTurbine, 1, 0, 150),
                Powerplant.Create("windpark2", PowerPlantType.WindTurbine, 1, 0, 36)
            };

            // Act.
            var service = GetProductionPlanService();
            var result = service.Calculate(load, fuels, plants).Result;

            var expectedResult = new List<PowerplantResult>
            {
                PowerplantResult.Create("windpark1", 90),
                PowerplantResult.Create("windpark2", 21.6m),
                PowerplantResult.Create("gasfiredbig1", 460),
                PowerplantResult.Create("gasfiredbig2", 338.4m),
                PowerplantResult.Create("gasfiredsomewhatsmaller", 0),
                PowerplantResult.Create("tj1", 0),
            };

            result.Sum(p => p.Power).Should().Be(load);
            result.Should().BeEquivalentTo(expectedResult);
        }
    }
}
