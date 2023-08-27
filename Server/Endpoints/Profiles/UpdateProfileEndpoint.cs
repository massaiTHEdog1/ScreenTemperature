using FastEndpoints;
using ScreenTemperature.DTOs;
using ScreenTemperature.Mappers;
using ScreenTemperature.Services;

public class UpdateProfileRequest
{
    public ProfileDto Profile { get; set; }
}

public class UpdateProfileResponse
{
    public ProfileDto Profile { get; set; }
}

public class UpdateProfileEndpoint : Endpoint<UpdateProfileRequest, UpdateProfileResponse>
{
    private readonly IProfileService _profileService;

    public UpdateProfileEndpoint(IProfileService profileService)
    {
        _profileService = profileService;
    }

    public override void Configure()
    {
        Put("/api/profiles/update");
        AllowAnonymous();
    }

    public override async Task HandleAsync(UpdateProfileRequest req, CancellationToken ct)
    {
        var profile = await _profileService.UpdateProfileAsync(req.Profile, ct);

        await SendAsync(new UpdateProfileResponse()
        {
            Profile = profile
        }, cancellation: ct);
    }
}