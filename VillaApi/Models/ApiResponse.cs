namespace VillaApi.Models
{
    public class ApiResponse<TData>
    {
        public bool Success { get; set; }

        public int StatusCode { get; set; }

        public string Message { get; set; } = string.Empty;

        public TData? Data { get; set; }

        public object? Errors { get; set; }

        public DateTime TimeStamp { get; set; } = DateTime.UtcNow;



        private static ApiResponse<TData> Create(bool sucess, int statusCode, string message, TData? data = default, object? errors = null)
        {
            return new ApiResponse<TData>
            {
                Success = sucess,
                StatusCode = statusCode,
                Message = message,
                Data = data,
                Errors = errors


            };
        }

        //Static Factory Methods
        public static ApiResponse<TData> NotFound(string message) => Create(false,StatusCodes.Status404NotFound,message);

        public static ApiResponse<TData> BadRequest(string message , object? errors) => Create(false, StatusCodes.Status400BadRequest, message , errors:errors );

        public static ApiResponse<TData> Unauthorized ( object? errors, string message = "Please Log in") => Create(false,StatusCodes.Status401Unauthorized,message,errors:errors);
        public static ApiResponse<TData> Forbidden ( object? errors, string message = "you dont have permission to do that") => Create(false,StatusCodes.Status403Forbidden,message,errors:errors);

        //No Content Will be Removed but Why ?????.................
        public static ApiResponse<TData> NoContent(string message="Operation Done Sucessfully") => Create(true, StatusCodes.Status204NoContent,message);
        public static ApiResponse<TData> Created(TData data,string message) => Create(true, StatusCodes.Status201Created,message,data);
        public static ApiResponse<TData> Ok(TData data,string message , object?errors) => Create(true, StatusCodes.Status200OK,message,data ,errors:errors );
        public static ApiResponse<TData> Conflict(string message , object?errors) => Create(false, StatusCodes.Status409Conflict,message,errors:errors);
        public static ApiResponse<TData> Error(int statusCode,string message , object?errors) => Create(false, statusCode,message,errors:errors);










    }
}
