namespace ScreenTemperature.Dto;

/// <summary>
/// A generic DTO for using primitive values in controller's [FromBody]
/// </summary>
public class PrimitiveDto<T>
{
    public T Value { get; set; }
}