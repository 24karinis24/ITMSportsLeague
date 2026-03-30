using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using SportsLeague.API.DTOs.Request;
using SportsLeague.API.DTOs.Response;
using SportsLeague.Domain.Entities;
using SportsLeague.Domain.Interfaces.Services;
using SportsLeague.Domain.Services;

namespace SportsLeague.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SponsorController : ControllerBase
{
    private readonly ISponsorService _sponsorService;
    private readonly IMapper _mapper;

    public SponsorController(
        ISponsorService sponsorService,
        IMapper mapper)
    {
        _sponsorService = sponsorService;
        _mapper = mapper;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<SponsorResponseDTO>>> GetAll()
    {
        var Sponsors = await _sponsorService.GetAllAsync();
        var SponsorsDto = _mapper.Map<IEnumerable<SponsorResponseDTO>>(Sponsors);
        return Ok(SponsorsDto);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<SponsorResponseDTO>> GetById(int id)
    {
        var Sponsor = await _sponsorService.GetByIdAsync(id);

        if (Sponsor == null)
            return NotFound(new { message = $"Equipo con ID {id} no encontrado" });

        var SponsorDto = _mapper.Map<SponsorResponseDTO>(Sponsor);
        return Ok(SponsorDto);
    }

    [HttpPost]
    public async Task<ActionResult<SponsorResponseDTO>> Create(SponsorRequestDTO dto)
    {
        try
        {
            var Sponsor = _mapper.Map<Sponsor>(dto);
            var createdSponsor = await _sponsorService.CreateAsync(Sponsor);
            var responseDto = _mapper.Map<SponsorResponseDTO>(createdSponsor);

            return CreatedAtAction(
                nameof(GetById),
                new { id = responseDto.Id },
                responseDto);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }

    [HttpPut("{id}")]
    public async Task<ActionResult> Update(int id, SponsorRequestDTO dto)
    {
        try
        {
            var Sponsor = _mapper.Map<Sponsor>(dto);
            await _sponsorService.UpdateAsync(id, Sponsor);
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> Delete(int id)
    {
        try
        {
            await _sponsorService.DeleteAsync(id);
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    [HttpGet("{id}/tournaments")]
    public async Task<ActionResult<IEnumerable<TournamentResponseDTO>>> GetTournamentsBySponsor(int id)
    {
        var tournaments = await _sponsorService.GetTournamentsBySponsorAsync(id);
        if (tournaments == null || !tournaments.Any())
            return NotFound(new { message = $"No se encontraron torneos para el patrocinador con ID {id}" });

        var tournamentsDto = _mapper.Map<IEnumerable<TournamentResponseDTO>>(tournaments);
        return Ok(tournamentsDto);
    }

    [HttpPost("{id}/tournaments")]
    public async Task<ActionResult<SponsorResponseDTO>> LinkSponsorAsync(int id, TournamentSponsorRequestDTO dto)
    {
        try
        {
            var tournamentSponsor = _mapper.Map<TournamentSponsor>(dto);
            tournamentSponsor.SponsorId = id;
            var createdLink = await _sponsorService.LinkSponsorAsync(tournamentSponsor);
            var responseDto = _mapper.Map<TournamentSponsorResponseDTO>(createdLink);

            return CreatedAtAction(
                nameof(GetById),
                new { id = responseDto.Id },
                responseDto);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }

    [HttpDelete("{id}/tournaments/{tid}")]
    public async Task<ActionResult> UnlinkSponsorAsync(int id, int tid)
    {
        try
        {
            await _sponsorService.UnlinkSponsorAsync(id, tid);
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }
}