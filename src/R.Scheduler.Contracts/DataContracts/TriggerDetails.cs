﻿using System;

namespace R.Scheduler.Contracts.DataContracts
{
    public class TriggerDetails
    {
        public string Name { get; set; }
        public string Group { get; set; }
        public string JobName { get; set; }
        public string JobGroup { get; set; }
        public string Description { get; set; }
        public DateTime StartTimeUtc { get; set; }
        public DateTime? NextFireTimeUtc { get; set; }
        public DateTime? PreviousFireTimeUtc { get; set; }
        public DateTime? EndTimeUtc { get; set; }
        public DateTime? FinalFireTimeUtc { get; set; }
    }
}