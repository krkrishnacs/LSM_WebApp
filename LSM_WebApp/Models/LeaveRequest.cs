using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LSM_WebApp.Models
{
    public class LeaveRequest
    {
        public int RequestID { get; set; }
        public int UserID { get; set; }
        public string LeaveType { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Reason { get; set; }
        public string Status { get; set; }
        public string ManagerComment { get; set; }
    }
}