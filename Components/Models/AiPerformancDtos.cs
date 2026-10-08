using System.Text.Json.Serialization;

namespace ChessAdmin.Components.Models
{
    public class AiPerformanceDto
    {
        [JsonPropertyName("algorithmType")]
        public string? AlgorithmType { get; set; }
        [JsonPropertyName("evaluationScore")]
        public float? EvaluationScore { get; set; }
        [JsonPropertyName("aiDepthSearched")]
        public int? AiDepthSearched { get; set; }
        [JsonPropertyName("aiNodesEvaluated")]
        public int? AiNodesEvaluated { get; set; }
        [JsonPropertyName("aiMoveTimeMs")]
        public int? AiMoveTimeMs { get; set; }
    }

    public class AiPerformanceResponseDto
    {
        public int SelectedDepth { get; set; } = 4;
        public AiMetricSummaryDto CurrentMetrics { get; set; } = new();
        public List<AiBenchmarkDepthDto> BenchmarkHistory { get; set; } = new();
    }

    public class  AiBenchmarkDepthDto
    {
        public int Depth { get; set; }
        public int MinimaxNodes { get; set; }
        public int AlphaBetaNodes { get; set; }
        public int MinimaxTimeMs { get; set; }
        public int AlphaBetaTimeMs { get; set; }
        public long PrunedNodes => Math.Max(0, MinimaxNodes - AlphaBetaNodes);
    }

    public class AiMetricSummaryDto
    {
        public long MinimaxNodes { get; set; }
        public long AlphaBetaNodes { get; set; }
        public int MinimaxTimeMs { get; set; }
        public int AlphaBetaTimeMs { get; set; }
        public double NodeReductionRate { get; set; }
        public string SpeedupFactor { get; set; } = "1.0x";
        public double PruningEfficiency { get; set; }

    }

}
