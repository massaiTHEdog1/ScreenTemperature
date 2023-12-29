namespace ScreenTemperature.Services
{
    public class ServiceResult<T>
    {
        public bool Success { get; set; }
        public string[] Errors { get; set; }
        public T Data { get; set; }
    }

    public class ServiceResult : ServiceResult<object>
    {

    }
}
