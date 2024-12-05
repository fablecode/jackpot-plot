namespace JackpotPlot.Domain.Models;

public record Result<T>
{
    public bool IsSuccess { get; }
    public T Value { get; }
    public string[] Errors { get; }
    public bool IsFailure => !IsSuccess;

    private Result(T value, bool isSuccess, params string[] errors)
    {
        IsSuccess = isSuccess;
        Value = value;
        Errors = errors;
    }

    public static Result<T> Success(T value) => new(value, true, null!);
    public static Result<T> Failure(params string[] error) => new(default!, false, error);

    // OnSuccess method for chaining operations
    public Result<TOutput> OnSuccess<TOutput>(Func<T, Result<TOutput>> func)
    {
        return !IsSuccess ? Result<TOutput>.Failure(Errors) : func(Value);
    }

    public Result<T> OnSuccess(Action<T> action)
    {
        if (IsSuccess)
        {
            action(Value);
        }

        return this;
    }

    // OnFailure method to handle errors
    public Result<T> OnFailure(Action<string[]> action)
    {
        if (!IsSuccess) action(Errors);
        return this;
    }
}