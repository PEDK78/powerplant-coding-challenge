using Engie.PCC.Api.Models;
using Engie.PCC.Api.Services.Calculators;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Engie.PCC.Api.Services
{
    /// <summary>
    /// Production Plan Service is responsible to compute the dispatching load on given powerplants
    /// </summary>
    public class ProductionPlanService : IProductionPlanService
    {
        private readonly ILogger<ProductionPlanService> _logger;
        private readonly PowerplantCalculatorFactory _calculatorFactory;

        public ProductionPlanService(
            PowerplantCalculatorFactory calculatorFactory,
            ILogger<ProductionPlanService> logger
            )
        {
            _calculatorFactory = calculatorFactory;
            _logger = logger;
        }

        /// <summary>
        /// General assumption, computing all possibilities using step of 0.1MWH will result in millions of options.
        /// Without a way to exclude a large range of options, computing time will be unreasonable.
        /// The choosen way is to first look at most restritives options (minimum power deliverd when powerplant is switched on) and then compensate using the merit-order.
        /// Finally, a try to power off some powerplants by cheapest alternatives is made.
        /// </summary>
        /// <param name="load">The requested load</param>
        /// <param name="fuels">Information about price</param>
        /// <param name="powerPlants">A list of <see cref="Powerplant"/> to use for dispatching the requested load.</param>
        /// <returns>A list of <see cref="PowerplantResult"/> containing the load to apply on each powerplant</returns>
        public async Task<List<PowerplantResult>> Calculate(decimal load, Fuels fuels, IEnumerable<Powerplant> powerPlants)
        {
            // compute some value to facilitate the processing.
            var computedPowerplants = powerPlants.Select(p => ComputedPowerplant.Create(fuels, p, _calculatorFactory));

            var maxAvailablePower = computedPowerplants.Sum(x => x.MaxAvailablePower);
            if(maxAvailablePower < load)
            {
                var msg = $"A load of {load}MWH is requested while only {maxAvailablePower}MWH can be delivered, unable to produce the requested quantity. ";
                _logger.LogError(msg);
                throw new ProductionPlanServiceException(msg);
            }

            // apply computation by using more restrictive first 
            _logger.LogDebug("Start dispatching using more restrictive powerplants");
            var powerPlantsByMeritOrder = computedPowerplants.OrderByDescending(p => p.MinAvailablePower)
                .ThenBy(p => p.CostPerMWH)
                .ThenBy(p => p.Powerplant.Name).ToArray();
            DispatchInitialLoad(load, powerPlantsByMeritOrder);


            // now that more restrictive powerplants are initialized, 
            // we can try to balanced the load by merit order.
            _logger.LogDebug("Ordering powerplant by merit order and compensate the overall load");
            powerPlantsByMeritOrder = powerPlantsByMeritOrder.OrderBy(p => p.CostPerMWH).ThenByDescending(p => p.MaxAvailablePower)
                .ThenBy(p => p.Powerplant.Name).ToArray();

            // try to compensate the full load, the remaining will be the sum of more restrictive powerplants minimum power
            var remaining = Compensate(powerPlantsByMeritOrder, load);
            _logger.LogDebug("Start dispatching the remaining load");
            // so, wa can now dispatch the remaining load by merit order.
            DispatchRemainingLoad(load - remaining, powerPlantsByMeritOrder);

            _logger.LogDebug("Affine dispatching by powering off some powerplants if possible");
            // try to find more efficient alternatives
            AffineDispatchedLoad(powerPlantsByMeritOrder);

            //Verify that the complete requested load has been dispatched
            var totalDispatchedPower = powerPlantsByMeritOrder.Sum(x => x.ToProduce);
            if (totalDispatchedPower != load)
            {
                var msg = $"Something went wrong, a load of {load}MWH is requested while {totalDispatchedPower}MWH was planned, planning additionnal power will induce an over production.";
                _logger.LogError(msg);
                throw new ProductionPlanServiceException(msg);
            }

            //It seems that we have an option, return the result
            var res = powerPlantsByMeritOrder.Select(p => PowerplantResult.Create(p.Powerplant.Name, p.ToProduce)).ToList();
            _logger.LogDebug($"Result is as follow: \"{JsonConvert.SerializeObject(res)}\".");
            return await Task.FromResult(res);
        }

        /// <summary>
        /// Initial power load dispatching. Apply the load on powerplants respecting the list order
        /// </summary>
        /// <param name="load">The power load to dispatch</param>
        /// <param name="powerPlants">The list of <see cref="Powerplant"/></param>
        private void DispatchInitialLoad(decimal load, ComputedPowerplant[] powerPlants)
        {
            // Start with the full load to produce.
            decimal remainingLoadToProduce = load;
            decimal availableCompensation = 0;
            for (int i = 0; i < powerPlants.Length; i++)
            {
                var powerPlant = powerPlants[i];
                if (remainingLoadToProduce <= 0)
                {
                    // the load is reached, the powerplant should not produce anything.
                    powerPlant.ToProduce = 0;
                    break;
                }

                if (remainingLoadToProduce >= powerPlant.MaxAvailablePower)
                {
                    // the load is not reached and it is under the maximum possibility of the powerplant, put powerplant at its max.
                    powerPlant.ToProduce = powerPlant.MaxAvailablePower;

                    remainingLoadToProduce = remainingLoadToProduce - powerPlant.ToProduce;

                    // keep trace of possible compensation for later in the process.
                    availableCompensation = availableCompensation + (powerPlant.MaxAvailablePower - powerPlant.MinAvailablePower);
                    continue;
                }

                if (powerPlant.MinAvailablePower <= remainingLoadToProduce)
                {
                    // the load is not reached and it is over the minimum possibility of the powerplant, ask powerplan to produce the remaing.
                    powerPlant.ToProduce = remainingLoadToProduce;

                    remainingLoadToProduce = 0;
                    continue;
                }


                if (powerPlant.MinAvailablePower > remainingLoadToProduce && powerPlant.MinAvailablePower - remainingLoadToProduce <= availableCompensation)
                {
                    // the load is not reached and it is under the minimum possibility of the powerplant and compensation is possibe
                    // put powerplant at its minimum and remove extra produciton on other powerplants.

                    powerPlant.ToProduce = powerPlant.MinAvailablePower;

                    Compensate(powerPlants, powerPlant.MinAvailablePower - remainingLoadToProduce);

                    remainingLoadToProduce = 0;
                    continue;
                }

                powerPlant.ToProduce = 0;
            }
        }

        private void DispatchRemainingLoad(decimal load, ComputedPowerplant[] powerPlants)
        {
            // Start with the full load to produce.
            decimal remainingLoadToProduce = load;
            for (int i = 0; i < powerPlants.Length; i++)
            {
                var powerPlant = powerPlants[i];
                if (remainingLoadToProduce <= 0)
                {
                    // the load is reached, the powerplant should not produce anything.
                    break;
                }

                if ((powerPlant.ToProduce + remainingLoadToProduce) >= powerPlant.MinAvailablePower && powerPlant.ToProduce < powerPlant.MaxAvailablePower)
                {
                    // there is remaing prower production available, we can ajust the load.
                    var allowed = powerPlant.MaxAvailablePower - powerPlant.ToProduce;
                    if(allowed > remainingLoadToProduce)
                    {
                        powerPlant.ToProduce += remainingLoadToProduce;
                        remainingLoadToProduce = 0;
                    }
                    else
                    {
                        powerPlant.ToProduce += allowed;
                        remainingLoadToProduce -= allowed;
                    }
                }
            }
        }

        /// <summary>
        /// Compensate the load, try to lowered most expansive powerplant at the benefit of cheapest.
        /// </summary>
        /// <param name="powerPlants">The list of <see cref="Powerplant"/></param>
        /// <param name="amount">The amout of power to compensate</param>
        /// <returns>Returns the remaining power whihc was not compensate.</returns>
        private decimal Compensate(ComputedPowerplant[] powerPlants, decimal amount)
        {
            // try to minimize the cost by reversing the merit order
            var powerPlantsByReversedMeritOrder = powerPlants.Reverse().ToArray(); 

            var toCompensate = amount;

            for (int i = 0; i < powerPlantsByReversedMeritOrder.Length; i++)
            {
                var powerPlant = powerPlantsByReversedMeritOrder[i];

                if (powerPlant.ToProduce > 0)
                {
                    // compute the minimum that can be lowered
                    var compensation = Math.Min(toCompensate, powerPlant.ToProduce - powerPlant.MinAvailablePower);

                    powerPlant.ToProduce = powerPlant.ToProduce - compensation;
                    toCompensate = toCompensate - compensation;
                }

                if(toCompensate == 0){ return 0; }
            }

            return toCompensate;
        }

        /// <summary>
        /// From the most expansive to the cheapest powerplant, try to find an alternative allowing to down of the powerplant
        /// </summary>
        /// <param name="powerPlants">The list of <see cref="Powerplant"/></param>
        private void AffineDispatchedLoad(ComputedPowerplant[] powerPlants)
        {
            for (int i = powerPlants.Length - 1; i > 0; i--)
            {
                var powerPlant = powerPlants[i];

                if (powerPlant.ToProduce > 0 && powerPlant.ToProduce >= powerPlant.MinAvailablePower && powerPlant.ToProduce < powerPlant.MaxAvailablePower)
                {
                    var powerToDispatch = powerPlant.ToProduce;

                    // take the upper powerplants list
                    var before = powerPlants.Take(i).ToArray();
                    // get available power
                    var availableBefore = before.Sum(x => x.MaxAvailablePower - x.ToProduce);
                    if (availableBefore == 0) { continue; }

                    // take the lower powerplants list
                    var after = powerPlants.Skip(i + 1).ToArray();
                    // get available power
                    var availableAfter = after.Sum(x => x.MaxAvailablePower - x.ToProduce);

                    // check that available power supports the power to dispatch
                    if ((availableBefore + availableAfter) < powerToDispatch) { continue; }

                    // if the upper available power is less than the power to dispatch, then it is the starting point 
                    if (availableBefore < powerToDispatch) { powerToDispatch = availableBefore; }

                    // the cost implied by the current option, we should find something be lowered than that
                    decimal currentCost = powerPlant.ToProduce * powerPlant.CostPerMWH;

                    decimal powerToDispatchOnUpperPart = 0;

                    //TODO: find an improvement to reduce time processing
                    while (powerToDispatch > 0)
                    {
                        var c1 = ComputeCostForLoad(powerToDispatch, before);
                        var c2 = after.Length > 0 ? ComputeCostForLoad(powerPlant.ToProduce - powerToDispatch, after) : 0;

                        if(c1 >= 0 && c2 >= 0 && (c1 + c2) <= currentCost)
                        {
                            // we get something cheaper.
                            // the next step is to compute cost with less power on the upper part
                            // assumption is the we found the cheapest option.
                            powerToDispatchOnUpperPart = powerToDispatch;
                            break;
                        }

                        powerToDispatch -= after.Length > 0 ? 0.1m : powerToDispatch;
                    }

                    if(powerToDispatchOnUpperPart > 0)
                    {
                        // here we are, dispatch the remaining load on other and shut down the powerplant
                        DispatchRemainingLoad(powerToDispatchOnUpperPart, before);
                        DispatchRemainingLoad(powerPlant.ToProduce - powerToDispatchOnUpperPart, after);
                        powerPlant.ToProduce = 0;
                    }
                }
            }
        }

        /// <summary>
        /// Compute the cost for the load using the <see cref="Powerplant"/> list.
        /// </summary>
        /// <param name="load"></param>
        /// <param name="powerPlants">The list of <see cref="Powerplant"/></param>
        /// <returns>The computed cost or -1 if the available power is less than the requested load</returns>
        /// <remarks>Cost is take into account only if there are still some availability on the powerplant</remarks>
        private decimal ComputeCostForLoad(decimal load, ComputedPowerplant[] powerPlants)
        {
            // Start with the full load to produce.
            var remainingLoadToProduce = load;
            decimal cost = 0;
            for (int i = 0; i < powerPlants.Length; i++)
            {
                var powerPlant = powerPlants[i];
                if (remainingLoadToProduce <= 0)
                {
                    // the load is reached, the powerplant should not produce anything.
                    break;
                }

                if ((powerPlant.ToProduce + remainingLoadToProduce) >= powerPlant.MinAvailablePower && powerPlant.ToProduce <= powerPlant.MaxAvailablePower)
                {
                    var allowed = powerPlant.MaxAvailablePower - powerPlant.ToProduce;
                    if (allowed > remainingLoadToProduce)
                    {
                        cost += remainingLoadToProduce * powerPlant.CostPerMWH;
                        remainingLoadToProduce = 0;
                    }
                    else
                    {
                        cost += allowed * powerPlant.CostPerMWH;
                        remainingLoadToProduce -= allowed;
                    }
                }
            }

            if(remainingLoadToProduce == 0) { return cost; }
            return -1;
        }
    }
}
