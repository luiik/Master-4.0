﻿using System;
using System.Collections.Generic;
using Master40.DB.Data.Context;
using System.Linq;
using Master40.DB.Interfaces;
using Master40.DB.Models;
using Microsoft.EntityFrameworkCore;
using MathNet.Numerics.Random;
using MathNet.Numerics.Distributions;


namespace Master40.Tools.Simulation
{
    public static class OrderGenerator
    {
        public static void GenerateOrders(ProductionDomainContext context, int simulationId)
        {
            var time = 0;
            var samples = context.SimulationConfigurations.Single(a => a.Id == simulationId).OrderQuantity;
            var seed = new Random(context.SimulationConfigurations.Single(a => a.Id == simulationId).Seed);
            var productIds = context.ArticleBoms.Where(b => b.ArticleParentId == null).Select(a => a.ArticleChildId).ToList();

            var dist = new Exponential(rate: 0.25, randomSource: seed);
            //get equal distribution from 0 to 1
            var norml = new DiscreteUniform(0, productIds.Count() - 1, seed);


            double[] exponential = new double[samples]; //new Exponential(0.25, seed);
            int[] prodVariation = new int[samples];
            dist.Samples(exponential);
            norml.Samples(prodVariation);
            //get products by searching for articles without parents
            for (int i = 0; i < samples; i++) { 
                //define the time between each new order
                time += 50 + (int)Math.Round(exponential[i], MidpointRounding.AwayFromZero);
                //get which product is to be ordered
                var productId = productIds.ElementAt(prodVariation[i]);
                //create order and orderpart, duetime is creationtime + 1 day
                context.CreateNewOrder(productId, 1, time, time + 1440);
            }
        }

        public static List<double> TestUniformDistribution(int amount)
        {
            var samples = new List<double>();
            for (var i = 0; i < amount; i++)
            {
               samples.Add(MathNet.Numerics.Distributions.DiscreteUniform.Sample(0, 1000)/1000.00);
            }
            return samples;
        }

        public static List<double> TestExponentialDistribution(int amount)
        {
            var seed = 1337;
            var samples = 1000;
            SystemRandomSource randomSource = new SystemRandomSource(seed);
            var dist = new Exponential(rate: 0.25, randomSource: new Random(seed));

            double[] list = new double[1000];
            dist.Samples(list);
            
            return list.ToList();
        }
    }
}