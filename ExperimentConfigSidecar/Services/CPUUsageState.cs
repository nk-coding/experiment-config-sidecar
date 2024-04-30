using System.Diagnostics;

namespace ExperimentConfigSidecar.Services
{
    public class CPUUsageState {
        private volatile int usageDuration;
        
        private volatile int pauseDuration;

        private volatile bool isTerminated = false;

        public int UsageDuration{
            get => usageDuration;
            set => usageDuration = value;
        }

        public int PauseDuration
        {
            get => pauseDuration;
            set => pauseDuration = value;
        }

        public bool IsTerminated
        {
            get => isTerminated;
            set => isTerminated = value;
        }

        public CPUUsageState(int usageDuration, int pauseDuration)
        {
            UsageDuration = usageDuration;
            PauseDuration = pauseDuration;
        }

        public void Execute()
        {
            var stopWatch = new Stopwatch();
            while (!IsTerminated)
            {
                var usageDuration = UsageDuration;
                stopWatch.Restart();
                while (stopWatch.ElapsedMilliseconds < usageDuration) {
                    // spin
                }
                Thread.Sleep(PauseDuration);
            }
        }
    };

}