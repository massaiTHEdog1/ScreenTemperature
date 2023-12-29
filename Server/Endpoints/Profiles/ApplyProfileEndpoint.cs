using FastEndpoints;
using Microsoft.AspNetCore.Http.HttpResults;
using ScreenTemperature.DTOs;
using ScreenTemperature.Services;

public class ApplyProfileRequest
{
    /// <summary>
    /// Profile Id
    /// </summary>
    public Guid Id { get; set; }
    /// <summary>
    /// Ids of configurations to apply
    /// </summary>
    public Guid[]? ConfigurationIds { get; set; }
}

public class ApplyProfileEndpoint : Endpoint<ApplyProfileRequest, Results<Ok<IList<ConfigurationApplyResultDto>>, BadRequest<APIErrorResponseDto>>>
{
    private readonly IProfileService _profileService;

    public ApplyProfileEndpoint(IProfileService profileService)
    {
        _profileService = profileService;
    }

    public override void Configure()
    {
        Post("/api/profiles/{Id}/apply");
        AllowAnonymous();
    }

    public override async Task<Results<Ok<IList<ConfigurationApplyResultDto>>, BadRequest<APIErrorResponseDto>>> ExecuteAsync(ApplyProfileRequest dto, CancellationToken ct)
    {
        var result = await _profileService.ApplyProfileAsync(dto.Id, dto.ConfigurationIds, ct);

        if (!result.Success) return TypedResults.BadRequest(new APIErrorResponseDto(result.Errors));

        return TypedResults.Ok(result.Data);
    }
}