using FastEndpoints;
using Microsoft.AspNetCore.Http.HttpResults;
using ScreenTemperature.DTOs;
using ScreenTemperature.Services;

public class ListProfilesEndpoint : EndpointWithoutRequest<Results<Ok<IList<ProfileDto>>, BadRequest<APIErrorResponseDto>>>
{
    private readonly IProfileService _profileService;

    public ListProfilesEndpoint(IProfileService profileService)
    {
        _profileService = profileService;
    }

    public override void Configure()
    {
        Get("/api/profiles");
        AllowAnonymous();
    }

    public override async Task<Results<Ok<IList<ProfileDto>>, BadRequest<APIErrorResponseDto>>> ExecuteAsync(CancellationToken ct)
    {
        var result = await _profileService.ListProfilesAsync(ct);

        if (!result.Success) return TypedResults.BadRequest(new APIErrorResponseDto(result.Errors));

        return TypedResults.Ok(result.Data);
    }
}