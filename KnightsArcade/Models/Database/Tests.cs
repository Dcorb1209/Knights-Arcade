﻿using System.ComponentModel.DataAnnotations;

namespace KnightsArcade.Models.Database
{
    public partial class Tests
    {
        [Required]
        public int? GameId { get; set; }
        public bool? TestOpens { get; set; }
        public bool? Test10min { get; set; }
		public string TestAverageRam { get; set; }
		public string TestPeakRam { get; set; }
		public bool? TestCloseOn3 { get; set; }
		public bool? TestCloseOnEscape { get; set; }
		public bool? TestCloses { get; set; }
        public int? TestAttempts { get; set; }
        public string TestFolderFileNames { get; set; }
        public int? TestNumExeFiles { get; set; }
    }
}
