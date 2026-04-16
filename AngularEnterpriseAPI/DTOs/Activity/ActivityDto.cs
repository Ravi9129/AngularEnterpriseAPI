using System;

namespace AngularEnterpriseAPI.DTOs.Activity
{
    public class ActivityDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public string Icon { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
    }
}
