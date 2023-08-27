using FastEndpoints;
using ScreenTemperature.DTOs;
using ScreenTemperature.Mappers;
using ScreenTemperature.Services;

public class DeleteProfileRequest
{
    public Guid Id { get; set; }
}

public class DeleteProfilesEndpoint : Endpoint<DeleteProfileRequest>
{
    private readonly IProfileService _profileService;

    public DeleteProfilesEndpoint(IProfileService profileService)
    {
        _profileService = profileService;
    }

    public override void Configure()
    {
        Delete("/api/profiles/delete");
        AllowAnonymous();
    }

    public override async Task HandleAsync(DeleteProfileRequest req, CancellationToken ct)
    {
        await _profileService.DeleteProfileAsync(req.Id, ct);

        await SendNoContentAsync(ct);
    }
}