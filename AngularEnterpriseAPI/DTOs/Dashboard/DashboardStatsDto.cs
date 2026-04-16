using System;
using System.Collections.Generic;

namespace AngularEnterpriseAPI.DTOs.Dashboard
{
    public class DashboardStatsDto
    {
        public int TotalUsers { get; set; }
        public int ActiveUsers { get; set; }
        public int NewUsersThisMonth { get; set; }
        public Dictionary<string, int> UsersByRole { get; set; } = new();
    }
}
