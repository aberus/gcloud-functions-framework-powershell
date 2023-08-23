namespace System.Threading.Tasks;

public static class TaskExtensions
{
    public static async Task<T> WithCancellation<T>(this Task<T> task, CancellationToken cancellationToken)
    {
        var tcs = new TaskCompletionSource<object>(TaskCreationOptions.RunContinuationsAsynchronously);

        await using (cancellationToken.Register(state => (state as TaskCompletionSource<object>)?.TrySetResult(null!), tcs)
          .ConfigureAwait(false))
        {
            var resultTask = await Task.WhenAny(task, tcs.Task).ConfigureAwait(false);
            if (resultTask == tcs.Task)
            {
                throw new OperationCanceledException(cancellationToken);
            }

            return await task.ConfigureAwait(false);
        }
    }
}
