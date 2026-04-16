namespace AngularEnterpriseAPI.DTOs.Dashboard
{
    public class UserStatsDto
    {
        public int TotalUsers { get; set; }
        public int ActiveUsers { get; set; }
        public int InactiveUsers { get; set; }
        public int AdminCount { get; set; }
        public int ManagerCount { get; set; }
        public int RegularUserCount { get; set; }
        public double UserGrowthPercentage { get; set; }
    }
}
