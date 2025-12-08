using System;
using System.Collections.Generic;
using System.ServiceModel;
using DamasChinas_Server.Common;
using DamasChinas_Server.Contracts;
using DamasChinas_Server.Dtos;
using DamasChinas_Server.GameRepositories;
using DamasChinas_Server.Interfaces;
using DamasChinas_Server.Logic;

namespace DamasChinas_Server.Services
{
    [ServiceBehavior(
        InstanceContextMode = InstanceContextMode.PerSession,
        ConcurrencyMode = ConcurrencyMode.Multiple)]
    public sealed class MatchService : IMatchService
    {
        private readonly RepositoryMatches _matchRepo;
        private readonly RepositoryReports _reportRepo;
        private readonly RepositorySanctions _sanctionRepo;
        private readonly RepositoryUsers _userRepo;

        // Umbrales de reportes para sanciones en BD
        private const int ReportsFirstBan = 3;
        private const int ReportsSecondBan = 6;
        private const int ReportsPermanentBan = 10;

        public MatchService()
            : this(
                  new RepositoryMatches(),
                  new RepositoryReports(),
                  new RepositorySanctions(),
                  new RepositoryUsers())
        {
        }

        internal MatchService(
            RepositoryMatches matchRepo,
            RepositoryReports reportRepo,
            RepositorySanctions sanctionRepo,
            RepositoryUsers userRepo)
        {
            _matchRepo = matchRepo ?? throw new ArgumentNullException(nameof(matchRepo));
            _reportRepo = reportRepo ?? throw new ArgumentNullException(nameof(reportRepo));
            _sanctionRepo = sanctionRepo ?? throw new ArgumentNullException(nameof(sanctionRepo));
            _userRepo = userRepo ?? throw new ArgumentNullException(nameof(userRepo));
        }

        public OperationResult FinishMatch(FinishMatchRequest request)
        {
            try
            {
                if (request == null ||
                    request.FinalPositions == null ||
                    request.FinalPositions.Count == 0)
                {
                    return OperationResult.Fail("Invalid match data.", MessageCode.UnknownError);
                }

                // ============================================
                // 1) Crear partida
                // ============================================
                int matchId = _matchRepo.CreateMatch();

                // ============================================
                // 2) Guardar posiciones finales
                // (aunque ranking use solo ganadas/perdidas,
                //  nos quedamos con las posiciones para estadísticas)
                // ============================================
                foreach (KeyValuePair<string, int> pair in request.FinalPositions)
                {
                    string username = pair.Key;
                    int finalPos = pair.Value;

                    int userId = _userRepo.GetUserIdByUsername(username);
                    _matchRepo.AddPlayerResult(matchId, userId, finalPos);
                }

                // ============================================
                // 3) Registrar reportes y generar sanciones
                // ============================================
                HashSet<string> reportedUsernames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

                if (request.Reports != null && request.Reports.Count > 0)
                {
                    foreach (ReportPlayerRequest report in request.Reports)
                    {
                        if (string.IsNullOrWhiteSpace(report.ReporterUsername) ||
                            string.IsNullOrWhiteSpace(report.ReportedUsername))
                        {
                            continue;
                        }

                        int reporterId = _userRepo.GetUserIdByUsername(report.ReporterUsername);
                        int reportedId = _userRepo.GetUserIdByUsername(report.ReportedUsername);

                        // 3.1 Guardar reporte en tabla Reportes
                        _reportRepo.AddReport(
                            reporterId,
                            reportedId,
                            matchId,
                            report.Reason ?? string.Empty);

                        reportedUsernames.Add(report.ReportedUsername);

                        // 3.2 Ver cuántos reportes acumula el usuario
                        int totalReports = _reportRepo.CountReportsForUser(reportedId);

                        // 3.3 Si ya tiene una sanción activa, no creamos otra
                        if (_sanctionRepo.HasActiveBan(reportedId))
                        {
                            continue;
                        }

                        if (totalReports >= ReportsPermanentBan)
                        {
                            _sanctionRepo.ApplyBan(
                                reportedId,
                                permanent: true,
                                untilUtc: null,
                                reason: "ban_permanent");
                        }
                        else if (totalReports >= ReportsSecondBan)
                        {
                            _sanctionRepo.ApplyBan(
                                reportedId,
                                permanent: false,
                                untilUtc: DateTime.UtcNow.AddHours(1),
                                reason: "ban_temp_1h");
                        }
                        else if (totalReports >= ReportsFirstBan)
                        {
                            _sanctionRepo.ApplyBan(
                                reportedId,
                                permanent: false,
                                untilUtc: DateTime.UtcNow.AddMinutes(10),
                                reason: "ban_temp_10m");
                        }

                    }

                    // 3.5 Sincronizar con LobbyManager (bans en memoria)
                    if (reportedUsernames.Count > 0 && request.LobbyCode > 0)
                    {
                        LobbyManager.Instance.ApplyReportsOnMatchEnd(
                            request.LobbyCode,
                            reportedUsernames);
                    }
                }

                // ============================================
                // 4) Notificar fin de partida al cliente (callback)
                // ============================================
                MatchResultDto resultDto = new MatchResultDto
                {
                    MatchId = matchId,
                    FinalPositions = new Dictionary<string, int>(request.FinalPositions),
                    BansApplied = new System.Collections.Generic.List<BanInfoDto>()
                    // Más adelante podemos rellenar BansApplied
                    // con info de sanciones si quieres.
                };

                try
                {
                    IMatchCallback callback =
                        OperationContext.Current.GetCallbackChannel<IMatchCallback>();

                    if (callback != null)
                    {
                        callback.OnMatchFinished(resultDto);
                    }
                }
                catch
                {
                    // Cliente desconectado → ignoramos el error del callback
                }

                // Por ahora el OperationResult solo indica que el flujo
                // terminó bien; el detalle viene por el callback.
                return OperationResult.Ok();
            }
            catch (RepositoryValidationException ex)
            {
                return OperationResult.Fail(ex.Message, ex.Code);
            }
            catch (Exception ex)
            {
                return OperationResult.Fail(ex.Message, MessageCode.UnknownError);
            }
        }
    }
}
