using FastEndpoints;
using Microsoft.AspNetCore.Http.HttpResults;
using ScreenTemperature.DTOs;
using ScreenTemperature.Services;

public class UpdateProfileEndpoint : Endpoint<ProfileDto, Results<Ok<ProfileDto>, BadRequest<APIErrorResponseDto>>>
{
    private readonly IProfileService _profileService;

    public UpdateProfileEndpoint(IProfileService profileService)
    {
        _profileService = profileService;
    }

    public override void Configure()
    {
        Put("/api/profiles/{Id}");
        AllowAnonymous();
    }

    public override async Task<Results<Ok<ProfileDto>, BadRequest<APIErrorResponseDto>>> ExecuteAsync(ProfileDto dto, CancellationToken ct)
    {
        var result = await _profileService.CreateOrUpdateProfileAsync(dto, ct);

        if (!result.Success) return TypedResults.BadRequest(new APIErrorResponseDto(result.Errors));

        return TypedResults.Ok(result.Data);
    }
}