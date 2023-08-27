using FastEndpoints;
using ScreenTemperature.DTOs;
using ScreenTemperature.Mappers;
using ScreenTemperature.Services;

public class AddProfileRequest
{
    public ProfileDto Profile { get; set; }
}

public class AddProfileResponse
{
    public ProfileDto Profile { get; set; }
}

public class AddProfileEndpoint : Endpoint<AddProfileRequest, AddProfileResponse>
{
    private readonly IProfileService _profileService;

    public AddProfileEndpoint(IProfileService profileService)
    {
        _profileService = profileService;
    }

    public override void Configure()
    {
        Post("/api/profiles/create");
        AllowAnonymous();
    }

    public override async Task HandleAsync(AddProfileRequest req, CancellationToken ct)
    {
        var profile = await _profileService.AddProfileAsync(req.Profile, ct);

        await SendAsync(new AddProfileResponse()
        {
            Profile = profile
        }, cancellation: ct);
    }
}