using FastEndpoints;
using Microsoft.AspNetCore.Http.HttpResults;
using ScreenTemperature.DTOs;
using ScreenTemperature.Services;

public class DeleteProfileEndpoint : EndpointWithoutRequest<Results<Ok, BadRequest<APIErrorResponseDto>>>
{
    private readonly IProfileService _profileService;

    public DeleteProfileEndpoint(IProfileService profileService)
    {
        _profileService = profileService;
    }

    public override void Configure()
    {
        Delete("/api/profiles/{Id}");
        AllowAnonymous();
    }

    public override async Task<Results<Ok, BadRequest<APIErrorResponseDto>>> ExecuteAsync(CancellationToken ct)
    {
        var result = await _profileService.DeleteProfileAsync(Route<Guid>("Id"), ct);

        if (!result.Success) return TypedResults.BadRequest(new APIErrorResponseDto(result.Errors));

        return TypedResults.Ok();
    }
}