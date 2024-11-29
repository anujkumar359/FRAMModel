namespace EasyJet.FRAMModel.SleepWake.Helpers
{

    /// <summary>
    /// Represents a sleep episode.
    /// </summary>
    internal class SleepEpisode
    {
        /// <summary>
        /// Gets or sets the lower limit of the sleep episode.
        /// </summary>
        public double Lower { get; set; }

        /// <summary>
        /// Gets or sets the upper limit of the sleep episode.
        /// </summary>
        public double Upper { get; set; }

        /// <summary>
        /// Gets or sets the phase of the sleep episode.
        /// </summary>
        public double Phase { get; set; }

        /// <summary>
        /// Gets or sets the start time of the sleep episode.
        /// </summary>
        public int SleepStart { get; set; }

        /// <summary>
        /// Gets or sets the end time of the sleep episode.
        /// </summary>
        public int SleepEnd { get; set; }

        /// <summary>
        /// Gets or sets the homeostatic value of the sleep episode.
        /// </summary>
        public double Homeostatic { get; set; }

        /// <summary>
        /// Gets or sets the alertness value of the sleep episode.
        /// </summary>
        public double Alertness { get; set; }

        /// <summary>
        /// Gets or sets the circadian values of the sleep episode.
        /// </summary>
        public double[] Circadians { get; set; }

        /// <summary>
        /// Gets or sets the homeostatic values of the sleep episode.
        /// </summary>
        public double[] Homeostatics { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="SleepEpisode"/> class.
        /// </summary>
        public SleepEpisode()
        {
            Lower = 0;
            Upper = 0;
            Phase = 0;
            SleepStart = 0;
            SleepEnd = 0;
            Homeostatic = 0;
            Alertness = 0;
            Circadians = new double[0];
            Homeostatics = new double[0];
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SleepEpisode"/> class with the specified parameters.
        /// </summary>
        /// <param name="lower">The lower limit of the sleep episode.</param>
        /// <param name="upper">The upper limit of the sleep episode.</param>
        /// <param name="phase">The phase of the sleep episode.</param>
        /// <param name="sleepStart">The start time of the sleep episode.</param>
        /// <param name="sleepEnd">The end time of the sleep episode.</param>
        /// <param name="homeostatic">The homeostatic value of the sleep episode.</param>
        /// <param name="alertness">The alertness value of the sleep episode.</param>
        /// <param name="circadians">The circadian values of the sleep episode.</param>
        /// <param name="homeostatics">The homeostatic values of the sleep episode.</param>
        public SleepEpisode(double lower, double upper, double phase, int sleepStart, int sleepEnd, double homeostatic, double alertness, double[] circadians, double[] homeostatics)
        {
            Lower = lower;
            Upper = upper;
            Phase = phase;
            SleepStart = sleepStart;
            SleepEnd = sleepEnd;
            Homeostatic = homeostatic;
            Alertness = alertness;
            Circadians = circadians;
            Homeostatics = homeostatics;
        }
    }
}
