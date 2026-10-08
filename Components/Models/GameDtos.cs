

using System.Text.Json.Serialization;

namespace ChessAdmin.Components.Models
{
    public class GameResultDto {
        public int GameId { get; set; }
        public string GameType { get; set; } = string.Empty;
        public string MatchMode { get; set; } = string.Empty;
        public string GameStatus { get; set; } = string.Empty;
        public string Result { get; set; } = string.Empty;


    }

    public class GamePagedRequest
    {
      public int PageNumber { get; set; }
      public int PageSize { get; set; }
      public int UserId { get; set; }
      public string? GameType { get; set; } = null;
      public string? MatchMode { get; set; } = null;
      public string? GameStatus { get; set; } = null;
      public string? Search { get; set; } = null;
    }

    public class GameListDto 
    {
        public int GameId { get; set; }
        public string GameType { get; set; } = string.Empty;
        public string MatchMode { get; set; } = string.Empty;
        public string GameStatus { get; set; } = string.Empty;
        public string Result { get; set; } = string.Empty;
        public string ResultReason { get; set; } = string.Empty;
        public int MoveCount { get; set; } = 0;

        // ผู้เล่นฝ่ายขาว (White Player)
        public int? WhitePlayerId { get; set; } = null;
        public string? WhitePlayerUsername { get; set; } = null;
        public int? WhitePlayerRating { get; set; } = null;
        public string WhitePlayerType { get; set; } = string.Empty;

        // ผู้เล่นฝ่ายดำ (Black Player)
        public int? BlackPlayerId { get; set; } = null;
        public string BlackPlayerUsername { get; set; } = null;
        public int? BlackPlayerRating { get; set; } = null;
        public string BlackPlayerType { get; set; } = string.Empty;

        // ผู้ชนะ
        public string? Winner { get; set; } = null;

        public DateTime? CreatedAt { get; set; } = null;
        public DateTime? StartedAt { get; set; } = null;
        public DateTime? FinishedAt { get; set; } = null;

    }

    public class PagedResult<T>
    {
        public List<T> Items { get; set; } = new();
        public int TotalCount { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
        public bool HasPreviousPage { get; set; }
        public bool HasNextPage { get; set; }
    }

    public class GameCountsDto
    {
        public int TotalGames { get; set; }
        public int ActiveMatches { get; set; }
        public int AiMatches { get; set; }
        public int RankedGames { get; set; }
        public int TotalCount { get; internal set; }
    }

    public class GameChartDataDto 
    {
        public string DateLabel { get; set; } = string.Empty;
        public string DateIso { get; set; } = string.Empty;
        public int AiGames { get; set; }
        public int MultiplayerGames { get; set; }
        public int TotalGames { get; set; }
    }

    public class GameChartRequest 
    {
        public int Days;
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }

    }

    public class GameDetails
    {
        public int GameId { get; set; }
        public string? GameType { get; set; }
        public string? MatchMode { get; set; }
        public string? GameStatus { get; set; }
        public string? Result { get; set; }
        public string? ResultReason { get; set; }

        // ประกาศเป็น Property เพื่อรองรับ JSON Serialization
        [JsonPropertyName("whitePlayer")]
        public PlayerDto? WhitePlayer { get; set; }

        [JsonPropertyName("blackPlayer")]
        public PlayerDto? BlackPlayer { get; set; }

        [JsonPropertyName("aiPerformance")]
        public AiPerformanceDto? AiPerformance { get; set; }

        [JsonPropertyName("moves")]
        public List<MoveDetailDto> Moves { get; set; } = new();
    }

    public class PlayerDto
    {
        [JsonPropertyName("userId")]
        public int UserId { get; set; }
        [JsonPropertyName("username")]
        public string? Username { get; set; }
        [JsonPropertyName("rating")]
        public int Rating { get; set; }
        [JsonPropertyName("playerType")]
        public string? PlayerType { get; set; }
    }


}

