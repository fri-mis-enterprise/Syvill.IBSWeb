namespace IBS.Models.Enums
{
    public enum NormalBalance
    {
        Debit,
        Credit
    }

    public enum DocumentType
    {
        Documented,
        Undocumented
    }

    public enum ServiceInvoiceCreationMode
    {
        Manual,
        Automatic
    }

    public enum Status
    {
        Pending,
        Posted,
        Voided,
        Canceled,
        Closed,
    }

    public enum CVType
    {
        Invoicing,
        Payment
    }

    public enum ModuleType
    {
        Sales,
        Purchase,
        Disbursement,
        DebitMemo,
        CreditMemo,
        Collection,
        Journal
    }

    public enum Module
    {
        ServiceInvoice,
        CollectionReceipt,
        CheckVoucher,
        JournalVoucher,
        DebitMemo,
        CreditMemo,
        ProvisionalReceipt
    }

    public enum SubAccountType
    {
        Customer = 1,
        Supplier = 2,
        Employee = 3,
        BankAccount = 4,
        Company = 5
    }
}
