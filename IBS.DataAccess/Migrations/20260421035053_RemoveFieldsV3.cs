using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace IBS.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class RemoveFieldsV3 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_collection_receipts_sales_invoices_sales_invoice_id",
                table: "collection_receipts");

            migrationBuilder.DropForeignKey(
                name: "fk_credit_memos_sales_invoices_sales_invoice_id",
                table: "credit_memos");

            migrationBuilder.DropForeignKey(
                name: "fk_customers_suppliers_commissionee_id",
                table: "customers");

            migrationBuilder.DropForeignKey(
                name: "fk_debit_memos_sales_invoices_sales_invoice_id",
                table: "debit_memos");

            migrationBuilder.DropForeignKey(
                name: "fk_service_invoices_filpride_delivery_receipts_delivery_receip",
                table: "service_invoices");

            migrationBuilder.DropTable(
                name: "book_atl_details");

            migrationBuilder.DropTable(
                name: "cash_receipt_books");

            migrationBuilder.DropTable(
                name: "cv_trade_payments");

            migrationBuilder.DropTable(
                name: "disbursement_books");

            migrationBuilder.DropTable(
                name: "inventories");

            migrationBuilder.DropTable(
                name: "journal_books");

            migrationBuilder.DropTable(
                name: "po_actual_prices");

            migrationBuilder.DropTable(
                name: "purchase_books");

            migrationBuilder.DropTable(
                name: "purchase_locked_records_queues");

            migrationBuilder.DropTable(
                name: "sales_books");

            migrationBuilder.DropTable(
                name: "sales_invoices");

            migrationBuilder.DropTable(
                name: "sales_locked_records_queues");

            migrationBuilder.DropTable(
                name: "cos_appointed_suppliers");

            migrationBuilder.DropTable(
                name: "receiving_reports");

            migrationBuilder.DropTable(
                name: "filpride_delivery_receipts");

            migrationBuilder.DropTable(
                name: "authority_to_loads");

            migrationBuilder.DropTable(
                name: "filpride_customer_order_slips");

            migrationBuilder.DropTable(
                name: "purchase_orders");

            migrationBuilder.DropIndex(
                name: "ix_service_invoices_delivery_receipt_id",
                table: "service_invoices");

            migrationBuilder.DropIndex(
                name: "ix_service_invoices_service_invoice_no_company",
                table: "service_invoices");

            migrationBuilder.DropIndex(
                name: "ix_pick_up_points_company",
                table: "pick_up_points");

            migrationBuilder.DropIndex(
                name: "ix_monthly_nibits_company",
                table: "monthly_nibits");

            migrationBuilder.DropIndex(
                name: "ix_journal_voucher_headers_journal_voucher_header_no_company",
                table: "journal_voucher_headers");

            migrationBuilder.DropIndex(
                name: "ix_debit_memos_debit_memo_no_company",
                table: "debit_memos");

            migrationBuilder.DropIndex(
                name: "ix_debit_memos_sales_invoice_id",
                table: "debit_memos");

            migrationBuilder.DropIndex(
                name: "ix_customers_commissionee_id",
                table: "customers");

            migrationBuilder.DropIndex(
                name: "ix_credit_memos_credit_memo_no_company",
                table: "credit_memos");

            migrationBuilder.DropIndex(
                name: "ix_credit_memos_sales_invoice_id",
                table: "credit_memos");

            migrationBuilder.DropIndex(
                name: "ix_collection_receipts_collection_receipt_no_company",
                table: "collection_receipts");

            migrationBuilder.DropIndex(
                name: "ix_collection_receipts_sales_invoice_id",
                table: "collection_receipts");

            migrationBuilder.DropIndex(
                name: "ix_check_voucher_headers_check_voucher_header_no_company",
                table: "check_voucher_headers");

            migrationBuilder.DropColumn(
                name: "company",
                table: "suppliers");

            migrationBuilder.DropColumn(
                name: "is_bienes",
                table: "suppliers");

            migrationBuilder.DropColumn(
                name: "is_filpride",
                table: "suppliers");

            migrationBuilder.DropColumn(
                name: "requires_price_adjustment",
                table: "suppliers");

            migrationBuilder.DropColumn(
                name: "trade_name",
                table: "suppliers");

            migrationBuilder.DropColumn(
                name: "company",
                table: "services");

            migrationBuilder.DropColumn(
                name: "company",
                table: "service_invoices");

            migrationBuilder.DropColumn(
                name: "delivery_receipt_id",
                table: "service_invoices");

            migrationBuilder.DropColumn(
                name: "company",
                table: "posted_periods");

            migrationBuilder.DropColumn(
                name: "company",
                table: "pick_up_points");

            migrationBuilder.DropColumn(
                name: "company",
                table: "monthly_nibits");

            migrationBuilder.DropColumn(
                name: "company",
                table: "journal_voucher_headers");

            migrationBuilder.DropColumn(
                name: "company",
                table: "gl_sub_account_balances");

            migrationBuilder.DropColumn(
                name: "company",
                table: "gl_period_balances");

            migrationBuilder.DropColumn(
                name: "company",
                table: "general_ledger_books");

            migrationBuilder.DropColumn(
                name: "company",
                table: "employees");

            migrationBuilder.DropColumn(
                name: "adjusted_price",
                table: "debit_memos");

            migrationBuilder.DropColumn(
                name: "company",
                table: "debit_memos");

            migrationBuilder.DropColumn(
                name: "quantity",
                table: "debit_memos");

            migrationBuilder.DropColumn(
                name: "sales_invoice_id",
                table: "debit_memos");

            migrationBuilder.DropColumn(
                name: "cluster_code",
                table: "customers");

            migrationBuilder.DropColumn(
                name: "commission_rate",
                table: "customers");

            migrationBuilder.DropColumn(
                name: "commissionee_id",
                table: "customers");

            migrationBuilder.DropColumn(
                name: "company",
                table: "customers");

            migrationBuilder.DropColumn(
                name: "customer_type",
                table: "customers");

            migrationBuilder.DropColumn(
                name: "is_bienes",
                table: "customers");

            migrationBuilder.DropColumn(
                name: "is_filpride",
                table: "customers");

            migrationBuilder.DropColumn(
                name: "requires_price_adjustment",
                table: "customers");

            migrationBuilder.DropColumn(
                name: "station_code",
                table: "customers");

            migrationBuilder.DropColumn(
                name: "adjusted_price",
                table: "credit_memos");

            migrationBuilder.DropColumn(
                name: "company",
                table: "credit_memos");

            migrationBuilder.DropColumn(
                name: "quantity",
                table: "credit_memos");

            migrationBuilder.DropColumn(
                name: "sales_invoice_id",
                table: "credit_memos");

            migrationBuilder.DropColumn(
                name: "company",
                table: "collection_receipts");

            migrationBuilder.DropColumn(
                name: "multiple_si",
                table: "collection_receipts");

            migrationBuilder.DropColumn(
                name: "multiple_si_id",
                table: "collection_receipts");

            migrationBuilder.DropColumn(
                name: "sales_invoice_id",
                table: "collection_receipts");

            migrationBuilder.DropColumn(
                name: "si_multiple_amount",
                table: "collection_receipts");

            migrationBuilder.DropColumn(
                name: "si_no",
                table: "collection_receipts");

            migrationBuilder.DropColumn(
                name: "category",
                table: "check_voucher_headers");

            migrationBuilder.DropColumn(
                name: "company",
                table: "check_voucher_headers");

            migrationBuilder.DropColumn(
                name: "old_cv_no",
                table: "check_voucher_headers");

            migrationBuilder.DropColumn(
                name: "rr_no",
                table: "check_voucher_headers");

            migrationBuilder.DropColumn(
                name: "company",
                table: "bank_accounts");

            migrationBuilder.DropColumn(
                name: "is_bienes",
                table: "bank_accounts");

            migrationBuilder.DropColumn(
                name: "is_filpride",
                table: "bank_accounts");

            migrationBuilder.DropColumn(
                name: "company",
                table: "audit_trails");

            migrationBuilder.AlterColumn<DateTime>(
                name: "edited_date",
                table: "terms",
                type: "timestamp with time zone",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone");

            migrationBuilder.AlterColumn<string>(
                name: "edited_by",
                table: "terms",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<string>(
                name: "supplier_code",
                table: "suppliers",
                type: "character varying(7)",
                maxLength: 7,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "character varying(7)",
                oldMaxLength: 7,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "created_by",
                table: "suppliers",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "service_no",
                table: "services",
                type: "character varying(5)",
                maxLength: 5,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "character varying(5)",
                oldMaxLength: 5,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "created_by",
                table: "services",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "type",
                table: "journal_voucher_headers",
                type: "character varying(13)",
                maxLength: 13,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "character varying(13)",
                oldMaxLength: 13,
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "service_invoice_id",
                table: "debit_memos",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "debit_memo_no",
                table: "debit_memos",
                type: "character varying(13)",
                maxLength: 13,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "character varying(13)",
                oldMaxLength: 13,
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "amount",
                table: "debit_memos",
                type: "numeric(18,4)",
                nullable: false,
                defaultValue: 0m,
                oldClrType: typeof(decimal),
                oldType: "numeric(18,4)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "customer_code",
                table: "customers",
                type: "character varying(7)",
                maxLength: 7,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "character varying(7)",
                oldMaxLength: 7,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "created_by",
                table: "customers",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100,
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "service_invoice_id",
                table: "credit_memos",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "credit_memo_no",
                table: "credit_memos",
                type: "character varying(13)",
                maxLength: 13,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "character varying(13)",
                oldMaxLength: 13,
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "amount",
                table: "credit_memos",
                type: "numeric(18,4)",
                nullable: false,
                defaultValue: 0m,
                oldClrType: typeof(decimal),
                oldType: "numeric(18,4)",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "edited_date",
                table: "companies",
                type: "timestamp without time zone",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "timestamp without time zone");

            migrationBuilder.AlterColumn<string>(
                name: "created_by",
                table: "companies",
                type: "varchar(50)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "varchar(50)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "company_code",
                table: "companies",
                type: "varchar(3)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "varchar(3)",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "service_invoice_id",
                table: "collection_receipts",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "collection_receipt_no",
                table: "collection_receipts",
                type: "character varying(13)",
                maxLength: 13,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "character varying(13)",
                oldMaxLength: 13,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "type",
                table: "check_voucher_headers",
                type: "character varying(13)",
                maxLength: 13,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "character varying(13)",
                oldMaxLength: 13,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "payee",
                table: "check_voucher_headers",
                type: "character varying(150)",
                maxLength: 150,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "character varying(150)",
                oldMaxLength: 150,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "check_voucher_header_no",
                table: "check_voucher_headers",
                type: "character varying(13)",
                maxLength: 13,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "character varying(13)",
                oldMaxLength: 13,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "normal_balance",
                table: "chart_of_accounts",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "character varying(20)",
                oldMaxLength: 20,
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "edited_date",
                table: "chart_of_accounts",
                type: "timestamp without time zone",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "timestamp without time zone");

            migrationBuilder.AlterColumn<string>(
                name: "created_by",
                table: "chart_of_accounts",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "character varying(50)",
                oldMaxLength: 50,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "account_type",
                table: "chart_of_accounts",
                type: "character varying(25)",
                maxLength: 25,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "character varying(25)",
                oldMaxLength: 25,
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "ix_service_invoices_service_invoice_no",
                table: "service_invoices",
                column: "service_invoice_no",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_journal_voucher_headers_journal_voucher_header_no",
                table: "journal_voucher_headers",
                column: "journal_voucher_header_no",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_debit_memos_debit_memo_no",
                table: "debit_memos",
                column: "debit_memo_no",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_credit_memos_credit_memo_no",
                table: "credit_memos",
                column: "credit_memo_no",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_collection_receipts_collection_receipt_no",
                table: "collection_receipts",
                column: "collection_receipt_no",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_check_voucher_headers_check_voucher_header_no",
                table: "check_voucher_headers",
                column: "check_voucher_header_no",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_service_invoices_service_invoice_no",
                table: "service_invoices");

            migrationBuilder.DropIndex(
                name: "ix_journal_voucher_headers_journal_voucher_header_no",
                table: "journal_voucher_headers");

            migrationBuilder.DropIndex(
                name: "ix_debit_memos_debit_memo_no",
                table: "debit_memos");

            migrationBuilder.DropIndex(
                name: "ix_credit_memos_credit_memo_no",
                table: "credit_memos");

            migrationBuilder.DropIndex(
                name: "ix_collection_receipts_collection_receipt_no",
                table: "collection_receipts");

            migrationBuilder.DropIndex(
                name: "ix_check_voucher_headers_check_voucher_header_no",
                table: "check_voucher_headers");

            migrationBuilder.AlterColumn<DateTime>(
                name: "edited_date",
                table: "terms",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "edited_by",
                table: "terms",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "supplier_code",
                table: "suppliers",
                type: "character varying(7)",
                maxLength: 7,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(7)",
                oldMaxLength: 7);

            migrationBuilder.AlterColumn<string>(
                name: "created_by",
                table: "suppliers",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100);

            migrationBuilder.AddColumn<string>(
                name: "company",
                table: "suppliers",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "is_bienes",
                table: "suppliers",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "is_filpride",
                table: "suppliers",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "requires_price_adjustment",
                table: "suppliers",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "trade_name",
                table: "suppliers",
                type: "character varying(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "service_no",
                table: "services",
                type: "character varying(5)",
                maxLength: 5,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(5)",
                oldMaxLength: 5);

            migrationBuilder.AlterColumn<string>(
                name: "created_by",
                table: "services",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100);

            migrationBuilder.AddColumn<string>(
                name: "company",
                table: "services",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "company",
                table: "service_invoices",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "delivery_receipt_id",
                table: "service_invoices",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "company",
                table: "posted_periods",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "company",
                table: "pick_up_points",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "company",
                table: "monthly_nibits",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AlterColumn<string>(
                name: "type",
                table: "journal_voucher_headers",
                type: "character varying(13)",
                maxLength: 13,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(13)",
                oldMaxLength: 13);

            migrationBuilder.AddColumn<string>(
                name: "company",
                table: "journal_voucher_headers",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "company",
                table: "gl_sub_account_balances",
                type: "varchar(50)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "company",
                table: "gl_period_balances",
                type: "varchar(50)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "company",
                table: "general_ledger_books",
                type: "varchar(50)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "company",
                table: "employees",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "service_invoice_id",
                table: "debit_memos",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AlterColumn<string>(
                name: "debit_memo_no",
                table: "debit_memos",
                type: "character varying(13)",
                maxLength: 13,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(13)",
                oldMaxLength: 13);

            migrationBuilder.AlterColumn<decimal>(
                name: "amount",
                table: "debit_memos",
                type: "numeric(18,4)",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "numeric(18,4)");

            migrationBuilder.AddColumn<decimal>(
                name: "adjusted_price",
                table: "debit_memos",
                type: "numeric(18,4)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "company",
                table: "debit_memos",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<decimal>(
                name: "quantity",
                table: "debit_memos",
                type: "numeric(18,4)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "sales_invoice_id",
                table: "debit_memos",
                type: "integer",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "customer_code",
                table: "customers",
                type: "character varying(7)",
                maxLength: 7,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(7)",
                oldMaxLength: 7);

            migrationBuilder.AlterColumn<string>(
                name: "created_by",
                table: "customers",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100);

            migrationBuilder.AddColumn<int>(
                name: "cluster_code",
                table: "customers",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "commission_rate",
                table: "customers",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "commissionee_id",
                table: "customers",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "company",
                table: "customers",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "customer_type",
                table: "customers",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "is_bienes",
                table: "customers",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "is_filpride",
                table: "customers",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "requires_price_adjustment",
                table: "customers",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "station_code",
                table: "customers",
                type: "character varying(3)",
                maxLength: 3,
                nullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "service_invoice_id",
                table: "credit_memos",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AlterColumn<string>(
                name: "credit_memo_no",
                table: "credit_memos",
                type: "character varying(13)",
                maxLength: 13,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(13)",
                oldMaxLength: 13);

            migrationBuilder.AlterColumn<decimal>(
                name: "amount",
                table: "credit_memos",
                type: "numeric(18,4)",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "numeric(18,4)");

            migrationBuilder.AddColumn<decimal>(
                name: "adjusted_price",
                table: "credit_memos",
                type: "numeric(18,4)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "company",
                table: "credit_memos",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<decimal>(
                name: "quantity",
                table: "credit_memos",
                type: "numeric(18,4)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "sales_invoice_id",
                table: "credit_memos",
                type: "integer",
                nullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "edited_date",
                table: "companies",
                type: "timestamp without time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                oldClrType: typeof(DateTime),
                oldType: "timestamp without time zone",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "created_by",
                table: "companies",
                type: "varchar(50)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(50)");

            migrationBuilder.AlterColumn<string>(
                name: "company_code",
                table: "companies",
                type: "varchar(3)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(3)");

            migrationBuilder.AlterColumn<int>(
                name: "service_invoice_id",
                table: "collection_receipts",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AlterColumn<string>(
                name: "collection_receipt_no",
                table: "collection_receipts",
                type: "character varying(13)",
                maxLength: 13,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(13)",
                oldMaxLength: 13);

            migrationBuilder.AddColumn<string>(
                name: "company",
                table: "collection_receipts",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string[]>(
                name: "multiple_si",
                table: "collection_receipts",
                type: "text[]",
                nullable: true);

            migrationBuilder.AddColumn<int[]>(
                name: "multiple_si_id",
                table: "collection_receipts",
                type: "integer[]",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "sales_invoice_id",
                table: "collection_receipts",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<decimal[]>(
                name: "si_multiple_amount",
                table: "collection_receipts",
                type: "numeric[]",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "si_no",
                table: "collection_receipts",
                type: "character varying(13)",
                maxLength: 13,
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "type",
                table: "check_voucher_headers",
                type: "character varying(13)",
                maxLength: 13,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(13)",
                oldMaxLength: 13);

            migrationBuilder.AlterColumn<string>(
                name: "payee",
                table: "check_voucher_headers",
                type: "character varying(150)",
                maxLength: 150,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(150)",
                oldMaxLength: 150);

            migrationBuilder.AlterColumn<string>(
                name: "check_voucher_header_no",
                table: "check_voucher_headers",
                type: "character varying(13)",
                maxLength: 13,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(13)",
                oldMaxLength: 13);

            migrationBuilder.AddColumn<string>(
                name: "category",
                table: "check_voucher_headers",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "company",
                table: "check_voucher_headers",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "old_cv_no",
                table: "check_voucher_headers",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string[]>(
                name: "rr_no",
                table: "check_voucher_headers",
                type: "varchar[]",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "normal_balance",
                table: "chart_of_accounts",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(20)",
                oldMaxLength: 20);

            migrationBuilder.AlterColumn<DateTime>(
                name: "edited_date",
                table: "chart_of_accounts",
                type: "timestamp without time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                oldClrType: typeof(DateTime),
                oldType: "timestamp without time zone",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "created_by",
                table: "chart_of_accounts",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(50)",
                oldMaxLength: 50);

            migrationBuilder.AlterColumn<string>(
                name: "account_type",
                table: "chart_of_accounts",
                type: "character varying(25)",
                maxLength: 25,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(25)",
                oldMaxLength: 25);

            migrationBuilder.AddColumn<string>(
                name: "company",
                table: "bank_accounts",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "is_bienes",
                table: "bank_accounts",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "is_filpride",
                table: "bank_accounts",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "company",
                table: "audit_trails",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "cash_receipt_books",
                columns: table => new
                {
                    cash_receipt_book_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    bank = table.Column<string>(type: "text", nullable: true),
                    coa = table.Column<string>(type: "text", nullable: false),
                    check_no = table.Column<string>(type: "text", nullable: true),
                    company = table.Column<string>(type: "text", nullable: false),
                    created_by = table.Column<string>(type: "varchar(100)", nullable: true),
                    created_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    credit = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    customer_name = table.Column<string>(type: "text", nullable: false),
                    date = table.Column<DateOnly>(type: "date", nullable: false),
                    debit = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    particulars = table.Column<string>(type: "text", nullable: false),
                    ref_no = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_cash_receipt_books", x => x.cash_receipt_book_id);
                });

            migrationBuilder.CreateTable(
                name: "cv_trade_payments",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    check_voucher_id = table.Column<int>(type: "integer", nullable: false),
                    amount_paid = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    document_id = table.Column<int>(type: "integer", nullable: false),
                    document_type = table.Column<string>(type: "character varying(5)", maxLength: 5, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_cv_trade_payments", x => x.id);
                    table.ForeignKey(
                        name: "fk_cv_trade_payments_check_voucher_headers_check_voucher_id",
                        column: x => x.check_voucher_id,
                        principalTable: "check_voucher_headers",
                        principalColumn: "check_voucher_header_id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "disbursement_books",
                columns: table => new
                {
                    disbursement_book_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    amount = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    bank = table.Column<string>(type: "text", nullable: false),
                    cv_no = table.Column<string>(type: "text", nullable: false),
                    chart_of_account = table.Column<string>(type: "text", nullable: false),
                    check_date = table.Column<string>(type: "text", nullable: false),
                    check_no = table.Column<string>(type: "text", nullable: false),
                    company = table.Column<string>(type: "text", nullable: false),
                    created_by = table.Column<string>(type: "varchar(100)", nullable: true),
                    created_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    credit = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    date = table.Column<DateOnly>(type: "date", nullable: false),
                    debit = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    particulars = table.Column<string>(type: "text", nullable: false),
                    payee = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_disbursement_books", x => x.disbursement_book_id);
                });

            migrationBuilder.CreateTable(
                name: "journal_books",
                columns: table => new
                {
                    journal_book_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    account_title = table.Column<string>(type: "text", nullable: false),
                    company = table.Column<string>(type: "text", nullable: false),
                    created_by = table.Column<string>(type: "varchar(100)", nullable: true),
                    created_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    credit = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    date = table.Column<DateOnly>(type: "date", nullable: false),
                    debit = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    description = table.Column<string>(type: "text", nullable: false),
                    reference = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_journal_books", x => x.journal_book_id);
                });

            migrationBuilder.CreateTable(
                name: "purchase_books",
                columns: table => new
                {
                    purchase_book_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    amount = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    company = table.Column<string>(type: "text", nullable: false),
                    created_by = table.Column<string>(type: "varchar(100)", nullable: true),
                    created_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    date = table.Column<DateOnly>(type: "date", nullable: false),
                    description = table.Column<string>(type: "text", nullable: false),
                    discount = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    document_no = table.Column<string>(type: "text", nullable: false),
                    due_date = table.Column<DateOnly>(type: "date", nullable: false),
                    net_purchases = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    po_no = table.Column<string>(type: "varchar(12)", nullable: false),
                    supplier_address = table.Column<string>(type: "text", nullable: false),
                    supplier_name = table.Column<string>(type: "text", nullable: false),
                    supplier_tin = table.Column<string>(type: "text", nullable: false),
                    vat_amount = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    wht_amount = table.Column<decimal>(type: "numeric(18,4)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_purchase_books", x => x.purchase_book_id);
                });

            migrationBuilder.CreateTable(
                name: "purchase_orders",
                columns: table => new
                {
                    purchase_order_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    customer_id = table.Column<int>(type: "integer", nullable: true),
                    pick_up_point_id = table.Column<int>(type: "integer", nullable: false),
                    product_id = table.Column<int>(type: "integer", nullable: false),
                    supplier_id = table.Column<int>(type: "integer", nullable: false),
                    amount = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    canceled_by = table.Column<string>(type: "varchar(50)", nullable: true),
                    canceled_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    cancellation_remarks = table.Column<string>(type: "varchar(255)", nullable: true),
                    company = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    created_by = table.Column<string>(type: "varchar(100)", nullable: true),
                    created_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    date = table.Column<DateOnly>(type: "date", nullable: false),
                    edited_by = table.Column<string>(type: "varchar(50)", nullable: true),
                    edited_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    final_price = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    is_closed = table.Column<bool>(type: "boolean", nullable: false),
                    is_printed = table.Column<bool>(type: "boolean", nullable: false),
                    is_received = table.Column<bool>(type: "boolean", nullable: false),
                    is_sub_po = table.Column<bool>(type: "boolean", nullable: false),
                    old_po_no = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    posted_by = table.Column<string>(type: "varchar(50)", nullable: true),
                    posted_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    price = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    product_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    purchase_order_no = table.Column<string>(type: "character varying(13)", maxLength: 13, nullable: true),
                    quantity = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    quantity_received = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    received_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    remarks = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    sub_po_series = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    supplier_address = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    supplier_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    supplier_sales_order_no = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    supplier_tin = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    tax_type = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    terms = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    trigger_date = table.Column<DateOnly>(type: "date", nullable: false),
                    type = table.Column<string>(type: "character varying(13)", maxLength: 13, nullable: true),
                    type_of_purchase = table.Column<string>(type: "character varying(4)", maxLength: 4, nullable: false),
                    un_triggered_quantity = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    vat_type = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    voided_by = table.Column<string>(type: "varchar(50)", nullable: true),
                    voided_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_purchase_orders", x => x.purchase_order_id);
                    table.ForeignKey(
                        name: "fk_purchase_orders_customers_customer_id",
                        column: x => x.customer_id,
                        principalTable: "customers",
                        principalColumn: "customer_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_purchase_orders_pick_up_points_pick_up_point_id",
                        column: x => x.pick_up_point_id,
                        principalTable: "pick_up_points",
                        principalColumn: "pick_up_point_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_purchase_orders_products_product_id",
                        column: x => x.product_id,
                        principalTable: "products",
                        principalColumn: "product_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_purchase_orders_suppliers_supplier_id",
                        column: x => x.supplier_id,
                        principalTable: "suppliers",
                        principalColumn: "supplier_id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "sales_books",
                columns: table => new
                {
                    sales_book_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    address = table.Column<string>(type: "text", nullable: false),
                    amount = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    company = table.Column<string>(type: "text", nullable: false),
                    created_by = table.Column<string>(type: "varchar(100)", nullable: true),
                    created_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    description = table.Column<string>(type: "text", nullable: false),
                    discount = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    document_id = table.Column<int>(type: "integer", nullable: true),
                    due_date = table.Column<DateOnly>(type: "date", nullable: false),
                    net_sales = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    serial_no = table.Column<string>(type: "text", nullable: false),
                    sold_to = table.Column<string>(type: "text", nullable: false),
                    tin_no = table.Column<string>(type: "text", nullable: false),
                    transaction_date = table.Column<DateOnly>(type: "date", nullable: false),
                    vat_amount = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    vat_exempt_sales = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    vatable_sales = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    zero_rated = table.Column<decimal>(type: "numeric(18,4)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_sales_books", x => x.sales_book_id);
                });

            migrationBuilder.CreateTable(
                name: "filpride_customer_order_slips",
                columns: table => new
                {
                    customer_order_slip_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    commissionee_id = table.Column<int>(type: "integer", nullable: true),
                    customer_id = table.Column<int>(type: "integer", nullable: false),
                    hauler_id = table.Column<int>(type: "integer", nullable: true),
                    pick_up_point_id = table.Column<int>(type: "integer", nullable: true),
                    product_id = table.Column<int>(type: "integer", nullable: false),
                    purchase_order_id = table.Column<int>(type: "integer", nullable: true),
                    supplier_id = table.Column<int>(type: "integer", nullable: true),
                    account_specialist = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    authority_to_load_no = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    available_credit_limit = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    balance_quantity = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    branch = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    business_style = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    cnc_approved_by = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    cnc_approved_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    commission_rate = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    commissionee_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    commissionee_tax_type = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    commissionee_vat_type = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    company = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    created_by = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    created_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    customer_address = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    customer_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    customer_order_slip_no = table.Column<string>(type: "character varying(13)", maxLength: 13, nullable: false),
                    customer_po_no = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    customer_tin = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    customer_type = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    date = table.Column<DateOnly>(type: "date", nullable: false),
                    delivered_price = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    delivered_quantity = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    delivery_option = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    depot = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    disapproved_by = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    disapproved_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    driver = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    edited_by = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    edited_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    expiration_date = table.Column<DateOnly>(type: "date", nullable: true),
                    finance_instruction = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    fm_approved_by = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    fm_approved_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    freight = table.Column<decimal>(type: "numeric(18,4)", nullable: true),
                    has_commission = table.Column<bool>(type: "boolean", nullable: false),
                    has_ewt = table.Column<bool>(type: "boolean", nullable: false),
                    has_multiple_po = table.Column<bool>(type: "boolean", nullable: false),
                    has_wvat = table.Column<bool>(type: "boolean", nullable: false),
                    is_cos_atl_finalized = table.Column<bool>(type: "boolean", nullable: false),
                    is_delivered = table.Column<bool>(type: "boolean", nullable: false),
                    is_printed = table.Column<bool>(type: "boolean", nullable: false),
                    om_reason = table.Column<string>(type: "text", nullable: true),
                    old_cos_no = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    old_price = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    om_approved_by = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    om_approved_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    plate_no = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    price_reference = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    product_name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    quantity = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    remarks = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    sub_po_remarks = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    terms = table.Column<string>(type: "character varying(15)", maxLength: 15, nullable: true),
                    total_amount = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    uploaded_files = table.Column<string[]>(type: "varchar[]", nullable: true),
                    vat_type = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_filpride_customer_order_slips", x => x.customer_order_slip_id);
                    table.ForeignKey(
                        name: "fk_filpride_customer_order_slips_customers_customer_id",
                        column: x => x.customer_id,
                        principalTable: "customers",
                        principalColumn: "customer_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_filpride_customer_order_slips_pick_up_points_pick_up_point_",
                        column: x => x.pick_up_point_id,
                        principalTable: "pick_up_points",
                        principalColumn: "pick_up_point_id");
                    table.ForeignKey(
                        name: "fk_filpride_customer_order_slips_products_product_id",
                        column: x => x.product_id,
                        principalTable: "products",
                        principalColumn: "product_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_filpride_customer_order_slips_purchase_orders_purchase_orde",
                        column: x => x.purchase_order_id,
                        principalTable: "purchase_orders",
                        principalColumn: "purchase_order_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_filpride_customer_order_slips_suppliers_commissionee_id",
                        column: x => x.commissionee_id,
                        principalTable: "suppliers",
                        principalColumn: "supplier_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_filpride_customer_order_slips_suppliers_hauler_id",
                        column: x => x.hauler_id,
                        principalTable: "suppliers",
                        principalColumn: "supplier_id");
                    table.ForeignKey(
                        name: "fk_filpride_customer_order_slips_suppliers_supplier_id",
                        column: x => x.supplier_id,
                        principalTable: "suppliers",
                        principalColumn: "supplier_id");
                });

            migrationBuilder.CreateTable(
                name: "inventories",
                columns: table => new
                {
                    inventory_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    po_id = table.Column<int>(type: "integer", nullable: true),
                    product_id = table.Column<int>(type: "integer", nullable: false),
                    average_cost = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    company = table.Column<string>(type: "text", nullable: false),
                    cost = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    date = table.Column<DateOnly>(type: "date", nullable: false),
                    inventory_balance = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    is_validated = table.Column<bool>(type: "boolean", nullable: false),
                    particular = table.Column<string>(type: "varchar(200)", nullable: false),
                    quantity = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    reference = table.Column<string>(type: "varchar(12)", nullable: true),
                    total = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    total_balance = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    unit = table.Column<string>(type: "varchar(2)", nullable: false),
                    validated_by = table.Column<string>(type: "varchar(100)", nullable: true),
                    validated_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_inventories", x => x.inventory_id);
                    table.ForeignKey(
                        name: "fk_inventories_products_product_id",
                        column: x => x.product_id,
                        principalTable: "products",
                        principalColumn: "product_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_inventories_purchase_orders_po_id",
                        column: x => x.po_id,
                        principalTable: "purchase_orders",
                        principalColumn: "purchase_order_id");
                });

            migrationBuilder.CreateTable(
                name: "po_actual_prices",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    purchase_order_id = table.Column<int>(type: "integer", nullable: false),
                    applied_volume = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    approved_by = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    approved_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    is_approved = table.Column<bool>(type: "boolean", nullable: false),
                    triggered_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    triggered_price = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    triggered_volume = table.Column<decimal>(type: "numeric(18,4)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_po_actual_prices", x => x.id);
                    table.ForeignKey(
                        name: "fk_po_actual_prices_purchase_orders_purchase_order_id",
                        column: x => x.purchase_order_id,
                        principalTable: "purchase_orders",
                        principalColumn: "purchase_order_id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "authority_to_loads",
                columns: table => new
                {
                    authority_to_load_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    customer_order_slip_id = table.Column<int>(type: "integer", nullable: true),
                    supplier_id = table.Column<int>(type: "integer", nullable: false),
                    authority_to_load_no = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    company = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    created_by = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    created_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    date_booked = table.Column<DateOnly>(type: "date", nullable: false),
                    depot = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    driver = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    freight = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    hauler_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    load_port_id = table.Column<int>(type: "integer", nullable: false),
                    plate_no = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    remarks = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    supplier_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    uppi_atl_no = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    valid_until = table.Column<DateOnly>(type: "date", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_authority_to_loads", x => x.authority_to_load_id);
                    table.ForeignKey(
                        name: "fk_authority_to_loads_filpride_customer_order_slips_customer_o",
                        column: x => x.customer_order_slip_id,
                        principalTable: "filpride_customer_order_slips",
                        principalColumn: "customer_order_slip_id");
                    table.ForeignKey(
                        name: "fk_authority_to_loads_suppliers_supplier_id",
                        column: x => x.supplier_id,
                        principalTable: "suppliers",
                        principalColumn: "supplier_id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "cos_appointed_suppliers",
                columns: table => new
                {
                    sequence_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    customer_order_slip_id = table.Column<int>(type: "integer", nullable: false),
                    purchase_order_id = table.Column<int>(type: "integer", nullable: false),
                    supplier_id = table.Column<int>(type: "integer", nullable: false),
                    atl_no = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    is_assigned_to_dr = table.Column<bool>(type: "boolean", nullable: false),
                    quantity = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    unreserved_quantity = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    unserved_quantity = table.Column<decimal>(type: "numeric(18,4)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_cos_appointed_suppliers", x => x.sequence_id);
                    table.ForeignKey(
                        name: "fk_cos_appointed_suppliers_filpride_customer_order_slips_custo",
                        column: x => x.customer_order_slip_id,
                        principalTable: "filpride_customer_order_slips",
                        principalColumn: "customer_order_slip_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_cos_appointed_suppliers_purchase_orders_purchase_order_id",
                        column: x => x.purchase_order_id,
                        principalTable: "purchase_orders",
                        principalColumn: "purchase_order_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_cos_appointed_suppliers_suppliers_supplier_id",
                        column: x => x.supplier_id,
                        principalTable: "suppliers",
                        principalColumn: "supplier_id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "filpride_delivery_receipts",
                columns: table => new
                {
                    delivery_receipt_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    authority_to_load_id = table.Column<int>(type: "integer", nullable: false),
                    commissionee_id = table.Column<int>(type: "integer", nullable: true),
                    customer_id = table.Column<int>(type: "integer", nullable: false),
                    customer_order_slip_id = table.Column<int>(type: "integer", nullable: false),
                    hauler_id = table.Column<int>(type: "integer", nullable: true),
                    purchase_order_id = table.Column<int>(type: "integer", nullable: true),
                    authority_to_load_no = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    canceled_by = table.Column<string>(type: "varchar(50)", nullable: true),
                    canceled_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    cancellation_remarks = table.Column<string>(type: "varchar(255)", nullable: true),
                    commission_amount = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    commission_amount_paid = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    commission_rate = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    company = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    created_by = table.Column<string>(type: "varchar(100)", nullable: true),
                    created_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    customer_address = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    customer_tin = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    date = table.Column<DateOnly>(type: "date", nullable: false),
                    delivered_date = table.Column<DateOnly>(type: "date", nullable: true),
                    delivery_receipt_no = table.Column<string>(type: "character varying(13)", maxLength: 13, nullable: false),
                    driver = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    ecc = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    edited_by = table.Column<string>(type: "varchar(50)", nullable: true),
                    edited_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    freight = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    freight_amount = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    freight_amount_paid = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    has_already_invoiced = table.Column<bool>(type: "boolean", nullable: false),
                    has_receiving_report = table.Column<bool>(type: "boolean", nullable: false),
                    hauler_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    hauler_tax_type = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    hauler_vat_type = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    is_commission_paid = table.Column<bool>(type: "boolean", nullable: false),
                    is_freight_paid = table.Column<bool>(type: "boolean", nullable: false),
                    is_printed = table.Column<bool>(type: "boolean", nullable: false),
                    manual_dr_no = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    plate_no = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    posted_by = table.Column<string>(type: "varchar(50)", nullable: true),
                    posted_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    quantity = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    remarks = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    total_amount = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    type = table.Column<string>(type: "varchar(15)", nullable: false),
                    voided_by = table.Column<string>(type: "varchar(50)", nullable: true),
                    voided_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_filpride_delivery_receipts", x => x.delivery_receipt_id);
                    table.ForeignKey(
                        name: "fk_filpride_delivery_receipts_authority_to_loads_authority_to_",
                        column: x => x.authority_to_load_id,
                        principalTable: "authority_to_loads",
                        principalColumn: "authority_to_load_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_filpride_delivery_receipts_customers_customer_id",
                        column: x => x.customer_id,
                        principalTable: "customers",
                        principalColumn: "customer_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_filpride_delivery_receipts_filpride_customer_order_slips_cu",
                        column: x => x.customer_order_slip_id,
                        principalTable: "filpride_customer_order_slips",
                        principalColumn: "customer_order_slip_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_filpride_delivery_receipts_purchase_orders_purchase_order_id",
                        column: x => x.purchase_order_id,
                        principalTable: "purchase_orders",
                        principalColumn: "purchase_order_id");
                    table.ForeignKey(
                        name: "fk_filpride_delivery_receipts_suppliers_commissionee_id",
                        column: x => x.commissionee_id,
                        principalTable: "suppliers",
                        principalColumn: "supplier_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_filpride_delivery_receipts_suppliers_hauler_id",
                        column: x => x.hauler_id,
                        principalTable: "suppliers",
                        principalColumn: "supplier_id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "book_atl_details",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    appointed_id = table.Column<int>(type: "integer", nullable: true),
                    authority_to_load_id = table.Column<int>(type: "integer", nullable: false),
                    customer_order_slip_id = table.Column<int>(type: "integer", nullable: false),
                    quantity = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    unserved_quantity = table.Column<decimal>(type: "numeric(18,4)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_book_atl_details", x => x.id);
                    table.ForeignKey(
                        name: "fk_book_atl_details_authority_to_loads_authority_to_load_id",
                        column: x => x.authority_to_load_id,
                        principalTable: "authority_to_loads",
                        principalColumn: "authority_to_load_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_book_atl_details_cos_appointed_suppliers_appointed_id",
                        column: x => x.appointed_id,
                        principalTable: "cos_appointed_suppliers",
                        principalColumn: "sequence_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_book_atl_details_filpride_customer_order_slips_customer_ord",
                        column: x => x.customer_order_slip_id,
                        principalTable: "filpride_customer_order_slips",
                        principalColumn: "customer_order_slip_id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "receiving_reports",
                columns: table => new
                {
                    receiving_report_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    delivery_receipt_id = table.Column<int>(type: "integer", nullable: true),
                    po_id = table.Column<int>(type: "integer", nullable: false),
                    amount = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    amount_paid = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    authority_to_load_no = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    canceled_by = table.Column<string>(type: "varchar(50)", nullable: true),
                    canceled_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    canceled_quantity = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    cancellation_remarks = table.Column<string>(type: "varchar(255)", nullable: true),
                    company = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    cost_based_on_soa = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    created_by = table.Column<string>(type: "varchar(100)", nullable: true),
                    created_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    date = table.Column<DateOnly>(type: "date", nullable: false),
                    due_date = table.Column<DateOnly>(type: "date", nullable: false),
                    edited_by = table.Column<string>(type: "varchar(50)", nullable: true),
                    edited_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    gain_or_loss = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    is_cost_updated = table.Column<bool>(type: "boolean", nullable: false),
                    is_paid = table.Column<bool>(type: "boolean", nullable: false),
                    is_printed = table.Column<bool>(type: "boolean", nullable: false),
                    old_rr_no = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    po_no = table.Column<string>(type: "character varying(13)", maxLength: 13, nullable: true),
                    paid_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    posted_by = table.Column<string>(type: "varchar(50)", nullable: true),
                    posted_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    quantity_delivered = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    quantity_received = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    received_date = table.Column<DateOnly>(type: "date", nullable: true),
                    receiving_report_no = table.Column<string>(type: "character varying(13)", maxLength: 13, nullable: true),
                    remarks = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    supplier_dr_no = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    supplier_invoice_date = table.Column<DateOnly>(type: "date", nullable: true),
                    supplier_invoice_number = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    tax_percentage = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    truck_or_vessels = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    type = table.Column<string>(type: "character varying(13)", maxLength: 13, nullable: true),
                    voided_by = table.Column<string>(type: "varchar(50)", nullable: true),
                    voided_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    withdrawal_certificate = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_receiving_reports", x => x.receiving_report_id);
                    table.ForeignKey(
                        name: "fk_receiving_reports_filpride_delivery_receipts_delivery_recei",
                        column: x => x.delivery_receipt_id,
                        principalTable: "filpride_delivery_receipts",
                        principalColumn: "delivery_receipt_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_receiving_reports_purchase_orders_po_id",
                        column: x => x.po_id,
                        principalTable: "purchase_orders",
                        principalColumn: "purchase_order_id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "sales_invoices",
                columns: table => new
                {
                    sales_invoice_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    customer_id = table.Column<int>(type: "integer", nullable: false),
                    customer_order_slip_id = table.Column<int>(type: "integer", nullable: true),
                    delivery_receipt_id = table.Column<int>(type: "integer", nullable: true),
                    product_id = table.Column<int>(type: "integer", nullable: false),
                    purchase_order_id = table.Column<int>(type: "integer", nullable: false),
                    amount = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    amount_paid = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    balance = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    canceled_by = table.Column<string>(type: "varchar(50)", nullable: true),
                    canceled_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    cancellation_remarks = table.Column<string>(type: "varchar(255)", nullable: true),
                    company = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    created_by = table.Column<string>(type: "varchar(100)", nullable: true),
                    created_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    customer_address = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    customer_tin = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    discount = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    due_date = table.Column<DateOnly>(type: "date", nullable: false),
                    edited_by = table.Column<string>(type: "varchar(50)", nullable: true),
                    edited_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    is_paid = table.Column<bool>(type: "boolean", nullable: false),
                    is_printed = table.Column<bool>(type: "boolean", nullable: false),
                    is_tax_and_vat_paid = table.Column<bool>(type: "boolean", nullable: false),
                    other_ref_no = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    payment_status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    posted_by = table.Column<string>(type: "varchar(50)", nullable: true),
                    posted_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    quantity = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    receiving_report_id = table.Column<int>(type: "integer", nullable: false),
                    remarks = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    sales_invoice_no = table.Column<string>(type: "character varying(13)", maxLength: 13, nullable: true),
                    status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    terms = table.Column<string>(type: "character varying(15)", maxLength: 15, nullable: false),
                    transaction_date = table.Column<DateOnly>(type: "date", nullable: false),
                    type = table.Column<string>(type: "character varying(13)", maxLength: 13, nullable: false),
                    unit_price = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    voided_by = table.Column<string>(type: "varchar(50)", nullable: true),
                    voided_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_sales_invoices", x => x.sales_invoice_id);
                    table.ForeignKey(
                        name: "fk_sales_invoices_customers_customer_id",
                        column: x => x.customer_id,
                        principalTable: "customers",
                        principalColumn: "customer_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_sales_invoices_filpride_customer_order_slips_customer_order",
                        column: x => x.customer_order_slip_id,
                        principalTable: "filpride_customer_order_slips",
                        principalColumn: "customer_order_slip_id");
                    table.ForeignKey(
                        name: "fk_sales_invoices_filpride_delivery_receipts_delivery_receipt_",
                        column: x => x.delivery_receipt_id,
                        principalTable: "filpride_delivery_receipts",
                        principalColumn: "delivery_receipt_id");
                    table.ForeignKey(
                        name: "fk_sales_invoices_products_product_id",
                        column: x => x.product_id,
                        principalTable: "products",
                        principalColumn: "product_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_sales_invoices_purchase_orders_purchase_order_id",
                        column: x => x.purchase_order_id,
                        principalTable: "purchase_orders",
                        principalColumn: "purchase_order_id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "sales_locked_records_queues",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    delivery_receipt_id = table.Column<int>(type: "integer", nullable: false),
                    locked_date = table.Column<DateOnly>(type: "date", nullable: false),
                    price = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    quantity = table.Column<decimal>(type: "numeric(18,4)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_sales_locked_records_queues", x => x.id);
                    table.ForeignKey(
                        name: "fk_sales_locked_records_queues_filpride_delivery_receipts_deli",
                        column: x => x.delivery_receipt_id,
                        principalTable: "filpride_delivery_receipts",
                        principalColumn: "delivery_receipt_id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "purchase_locked_records_queues",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    receiving_report_id = table.Column<int>(type: "integer", nullable: false),
                    locked_date = table.Column<DateOnly>(type: "date", nullable: false),
                    price = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    quantity = table.Column<decimal>(type: "numeric(18,4)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_purchase_locked_records_queues", x => x.id);
                    table.ForeignKey(
                        name: "fk_purchase_locked_records_queues_receiving_reports_receiving_",
                        column: x => x.receiving_report_id,
                        principalTable: "receiving_reports",
                        principalColumn: "receiving_report_id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "ix_service_invoices_delivery_receipt_id",
                table: "service_invoices",
                column: "delivery_receipt_id");

            migrationBuilder.CreateIndex(
                name: "ix_service_invoices_service_invoice_no_company",
                table: "service_invoices",
                columns: new[] { "service_invoice_no", "company" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_pick_up_points_company",
                table: "pick_up_points",
                column: "company");

            migrationBuilder.CreateIndex(
                name: "ix_monthly_nibits_company",
                table: "monthly_nibits",
                column: "company");

            migrationBuilder.CreateIndex(
                name: "ix_journal_voucher_headers_journal_voucher_header_no_company",
                table: "journal_voucher_headers",
                columns: new[] { "journal_voucher_header_no", "company" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_debit_memos_debit_memo_no_company",
                table: "debit_memos",
                columns: new[] { "debit_memo_no", "company" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_debit_memos_sales_invoice_id",
                table: "debit_memos",
                column: "sales_invoice_id");

            migrationBuilder.CreateIndex(
                name: "ix_customers_commissionee_id",
                table: "customers",
                column: "commissionee_id");

            migrationBuilder.CreateIndex(
                name: "ix_credit_memos_credit_memo_no_company",
                table: "credit_memos",
                columns: new[] { "credit_memo_no", "company" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_credit_memos_sales_invoice_id",
                table: "credit_memos",
                column: "sales_invoice_id");

            migrationBuilder.CreateIndex(
                name: "ix_collection_receipts_collection_receipt_no_company",
                table: "collection_receipts",
                columns: new[] { "collection_receipt_no", "company" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_collection_receipts_sales_invoice_id",
                table: "collection_receipts",
                column: "sales_invoice_id");

            migrationBuilder.CreateIndex(
                name: "ix_check_voucher_headers_check_voucher_header_no_company",
                table: "check_voucher_headers",
                columns: new[] { "check_voucher_header_no", "company" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_authority_to_loads_authority_to_load_no_company",
                table: "authority_to_loads",
                columns: new[] { "authority_to_load_no", "company" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_authority_to_loads_customer_order_slip_id",
                table: "authority_to_loads",
                column: "customer_order_slip_id");

            migrationBuilder.CreateIndex(
                name: "ix_authority_to_loads_supplier_id",
                table: "authority_to_loads",
                column: "supplier_id");

            migrationBuilder.CreateIndex(
                name: "ix_book_atl_details_appointed_id",
                table: "book_atl_details",
                column: "appointed_id");

            migrationBuilder.CreateIndex(
                name: "ix_book_atl_details_authority_to_load_id",
                table: "book_atl_details",
                column: "authority_to_load_id");

            migrationBuilder.CreateIndex(
                name: "ix_book_atl_details_customer_order_slip_id",
                table: "book_atl_details",
                column: "customer_order_slip_id");

            migrationBuilder.CreateIndex(
                name: "ix_cos_appointed_suppliers_customer_order_slip_id",
                table: "cos_appointed_suppliers",
                column: "customer_order_slip_id");

            migrationBuilder.CreateIndex(
                name: "ix_cos_appointed_suppliers_purchase_order_id",
                table: "cos_appointed_suppliers",
                column: "purchase_order_id");

            migrationBuilder.CreateIndex(
                name: "ix_cos_appointed_suppliers_supplier_id",
                table: "cos_appointed_suppliers",
                column: "supplier_id");

            migrationBuilder.CreateIndex(
                name: "ix_cv_trade_payments_check_voucher_id",
                table: "cv_trade_payments",
                column: "check_voucher_id");

            migrationBuilder.CreateIndex(
                name: "ix_filpride_customer_order_slips_commissionee_id",
                table: "filpride_customer_order_slips",
                column: "commissionee_id");

            migrationBuilder.CreateIndex(
                name: "ix_filpride_customer_order_slips_customer_id",
                table: "filpride_customer_order_slips",
                column: "customer_id");

            migrationBuilder.CreateIndex(
                name: "ix_filpride_customer_order_slips_customer_order_slip_no_company",
                table: "filpride_customer_order_slips",
                columns: new[] { "customer_order_slip_no", "company" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_filpride_customer_order_slips_date",
                table: "filpride_customer_order_slips",
                column: "date");

            migrationBuilder.CreateIndex(
                name: "ix_filpride_customer_order_slips_hauler_id",
                table: "filpride_customer_order_slips",
                column: "hauler_id");

            migrationBuilder.CreateIndex(
                name: "ix_filpride_customer_order_slips_pick_up_point_id",
                table: "filpride_customer_order_slips",
                column: "pick_up_point_id");

            migrationBuilder.CreateIndex(
                name: "ix_filpride_customer_order_slips_product_id",
                table: "filpride_customer_order_slips",
                column: "product_id");

            migrationBuilder.CreateIndex(
                name: "ix_filpride_customer_order_slips_purchase_order_id",
                table: "filpride_customer_order_slips",
                column: "purchase_order_id");

            migrationBuilder.CreateIndex(
                name: "ix_filpride_customer_order_slips_supplier_id",
                table: "filpride_customer_order_slips",
                column: "supplier_id");

            migrationBuilder.CreateIndex(
                name: "ix_filpride_delivery_receipts_authority_to_load_id",
                table: "filpride_delivery_receipts",
                column: "authority_to_load_id");

            migrationBuilder.CreateIndex(
                name: "ix_filpride_delivery_receipts_commissionee_id",
                table: "filpride_delivery_receipts",
                column: "commissionee_id");

            migrationBuilder.CreateIndex(
                name: "ix_filpride_delivery_receipts_customer_id",
                table: "filpride_delivery_receipts",
                column: "customer_id");

            migrationBuilder.CreateIndex(
                name: "ix_filpride_delivery_receipts_customer_order_slip_id",
                table: "filpride_delivery_receipts",
                column: "customer_order_slip_id");

            migrationBuilder.CreateIndex(
                name: "ix_filpride_delivery_receipts_date",
                table: "filpride_delivery_receipts",
                column: "date");

            migrationBuilder.CreateIndex(
                name: "ix_filpride_delivery_receipts_delivery_receipt_no_company",
                table: "filpride_delivery_receipts",
                columns: new[] { "delivery_receipt_no", "company" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_filpride_delivery_receipts_hauler_id",
                table: "filpride_delivery_receipts",
                column: "hauler_id");

            migrationBuilder.CreateIndex(
                name: "ix_filpride_delivery_receipts_purchase_order_id",
                table: "filpride_delivery_receipts",
                column: "purchase_order_id");

            migrationBuilder.CreateIndex(
                name: "ix_inventories_po_id",
                table: "inventories",
                column: "po_id");

            migrationBuilder.CreateIndex(
                name: "ix_inventories_product_id",
                table: "inventories",
                column: "product_id");

            migrationBuilder.CreateIndex(
                name: "ix_po_actual_prices_purchase_order_id",
                table: "po_actual_prices",
                column: "purchase_order_id");

            migrationBuilder.CreateIndex(
                name: "ix_purchase_locked_records_queues_locked_date",
                table: "purchase_locked_records_queues",
                column: "locked_date");

            migrationBuilder.CreateIndex(
                name: "ix_purchase_locked_records_queues_receiving_report_id",
                table: "purchase_locked_records_queues",
                column: "receiving_report_id");

            migrationBuilder.CreateIndex(
                name: "ix_purchase_orders_customer_id",
                table: "purchase_orders",
                column: "customer_id");

            migrationBuilder.CreateIndex(
                name: "ix_purchase_orders_pick_up_point_id",
                table: "purchase_orders",
                column: "pick_up_point_id");

            migrationBuilder.CreateIndex(
                name: "ix_purchase_orders_product_id",
                table: "purchase_orders",
                column: "product_id");

            migrationBuilder.CreateIndex(
                name: "ix_purchase_orders_purchase_order_no_company",
                table: "purchase_orders",
                columns: new[] { "purchase_order_no", "company" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_purchase_orders_supplier_id",
                table: "purchase_orders",
                column: "supplier_id");

            migrationBuilder.CreateIndex(
                name: "ix_receiving_reports_delivery_receipt_id",
                table: "receiving_reports",
                column: "delivery_receipt_id");

            migrationBuilder.CreateIndex(
                name: "ix_receiving_reports_po_id",
                table: "receiving_reports",
                column: "po_id");

            migrationBuilder.CreateIndex(
                name: "ix_receiving_reports_receiving_report_no_company",
                table: "receiving_reports",
                columns: new[] { "receiving_report_no", "company" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_sales_invoices_customer_id",
                table: "sales_invoices",
                column: "customer_id");

            migrationBuilder.CreateIndex(
                name: "ix_sales_invoices_customer_order_slip_id",
                table: "sales_invoices",
                column: "customer_order_slip_id");

            migrationBuilder.CreateIndex(
                name: "ix_sales_invoices_delivery_receipt_id",
                table: "sales_invoices",
                column: "delivery_receipt_id");

            migrationBuilder.CreateIndex(
                name: "ix_sales_invoices_product_id",
                table: "sales_invoices",
                column: "product_id");

            migrationBuilder.CreateIndex(
                name: "ix_sales_invoices_purchase_order_id",
                table: "sales_invoices",
                column: "purchase_order_id");

            migrationBuilder.CreateIndex(
                name: "ix_sales_invoices_sales_invoice_no_company",
                table: "sales_invoices",
                columns: new[] { "sales_invoice_no", "company" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_sales_locked_records_queues_delivery_receipt_id",
                table: "sales_locked_records_queues",
                column: "delivery_receipt_id");

            migrationBuilder.CreateIndex(
                name: "ix_sales_locked_records_queues_locked_date",
                table: "sales_locked_records_queues",
                column: "locked_date");

            migrationBuilder.AddForeignKey(
                name: "fk_collection_receipts_sales_invoices_sales_invoice_id",
                table: "collection_receipts",
                column: "sales_invoice_id",
                principalTable: "sales_invoices",
                principalColumn: "sales_invoice_id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "fk_credit_memos_sales_invoices_sales_invoice_id",
                table: "credit_memos",
                column: "sales_invoice_id",
                principalTable: "sales_invoices",
                principalColumn: "sales_invoice_id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "fk_customers_suppliers_commissionee_id",
                table: "customers",
                column: "commissionee_id",
                principalTable: "suppliers",
                principalColumn: "supplier_id");

            migrationBuilder.AddForeignKey(
                name: "fk_debit_memos_sales_invoices_sales_invoice_id",
                table: "debit_memos",
                column: "sales_invoice_id",
                principalTable: "sales_invoices",
                principalColumn: "sales_invoice_id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "fk_service_invoices_filpride_delivery_receipts_delivery_receip",
                table: "service_invoices",
                column: "delivery_receipt_id",
                principalTable: "filpride_delivery_receipts",
                principalColumn: "delivery_receipt_id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
