using FastEndpoints;
using ScreenTemperature.DTOs;
using ScreenTemperature.Services;

public class ListProfilesEndpoint : EndpointWithoutRequest<IList<ProfileDto>>
{
    private readonly IProfileService _profileService;

    public ListProfilesEndpoint(IProfileService profileService)
    {
        _profileService = profileService;
    }

    public override void Configure()
    {
        Get("/api/profiles/list");
        AllowAnonymous();
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var profiles = await _profileService.ListProfilesAsync(ct);

        await SendAsync(profiles, cancellation: ct);
    }
}