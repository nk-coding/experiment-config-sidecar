using ExperimentConfigSidecar.Models;

namespace ExperimentConfigSidecar.Services
{

    public class CPUUsageService
    {
        private readonly List<CPUUsageState> cpuUsageStates = [];

        private readonly object mutex = new();

        public void UpdateCPUUsage(List<CPUUsage> cpuUsages)
        {
            lock(mutex)
            {
                for (int i = cpuUsages.Count; i < cpuUsageStates.Count; i++)
                {
                    cpuUsageStates[i].IsTerminated = true;
                }
                if (cpuUsageStates.Count > cpuUsages.Count)
                {
                    cpuUsageStates.RemoveRange(cpuUsages.Count, cpuUsageStates.Count - cpuUsages.Count);
                }
                for (int i = 0; i < cpuUsageStates.Count; i++)
                {
                    cpuUsageStates[i].PauseDuration = cpuUsages[i].PauseDuration;
                    cpuUsageStates[i].UsageDuration = cpuUsages[i].UsageDuration;
                }
                for (int i = cpuUsageStates.Count; i < cpuUsages.Count; i++)
                {
                    var usage = cpuUsages[i];
                    var newState = new CPUUsageState(usage.UsageDuration, usage.PauseDuration);
                    cpuUsageStates.Add(newState);
                    new Thread(new ThreadStart(newState.Execute)).Start();
                }
            }
        }
    }

}