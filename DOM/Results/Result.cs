namespace DOM.Results;

public class Result<T>
{
    public bool IsSuccess { get; private set; }
    public T? Value { get; private set; }
    public string? ErrorMessage { get; private set; }
    public ResultFailureType FailureType { get; private set; }

    private Result(bool isSuccess, T? value, string? errorMessage, ResultFailureType failureType)
    {
        IsSuccess = isSuccess;
        Value = value;
        ErrorMessage = errorMessage;
        FailureType = failureType;
    }

    public static Result<T> Success(T value)
    {
        return new Result<T>(true, value, null, ResultFailureType.None);
    }
    
    public static Result<T> Failure(string errorMessage, ResultFailureType failureType)
    {
        return new Result<T>(false, default, errorMessage, failureType);
    }
}