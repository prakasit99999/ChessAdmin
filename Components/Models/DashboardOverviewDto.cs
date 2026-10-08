namespace ChessAdmin.Components.Models
{
        public class DashboardOverviewDto
        {
            public int TotalUsers { get; set; }
            public int TotalGames { get; set; }
            public int AvgMoveTimeMs { get; set; }
            public double PruningEfficiency { get; set; }
            public int SystemIssues { get; set; }
            public List<RecentUserDto> RecentUsers { get; set; } = new();
        }

        public class RecentUserDto
        {
            public string Username { get; set; } = string.Empty;
            public string Email { get; set; } = string.Empty;
            public string Role { get; set; } = "Player";
            public int GamesPlayed { get; set; }
            public string Status { get; set; } = "offline"; // สอดคล้องกับ enum('offline','online','playing') ใน DB
            public bool IsActive => Status != "offline";
        }
}

