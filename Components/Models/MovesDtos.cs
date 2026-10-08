namespace ChessAdmin.Components.Models
{
    public class MoveDetailDto
    {
        public int MoveId { get; set; }
        public int GameId { get; set; }
        public int MoveNumber { get; set; }

        public int StartX { get; set; }
        public int StartY { get; set; }
        public int EndX { get; set; }
        public int EndY { get; set; }

        public string FromPosition { get; set; } = string.Empty; // e.g. "e2"
        public string ToPosition { get; set; } = string.Empty;   // e.g. "e4"
        public string MoveText { get; set; } = string.Empty;     // e.g. "e2 -> e4"
        public string San { get; set; } = string.Empty;          // e.g. "e4", "Nf3"

        public string PieceType { get; set; } = string.Empty;
        public string Team { get; set; } = string.Empty;         // "white", "black"

        public bool IsCapture { get; set; }
        public string? CapturedPieceType { get; set; }
        public string? CapturedPieceTeam { get; set; }
        public int? CapturedX { get; set; }
        public int? CapturedY { get; set; }

        public bool IsCastling { get; set; }
        public bool IsEnPassant { get; set; }
        public bool IsCheck { get; set; }
        public bool IsPawnTwoStep { get; set; }
        public bool PieceHasMovedBefore { get; set; }

        public string? PromotedFrom { get; set; }
        public string? PromotedTo { get; set; }

        public string? AlgorithmType { get; set; }
        public int? AiEvaluationScore { get; set; }
        public int? AiDepthSearched { get; set; }
        public int? AiNodesEvaluated { get; set; }
        public int? MoveTimeMs { get; set; }

        public DateTime? CreatedAt { get; set; }
    }
}
