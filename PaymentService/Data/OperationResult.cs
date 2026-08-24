namespace PaymentService.Data
{
    //rezultat operacije - umesto bacanja izuzetaka za ocekivane situacije
    public class OperationResult<T>
    {
        public OperationOutcome Outcome { get; }
        public T? Value { get; }

        private OperationResult(OperationOutcome outcome, T? value)
        {
            Outcome = outcome;
            Value = value;
        }

        public bool IsSuccess => Outcome == OperationOutcome.Success;

        public static OperationResult<T> Ok(T value) => new OperationResult<T>(OperationOutcome.Success, value);

        public static OperationResult<T> Fail(OperationOutcome outcome) => new OperationResult<T>(outcome, default);
    }
}
