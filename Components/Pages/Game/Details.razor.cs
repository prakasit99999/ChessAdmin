using Azure;
using ChessAdmin.Components.Models;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Rudzoft.ChessLib;
using Rudzoft.ChessLib.Factories;
using Rudzoft.ChessLib.Fen;
using Rudzoft.ChessLib.MoveGeneration;
using Rudzoft.ChessLib.Types;

using ChessFile = Rudzoft.ChessLib.Types.File;
using ChessRank = Rudzoft.ChessLib.Types.Rank;

namespace ChessAdmin.Components.Pages.Game
{
    public partial class Details : ComponentBase, IDisposable
    {
        [Parameter] public int GameId { get; set; }

        [Inject] protected HttpClient Http { get; set; } = default!;

        // Model สำหรับเก็บข้อมูลเกมทั้งหมด
        protected GameDetails? gameData;
        protected bool isLoading = true;
        protected string? errorMessage;

        // Replay Engine State
        protected int currentMoveIndex = 0;
        protected string currentFen = Fen.StartPositionFen.ToString();
        protected List<string> fenHistory = new();
        protected bool isPlaying = false;
        private int loadedGameId = 0;
        private PeriodicTimer? playTimer;
        private CancellationTokenSource? cts;
        private CancellationTokenSource? loadCts;

        protected override async Task OnParametersSetAsync()
        {
            if (GameId > 0 && GameId != loadedGameId)
            {
                loadedGameId = GameId;
                await LoadGameDataAsync();
            }
        }

        /// เรียก API เพียงครั้งเดียวเพื่อดึง GameDetails + Moves
        private async Task LoadGameDataAsync()
        {
            isLoading = true;
            errorMessage = null;

            if (GameId <= 0)
            {
                errorMessage = "GameId ไม่ถูกต้อง";
                isLoading = false;
                return;
            }

            // ยกเลิก Request เก่าหากมี Parameter เปลี่ยนแปลง
            loadCts?.Cancel();
            loadCts?.Dispose();
            loadCts = new CancellationTokenSource();

            try
            {
                Console.WriteLine($"[DEBUG] Starting request for GameId: {GameId}");

                var requestUrl = $"/api/Game/admin/{GameId}";

                // ส่ง CancellationToken เข้าไปร่วมกับ GetAsync
                var response = await Http.GetAsync(requestUrl, loadCts.Token);
                //Console.WriteLine($"[DEBUG] HTTP Status Code: {response.StatusCode}");

                if (response.IsSuccessStatusCode)
                {
                    var rawJson = await response.Content.ReadAsStringAsync(loadCts.Token);

                    gameData = System.Text.Json.JsonSerializer.Deserialize<GameDetails>(rawJson, new System.Text.Json.JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    //Console.WriteLine($"[DEBUG] Parse Success! Moves Count: {gameData?.Moves?.Count ?? 0}");

                    if (gameData != null)
                    {
                        GenerateFenHistory();
                        GoToMove(0);
                    }
                    else
                    {
                        errorMessage = "แปลงข้อมูล JSON เป็น GameDetails ไม่สำเร็จ (gameData is null)";
                    }
                }
                else
                {
                    errorMessage = $"API ตอบกลับด้วย Status: {response.StatusCode}";
                }
            }
            catch (OperationCanceledException)
            {
                Console.WriteLine("[DEBUG] Request was canceled.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR EXCEPTION] {ex}");
                errorMessage = $"เกิดข้อผิดพลาด: {ex.Message}";
            }
            finally
            {
                isLoading = false;
                await InvokeAsync(StateHasChanged);
            }
        }

        /// ใช้ Rudzoft.ChessLib คำนวณ FEN position สำหรับทุกๆ ตาเดิน
        private void GenerateFenHistory()
        {
            fenHistory.Clear();

            try
            {
                IGame chessGame = GameFactory.Create();
                chessGame.NewGame(Fen.StartPositionFen);

                // Move 0: ตำแหน่งเริ่มต้น
                string startFen = chessGame.Pos.GenerateFen().ToString();
                fenHistory.Add(startFen);

                if (gameData?.Moves == null || !gameData.Moves.Any()) return;

                foreach (var move in gameData.Moves.OrderBy(m => m.MoveNumber))
                {
                    try
                    {
                        Square fromSq = new Square((ChessRank)move.StartY, (ChessFile)move.StartX);
                        Square toSq = new Square((ChessRank)move.EndY, (ChessFile)move.EndX);

                        // 1. ดึงรายการ ตาเดินที่ถูกกติกา (Legal Moves) ทั้งหมดในตำแหน่งกระดานปัจจุบัน
                        var legalMoves = chessGame.Pos.GenerateMoves();
                        Move matchedMove = Move.EmptyMove;
                        foreach (var legalMove in legalMoves)
                        {
                            var m = legalMove.Move;

                            // 1.1 เปรียบเทียบตำแหน่ง จาก->ไป ตรงๆ
                            if (m.FromSquare() == fromSq && m.ToSquare() == toSq)
                            {
                                matchedMove = m;
                                break;
                            }

                            // 1.2 ดักจับกรณีการเข้าป้อม (Castling)
                            // เช็กว่าตัวหมากในช่องเริ่มต้นเป็น King และมีการขยับข้ามคอลัมน์เกิน 1 ช่อง
                            var movingPiece = chessGame.Pos.GetPiece(fromSq);
                            bool isKingMoving = movingPiece.Type() == PieceTypes.King;
                            int fileDistance = Math.Abs(toSq.File.AsInt() - fromSq.File.AsInt());

                            if (isKingMoving && fileDistance > 1 && m.FromSquare() == fromSq)
                            {
                                int toFile = toSq.File.AsInt();
                                int mToFile = m.ToSquare().File.AsInt();

                                // Kingside Castling (ฝั่งคิง: คอลัมน์ g=6 หรือ h=7)
                                bool isKingside = (toFile == 6 || toFile == 7) && (mToFile == 6 || mToFile == 7);

                                // Queenside Castling (ฝั่งควีน: คอลัมน์ c=2 หรือ a=0)
                                bool isQueenside = (toFile == 2 || toFile == 0) && (mToFile == 2 || mToFile == 0);

                                if (isKingside || isQueenside)
                                {
                                    matchedMove = m;
                                    break;
                                }
                            }
                        }

                        // 2. หากเจอ Legal Move ให้ MakeMove ด้วย State ใหม่
                        if (!matchedMove.IsNullMove())
                        {
                            State moveState = new State();
                            chessGame.Pos.MakeMove(matchedMove, moveState);

                            string newFen = chessGame.Pos.GenerateFen().ToString();
                            fenHistory.Add(newFen);
                            //Console.WriteLine($"[DEBUG] Move #{move.MoveNumber} ({fromSq}->{toSq}) -> FEN: {newFen}");
                        }
                        else
                        {
                            // Fallback กรณีหา Legal Move ไม่เจอจริงๆ
                            Move directMove = new Move(fromSq, toSq);
                            State moveState = new State();
                            chessGame.Pos.MakeMove(directMove, moveState);

                            string newFen = chessGame.Pos.GenerateFen().ToString();
                            fenHistory.Add(newFen);
                            //Console.WriteLine($"[DEBUG DirectMove] Move #{move.MoveNumber} -> FEN: {newFen}");
                        }
                    }
                    catch (Exception ex)
                    {
                        //Console.WriteLine($"[ERROR] Failed at Move #{move.MoveNumber}: {ex.Message}");
                        fenHistory.Add(fenHistory.LastOrDefault() ?? Fen.StartPositionFen.ToString());
                    }
                }

                //Console.WriteLine($"[DEBUG] Total FEN History Count: {fenHistory.Count}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR EXCEPTION GenerateFenHistory] {ex}");
            }
        }

        private string GetSquareNotation(int x, int y, string? rawPos)
        {
            if (!string.IsNullOrWhiteSpace(rawPos))
                return rawPos.Trim().ToLower();

            char file = (char)('a' + Math.Clamp(x, 0, 7));
            char rank = (char)('1' + Math.Clamp(y, 0, 7));
            return $"{file}{rank}";
        }

        // --- Controls สำหรับ Replay กระดาน ---

        protected void GoToMove(int moveIndex)
        {
            if (fenHistory == null || !fenHistory.Any()) return;

            currentMoveIndex = Math.Clamp(moveIndex, 0, fenHistory.Count - 1);
            currentFen = fenHistory[currentMoveIndex];

            Console.WriteLine($"[DEBUG Replay] MoveIndex: {currentMoveIndex}/{fenHistory.Count - 1}, FEN: {currentFen}");

            StateHasChanged();
        }

        protected void FirstMove() => GoToMove(0);
        protected void PrevMove() => GoToMove(currentMoveIndex - 1);
        protected void NextMove() => GoToMove(currentMoveIndex + 1);
        protected void LastMove() => GoToMove(fenHistory.Count - 1);

        protected void TogglePlay()
        {
            if (isPlaying)
            {
                StopAutoPlay();
            }
            else
            {
                _ = StartAutoPlayAsync();
            }
        }

        private async Task StartAutoPlayAsync()
        {
            if (fenHistory.Count <= 1) return;

            if (currentMoveIndex >= fenHistory.Count - 1)
            {
                GoToMove(0);
            }

            isPlaying = true;
            cts = new CancellationTokenSource();
            playTimer = new PeriodicTimer(TimeSpan.FromMilliseconds(1000));

            try
            {
                while (await playTimer.WaitForNextTickAsync(cts.Token))
                {
                    if (currentMoveIndex < fenHistory.Count - 1)
                    {
                        currentMoveIndex++;
                        currentFen = fenHistory[currentMoveIndex];
                        await InvokeAsync(StateHasChanged);
                    }
                    else
                    {
                        StopAutoPlay();
                        await InvokeAsync(StateHasChanged);
                        break;
                    }
                }
            }
            catch (OperationCanceledException)
            {
                // หยุดเล่นเมื่อมีการ Cancel
            }
        }

        private void StopAutoPlay()
        {
            isPlaying = false;
            cts?.Cancel();
            playTimer?.Dispose();
            playTimer = null;
        }

        public void Dispose()
        {
            StopAutoPlay();
            cts?.Dispose();
            loadCts?.Dispose();
        }
    }
}