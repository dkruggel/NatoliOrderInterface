﻿using System;
using System.Collections.Generic;

namespace NatoliOrderInterface.Models
{
    public partial class EoiNotificationsActive
    {
        public int Id { get; set; }
        public string Type { get; set; }
        public string Number { get; set; }
        public string Message { get; set; }
        public string User { get; set; }
        public DateTime Timestamp { get; set; }
    }
}
