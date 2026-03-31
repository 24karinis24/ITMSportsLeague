using Microsoft.Extensions.Logging;
using SportsLeague.Domain.Entities;
using SportsLeague.Domain.Interfaces.Repositories;
using SportsLeague.Domain.Interfaces.Services;
using System.Text.RegularExpressions;

namespace SportsLeague.Domain.Services;

public class SponsorService : ISponsorService
{
    private readonly ISponsorRepository _sponsorRepository;
    private readonly ITournamentRepository _tournamentRepository;
    private readonly ITournamentSponsorRepository _tournamentSponsorRepository;
    private readonly ILogger<SponsorService> _logger;

    public SponsorService(
        ISponsorRepository SponsorRepository,
        ITournamentRepository TournamentRepository,
        ITournamentSponsorRepository tournamentSponsorRepository,
        ILogger<SponsorService> logger)
    {
        _sponsorRepository = SponsorRepository;
        _tournamentRepository = TournamentRepository;
        _tournamentSponsorRepository = tournamentSponsorRepository;
        _logger = logger;
    }

    public async Task<IEnumerable<Sponsor>> GetAllAsync()
    {
        _logger.LogInformation("Retrieving all Sponsors");
        return await _sponsorRepository.GetAllAsync();
    }

    public async Task<Sponsor?> GetByIdAsync(int id)
    {
        _logger.LogInformation("Retrieving Sponsor with ID: {SponsorId}", id);
        var sponsor = await _sponsorRepository.GetByIdAsync(id);

        if (sponsor == null)
            _logger.LogWarning("Sponsor with ID {SponsorId} not found", id);

        return sponsor;
    }

    public async Task<Sponsor> CreateAsync(Sponsor sponsor)
    {
        // Validación de negocio: nombre único
        var exists = await _sponsorRepository.ExistsByNameAsync(sponsor.Name);
        if (exists)
        {
            _logger.LogWarning("Sponsor with name '{SponsorName}' already exists", sponsor.Name);
            throw new InvalidOperationException(
                $"Ya existe un patrocinador con el nombre '{sponsor.Name}'");
        }

        // Validación de formato de email
        if (!IsValidEmail(sponsor.ContactEmail))
            throw new InvalidOperationException(
                "El formato del email de contacto no es válido");

        _logger.LogInformation("Creating Sponsor: {SponsorName}", sponsor.Name);
        return await _sponsorRepository.CreateAsync(sponsor);
    }

    public async Task UpdateAsync(int id, Sponsor sponsor)
    {
        var existingSponsor = await _sponsorRepository.GetByIdAsync(id);
        if (existingSponsor == null)
        {
            _logger.LogWarning("Sponsor with ID {SponsorId} not found for update", id);
            throw new KeyNotFoundException(
                $"No se encontró el patrocinador con ID {id}");
        }

        // Validar nombre único (si cambió)
        if (existingSponsor.Name != sponsor.Name)
        {
            var exists = await _sponsorRepository.ExistsByNameAsync(sponsor.Name);
            if (exists)
            {
                throw new InvalidOperationException(
                    $"Ya existe un patrocinador con el nombre '{sponsor.Name}'");
            }
        }

        // Validación de formato de email
        if (!IsValidEmail(sponsor.ContactEmail))
            throw new InvalidOperationException(
                "El formato del email de contacto no es válido");

        existingSponsor.Name = sponsor.Name;
        existingSponsor.ContactEmail = sponsor.ContactEmail;
        existingSponsor.Phone = sponsor.Phone;
        existingSponsor.WebsiteUrl = sponsor.WebsiteUrl;
        existingSponsor.Category = sponsor.Category;

        _logger.LogInformation("Updating Sponsor with ID: {SponsorId}", id);
        await _sponsorRepository.UpdateAsync(existingSponsor);
    }

    public async Task DeleteAsync(int id)
    {
        var exists = await _sponsorRepository.ExistsAsync(id);
        if (!exists)
        {
            _logger.LogWarning("Sponsor with ID {SponsorId} not found for deletion", id);
            throw new KeyNotFoundException(
                $"No se encontró el sponsor con ID {id}");
        }

        _logger.LogInformation("Deleting Sponsor with ID: {SponsorId}", id);
        await _sponsorRepository.DeleteAsync(id);
    }

    public async Task<IEnumerable<Tournament>> GetTournamentsBySponsorAsync(int id)
    {
        var sponsor = await _sponsorRepository.GetByIdAsync(id);
        if (sponsor == null)
            throw new KeyNotFoundException(
                $"No se encontró el patrocinador con ID {id}");

        var sponsorTournaments = await _tournamentSponsorRepository
            .GetBySponsorIdAsync(id);

        return sponsorTournaments.Select(tt => tt.Tournament);
    }

    public async Task<TournamentSponsor?> LinkSponsorAsync(TournamentSponsor tournamentSponsor)
    {
        // Validar que el torneo existe
        var tournament = await _tournamentRepository.GetByIdAsync(tournamentSponsor.TournamentId);
        if (tournament == null)
        {
            _logger.LogWarning("Tournament with ID {TournamentId} not found", tournamentSponsor.TournamentId);
            throw new KeyNotFoundException($"No se encontró el torneo con ID {tournamentSponsor.TournamentId}");
        }

        // Validar que el sponsor existe
        if (!await _sponsorRepository.ExistsAsync(tournamentSponsor.SponsorId))
        {
            _logger.LogWarning("Sponsor with ID {SponsorId} not found", tournamentSponsor.SponsorId);
            throw new KeyNotFoundException($"No se encontró el patrocinador con ID {tournamentSponsor.SponsorId}");
        }

        // Validar que no exista ya la vinculación
        var existing = await _tournamentSponsorRepository.GetByTournamentAndSponsorAsync(tournamentSponsor.TournamentId, tournamentSponsor.SponsorId);
        if (existing != null)
        {
            _logger.LogWarning("Sponsor {SponsorId} already linked to tournament {TournamentId}", tournamentSponsor.SponsorId, tournamentSponsor.TournamentId);
            throw new InvalidOperationException("Este patrocinador ya está vinculado a este torneo");
        }

        // Validar monto del contrato > 0
        if (tournamentSponsor.ContractAmount <= 0)
            throw new InvalidOperationException("El monto del contrato debe ser mayor a 0");

        // Asignar fecha si no viene
        if (tournamentSponsor.JoinedAt == default)
            tournamentSponsor.JoinedAt = DateTime.UtcNow;

        _logger.LogInformation("Linking sponsor {SponsorId} to tournament {TournamentId}", tournamentSponsor.SponsorId, tournamentSponsor.TournamentId);
        var created = await _tournamentSponsorRepository.CreateAsync(tournamentSponsor);

        // Cargar las propiedades de navegación
        return await _tournamentSponsorRepository.GetByIdWithDetailsAsync(created.Id);
    }

    public async Task UnlinkSponsorAsync(int sponsorId, int tournamentId)
    {
        var relation = await _tournamentSponsorRepository.GetByTournamentAndSponsorAsync(tournamentId, sponsorId);
        if (relation == null)
        {
            _logger.LogWarning("Link between sponsor {SponsorId} and tournament {TournamentId} not found", sponsorId, tournamentId);
            throw new KeyNotFoundException("No se encontró la vinculación entre el patrocinador y el torneo");
        }

        _logger.LogInformation("Unlinking sponsor {SponsorId} from tournament {TournamentId}", sponsorId, tournamentId);
        await _tournamentSponsorRepository.DeleteAsync(relation.Id);
    }

    private static bool IsValidEmail(string email) =>
        Regex.IsMatch(email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$");
}