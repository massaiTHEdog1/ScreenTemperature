using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using ScreenTemperature.DTOs;
using ScreenTemperature.Entities;
using ScreenTemperature.Mappers;
using ScreenTemperature.Services;
using System.ComponentModel.DataAnnotations;

public class ParametersController(IParametersService parametersService)
{
    [HttpGet("/api/parameters")]
    public IResult GetParameters()
    {
        return TypedResults.Ok(parametersService.GetParameters());
    }

    [HttpPut("/api/parameters")]
    public IResult UpdateParameters([FromBody][Required] Parameters parameters)
    {
        return TypedResults.Ok(parametersService.UpdateParameters(parameters));
    }
}