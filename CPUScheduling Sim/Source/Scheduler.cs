﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
    internal class Scheduler
    {
        public static Processes Processes { get; set; } = new Processes();
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

        private static Processes FCFSSchedule()
        {
            Processes processes = new Processes();

            processes.AddRange(Processes.OrderBy(p => p.ArriveTime));

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

            return processes;
        }

        private static Processes SJFPreSchedule()
        {
            Processes processes = new Processes();
            // do the sjf preemtive sort and calculate properties
            return processes;
        }
        private static Processes SJFNonPreSchedule()
        {
            Processes processes = new Processes();
            // do the sjf non-preemtive sort and calculate properties
            return processes;
        }
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

            while(timer <= completionTime)
            {
                var ms = TimeSpan.FromMilliseconds(1);

                current.CPUTime += ms;
                processes[i].CPUTime -= ms;
                timer += ms;

                var next = processes.Find(p => p.Priority < processes[i].Priority && timer >= p.ArriveTime);

                if (processes[i].CPUTime == TimeSpan.Zero)
                {
                    if(processes.Count > 1)
                        processes.Remove(processes[i]);
                    next = processes.MinBy(p => p.Priority);
                }

                if (next != null)
                {
                    final.Add(current);

                    i = processes.IndexOf(next);
                    current = new Process { PID = processes[i].PID, ArriveTime = timer, CPUTime = TimeSpan.Zero };
                }
            }

            // Calculating Turn around time and waiting time
            var avgTrTime = TimeSpan.Zero;
            var avgWtTime = TimeSpan.Zero;
            for (int j = 0; j < Processes.Count; j++)
            {
                var lastExecution = final.FindLast(p => p.PID == Processes[j].PID);

                var ct = lastExecution.ArriveTime + lastExecution.CPUTime;
                var arrival = Processes[j].ArriveTime;
                var cpuTime = Processes[j].CPUTime;

                avgTrTime += ct - arrival;
                avgWtTime += ct - arrival - cpuTime;
            }

            avgTrTime /= Processes.Count;
            avgWtTime /= Processes.Count;

            final.AverageTurnAroundTime = avgTrTime;
            final.AverageWaitingTime = avgWtTime;

            return final;
        }
        private static Processes PriorityNonPreSchedule()
        {
            Processes processes = new Processes();
            // do the priority non preemptive sort and calculate properties
            return processes;
        }
        private static Processes RoundRobinSchedule()
        {
            Processes processes = new Processes();
            // do the roundrobin sort sort and calculate properties
            return processes;
        }

        public static TimeSpan FindCompletionTime(Processes processes, int index)
        {
            TimeSpan completionTime = processes[0].ArriveTime;
            int i = 0;
            while(i <= index)
            {
                var arrive = processes[i].ArriveTime;
                if(arrive > completionTime)
                    completionTime += arrive - completionTime;
                else
                {
                    completionTime += processes[i].CPUTime;
                    i++;
                }
            }
            return completionTime;
        }
        private static Process FindMinPriority(Processes processes, int except)
        {
            if (processes.Count == 0)
                return null;
            int minPriority = except == 0 ? 1 : 0;

            for (int i = 0; i < processes.Count; i++)
            {
                if(i != except && processes[i].Priority < processes[minPriority].Priority)
                    minPriority = i;
            }
            return processes[minPriority];
        }
    }
}
