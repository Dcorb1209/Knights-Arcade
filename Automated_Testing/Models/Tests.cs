﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace Automated_Testing.Models
{
    public class Tests
    {
        [Required]
        public int? GameId { get; set; }
        public bool? TestOpens { get; set; }
        public bool? Test5min { get; set; }
        public bool? TestCloses { get; set; }
        public bool? TestRandombuttons { get; set; }
        public int? TestAttempts { get; set; }
        public string TestAverageRam { get; set; }
    }
}
