﻿namespace StudentCertificatePortal_API.Commons
{
    public record Result<T>
    {
        public bool Succeeded { get; set; }
        public string? Message { get; set; }
        public T? Data { get; set; }

        public static Result<T> Succeed(T data)
        {
            return new Result<T> { Succeeded = true, Data = data };
        }

        public static Result<T> Fail(Exception ex)
        {
            return new Result<T> { Succeeded = false, Message = ex.Message };
        }
        public static Result<T> Succeed(T data, string message)
        {
            return new Result<T>
            {
                Succeeded = true,
                Data = data,
                Message = message
            };
        }
    }
}
