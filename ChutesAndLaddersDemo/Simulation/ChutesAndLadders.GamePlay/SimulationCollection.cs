﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ChutesAndLadders.Entities;

namespace ChutesAndLadders.GamePlay
{
    /// <summary>
    /// Runs multiple simulations in parallel, rotating the players so that each player 
    /// gets to be player 1 for the same number of simulations.
    /// </summary>
    public class SimulationCollection
    {
        const int _maxStartingLocation = 25;

        GameBoard _board = new GameBoard();


        public SimulationResults RunSimulations(Player[] players, int maxExecutionCount, bool outputResults = false, string outputGameActionsFolder = null)
        {
            if (maxExecutionCount < players.Count())
                throw new ArgumentException("You must execute the simulation at least once per player");

            double executionsPerPlayer = maxExecutionCount / players.Count();
            int executions = Convert.ToInt32(Math.Round(executionsPerPlayer));

            var tasks = new Task<SimulationResults>[players.Count()];
            for (int i = 0; i < players.Count(); i++)
            {
                if (!string.IsNullOrWhiteSpace(outputGameActionsFolder))
                {
                    string gameActionFilePath = System.IO.Path.Combine(outputGameActionsFolder, $"GameActions_Player{i+1}First.csv");
                    tasks[i] = Task.Factory.StartNew(() => (new Simulation(_board, _maxStartingLocation)).Run(players.DeepCopy().ToArray(), executions, gameActionFilePath));
                }
                else
                    tasks[i] = Task.Factory.StartNew(() => (new Simulation(_board, _maxStartingLocation)).Run(players.DeepCopy().ToArray(), executions));

                players = players.Rotate();
            }

            Task.WaitAll(tasks);

            int totalGames = 0;
            foreach (var player in players)
            {
                var thisPlayerInstances = tasks.Select(t => t.Result.Players.Where(p => p.Id == player.Id).Single());
                player.WinCount = thisPlayerInstances.Sum(p => p.WinCount);
                totalGames += player.WinCount;
            }

            if (outputResults)
            {
                Console.WriteLine($"Total Games: {totalGames}");
                foreach (var player in players)
                    Console.WriteLine($"{player.Name} using strategy: {player.Strategy.Name}  wins: {player.WinCount}");
                Console.WriteLine();
            }

            return new SimulationResults()
            {
                Players = players
            };
        }

    }
}
