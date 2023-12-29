namespace ScreenTemperature.DTOs
{
    public class APIErrorResponseDto
    {
        public string[] Errors { get; set; }

        public APIErrorResponseDto(string[] errors)
        {
            Errors = errors;
        }
    }
}
