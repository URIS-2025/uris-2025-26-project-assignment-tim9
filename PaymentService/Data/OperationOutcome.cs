namespace PaymentService.Data
{
    //ishodi poslovnih pravila, kontroler ih prevodi u HTTP status kodove
    public enum OperationOutcome
    {
        Success,
        NotFound,
        InvoiceIsPaid,
        InvoiceIsCancelled,
        AmountExceedsRemainingDebt
    }
}
