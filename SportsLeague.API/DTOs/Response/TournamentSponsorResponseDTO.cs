using SportsLeague.Domain.Enums;

namespace SportsLeague.API.DTOs.Response;

public class TournamentSponsorResponseDTO
{
    public int Id { get; set; }
    public int TournamentId { get; set; }
    public int SponsorId { get; set; }
    public decimal ContractAmount { get; set; }
    public DateTime JoinedAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
