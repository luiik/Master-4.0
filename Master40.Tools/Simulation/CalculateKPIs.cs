﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Master40.DB.Data.Context;
using Master40.DB.Enums;
using Master40.DB.Models;

namespace Master40.Tools.Simulation
{
    public static class CalculateKpis
    {
        /// <summary>
        /// calls all implemented Kpi-calculating methods
        /// </summary>
        /// <param name="context"></param>
        /// <param name="simulationId"></param>
        public static void CalculateAllKpis(ProductionDomainContext context, int simulationId)
        {
            CalculateLeadTime(context, simulationId);
            CalculateMachineUtilization(context, simulationId);
            CalculateTimeliness(context);
        }

        /// <summary>
        /// must be called after filling SimulationSchedules!
        /// </summary>
        /// <param name="context"></param>
        /// <param name="simulationId"></param>
        public static void CalculateLeadTime(ProductionDomainContext context, int simulationId)
        {
            //calculate lead times for each product
            var leadTimes = new List<Kpi>();
            var finishedProducts = context.SimulationWorkschedules.Where(a => a.ParentId.Equals("[]") && a.SimulationConfigurationId == simulationId);
            foreach (var product in finishedProducts )
            {
                var endTime = product.End;
                var startTime = context.GetEarliestStart(context, product);
                leadTimes.Add(new Kpi(){
                    Value = endTime - startTime,
                    Name = product.Article
                });
            }
            //calculate Average per article
            var leadTimesAverage = new List<Kpi>();
            while (leadTimes.Any())
            {
                var relevantItems = leadTimes.Where(a => a.Name.Equals(leadTimes.First().Name)).ToList();
                leadTimesAverage.Add(new Kpi()
                {
                    Name = "LeadTime: "+relevantItems.First().Name,
                    Value = relevantItems.Sum(a => a.Value)/relevantItems.Count(),
                    IsKpi = true
                });
                foreach (var item in relevantItems)
                {
                    leadTimes.Remove(item);
                }
            }
            //insert
            context.Kpis.AddRange(leadTimesAverage);
            context.SaveChanges();
        }

        public static void CalculateMachineUtilization(ProductionDomainContext context, int simulationId)
        {
            //get machines
            var machines = context.Machines.Select(a => a.Name).ToList();
            //get SimulationTime
            var simulationTime = context.SimulationConfigurations.Single(a => a.Id == simulationId).SimulationEndTime;
            //get working time
            var kpis = (from machine in machines
                let relevantItems = context.SimulationWorkschedules.Where(a => a.Machine.Equals(machine)).ToList()
                let relevantItemTimes = relevantItems.Select(item => item.End - item.Start).ToList()
                select new Kpi()
                {
                    Value = (double) relevantItemTimes.Sum() / simulationTime,
                    Name = "MachineUtilization: " + machine,
                    IsKpi = true
                }).ToList();
            context.Kpis.AddRange(kpis);
            context.SaveChanges();
        }

        public static void CalculateTimeliness(ProductionDomainContext context)
        {
            var orderTimeliness = context.Orders.Where(a => a.State == State.Finished)
                                                .ToList()
                                                .Select(order => new Kpi()
            {
                Name = order.Name,
                Value = order.FinishingTime - order.DueTime
            }).ToList();

            var kpis = new Kpi()
            {
                Name = "Timeliness",
                Value = (double)orderTimeliness.Count(a => a.Value >= 0)/orderTimeliness.Count(),
                IsKpi = true
            };

            context.Add(kpis);
            context.SaveChanges();
        }

    }
}