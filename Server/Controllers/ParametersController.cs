using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using ScreenTemperature;
using ScreenTemperature.DTOs;
using ScreenTemperature.Entities;
using ScreenTemperature.Mappers;
using ScreenTemperature.Services;
using System.ComponentModel.DataAnnotations;

public class ParametersController(IParametersService parametersService)
{
    [HttpGet("/api/parameters")]
    public async Task<IResult> GetParametersAsync()
    {
        return TypedResults.Ok(await parametersService.GetParametersAsync());
    }

    [HttpPut("/api/parameters")]
    public async Task<IResult> UpdateParametersAsync([FromBody][Required] ParametersDto parameters)
    {
        return TypedResults.Ok(await parametersService.UpdateParametersAsync(parameters));
    }
}