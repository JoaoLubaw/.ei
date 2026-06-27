using System.Net;
using System.Text.Json.Serialization;

namespace Pontuei.Api.Dtos.Objects;

public enum InternalResultCode
{
    NO_ERROR = 0000,

    // Authentication and Authorization Errors
    INVALID_CREDENTIALS = 1000,
    UNLOGGED = 1001,
    REFRESH_TOKEN_EXPIRED = 1002,
    NOT_ADMIN = 1003,
    NOT_ALLOWED_TO_EDIT_USER = 1004,
    NOT_ALLOWED_TO_CREATE_OR_EDIT_ITEM = 1005,
    NOT_ALLOWED_TO_GET_THIS_ITEM = 1006,
    NOT_ALLOWED_TO_GET_THIS_USER = 1007,
    INVALID_RESET_PASSWORD_TOKEN = 1008,
    TOO_MANY_RESET_REQUESTS = 1009,
    TOO_MANY_INVALID_TOKEN_ATTEMPTS = 1010,
    TOO_MANY_REQUESTS = 1011,
    EMAIL_NOT_VERIFIED = 1012,
    EMAIL_ALREADY_VERIFIED = 1013,

    // Validation Errors
    INVALID_PARAMETERS = 2000,
    MISSING_INFORMATION = 2001,
    INFO_ALREADY_EXISTS = 2002,
    INVALID_TYPE = 2003,
    INVALID_USER_EMAIL = 2004,
    INVALID_CURRENT_PASSWORD = 2005,
    INVALID_USER_PASSWORD = 2006,
    INVALID_INPUT = 2007,
    EMAIL_ALREADY_TAKEN = 2008,
    PASSWORDS_DO_NOT_MATCH = 2009,
    INVALID_USER_PHONE_NUMBER = 2010,
    INVALID_CODE = 2011,
    TRANSACTION_NOT_PENDING = 2012,


    // Entity Errors
    ENTITY_NOT_FOUND = 3000,

    // System Errors
    UNHANDLED_ERROR = 8000,
    RUNTIME_ERROR = 8001,
    DATABASE_CONNECTION = 8002,
    TIMEOUT = 8003,
    METHOD_NOT_ALLOWED = 8004,
    SERVICE_UNAVAILABLE = 8005,
    EMAIL_SEND_ERROR = 8007
}

/// <summary>
/// A map of internal services response, containing an ApiResultCode and data that must be returned
/// </summary>
public class ApiResult<T>
{
    public InternalResultCode ResultCode { get; set; }
    public T? Data { get; set; }
    public HttpStatusCode HttpCode { get; set; }

    public ApiResult(InternalResultCode resultCode, HttpStatusCode httpCode, T? data)
    {
        this.ResultCode = resultCode;
        this.HttpCode = httpCode;
        this.Data = data;
    }
}

/// <summary>
/// Just a dummy class to represent ApiResults that has no data returned
/// </summary>
public class EmptyDto
{
    public EmptyDto() { }
}

/// <summary>
/// A map of internal problems, description and final http response code.
/// </summary>
public class ErrorApiResult
{
    /// <summary>
    /// Internal API error code.
    /// </summary>
    /// <value></value>
    public InternalResultCode Code { get; set; }

    /// <summary>
    /// Http status code.
    /// </summary>
    [JsonIgnore]
    public int Status { get; set; }

    /// <summary>
    /// Message returned into response body.
    /// </summary>
    public string Message { get; set; }

    /// <summary>
    /// Default class constructor.
    /// </summary>
    /// <param name="errorCode"></param>
    /// <param name="statusCode"></param>
    /// <param name="message"></param>
    public ErrorApiResult(InternalResultCode errorCode, int statusCode, string message)
    {
        Code = errorCode;
        Status = statusCode;
        Message = message;
    }

}