﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace CPUScheduling_Sim.Source
{
    internal enum Algorithm
    {
        FCFS = 1,
        SJF_PREEMPTIVE,
        SJF_NONPREEMPTIVE,
        PRIORITY_PREEMPTIVE,
        PRIORITY_NONPREEMPTIVE,
        ROUND_ROBIN
    }
    internal static class Scheduler
    {
        /// <summary>
        /// The list of the processes.
        /// </summary>
        public static Processes Processes { get; set; } = new Processes();
        public static TimeSpan TimeQuantum { get; set; }

        /// <summary>
        /// Schedules processes by the given algorithm
        /// </summary>
        /// <param name="algorithm">algorithm to be used to schedule the processes</param>
        /// <returns>Scheduled processes based on the given algorithm</returns>
        /// <exception cref="NotImplementedException">Thrown when the given algorithm is not implemented</exception>
        public static Processes ScheduleProcesses(Algorithm algorithm) => algorithm switch
        {
            Algorithm.FCFS => FCFSSchedule(),
            Algorithm.SJF_PREEMPTIVE => SJFPreSchedule(),
            Algorithm.SJF_NONPREEMPTIVE => SJFNonPreSchedule(),
            Algorithm.PRIORITY_PREEMPTIVE => PriorityPreSchedule(),
            Algorithm.PRIORITY_NONPREEMPTIVE => PriorityNonPreSchedule(),
            Algorithm.ROUND_ROBIN => RoundRobinSchedule(),
            _ => throw new NotImplementedException("Please select a valid algorithm")
        };

        /// <summary>
        /// Schedule processes based on the FCFS algorithm and calculates the average waiting and turn around time
        /// </summary>
        /// <returns>Processes scheduled based on the FCFS algorithm</returns>
        private static Processes FCFSSchedule()
        {
            Processes processes = new Processes();

            processes.AddRange(Processes.OrderBy(p => p.ArriveTime));

            CalculateNonPreAverageTime(processes);

            return processes;
        }

        /// <summary>
        /// Schedule processes based on the Preemptive SJF algorithm and calculates the average waiting and turn around time
        /// </summary>
        /// <returns>Processes scheduled based on the Preemptive SJF algorithm</returns>
        private static Processes SJFPreSchedule()
        {
            Processes processes = new Processes();
            Processes final = new Processes();
            // do the sjf preemptive sort and calculate properties
            processes.AddRange(((Processes)Processes.Clone()).OrderBy(p => p.ArriveTime).ThenBy(p => p.CPUTime));

            var completionTime = FindCompletionTime(processes, processes.Count - 1);
            var timer = processes[0].ArriveTime;
            Process current = new Process { PID = processes[0].PID, ArriveTime = timer, CPUTime = TimeSpan.Zero };
            int i = 0;

            while (timer <= completionTime)
            {
                var ms = TimeSpan.FromMilliseconds(1);

                current.CPUTime += ms;
                processes[i].CPUTime -= ms;
                timer += ms;

                var next = processes.Find(p => p.CPUTime < processes[i].CPUTime && timer >= p.ArriveTime);

                if (processes[i].CPUTime == TimeSpan.Zero)
                {
                    if (processes.Count > 1)
                        processes.Remove(processes[i]);

                    next = processes.FindAll(p => timer >= p.ArriveTime).MinBy(p => p.CPUTime);
                    while (timer <= completionTime && next is null)
                    {
                        timer += ms;
                        next = processes.FindAll(p => timer >= p.ArriveTime).MinBy(p => p.CPUTime);
                    }

                }

                if (next is not null)
                {
                    final.Add(current);

                    i = processes.IndexOf(next);
                    current = new Process { PID = processes[i].PID, ArriveTime = timer, CPUTime = TimeSpan.Zero };
                }
            }
            // Calculating Turn around time and waiting time
            CalculatePreAverageTime(final);

            return final;
        }
        /// <summary>
        /// Schedule processes based on the Non-Preemptive SJF algorithm and calculates the average waiting and turn around time
        /// </summary>
        /// <returns>Processes scheduled based on the Non-Preemptive SJF algorithm</returns>
        private static Processes SJFNonPreSchedule()
        {
            Processes processes = new Processes();
            Processes final = new Processes();
            processes.AddRange(((Processes)Processes.Clone()).OrderBy(p => p.ArriveTime).ThenBy(p => p.CPUTime));

            var completionTime = FindCompletionTime(processes, processes.Count - 1);
            var timer = processes[0].ArriveTime;
            Process current = new Process { PID = processes[0].PID, ArriveTime = timer, CPUTime = TimeSpan.Zero };
            int i = 0;

            while (timer <= completionTime)
            {


                var ms = TimeSpan.FromMilliseconds(1);

                timer += ms;
                current.CPUTime += ms;
                processes[i].CPUTime -= ms;

                if (processes[i].CPUTime > TimeSpan.Zero)
                    continue;

                processes.Remove(processes[i]);


                if (processes.Count == 0)
                {
                    final.Add(current);
                    break;
                }
                var next = processes.FindAll(p => timer >= p.ArriveTime).MinBy(p => p.CPUTime);
                while (timer <= completionTime && next is null)
                {
                    timer += ms;
                    next = processes.FindAll(p => timer >= p.ArriveTime).MinBy(p => p.CPUTime);
                }

                if (next is not null)
                {
                    final.Add(current);

                    i = processes.IndexOf(next);
                    current = new Process { PID = processes[i].PID, ArriveTime = processes[i].ArriveTime, CPUTime = TimeSpan.Zero };
                }
            }


            CalculateNonPreAverageTime(final);

            return final;


        }

        /// <summary>
        /// Schedule processes based on the Preemptive Priority algorithm and calculates the average waiting and turn around time
        /// </summary>
        /// <returns>Processes scheduled based on the Preemptive Priority algorithm</returns>
        private static Processes PriorityPreSchedule()
        {
            Processes processes = new Processes();
            Processes final = new Processes();
            // do the priority sort and calculate properties
            processes.AddRange(((Processes)Processes.Clone()).OrderBy(p => p.ArriveTime).ThenBy(p => p.Priority));

            var completionTime = FindCompletionTime(processes, processes.Count - 1);
            var timer = processes[0].ArriveTime;
            Process current = new Process { PID = processes[0].PID, ArriveTime = timer, CPUTime = TimeSpan.Zero };
            int i = 0;

            while (timer <= completionTime)
            {
                var ms = TimeSpan.FromMilliseconds(1);

                current.CPUTime += ms;
                processes[i].CPUTime -= ms;
                timer += ms;

                var next = processes.Find(p => p.Priority < processes[i].Priority && timer >= p.ArriveTime);

                if (processes[i].CPUTime == TimeSpan.Zero)
                {
                    if (processes.Count > 1)
                        processes.Remove(processes[i]);
                    next = processes.FindAll(p => timer >= p.ArriveTime).MinBy(p => p.Priority);
                    while (timer <= completionTime && next is null)
                    {
                        timer += ms;
                        next = processes.FindAll(p => timer >= p.ArriveTime).MinBy(p => p.Priority);
                    }
                }

                if (next is not null)
                {
                    final.Add(current);

                    i = processes.IndexOf(next);
                    current = new Process { PID = processes[i].PID, ArriveTime = timer, CPUTime = TimeSpan.Zero };
                }
            }

            CalculatePreAverageTime(final);

            return final;
        }

        /// <summary>
        /// Schedule processes based on the Non-Preemptive Priority algorithm and calculates the average waiting and turn around time
        /// </summary>
        /// <returns>Processes scheduled based on the Non-Preemptive Priority algorithm</returns>
        private static Processes PriorityNonPreSchedule()
        {
            Processes processes = new Processes();
            Processes final = new Processes();
            // do the priority sort and calculate properties
            processes.AddRange(((Processes)Processes.Clone()).OrderBy(p => p.ArriveTime).ThenBy(p => p.Priority));

            var completionTime = FindCompletionTime(processes, processes.Count - 1);
            var timer = processes[0].ArriveTime;
            Process current = new Process { PID = processes[0].PID, ArriveTime = timer, CPUTime = TimeSpan.Zero };
            int i = 0;

            while (timer <= completionTime)
            {
                var ms = TimeSpan.FromMilliseconds(1);

                current.CPUTime += ms;
                processes[i].CPUTime -= ms;
                timer += ms;

                if (processes[i].CPUTime > TimeSpan.Zero)
                    continue;

                processes.Remove(processes[i]);


                if (processes.Count == 0)
                {
                    final.Add(current);
                    break;
                }
                var next = processes.FindAll(p => timer >= p.ArriveTime).MinBy(p => p.Priority);
                while (timer <= completionTime && next is null)
                {
                    timer += ms;
                    next = processes.FindAll(p => timer >= p.ArriveTime).MinBy(p => p.Priority);
                }


                if (next != null)
                {
                    final.Add(current);

                    i = processes.IndexOf(next);
                    current = new Process { PID = processes[i].PID, ArriveTime = processes[i].ArriveTime, CPUTime = TimeSpan.Zero };
                }
            }


            CalculateNonPreAverageTime(final);

            return final;


        }
        private static Processes RoundRobinSchedule()
        {
            Processes processes = new Processes();
            Processes final = new Processes();
            processes.AddRange(((Processes)Processes.Clone()).OrderBy(p => p.ArriveTime));

            var readyQueue = new Queue<Process>();
            var quantum = TimeQuantum;
            var completionTime = FindCompletionTime(processes, processes.Count - 1);
            var timer = processes[0].ArriveTime;

            Process current = new Process { PID = processes[0].PID, ArriveTime = timer, CPUTime = TimeSpan.Zero };

            var next = processes.FindAll(p => timer == p.ArriveTime);
            foreach (var process in next)
            {
                readyQueue.Enqueue(process);
            }

            Process instance = readyQueue.Dequeue();


            while (timer <= completionTime)
            {

                var ms = TimeSpan.FromMilliseconds(1);

                quantum -= ms;

                current.CPUTime += ms;
                instance.CPUTime -= ms;
                timer += ms;

                next = processes.FindAll(p => timer == p.ArriveTime);
                foreach (var process in next)
                {
                    readyQueue.Enqueue(process);
                }

                if (quantum == TimeSpan.Zero)
                {
                    if (instance.CPUTime != TimeSpan.Zero)
                        readyQueue.Enqueue(instance);
                    else
                        processes.Remove(instance);

                    final.Add(current);

                    while (timer <= completionTime && readyQueue.Count == 0 && processes.Count > 0)
                    {
                        timer += ms;
                        next = processes.FindAll(p => timer == p.ArriveTime);
                        foreach (var process in next)
                            readyQueue.Enqueue(process);
                    }
                    if (timer >= completionTime)
                        break;

                    instance = readyQueue.Dequeue();

                    current = new Process { PID = instance.PID, ArriveTime = timer, CPUTime = TimeSpan.Zero };
                }

                if (instance.CPUTime == TimeSpan.Zero)
                {
                    processes.Remove(instance);
                    final.Add(current);


                    while(timer <= completionTime && readyQueue.Count == 0 && processes.Count > 0)
                    {
                        timer += ms;
                        next = processes.FindAll(p => timer == p.ArriveTime);
                        foreach(var process in next)
                            readyQueue.Enqueue(process);
                    }
                    if (timer >= completionTime)
                        break;

                    instance = readyQueue.Dequeue();
                    quantum = TimeQuantum;
                    current = new Process { PID = instance.PID, ArriveTime = timer, CPUTime = TimeSpan.Zero };

                }

                if (quantum == TimeSpan.Zero)
                {
                    quantum = TimeQuantum;
                }

            }
            // Calculating Turn around time and waiting time
            CalculatePreAverageTime(final);

            return final;
        }

        /// <summary>
        /// Finds the completion time of a processes from a list of processes
        /// </summary>
        /// <param name="processes">The list of processes</param>
        /// <param name="index">The index of the processes in the list</param>
        /// <returns>The completion time of a processes</returns>
        public static TimeSpan FindCompletionTime(Processes processes, int index)
        {
            TimeSpan completionTime = processes[0].ArriveTime;
            int i = 0;
            while (i <= index)
            {
                var arrive = processes[i].ArriveTime;
                if (arrive > completionTime)
                    completionTime += arrive - completionTime;
                else
                {
                    completionTime += processes[i].CPUTime;
                    i++;
                }
            }
            return completionTime;
        }

        /// <summary>
        /// Calculates the average waiting and turn around time for non-preemtive algorithms
        /// </summary>
        /// <param name="processes">the scheduled processes</param>
        private static void CalculateNonPreAverageTime(Processes processes)
        {
            var avgTrTime = TimeSpan.Zero;
            var avgWtTime = TimeSpan.Zero;
            for (int i = 0; i < processes.Count; i++)
            {
                var ct = FindCompletionTime(processes, i);
                var arrival = processes[i].ArriveTime;
                var cpuTime = processes[i].CPUTime;

                avgTrTime += ct - arrival;
                avgWtTime += ct - arrival - cpuTime;
            }

            avgTrTime /= processes.Count;
            avgWtTime /= processes.Count;

            processes.AverageTurnAroundTime = avgTrTime;
            processes.AverageWaitingTime = avgWtTime;
        }

        /// <summary>
        /// Calculates the average waiting and turn around time for preemtive algorithms
        /// </summary>
        /// <param name="processes">the scheduled processes</param>
        private static void CalculatePreAverageTime(Processes processes)
        {
            // Calculating Turn around time and waiting time
            var avgTrTime = TimeSpan.Zero;
            var avgWtTime = TimeSpan.Zero;
            for (int j = 0; j < Processes.Count; j++)
            {
                var lastExecution = processes.FindLast(p => p.PID == Processes[j].PID);

                var ct = lastExecution.ArriveTime + lastExecution.CPUTime;
                var arrival = Processes[j].ArriveTime;
                var cpuTime = Processes[j].CPUTime;

                avgTrTime += ct - arrival;
                avgWtTime += ct - arrival - cpuTime;
            }

            avgTrTime /= Processes.Count;
            avgWtTime /= Processes.Count;

            processes.AverageTurnAroundTime = avgTrTime;
            processes.AverageWaitingTime = avgWtTime;
        }
    }
}
