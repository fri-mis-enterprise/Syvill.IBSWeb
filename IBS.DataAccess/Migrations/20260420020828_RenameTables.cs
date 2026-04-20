using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace IBS.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class RenameTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_filpride_check_voucher_headers_filpride_bank_accounts_bank_",
                table: "filpride_check_voucher_headers");

            migrationBuilder.DropForeignKey(
                name: "fk_filpride_check_voucher_headers_filpride_employees_employee_",
                table: "filpride_check_voucher_headers");

            migrationBuilder.DropForeignKey(
                name: "fk_filpride_check_voucher_headers_filpride_suppliers_supplier_",
                table: "filpride_check_voucher_headers");

            migrationBuilder.DropForeignKey(
                name: "fk_filpride_customer_order_slips_filpride_customers_customer_id",
                table: "filpride_customer_order_slips");

            migrationBuilder.DropForeignKey(
                name: "fk_filpride_customer_order_slips_filpride_pick_up_points_pick_",
                table: "filpride_customer_order_slips");

            migrationBuilder.DropForeignKey(
                name: "fk_filpride_customer_order_slips_filpride_purchase_orders_purc",
                table: "filpride_customer_order_slips");

            migrationBuilder.DropForeignKey(
                name: "fk_filpride_customer_order_slips_filpride_suppliers_commission",
                table: "filpride_customer_order_slips");

            migrationBuilder.DropForeignKey(
                name: "fk_filpride_customer_order_slips_filpride_suppliers_hauler_id",
                table: "filpride_customer_order_slips");

            migrationBuilder.DropForeignKey(
                name: "fk_filpride_customer_order_slips_filpride_suppliers_supplier_id",
                table: "filpride_customer_order_slips");

            migrationBuilder.DropForeignKey(
                name: "fk_filpride_delivery_receipts_filpride_authority_to_loads_auth",
                table: "filpride_delivery_receipts");

            migrationBuilder.DropForeignKey(
                name: "fk_filpride_delivery_receipts_filpride_customers_customer_id",
                table: "filpride_delivery_receipts");

            migrationBuilder.DropForeignKey(
                name: "fk_filpride_delivery_receipts_filpride_purchase_orders_purchas",
                table: "filpride_delivery_receipts");

            migrationBuilder.DropForeignKey(
                name: "fk_filpride_delivery_receipts_filpride_suppliers_commissionee_",
                table: "filpride_delivery_receipts");

            migrationBuilder.DropForeignKey(
                name: "fk_filpride_delivery_receipts_filpride_suppliers_hauler_id",
                table: "filpride_delivery_receipts");

            migrationBuilder.DropForeignKey(
                name: "fk_filpride_journal_voucher_headers_filpride_check_voucher_hea",
                table: "filpride_journal_voucher_headers");

            migrationBuilder.DropForeignKey(
                name: "fk_filpride_purchase_orders_filpride_customers_customer_id",
                table: "filpride_purchase_orders");

            migrationBuilder.DropForeignKey(
                name: "fk_filpride_purchase_orders_filpride_pick_up_points_pick_up_po",
                table: "filpride_purchase_orders");

            migrationBuilder.DropForeignKey(
                name: "fk_filpride_purchase_orders_filpride_suppliers_supplier_id",
                table: "filpride_purchase_orders");

            migrationBuilder.DropForeignKey(
                name: "fk_filpride_purchase_orders_products_product_id",
                table: "filpride_purchase_orders");

            migrationBuilder.DropForeignKey(
                name: "fk_filpride_sales_locked_records_queues_filpride_delivery_rece",
                table: "filpride_sales_locked_records_queues");

            migrationBuilder.DropForeignKey(
                name: "fk_jv_amortization_settings_filpride_journal_voucher_headers_j",
                table: "jv_amortization_settings");

            migrationBuilder.DropTable(
                name: "filpride_audit_trails");

            migrationBuilder.DropTable(
                name: "filpride_book_atl_details");

            migrationBuilder.DropTable(
                name: "filpride_cash_receipt_books");

            migrationBuilder.DropTable(
                name: "filpride_check_voucher_details");

            migrationBuilder.DropTable(
                name: "filpride_collection_receipt_details");

            migrationBuilder.DropTable(
                name: "filpride_credit_memos");

            migrationBuilder.DropTable(
                name: "filpride_customer_branches");

            migrationBuilder.DropTable(
                name: "filpride_cv_trade_payments");

            migrationBuilder.DropTable(
                name: "filpride_debit_memos");

            migrationBuilder.DropTable(
                name: "filpride_disbursement_books");

            migrationBuilder.DropTable(
                name: "filpride_freights");

            migrationBuilder.DropTable(
                name: "filpride_general_ledger_books");

            migrationBuilder.DropTable(
                name: "filpride_gl_period_balances");

            migrationBuilder.DropTable(
                name: "filpride_gl_sub_account_balances");

            migrationBuilder.DropTable(
                name: "filpride_inventories");

            migrationBuilder.DropTable(
                name: "filpride_journal_books");

            migrationBuilder.DropTable(
                name: "filpride_journal_voucher_details");

            migrationBuilder.DropTable(
                name: "filpride_monthly_nibits");

            migrationBuilder.DropTable(
                name: "filpride_multiple_check_voucher_payments");

            migrationBuilder.DropTable(
                name: "filpride_offsettings");

            migrationBuilder.DropTable(
                name: "filpride_po_actual_prices");

            migrationBuilder.DropTable(
                name: "filpride_provisional_receipts");

            migrationBuilder.DropTable(
                name: "filpride_purchase_books");

            migrationBuilder.DropTable(
                name: "filpride_purchase_locked_records_queues");

            migrationBuilder.DropTable(
                name: "filpride_sales_books");

            migrationBuilder.DropTable(
                name: "filpride_terms");

            migrationBuilder.DropTable(
                name: "filpride_authority_to_loads");

            migrationBuilder.DropTable(
                name: "filpride_cos_appointed_suppliers");

            migrationBuilder.DropTable(
                name: "filpride_collection_receipts");

            migrationBuilder.DropTable(
                name: "filpride_pick_up_points");

            migrationBuilder.DropTable(
                name: "filpride_chart_of_accounts");

            migrationBuilder.DropTable(
                name: "filpride_employees");

            migrationBuilder.DropTable(
                name: "filpride_receiving_reports");

            migrationBuilder.DropTable(
                name: "filpride_bank_accounts");

            migrationBuilder.DropTable(
                name: "filpride_sales_invoices");

            migrationBuilder.DropTable(
                name: "filpride_service_invoices");

            migrationBuilder.DropTable(
                name: "filpride_customers");

            migrationBuilder.DropTable(
                name: "filpride_services");

            migrationBuilder.DropTable(
                name: "filpride_suppliers");

            migrationBuilder.DropPrimaryKey(
                name: "pk_filpride_sales_locked_records_queues",
                table: "filpride_sales_locked_records_queues");

            migrationBuilder.DropPrimaryKey(
                name: "pk_filpride_purchase_orders",
                table: "filpride_purchase_orders");

            migrationBuilder.DropPrimaryKey(
                name: "pk_filpride_journal_voucher_headers",
                table: "filpride_journal_voucher_headers");

            migrationBuilder.DropPrimaryKey(
                name: "pk_filpride_check_voucher_headers",
                table: "filpride_check_voucher_headers");

            migrationBuilder.RenameTable(
                name: "filpride_sales_locked_records_queues",
                newName: "sales_locked_records_queues");

            migrationBuilder.RenameTable(
                name: "filpride_purchase_orders",
                newName: "purchase_orders");

            migrationBuilder.RenameTable(
                name: "filpride_journal_voucher_headers",
                newName: "journal_voucher_headers");

            migrationBuilder.RenameTable(
                name: "filpride_check_voucher_headers",
                newName: "check_voucher_headers");

            migrationBuilder.RenameIndex(
                name: "ix_filpride_sales_locked_records_queues_locked_date",
                table: "sales_locked_records_queues",
                newName: "ix_sales_locked_records_queues_locked_date");

            migrationBuilder.RenameIndex(
                name: "ix_filpride_sales_locked_records_queues_delivery_receipt_id",
                table: "sales_locked_records_queues",
                newName: "ix_sales_locked_records_queues_delivery_receipt_id");

            migrationBuilder.RenameIndex(
                name: "ix_filpride_purchase_orders_supplier_id",
                table: "purchase_orders",
                newName: "ix_purchase_orders_supplier_id");

            migrationBuilder.RenameIndex(
                name: "ix_filpride_purchase_orders_purchase_order_no_company",
                table: "purchase_orders",
                newName: "ix_purchase_orders_purchase_order_no_company");

            migrationBuilder.RenameIndex(
                name: "ix_filpride_purchase_orders_product_id",
                table: "purchase_orders",
                newName: "ix_purchase_orders_product_id");

            migrationBuilder.RenameIndex(
                name: "ix_filpride_purchase_orders_pick_up_point_id",
                table: "purchase_orders",
                newName: "ix_purchase_orders_pick_up_point_id");

            migrationBuilder.RenameIndex(
                name: "ix_filpride_purchase_orders_customer_id",
                table: "purchase_orders",
                newName: "ix_purchase_orders_customer_id");

            migrationBuilder.RenameIndex(
                name: "ix_filpride_journal_voucher_headers_journal_voucher_header_no_",
                table: "journal_voucher_headers",
                newName: "ix_journal_voucher_headers_journal_voucher_header_no_company");

            migrationBuilder.RenameIndex(
                name: "ix_filpride_journal_voucher_headers_cv_id",
                table: "journal_voucher_headers",
                newName: "ix_journal_voucher_headers_cv_id");

            migrationBuilder.RenameIndex(
                name: "ix_filpride_check_voucher_headers_supplier_id",
                table: "check_voucher_headers",
                newName: "ix_check_voucher_headers_supplier_id");

            migrationBuilder.RenameIndex(
                name: "ix_filpride_check_voucher_headers_employee_id",
                table: "check_voucher_headers",
                newName: "ix_check_voucher_headers_employee_id");

            migrationBuilder.RenameIndex(
                name: "ix_filpride_check_voucher_headers_check_voucher_header_no_comp",
                table: "check_voucher_headers",
                newName: "ix_check_voucher_headers_check_voucher_header_no_company");

            migrationBuilder.RenameIndex(
                name: "ix_filpride_check_voucher_headers_bank_id",
                table: "check_voucher_headers",
                newName: "ix_check_voucher_headers_bank_id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_sales_locked_records_queues",
                table: "sales_locked_records_queues",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_purchase_orders",
                table: "purchase_orders",
                column: "purchase_order_id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_journal_voucher_headers",
                table: "journal_voucher_headers",
                column: "journal_voucher_header_id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_check_voucher_headers",
                table: "check_voucher_headers",
                column: "check_voucher_header_id");

            migrationBuilder.CreateTable(
                name: "audit_trails",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    username = table.Column<string>(type: "text", nullable: false),
                    date = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    machine_name = table.Column<string>(type: "text", nullable: false),
                    activity = table.Column<string>(type: "text", nullable: false),
                    document_type = table.Column<string>(type: "text", nullable: false),
                    company = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_audit_trails", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "bank_accounts",
                columns: table => new
                {
                    bank_account_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    bank = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    branch = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    account_no = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    account_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    created_by = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    created_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    company = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    is_filpride = table.Column<bool>(type: "boolean", nullable: false),
                    is_bienes = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_bank_accounts", x => x.bank_account_id);
                });

            migrationBuilder.CreateTable(
                name: "cash_receipt_books",
                columns: table => new
                {
                    cash_receipt_book_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    date = table.Column<DateOnly>(type: "date", nullable: false),
                    ref_no = table.Column<string>(type: "text", nullable: false),
                    customer_name = table.Column<string>(type: "text", nullable: false),
                    bank = table.Column<string>(type: "text", nullable: true),
                    check_no = table.Column<string>(type: "text", nullable: true),
                    coa = table.Column<string>(type: "text", nullable: false),
                    particulars = table.Column<string>(type: "text", nullable: false),
                    debit = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    credit = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    created_by = table.Column<string>(type: "varchar(100)", nullable: true),
                    created_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    company = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_cash_receipt_books", x => x.cash_receipt_book_id);
                });

            migrationBuilder.CreateTable(
                name: "chart_of_accounts",
                columns: table => new
                {
                    account_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    is_main = table.Column<bool>(type: "boolean", nullable: false),
                    account_number = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    account_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    account_type = table.Column<string>(type: "character varying(25)", maxLength: 25, nullable: true),
                    normal_balance = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    level = table.Column<int>(type: "integer", nullable: false),
                    parent_account_id = table.Column<int>(type: "integer", nullable: true),
                    created_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    created_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    edited_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    edited_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    has_children = table.Column<bool>(type: "boolean", nullable: false),
                    financial_statement_type = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_chart_of_accounts", x => x.account_id);
                    table.ForeignKey(
                        name: "fk_chart_of_accounts_chart_of_accounts_parent_account_id",
                        column: x => x.parent_account_id,
                        principalTable: "chart_of_accounts",
                        principalColumn: "account_id");
                });

            migrationBuilder.CreateTable(
                name: "check_voucher_details",
                columns: table => new
                {
                    check_voucher_detail_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    account_no = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    account_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    transaction_no = table.Column<string>(type: "character varying(13)", maxLength: 13, nullable: false),
                    debit = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    credit = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    check_voucher_header_id = table.Column<int>(type: "integer", nullable: false),
                    amount = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    amount_paid = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    is_vatable = table.Column<bool>(type: "boolean", nullable: false),
                    ewt_percent = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    is_user_selected = table.Column<bool>(type: "boolean", nullable: false),
                    is_display_entry = table.Column<bool>(type: "boolean", nullable: false),
                    sub_account_type = table.Column<int>(type: "integer", nullable: true),
                    sub_account_id = table.Column<int>(type: "integer", nullable: true),
                    sub_account_name = table.Column<string>(type: "varchar(200)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_check_voucher_details", x => x.check_voucher_detail_id);
                    table.ForeignKey(
                        name: "fk_check_voucher_details_check_voucher_headers_check_voucher_h",
                        column: x => x.check_voucher_header_id,
                        principalTable: "check_voucher_headers",
                        principalColumn: "check_voucher_header_id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "cv_trade_payments",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    document_id = table.Column<int>(type: "integer", nullable: false),
                    document_type = table.Column<string>(type: "character varying(5)", maxLength: 5, nullable: false),
                    check_voucher_id = table.Column<int>(type: "integer", nullable: false),
                    amount_paid = table.Column<decimal>(type: "numeric(18,4)", nullable: false)
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
                    date = table.Column<DateOnly>(type: "date", nullable: false),
                    cv_no = table.Column<string>(type: "text", nullable: false),
                    payee = table.Column<string>(type: "text", nullable: false),
                    amount = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    particulars = table.Column<string>(type: "text", nullable: false),
                    bank = table.Column<string>(type: "text", nullable: false),
                    check_no = table.Column<string>(type: "text", nullable: false),
                    check_date = table.Column<string>(type: "text", nullable: false),
                    chart_of_account = table.Column<string>(type: "text", nullable: false),
                    debit = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    credit = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    created_by = table.Column<string>(type: "varchar(100)", nullable: true),
                    created_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    company = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_disbursement_books", x => x.disbursement_book_id);
                });

            migrationBuilder.CreateTable(
                name: "employees",
                columns: table => new
                {
                    employee_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    employee_number = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    initial = table.Column<string>(type: "character varying(5)", maxLength: 5, nullable: true),
                    first_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    middle_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    last_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    suffix = table.Column<string>(type: "character varying(5)", maxLength: 5, nullable: true),
                    address = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    birth_date = table.Column<DateOnly>(type: "date", nullable: true),
                    tel_no = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    sss_no = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    tin_no = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    philhealth_no = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    pagibig_no = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    company = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    department = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    date_hired = table.Column<DateOnly>(type: "date", nullable: false),
                    date_resigned = table.Column<DateOnly>(type: "date", nullable: true),
                    position = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    is_managerial = table.Column<bool>(type: "boolean", nullable: false),
                    supervisor = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    paygrade = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    salary = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_employees", x => x.employee_id);
                });

            migrationBuilder.CreateTable(
                name: "inventories",
                columns: table => new
                {
                    inventory_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    date = table.Column<DateOnly>(type: "date", nullable: false),
                    product_id = table.Column<int>(type: "integer", nullable: false),
                    particular = table.Column<string>(type: "varchar(200)", nullable: false),
                    reference = table.Column<string>(type: "varchar(12)", nullable: true),
                    quantity = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    cost = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    total = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    inventory_balance = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    average_cost = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    total_balance = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    unit = table.Column<string>(type: "varchar(2)", nullable: false),
                    is_validated = table.Column<bool>(type: "boolean", nullable: false),
                    validated_by = table.Column<string>(type: "varchar(100)", nullable: true),
                    validated_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    po_id = table.Column<int>(type: "integer", nullable: true),
                    company = table.Column<string>(type: "text", nullable: false)
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
                name: "journal_books",
                columns: table => new
                {
                    journal_book_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    date = table.Column<DateOnly>(type: "date", nullable: false),
                    reference = table.Column<string>(type: "text", nullable: false),
                    description = table.Column<string>(type: "text", nullable: false),
                    account_title = table.Column<string>(type: "text", nullable: false),
                    debit = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    credit = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    created_by = table.Column<string>(type: "varchar(100)", nullable: true),
                    created_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    company = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_journal_books", x => x.journal_book_id);
                });

            migrationBuilder.CreateTable(
                name: "journal_voucher_details",
                columns: table => new
                {
                    journal_voucher_detail_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    account_no = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    account_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    transaction_no = table.Column<string>(type: "character varying(13)", maxLength: 13, nullable: false),
                    debit = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    credit = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    journal_voucher_header_id = table.Column<int>(type: "integer", nullable: false),
                    sub_account_type = table.Column<int>(type: "integer", nullable: true),
                    sub_account_id = table.Column<int>(type: "integer", nullable: true),
                    sub_account_name = table.Column<string>(type: "varchar(200)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_journal_voucher_details", x => x.journal_voucher_detail_id);
                    table.ForeignKey(
                        name: "fk_journal_voucher_details_journal_voucher_headers_journal_vou",
                        column: x => x.journal_voucher_header_id,
                        principalTable: "journal_voucher_headers",
                        principalColumn: "journal_voucher_header_id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "monthly_nibits",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    month = table.Column<int>(type: "integer", nullable: false),
                    year = table.Column<int>(type: "integer", nullable: false),
                    beginning_balance = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    net_income = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    prior_period_adjustment = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    ending_balance = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    company = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    is_valid = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_monthly_nibits", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "multiple_check_voucher_payments",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    check_voucher_header_payment_id = table.Column<int>(type: "integer", nullable: false),
                    check_voucher_header_invoice_id = table.Column<int>(type: "integer", nullable: false),
                    amount_paid = table.Column<decimal>(type: "numeric(18,4)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_multiple_check_voucher_payments", x => x.id);
                    table.ForeignKey(
                        name: "fk_multiple_check_voucher_payments_check_voucher_headers_check",
                        column: x => x.check_voucher_header_invoice_id,
                        principalTable: "check_voucher_headers",
                        principalColumn: "check_voucher_header_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_multiple_check_voucher_payments_check_voucher_headers_check1",
                        column: x => x.check_voucher_header_payment_id,
                        principalTable: "check_voucher_headers",
                        principalColumn: "check_voucher_header_id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "po_actual_prices",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    purchase_order_id = table.Column<int>(type: "integer", nullable: false),
                    triggered_volume = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    applied_volume = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    triggered_price = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    is_approved = table.Column<bool>(type: "boolean", nullable: false),
                    approved_by = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    approved_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    triggered_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
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
                name: "purchase_books",
                columns: table => new
                {
                    purchase_book_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    date = table.Column<DateOnly>(type: "date", nullable: false),
                    supplier_name = table.Column<string>(type: "text", nullable: false),
                    supplier_tin = table.Column<string>(type: "text", nullable: false),
                    supplier_address = table.Column<string>(type: "text", nullable: false),
                    document_no = table.Column<string>(type: "text", nullable: false),
                    description = table.Column<string>(type: "text", nullable: false),
                    discount = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    vat_amount = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    amount = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    wht_amount = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    net_purchases = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    created_by = table.Column<string>(type: "varchar(100)", nullable: true),
                    created_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    po_no = table.Column<string>(type: "varchar(12)", nullable: false),
                    due_date = table.Column<DateOnly>(type: "date", nullable: false),
                    company = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_purchase_books", x => x.purchase_book_id);
                });

            migrationBuilder.CreateTable(
                name: "receiving_reports",
                columns: table => new
                {
                    receiving_report_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    receiving_report_no = table.Column<string>(type: "character varying(13)", maxLength: 13, nullable: true),
                    date = table.Column<DateOnly>(type: "date", nullable: false),
                    due_date = table.Column<DateOnly>(type: "date", nullable: false),
                    po_id = table.Column<int>(type: "integer", nullable: false),
                    po_no = table.Column<string>(type: "character varying(13)", maxLength: 13, nullable: true),
                    supplier_invoice_number = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    supplier_invoice_date = table.Column<DateOnly>(type: "date", nullable: true),
                    supplier_dr_no = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    withdrawal_certificate = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    truck_or_vessels = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    quantity_delivered = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    quantity_received = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    gain_or_loss = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    amount = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    authority_to_load_no = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    remarks = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    amount_paid = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    is_paid = table.Column<bool>(type: "boolean", nullable: false),
                    paid_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    canceled_quantity = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    company = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    is_printed = table.Column<bool>(type: "boolean", nullable: false),
                    received_date = table.Column<DateOnly>(type: "date", nullable: true),
                    delivery_receipt_id = table.Column<int>(type: "integer", nullable: true),
                    status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    type = table.Column<string>(type: "character varying(13)", maxLength: 13, nullable: true),
                    is_cost_updated = table.Column<bool>(type: "boolean", nullable: false),
                    old_rr_no = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    cost_based_on_soa = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    tax_percentage = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    created_by = table.Column<string>(type: "varchar(100)", nullable: true),
                    created_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    edited_by = table.Column<string>(type: "varchar(50)", nullable: true),
                    edited_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    cancellation_remarks = table.Column<string>(type: "varchar(255)", nullable: true),
                    canceled_by = table.Column<string>(type: "varchar(50)", nullable: true),
                    canceled_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    voided_by = table.Column<string>(type: "varchar(50)", nullable: true),
                    voided_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    posted_by = table.Column<string>(type: "varchar(50)", nullable: true),
                    posted_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
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
                name: "sales_books",
                columns: table => new
                {
                    sales_book_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    transaction_date = table.Column<DateOnly>(type: "date", nullable: false),
                    serial_no = table.Column<string>(type: "text", nullable: false),
                    sold_to = table.Column<string>(type: "text", nullable: false),
                    tin_no = table.Column<string>(type: "text", nullable: false),
                    address = table.Column<string>(type: "text", nullable: false),
                    description = table.Column<string>(type: "text", nullable: false),
                    amount = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    vat_amount = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    vatable_sales = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    vat_exempt_sales = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    zero_rated = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    discount = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    net_sales = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    created_by = table.Column<string>(type: "varchar(100)", nullable: true),
                    created_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    due_date = table.Column<DateOnly>(type: "date", nullable: false),
                    document_id = table.Column<int>(type: "integer", nullable: true),
                    company = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_sales_books", x => x.sales_book_id);
                });

            migrationBuilder.CreateTable(
                name: "services",
                columns: table => new
                {
                    service_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    service_no = table.Column<string>(type: "character varying(5)", maxLength: 5, nullable: true),
                    current_and_previous_no = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    current_and_previous_title = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    unearned_title = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    unearned_no = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    percent = table.Column<int>(type: "integer", nullable: false),
                    created_by = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    created_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    company = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    is_filpride = table.Column<bool>(type: "boolean", nullable: false),
                    is_bienes = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_services", x => x.service_id);
                });

            migrationBuilder.CreateTable(
                name: "suppliers",
                columns: table => new
                {
                    supplier_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    supplier_code = table.Column<string>(type: "character varying(7)", maxLength: 7, nullable: true),
                    supplier_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    supplier_address = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    supplier_tin = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    supplier_terms = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    vat_type = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    tax_type = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    proof_of_registration_file_path = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: true),
                    proof_of_registration_file_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    proof_of_exemption_file_path = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: true),
                    proof_of_exemption_file_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    created_by = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    created_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    edited_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    edited_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    category = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    trade_name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    branch = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    default_expense_number = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    withholding_tax_percent = table.Column<decimal>(type: "numeric(18,4)", nullable: true),
                    withholding_tax_title = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    reason_of_exemption = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    validity = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    validity_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    company = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    zip_code = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    is_filpride = table.Column<bool>(type: "boolean", nullable: false),
                    is_bienes = table.Column<bool>(type: "boolean", nullable: false),
                    requires_price_adjustment = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_suppliers", x => x.supplier_id);
                });

            migrationBuilder.CreateTable(
                name: "terms",
                columns: table => new
                {
                    terms_code = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    number_of_days = table.Column<int>(type: "integer", nullable: false),
                    number_of_months = table.Column<int>(type: "integer", nullable: false),
                    created_by = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    created_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    edited_by = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    edited_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_terms", x => x.terms_code);
                });

            migrationBuilder.CreateTable(
                name: "general_ledger_books",
                columns: table => new
                {
                    general_ledger_book_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    date = table.Column<DateOnly>(type: "date", nullable: false),
                    reference = table.Column<string>(type: "varchar(20)", nullable: false),
                    account_no = table.Column<string>(type: "varchar(50)", nullable: false),
                    account_title = table.Column<string>(type: "varchar(200)", nullable: false),
                    description = table.Column<string>(type: "text", nullable: false),
                    debit = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    credit = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    created_by = table.Column<string>(type: "varchar(100)", nullable: false),
                    created_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    is_posted = table.Column<bool>(type: "boolean", nullable: false),
                    company = table.Column<string>(type: "varchar(50)", nullable: false),
                    module_type = table.Column<string>(type: "varchar(50)", nullable: false),
                    account_id = table.Column<int>(type: "integer", nullable: false),
                    sub_account_type = table.Column<int>(type: "integer", nullable: true),
                    sub_account_id = table.Column<int>(type: "integer", nullable: true),
                    sub_account_name = table.Column<string>(type: "varchar(200)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_general_ledger_books", x => x.general_ledger_book_id);
                    table.ForeignKey(
                        name: "fk_general_ledger_books_chart_of_accounts_account_id",
                        column: x => x.account_id,
                        principalTable: "chart_of_accounts",
                        principalColumn: "account_id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "gl_period_balances",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    account_id = table.Column<int>(type: "integer", nullable: false),
                    period_start_date = table.Column<DateOnly>(type: "date", nullable: false),
                    period_end_date = table.Column<DateOnly>(type: "date", nullable: false),
                    fiscal_year = table.Column<int>(type: "integer", nullable: false),
                    fiscal_period = table.Column<int>(type: "integer", nullable: false),
                    beginning_balance = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    debit_total = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    credit_total = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    ending_balance = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    adjustment_debit_total = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    adjustment_credit_total = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    adjusted_ending_balance = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    is_closed = table.Column<bool>(type: "boolean", nullable: false),
                    closed_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    company = table.Column<string>(type: "varchar(50)", nullable: false),
                    is_valid = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_gl_period_balances", x => x.id);
                    table.ForeignKey(
                        name: "fk_gl_period_balances_chart_of_accounts_account_id",
                        column: x => x.account_id,
                        principalTable: "chart_of_accounts",
                        principalColumn: "account_id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "gl_sub_account_balances",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    account_id = table.Column<int>(type: "integer", nullable: false),
                    sub_account_type = table.Column<int>(type: "integer", nullable: false),
                    sub_account_id = table.Column<int>(type: "integer", nullable: false),
                    sub_account_name = table.Column<string>(type: "varchar(200)", nullable: false),
                    period_start_date = table.Column<DateOnly>(type: "date", nullable: false),
                    period_end_date = table.Column<DateOnly>(type: "date", nullable: false),
                    fiscal_year = table.Column<int>(type: "integer", nullable: false),
                    fiscal_period = table.Column<int>(type: "integer", nullable: false),
                    beginning_balance = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    debit_total = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    credit_total = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    ending_balance = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    is_closed = table.Column<bool>(type: "boolean", nullable: false),
                    company = table.Column<string>(type: "varchar(50)", nullable: false),
                    is_valid = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_gl_sub_account_balances", x => x.id);
                    table.ForeignKey(
                        name: "fk_gl_sub_account_balances_chart_of_accounts_account_id",
                        column: x => x.account_id,
                        principalTable: "chart_of_accounts",
                        principalColumn: "account_id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "provisional_receipts",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    series_number = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    transaction_date = table.Column<DateOnly>(type: "date", nullable: false),
                    employee_id = table.Column<int>(type: "integer", nullable: false),
                    reference_no = table.Column<string>(type: "text", nullable: false),
                    remarks = table.Column<string>(type: "text", nullable: false),
                    cash_amount = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    check_amount = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    check_date = table.Column<DateOnly>(type: "date", nullable: true),
                    check_no = table.Column<string>(type: "text", nullable: true),
                    check_bank = table.Column<string>(type: "text", nullable: true),
                    check_branch = table.Column<string>(type: "text", nullable: true),
                    managers_check_amount = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    managers_check_date = table.Column<DateOnly>(type: "date", nullable: true),
                    managers_check_no = table.Column<string>(type: "text", nullable: true),
                    managers_check_bank = table.Column<string>(type: "text", nullable: true),
                    managers_check_branch = table.Column<string>(type: "text", nullable: true),
                    bank_id = table.Column<int>(type: "integer", nullable: true),
                    bank_account_no = table.Column<string>(type: "text", nullable: true),
                    bank_account_name = table.Column<string>(type: "text", nullable: true),
                    ewt = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    wvat = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    total = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    company = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    is_printed = table.Column<bool>(type: "boolean", nullable: false),
                    status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    type = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    deposited_date = table.Column<DateOnly>(type: "date", nullable: true),
                    cleared_date = table.Column<DateOnly>(type: "date", nullable: true),
                    batch_number = table.Column<string>(type: "text", nullable: true),
                    created_by = table.Column<string>(type: "varchar(100)", nullable: true),
                    created_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    edited_by = table.Column<string>(type: "varchar(50)", nullable: true),
                    edited_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    cancellation_remarks = table.Column<string>(type: "varchar(255)", nullable: true),
                    canceled_by = table.Column<string>(type: "varchar(50)", nullable: true),
                    canceled_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    voided_by = table.Column<string>(type: "varchar(50)", nullable: true),
                    voided_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    posted_by = table.Column<string>(type: "varchar(50)", nullable: true),
                    posted_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_provisional_receipts", x => x.id);
                    table.ForeignKey(
                        name: "fk_provisional_receipts_bank_accounts_bank_id",
                        column: x => x.bank_id,
                        principalTable: "bank_accounts",
                        principalColumn: "bank_account_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_provisional_receipts_employees_employee_id",
                        column: x => x.employee_id,
                        principalTable: "employees",
                        principalColumn: "employee_id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "purchase_locked_records_queues",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    locked_date = table.Column<DateOnly>(type: "date", nullable: false),
                    receiving_report_id = table.Column<int>(type: "integer", nullable: false),
                    quantity = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    price = table.Column<decimal>(type: "numeric(18,4)", nullable: false)
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

            migrationBuilder.CreateTable(
                name: "authority_to_loads",
                columns: table => new
                {
                    authority_to_load_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    authority_to_load_no = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    customer_order_slip_id = table.Column<int>(type: "integer", nullable: true),
                    date_booked = table.Column<DateOnly>(type: "date", nullable: false),
                    valid_until = table.Column<DateOnly>(type: "date", nullable: false),
                    uppi_atl_no = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    remarks = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    created_by = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    created_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    supplier_id = table.Column<int>(type: "integer", nullable: false),
                    company = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    hauler_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    driver = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    plate_no = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    supplier_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    depot = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    load_port_id = table.Column<int>(type: "integer", nullable: false),
                    freight = table.Column<decimal>(type: "numeric(18,4)", nullable: false)
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
                    quantity = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    unserved_quantity = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    is_assigned_to_dr = table.Column<bool>(type: "boolean", nullable: false),
                    supplier_id = table.Column<int>(type: "integer", nullable: false),
                    atl_no = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    unreserved_quantity = table.Column<decimal>(type: "numeric(18,4)", nullable: false)
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
                name: "customers",
                columns: table => new
                {
                    customer_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    customer_code = table.Column<string>(type: "character varying(7)", maxLength: 7, nullable: true),
                    customer_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    customer_address = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    customer_tin = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    business_style = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    customer_terms = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    customer_type = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    vat_type = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    with_holding_vat = table.Column<bool>(type: "boolean", nullable: false),
                    with_holding_tax = table.Column<bool>(type: "boolean", nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    created_by = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    created_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    edited_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    edited_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    company = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    cluster_code = table.Column<int>(type: "integer", nullable: true),
                    station_code = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: true),
                    credit_limit = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    credit_limit_as_of_today = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    has_branch = table.Column<bool>(type: "boolean", nullable: false),
                    zip_code = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    retention_rate = table.Column<decimal>(type: "numeric(18,4)", nullable: true),
                    has_multiple_terms = table.Column<bool>(type: "boolean", nullable: false),
                    is_filpride = table.Column<bool>(type: "boolean", nullable: false),
                    is_bienes = table.Column<bool>(type: "boolean", nullable: false),
                    type = table.Column<string>(type: "character varying(13)", maxLength: 13, nullable: false),
                    requires_price_adjustment = table.Column<bool>(type: "boolean", nullable: false),
                    commissionee_id = table.Column<int>(type: "integer", nullable: true),
                    commission_rate = table.Column<decimal>(type: "numeric", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_customers", x => x.customer_id);
                    table.ForeignKey(
                        name: "fk_customers_suppliers_commissionee_id",
                        column: x => x.commissionee_id,
                        principalTable: "suppliers",
                        principalColumn: "supplier_id");
                });

            migrationBuilder.CreateTable(
                name: "pick_up_points",
                columns: table => new
                {
                    pick_up_point_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    depot = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    created_by = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    created_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    supplier_id = table.Column<int>(type: "integer", nullable: false),
                    company = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    is_filpride = table.Column<bool>(type: "boolean", nullable: false),
                    is_bienes = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_pick_up_points", x => x.pick_up_point_id);
                    table.ForeignKey(
                        name: "fk_pick_up_points_suppliers_supplier_id",
                        column: x => x.supplier_id,
                        principalTable: "suppliers",
                        principalColumn: "supplier_id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "book_atl_details",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    authority_to_load_id = table.Column<int>(type: "integer", nullable: false),
                    customer_order_slip_id = table.Column<int>(type: "integer", nullable: false),
                    quantity = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    unserved_quantity = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    appointed_id = table.Column<int>(type: "integer", nullable: true)
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
                name: "customer_branches",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    customer_id = table.Column<int>(type: "integer", nullable: false),
                    branch_name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    branch_address = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    branch_tin = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_customer_branches", x => x.id);
                    table.ForeignKey(
                        name: "fk_customer_branches_customers_customer_id",
                        column: x => x.customer_id,
                        principalTable: "customers",
                        principalColumn: "customer_id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "sales_invoices",
                columns: table => new
                {
                    sales_invoice_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    sales_invoice_no = table.Column<string>(type: "character varying(13)", maxLength: 13, nullable: true),
                    customer_id = table.Column<int>(type: "integer", nullable: false),
                    customer_address = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    customer_tin = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    product_id = table.Column<int>(type: "integer", nullable: false),
                    other_ref_no = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    quantity = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    unit_price = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    amount = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    remarks = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    payment_status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    transaction_date = table.Column<DateOnly>(type: "date", nullable: false),
                    discount = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    amount_paid = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    balance = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    is_paid = table.Column<bool>(type: "boolean", nullable: false),
                    is_tax_and_vat_paid = table.Column<bool>(type: "boolean", nullable: false),
                    due_date = table.Column<DateOnly>(type: "date", nullable: false),
                    purchase_order_id = table.Column<int>(type: "integer", nullable: false),
                    company = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    is_printed = table.Column<bool>(type: "boolean", nullable: false),
                    type = table.Column<string>(type: "character varying(13)", maxLength: 13, nullable: false),
                    receiving_report_id = table.Column<int>(type: "integer", nullable: false),
                    status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    delivery_receipt_id = table.Column<int>(type: "integer", nullable: true),
                    customer_order_slip_id = table.Column<int>(type: "integer", nullable: true),
                    terms = table.Column<string>(type: "character varying(15)", maxLength: 15, nullable: false),
                    created_by = table.Column<string>(type: "varchar(100)", nullable: true),
                    created_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    edited_by = table.Column<string>(type: "varchar(50)", nullable: true),
                    edited_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    cancellation_remarks = table.Column<string>(type: "varchar(255)", nullable: true),
                    canceled_by = table.Column<string>(type: "varchar(50)", nullable: true),
                    canceled_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    voided_by = table.Column<string>(type: "varchar(50)", nullable: true),
                    voided_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    posted_by = table.Column<string>(type: "varchar(50)", nullable: true),
                    posted_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
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
                name: "service_invoices",
                columns: table => new
                {
                    service_invoice_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    service_invoice_no = table.Column<string>(type: "character varying(13)", maxLength: 13, nullable: false),
                    customer_id = table.Column<int>(type: "integer", nullable: false),
                    customer_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    customer_address = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    customer_tin = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    customer_business_style = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    service_id = table.Column<int>(type: "integer", nullable: false),
                    service_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    service_percent = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    due_date = table.Column<DateOnly>(type: "date", nullable: false),
                    period = table.Column<DateOnly>(type: "date", nullable: false),
                    total = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    discount = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    current_and_previous_amount = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    unearned_amount = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    payment_status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    amount_paid = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    balance = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    instructions = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    is_paid = table.Column<bool>(type: "boolean", nullable: false),
                    company = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    is_printed = table.Column<bool>(type: "boolean", nullable: false),
                    status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    type = table.Column<string>(type: "character varying(13)", maxLength: 13, nullable: false),
                    vat_type = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    has_ewt = table.Column<bool>(type: "boolean", nullable: false),
                    has_wvat = table.Column<bool>(type: "boolean", nullable: false),
                    delivery_receipt_id = table.Column<int>(type: "integer", nullable: true),
                    created_by = table.Column<string>(type: "varchar(100)", nullable: true),
                    created_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    edited_by = table.Column<string>(type: "varchar(50)", nullable: true),
                    edited_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    cancellation_remarks = table.Column<string>(type: "varchar(255)", nullable: true),
                    canceled_by = table.Column<string>(type: "varchar(50)", nullable: true),
                    canceled_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    voided_by = table.Column<string>(type: "varchar(50)", nullable: true),
                    voided_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    posted_by = table.Column<string>(type: "varchar(50)", nullable: true),
                    posted_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_service_invoices", x => x.service_invoice_id);
                    table.ForeignKey(
                        name: "fk_service_invoices_customers_customer_id",
                        column: x => x.customer_id,
                        principalTable: "customers",
                        principalColumn: "customer_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_service_invoices_filpride_delivery_receipts_delivery_receip",
                        column: x => x.delivery_receipt_id,
                        principalTable: "filpride_delivery_receipts",
                        principalColumn: "delivery_receipt_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_service_invoices_services_service_id",
                        column: x => x.service_id,
                        principalTable: "services",
                        principalColumn: "service_id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "collection_receipts",
                columns: table => new
                {
                    collection_receipt_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    collection_receipt_no = table.Column<string>(type: "character varying(13)", maxLength: 13, nullable: true),
                    sales_invoice_id = table.Column<int>(type: "integer", nullable: true),
                    si_no = table.Column<string>(type: "character varying(13)", maxLength: 13, nullable: true),
                    multiple_si_id = table.Column<int[]>(type: "integer[]", nullable: true),
                    multiple_si = table.Column<string[]>(type: "text[]", nullable: true),
                    service_invoice_id = table.Column<int>(type: "integer", nullable: true),
                    sv_no = table.Column<string>(type: "character varying(13)", maxLength: 13, nullable: true),
                    customer_id = table.Column<int>(type: "integer", nullable: false),
                    transaction_date = table.Column<DateOnly>(type: "date", nullable: false),
                    reference_no = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    remarks = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    cash_amount = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    check_date = table.Column<DateOnly>(type: "date", nullable: true),
                    check_no = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    check_bank = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    check_branch = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    managers_check_date = table.Column<DateOnly>(type: "date", nullable: true),
                    managers_check_no = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    managers_check_bank = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    managers_check_branch = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    bank_id = table.Column<int>(type: "integer", nullable: true),
                    bank_account_name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    bank_account_number = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    check_amount = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    managers_check_amount = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    ewt = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    wvat = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    total = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    is_certificate_upload = table.Column<bool>(type: "boolean", nullable: false),
                    f2306file_path = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    f2306file_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    f2307file_path = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    f2307file_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    si_multiple_amount = table.Column<decimal[]>(type: "numeric[]", nullable: true),
                    company = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    is_printed = table.Column<bool>(type: "boolean", nullable: false),
                    multiple_transaction_date = table.Column<DateOnly[]>(type: "date[]", nullable: true),
                    status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    type = table.Column<string>(type: "character varying(13)", maxLength: 13, nullable: true),
                    deposited_date = table.Column<DateOnly>(type: "date", nullable: true),
                    cleared_date = table.Column<DateOnly>(type: "date", nullable: true),
                    batch_number = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    created_by = table.Column<string>(type: "varchar(100)", nullable: true),
                    created_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    edited_by = table.Column<string>(type: "varchar(50)", nullable: true),
                    edited_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    cancellation_remarks = table.Column<string>(type: "varchar(255)", nullable: true),
                    canceled_by = table.Column<string>(type: "varchar(50)", nullable: true),
                    canceled_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    voided_by = table.Column<string>(type: "varchar(50)", nullable: true),
                    voided_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    posted_by = table.Column<string>(type: "varchar(50)", nullable: true),
                    posted_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_collection_receipts", x => x.collection_receipt_id);
                    table.ForeignKey(
                        name: "fk_collection_receipts_bank_accounts_bank_id",
                        column: x => x.bank_id,
                        principalTable: "bank_accounts",
                        principalColumn: "bank_account_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_collection_receipts_customers_customer_id",
                        column: x => x.customer_id,
                        principalTable: "customers",
                        principalColumn: "customer_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_collection_receipts_sales_invoices_sales_invoice_id",
                        column: x => x.sales_invoice_id,
                        principalTable: "sales_invoices",
                        principalColumn: "sales_invoice_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_collection_receipts_service_invoices_service_invoice_id",
                        column: x => x.service_invoice_id,
                        principalTable: "service_invoices",
                        principalColumn: "service_invoice_id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "credit_memos",
                columns: table => new
                {
                    credit_memo_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    credit_memo_no = table.Column<string>(type: "character varying(13)", maxLength: 13, nullable: true),
                    transaction_date = table.Column<DateOnly>(type: "date", nullable: false),
                    sales_invoice_id = table.Column<int>(type: "integer", nullable: true),
                    service_invoice_id = table.Column<int>(type: "integer", nullable: true),
                    description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    adjusted_price = table.Column<decimal>(type: "numeric(18,4)", nullable: true),
                    quantity = table.Column<decimal>(type: "numeric(18,4)", nullable: true),
                    credit_amount = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    source = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    remarks = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    period = table.Column<DateOnly>(type: "date", nullable: false),
                    amount = table.Column<decimal>(type: "numeric(18,4)", nullable: true),
                    current_and_previous_amount = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    unearned_amount = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    company = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    is_printed = table.Column<bool>(type: "boolean", nullable: false),
                    status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    type = table.Column<string>(type: "character varying(13)", maxLength: 13, nullable: true),
                    created_by = table.Column<string>(type: "varchar(100)", nullable: true),
                    created_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    edited_by = table.Column<string>(type: "varchar(50)", nullable: true),
                    edited_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    cancellation_remarks = table.Column<string>(type: "varchar(255)", nullable: true),
                    canceled_by = table.Column<string>(type: "varchar(50)", nullable: true),
                    canceled_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    voided_by = table.Column<string>(type: "varchar(50)", nullable: true),
                    voided_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    posted_by = table.Column<string>(type: "varchar(50)", nullable: true),
                    posted_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_credit_memos", x => x.credit_memo_id);
                    table.ForeignKey(
                        name: "fk_credit_memos_sales_invoices_sales_invoice_id",
                        column: x => x.sales_invoice_id,
                        principalTable: "sales_invoices",
                        principalColumn: "sales_invoice_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_credit_memos_service_invoices_service_invoice_id",
                        column: x => x.service_invoice_id,
                        principalTable: "service_invoices",
                        principalColumn: "service_invoice_id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "debit_memos",
                columns: table => new
                {
                    debit_memo_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    sales_invoice_id = table.Column<int>(type: "integer", nullable: true),
                    service_invoice_id = table.Column<int>(type: "integer", nullable: true),
                    debit_memo_no = table.Column<string>(type: "character varying(13)", maxLength: 13, nullable: true),
                    transaction_date = table.Column<DateOnly>(type: "date", nullable: false),
                    debit_amount = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    adjusted_price = table.Column<decimal>(type: "numeric(18,4)", nullable: true),
                    quantity = table.Column<decimal>(type: "numeric(18,4)", nullable: true),
                    source = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    remarks = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    period = table.Column<DateOnly>(type: "date", nullable: false),
                    amount = table.Column<decimal>(type: "numeric(18,4)", nullable: true),
                    current_and_previous_amount = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    unearned_amount = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    company = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    is_printed = table.Column<bool>(type: "boolean", nullable: false),
                    status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    type = table.Column<string>(type: "character varying(13)", maxLength: 13, nullable: true),
                    created_by = table.Column<string>(type: "varchar(100)", nullable: true),
                    created_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    edited_by = table.Column<string>(type: "varchar(50)", nullable: true),
                    edited_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    cancellation_remarks = table.Column<string>(type: "varchar(255)", nullable: true),
                    canceled_by = table.Column<string>(type: "varchar(50)", nullable: true),
                    canceled_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    voided_by = table.Column<string>(type: "varchar(50)", nullable: true),
                    voided_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    posted_by = table.Column<string>(type: "varchar(50)", nullable: true),
                    posted_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_debit_memos", x => x.debit_memo_id);
                    table.ForeignKey(
                        name: "fk_debit_memos_sales_invoices_sales_invoice_id",
                        column: x => x.sales_invoice_id,
                        principalTable: "sales_invoices",
                        principalColumn: "sales_invoice_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_debit_memos_service_invoices_service_invoice_id",
                        column: x => x.service_invoice_id,
                        principalTable: "service_invoices",
                        principalColumn: "service_invoice_id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "collection_receipt_details",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    collection_receipt_id = table.Column<int>(type: "integer", nullable: false),
                    collection_receipt_no = table.Column<string>(type: "character varying(13)", maxLength: 13, nullable: false),
                    invoice_date = table.Column<DateOnly>(type: "date", nullable: false),
                    invoice_no = table.Column<string>(type: "character varying(13)", maxLength: 13, nullable: false),
                    amount = table.Column<decimal>(type: "numeric(18,4)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_collection_receipt_details", x => x.id);
                    table.ForeignKey(
                        name: "fk_collection_receipt_details_collection_receipts_collection_r",
                        column: x => x.collection_receipt_id,
                        principalTable: "collection_receipts",
                        principalColumn: "collection_receipt_id",
                        onDelete: ReferentialAction.Restrict);
                });

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
                name: "ix_chart_of_accounts_account_name",
                table: "chart_of_accounts",
                column: "account_name");

            migrationBuilder.CreateIndex(
                name: "ix_chart_of_accounts_account_number",
                table: "chart_of_accounts",
                column: "account_number",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_chart_of_accounts_parent_account_id",
                table: "chart_of_accounts",
                column: "parent_account_id");

            migrationBuilder.CreateIndex(
                name: "ix_check_voucher_details_check_voucher_header_id",
                table: "check_voucher_details",
                column: "check_voucher_header_id");

            migrationBuilder.CreateIndex(
                name: "ix_collection_receipt_details_collection_receipt_id",
                table: "collection_receipt_details",
                column: "collection_receipt_id");

            migrationBuilder.CreateIndex(
                name: "ix_collection_receipt_details_collection_receipt_no",
                table: "collection_receipt_details",
                column: "collection_receipt_no");

            migrationBuilder.CreateIndex(
                name: "ix_collection_receipt_details_invoice_no",
                table: "collection_receipt_details",
                column: "invoice_no");

            migrationBuilder.CreateIndex(
                name: "ix_collection_receipts_bank_id",
                table: "collection_receipts",
                column: "bank_id");

            migrationBuilder.CreateIndex(
                name: "ix_collection_receipts_collection_receipt_no_company",
                table: "collection_receipts",
                columns: new[] { "collection_receipt_no", "company" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_collection_receipts_customer_id",
                table: "collection_receipts",
                column: "customer_id");

            migrationBuilder.CreateIndex(
                name: "ix_collection_receipts_sales_invoice_id",
                table: "collection_receipts",
                column: "sales_invoice_id");

            migrationBuilder.CreateIndex(
                name: "ix_collection_receipts_service_invoice_id",
                table: "collection_receipts",
                column: "service_invoice_id");

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
                name: "ix_credit_memos_credit_memo_no_company",
                table: "credit_memos",
                columns: new[] { "credit_memo_no", "company" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_credit_memos_sales_invoice_id",
                table: "credit_memos",
                column: "sales_invoice_id");

            migrationBuilder.CreateIndex(
                name: "ix_credit_memos_service_invoice_id",
                table: "credit_memos",
                column: "service_invoice_id");

            migrationBuilder.CreateIndex(
                name: "ix_customer_branches_customer_id",
                table: "customer_branches",
                column: "customer_id");

            migrationBuilder.CreateIndex(
                name: "ix_customers_commissionee_id",
                table: "customers",
                column: "commissionee_id");

            migrationBuilder.CreateIndex(
                name: "ix_customers_customer_code",
                table: "customers",
                column: "customer_code");

            migrationBuilder.CreateIndex(
                name: "ix_customers_customer_name",
                table: "customers",
                column: "customer_name");

            migrationBuilder.CreateIndex(
                name: "ix_cv_trade_payments_check_voucher_id",
                table: "cv_trade_payments",
                column: "check_voucher_id");

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
                name: "ix_debit_memos_service_invoice_id",
                table: "debit_memos",
                column: "service_invoice_id");

            migrationBuilder.CreateIndex(
                name: "ix_employees_employee_number",
                table: "employees",
                column: "employee_number");

            migrationBuilder.CreateIndex(
                name: "ix_general_ledger_books_account_id",
                table: "general_ledger_books",
                column: "account_id");

            migrationBuilder.CreateIndex(
                name: "ix_gl_period_balances_account_id",
                table: "gl_period_balances",
                column: "account_id");

            migrationBuilder.CreateIndex(
                name: "ix_gl_sub_account_balances_account_id",
                table: "gl_sub_account_balances",
                column: "account_id");

            migrationBuilder.CreateIndex(
                name: "ix_inventories_po_id",
                table: "inventories",
                column: "po_id");

            migrationBuilder.CreateIndex(
                name: "ix_inventories_product_id",
                table: "inventories",
                column: "product_id");

            migrationBuilder.CreateIndex(
                name: "ix_journal_voucher_details_journal_voucher_header_id",
                table: "journal_voucher_details",
                column: "journal_voucher_header_id");

            migrationBuilder.CreateIndex(
                name: "ix_monthly_nibits_company",
                table: "monthly_nibits",
                column: "company");

            migrationBuilder.CreateIndex(
                name: "ix_monthly_nibits_month",
                table: "monthly_nibits",
                column: "month");

            migrationBuilder.CreateIndex(
                name: "ix_monthly_nibits_year",
                table: "monthly_nibits",
                column: "year");

            migrationBuilder.CreateIndex(
                name: "ix_multiple_check_voucher_payments_check_voucher_header_invoic",
                table: "multiple_check_voucher_payments",
                column: "check_voucher_header_invoice_id");

            migrationBuilder.CreateIndex(
                name: "ix_multiple_check_voucher_payments_check_voucher_header_paymen",
                table: "multiple_check_voucher_payments",
                column: "check_voucher_header_payment_id");

            migrationBuilder.CreateIndex(
                name: "ix_pick_up_points_company",
                table: "pick_up_points",
                column: "company");

            migrationBuilder.CreateIndex(
                name: "ix_pick_up_points_supplier_id",
                table: "pick_up_points",
                column: "supplier_id");

            migrationBuilder.CreateIndex(
                name: "ix_po_actual_prices_purchase_order_id",
                table: "po_actual_prices",
                column: "purchase_order_id");

            migrationBuilder.CreateIndex(
                name: "ix_provisional_receipts_bank_id",
                table: "provisional_receipts",
                column: "bank_id");

            migrationBuilder.CreateIndex(
                name: "ix_provisional_receipts_employee_id",
                table: "provisional_receipts",
                column: "employee_id");

            migrationBuilder.CreateIndex(
                name: "ix_provisional_receipts_series_number_company",
                table: "provisional_receipts",
                columns: new[] { "series_number", "company" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_purchase_locked_records_queues_locked_date",
                table: "purchase_locked_records_queues",
                column: "locked_date");

            migrationBuilder.CreateIndex(
                name: "ix_purchase_locked_records_queues_receiving_report_id",
                table: "purchase_locked_records_queues",
                column: "receiving_report_id");

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
                name: "ix_service_invoices_customer_id",
                table: "service_invoices",
                column: "customer_id");

            migrationBuilder.CreateIndex(
                name: "ix_service_invoices_delivery_receipt_id",
                table: "service_invoices",
                column: "delivery_receipt_id");

            migrationBuilder.CreateIndex(
                name: "ix_service_invoices_service_id",
                table: "service_invoices",
                column: "service_id");

            migrationBuilder.CreateIndex(
                name: "ix_service_invoices_service_invoice_no_company",
                table: "service_invoices",
                columns: new[] { "service_invoice_no", "company" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_suppliers_supplier_code",
                table: "suppliers",
                column: "supplier_code");

            migrationBuilder.CreateIndex(
                name: "ix_suppliers_supplier_name",
                table: "suppliers",
                column: "supplier_name");

            migrationBuilder.AddForeignKey(
                name: "fk_check_voucher_headers_bank_accounts_bank_id",
                table: "check_voucher_headers",
                column: "bank_id",
                principalTable: "bank_accounts",
                principalColumn: "bank_account_id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "fk_check_voucher_headers_employees_employee_id",
                table: "check_voucher_headers",
                column: "employee_id",
                principalTable: "employees",
                principalColumn: "employee_id");

            migrationBuilder.AddForeignKey(
                name: "fk_check_voucher_headers_suppliers_supplier_id",
                table: "check_voucher_headers",
                column: "supplier_id",
                principalTable: "suppliers",
                principalColumn: "supplier_id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "fk_filpride_customer_order_slips_customers_customer_id",
                table: "filpride_customer_order_slips",
                column: "customer_id",
                principalTable: "customers",
                principalColumn: "customer_id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "fk_filpride_customer_order_slips_pick_up_points_pick_up_point_",
                table: "filpride_customer_order_slips",
                column: "pick_up_point_id",
                principalTable: "pick_up_points",
                principalColumn: "pick_up_point_id");

            migrationBuilder.AddForeignKey(
                name: "fk_filpride_customer_order_slips_purchase_orders_purchase_orde",
                table: "filpride_customer_order_slips",
                column: "purchase_order_id",
                principalTable: "purchase_orders",
                principalColumn: "purchase_order_id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "fk_filpride_customer_order_slips_suppliers_commissionee_id",
                table: "filpride_customer_order_slips",
                column: "commissionee_id",
                principalTable: "suppliers",
                principalColumn: "supplier_id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "fk_filpride_customer_order_slips_suppliers_hauler_id",
                table: "filpride_customer_order_slips",
                column: "hauler_id",
                principalTable: "suppliers",
                principalColumn: "supplier_id");

            migrationBuilder.AddForeignKey(
                name: "fk_filpride_customer_order_slips_suppliers_supplier_id",
                table: "filpride_customer_order_slips",
                column: "supplier_id",
                principalTable: "suppliers",
                principalColumn: "supplier_id");

            migrationBuilder.AddForeignKey(
                name: "fk_filpride_delivery_receipts_authority_to_loads_authority_to_",
                table: "filpride_delivery_receipts",
                column: "authority_to_load_id",
                principalTable: "authority_to_loads",
                principalColumn: "authority_to_load_id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "fk_filpride_delivery_receipts_customers_customer_id",
                table: "filpride_delivery_receipts",
                column: "customer_id",
                principalTable: "customers",
                principalColumn: "customer_id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "fk_filpride_delivery_receipts_purchase_orders_purchase_order_id",
                table: "filpride_delivery_receipts",
                column: "purchase_order_id",
                principalTable: "purchase_orders",
                principalColumn: "purchase_order_id");

            migrationBuilder.AddForeignKey(
                name: "fk_filpride_delivery_receipts_suppliers_commissionee_id",
                table: "filpride_delivery_receipts",
                column: "commissionee_id",
                principalTable: "suppliers",
                principalColumn: "supplier_id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "fk_filpride_delivery_receipts_suppliers_hauler_id",
                table: "filpride_delivery_receipts",
                column: "hauler_id",
                principalTable: "suppliers",
                principalColumn: "supplier_id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "fk_journal_voucher_headers_check_voucher_headers_cv_id",
                table: "journal_voucher_headers",
                column: "cv_id",
                principalTable: "check_voucher_headers",
                principalColumn: "check_voucher_header_id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "fk_jv_amortization_settings_journal_voucher_headers_jv_id",
                table: "jv_amortization_settings",
                column: "jv_id",
                principalTable: "journal_voucher_headers",
                principalColumn: "journal_voucher_header_id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_purchase_orders_customers_customer_id",
                table: "purchase_orders",
                column: "customer_id",
                principalTable: "customers",
                principalColumn: "customer_id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "fk_purchase_orders_pick_up_points_pick_up_point_id",
                table: "purchase_orders",
                column: "pick_up_point_id",
                principalTable: "pick_up_points",
                principalColumn: "pick_up_point_id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "fk_purchase_orders_products_product_id",
                table: "purchase_orders",
                column: "product_id",
                principalTable: "products",
                principalColumn: "product_id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "fk_purchase_orders_suppliers_supplier_id",
                table: "purchase_orders",
                column: "supplier_id",
                principalTable: "suppliers",
                principalColumn: "supplier_id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "fk_sales_locked_records_queues_filpride_delivery_receipts_deli",
                table: "sales_locked_records_queues",
                column: "delivery_receipt_id",
                principalTable: "filpride_delivery_receipts",
                principalColumn: "delivery_receipt_id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_check_voucher_headers_bank_accounts_bank_id",
                table: "check_voucher_headers");

            migrationBuilder.DropForeignKey(
                name: "fk_check_voucher_headers_employees_employee_id",
                table: "check_voucher_headers");

            migrationBuilder.DropForeignKey(
                name: "fk_check_voucher_headers_suppliers_supplier_id",
                table: "check_voucher_headers");

            migrationBuilder.DropForeignKey(
                name: "fk_filpride_customer_order_slips_customers_customer_id",
                table: "filpride_customer_order_slips");

            migrationBuilder.DropForeignKey(
                name: "fk_filpride_customer_order_slips_pick_up_points_pick_up_point_",
                table: "filpride_customer_order_slips");

            migrationBuilder.DropForeignKey(
                name: "fk_filpride_customer_order_slips_purchase_orders_purchase_orde",
                table: "filpride_customer_order_slips");

            migrationBuilder.DropForeignKey(
                name: "fk_filpride_customer_order_slips_suppliers_commissionee_id",
                table: "filpride_customer_order_slips");

            migrationBuilder.DropForeignKey(
                name: "fk_filpride_customer_order_slips_suppliers_hauler_id",
                table: "filpride_customer_order_slips");

            migrationBuilder.DropForeignKey(
                name: "fk_filpride_customer_order_slips_suppliers_supplier_id",
                table: "filpride_customer_order_slips");

            migrationBuilder.DropForeignKey(
                name: "fk_filpride_delivery_receipts_authority_to_loads_authority_to_",
                table: "filpride_delivery_receipts");

            migrationBuilder.DropForeignKey(
                name: "fk_filpride_delivery_receipts_customers_customer_id",
                table: "filpride_delivery_receipts");

            migrationBuilder.DropForeignKey(
                name: "fk_filpride_delivery_receipts_purchase_orders_purchase_order_id",
                table: "filpride_delivery_receipts");

            migrationBuilder.DropForeignKey(
                name: "fk_filpride_delivery_receipts_suppliers_commissionee_id",
                table: "filpride_delivery_receipts");

            migrationBuilder.DropForeignKey(
                name: "fk_filpride_delivery_receipts_suppliers_hauler_id",
                table: "filpride_delivery_receipts");

            migrationBuilder.DropForeignKey(
                name: "fk_journal_voucher_headers_check_voucher_headers_cv_id",
                table: "journal_voucher_headers");

            migrationBuilder.DropForeignKey(
                name: "fk_jv_amortization_settings_journal_voucher_headers_jv_id",
                table: "jv_amortization_settings");

            migrationBuilder.DropForeignKey(
                name: "fk_purchase_orders_customers_customer_id",
                table: "purchase_orders");

            migrationBuilder.DropForeignKey(
                name: "fk_purchase_orders_pick_up_points_pick_up_point_id",
                table: "purchase_orders");

            migrationBuilder.DropForeignKey(
                name: "fk_purchase_orders_products_product_id",
                table: "purchase_orders");

            migrationBuilder.DropForeignKey(
                name: "fk_purchase_orders_suppliers_supplier_id",
                table: "purchase_orders");

            migrationBuilder.DropForeignKey(
                name: "fk_sales_locked_records_queues_filpride_delivery_receipts_deli",
                table: "sales_locked_records_queues");

            migrationBuilder.DropTable(
                name: "audit_trails");

            migrationBuilder.DropTable(
                name: "book_atl_details");

            migrationBuilder.DropTable(
                name: "cash_receipt_books");

            migrationBuilder.DropTable(
                name: "check_voucher_details");

            migrationBuilder.DropTable(
                name: "collection_receipt_details");

            migrationBuilder.DropTable(
                name: "credit_memos");

            migrationBuilder.DropTable(
                name: "customer_branches");

            migrationBuilder.DropTable(
                name: "cv_trade_payments");

            migrationBuilder.DropTable(
                name: "debit_memos");

            migrationBuilder.DropTable(
                name: "disbursement_books");

            migrationBuilder.DropTable(
                name: "general_ledger_books");

            migrationBuilder.DropTable(
                name: "gl_period_balances");

            migrationBuilder.DropTable(
                name: "gl_sub_account_balances");

            migrationBuilder.DropTable(
                name: "inventories");

            migrationBuilder.DropTable(
                name: "journal_books");

            migrationBuilder.DropTable(
                name: "journal_voucher_details");

            migrationBuilder.DropTable(
                name: "monthly_nibits");

            migrationBuilder.DropTable(
                name: "multiple_check_voucher_payments");

            migrationBuilder.DropTable(
                name: "pick_up_points");

            migrationBuilder.DropTable(
                name: "po_actual_prices");

            migrationBuilder.DropTable(
                name: "provisional_receipts");

            migrationBuilder.DropTable(
                name: "purchase_books");

            migrationBuilder.DropTable(
                name: "purchase_locked_records_queues");

            migrationBuilder.DropTable(
                name: "sales_books");

            migrationBuilder.DropTable(
                name: "terms");

            migrationBuilder.DropTable(
                name: "authority_to_loads");

            migrationBuilder.DropTable(
                name: "cos_appointed_suppliers");

            migrationBuilder.DropTable(
                name: "collection_receipts");

            migrationBuilder.DropTable(
                name: "chart_of_accounts");

            migrationBuilder.DropTable(
                name: "employees");

            migrationBuilder.DropTable(
                name: "receiving_reports");

            migrationBuilder.DropTable(
                name: "bank_accounts");

            migrationBuilder.DropTable(
                name: "sales_invoices");

            migrationBuilder.DropTable(
                name: "service_invoices");

            migrationBuilder.DropTable(
                name: "customers");

            migrationBuilder.DropTable(
                name: "services");

            migrationBuilder.DropTable(
                name: "suppliers");

            migrationBuilder.DropPrimaryKey(
                name: "pk_sales_locked_records_queues",
                table: "sales_locked_records_queues");

            migrationBuilder.DropPrimaryKey(
                name: "pk_purchase_orders",
                table: "purchase_orders");

            migrationBuilder.DropPrimaryKey(
                name: "pk_journal_voucher_headers",
                table: "journal_voucher_headers");

            migrationBuilder.DropPrimaryKey(
                name: "pk_check_voucher_headers",
                table: "check_voucher_headers");

            migrationBuilder.RenameTable(
                name: "sales_locked_records_queues",
                newName: "filpride_sales_locked_records_queues");

            migrationBuilder.RenameTable(
                name: "purchase_orders",
                newName: "filpride_purchase_orders");

            migrationBuilder.RenameTable(
                name: "journal_voucher_headers",
                newName: "filpride_journal_voucher_headers");

            migrationBuilder.RenameTable(
                name: "check_voucher_headers",
                newName: "filpride_check_voucher_headers");

            migrationBuilder.RenameIndex(
                name: "ix_sales_locked_records_queues_locked_date",
                table: "filpride_sales_locked_records_queues",
                newName: "ix_filpride_sales_locked_records_queues_locked_date");

            migrationBuilder.RenameIndex(
                name: "ix_sales_locked_records_queues_delivery_receipt_id",
                table: "filpride_sales_locked_records_queues",
                newName: "ix_filpride_sales_locked_records_queues_delivery_receipt_id");

            migrationBuilder.RenameIndex(
                name: "ix_purchase_orders_supplier_id",
                table: "filpride_purchase_orders",
                newName: "ix_filpride_purchase_orders_supplier_id");

            migrationBuilder.RenameIndex(
                name: "ix_purchase_orders_purchase_order_no_company",
                table: "filpride_purchase_orders",
                newName: "ix_filpride_purchase_orders_purchase_order_no_company");

            migrationBuilder.RenameIndex(
                name: "ix_purchase_orders_product_id",
                table: "filpride_purchase_orders",
                newName: "ix_filpride_purchase_orders_product_id");

            migrationBuilder.RenameIndex(
                name: "ix_purchase_orders_pick_up_point_id",
                table: "filpride_purchase_orders",
                newName: "ix_filpride_purchase_orders_pick_up_point_id");

            migrationBuilder.RenameIndex(
                name: "ix_purchase_orders_customer_id",
                table: "filpride_purchase_orders",
                newName: "ix_filpride_purchase_orders_customer_id");

            migrationBuilder.RenameIndex(
                name: "ix_journal_voucher_headers_journal_voucher_header_no_company",
                table: "filpride_journal_voucher_headers",
                newName: "ix_filpride_journal_voucher_headers_journal_voucher_header_no_");

            migrationBuilder.RenameIndex(
                name: "ix_journal_voucher_headers_cv_id",
                table: "filpride_journal_voucher_headers",
                newName: "ix_filpride_journal_voucher_headers_cv_id");

            migrationBuilder.RenameIndex(
                name: "ix_check_voucher_headers_supplier_id",
                table: "filpride_check_voucher_headers",
                newName: "ix_filpride_check_voucher_headers_supplier_id");

            migrationBuilder.RenameIndex(
                name: "ix_check_voucher_headers_employee_id",
                table: "filpride_check_voucher_headers",
                newName: "ix_filpride_check_voucher_headers_employee_id");

            migrationBuilder.RenameIndex(
                name: "ix_check_voucher_headers_check_voucher_header_no_company",
                table: "filpride_check_voucher_headers",
                newName: "ix_filpride_check_voucher_headers_check_voucher_header_no_comp");

            migrationBuilder.RenameIndex(
                name: "ix_check_voucher_headers_bank_id",
                table: "filpride_check_voucher_headers",
                newName: "ix_filpride_check_voucher_headers_bank_id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_filpride_sales_locked_records_queues",
                table: "filpride_sales_locked_records_queues",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_filpride_purchase_orders",
                table: "filpride_purchase_orders",
                column: "purchase_order_id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_filpride_journal_voucher_headers",
                table: "filpride_journal_voucher_headers",
                column: "journal_voucher_header_id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_filpride_check_voucher_headers",
                table: "filpride_check_voucher_headers",
                column: "check_voucher_header_id");

            migrationBuilder.CreateTable(
                name: "filpride_audit_trails",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    activity = table.Column<string>(type: "text", nullable: false),
                    company = table.Column<string>(type: "text", nullable: false),
                    date = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    document_type = table.Column<string>(type: "text", nullable: false),
                    machine_name = table.Column<string>(type: "text", nullable: false),
                    username = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_filpride_audit_trails", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "filpride_bank_accounts",
                columns: table => new
                {
                    bank_account_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    account_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    account_no = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    bank = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    branch = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    company = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    created_by = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    created_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    is_bienes = table.Column<bool>(type: "boolean", nullable: false),
                    is_filpride = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_filpride_bank_accounts", x => x.bank_account_id);
                });

            migrationBuilder.CreateTable(
                name: "filpride_cash_receipt_books",
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
                    table.PrimaryKey("pk_filpride_cash_receipt_books", x => x.cash_receipt_book_id);
                });

            migrationBuilder.CreateTable(
                name: "filpride_chart_of_accounts",
                columns: table => new
                {
                    account_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    parent_account_id = table.Column<int>(type: "integer", nullable: true),
                    account_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    account_number = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    account_type = table.Column<string>(type: "character varying(25)", maxLength: 25, nullable: true),
                    created_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    created_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    edited_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    edited_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    financial_statement_type = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    has_children = table.Column<bool>(type: "boolean", nullable: false),
                    is_main = table.Column<bool>(type: "boolean", nullable: false),
                    level = table.Column<int>(type: "integer", nullable: false),
                    normal_balance = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_filpride_chart_of_accounts", x => x.account_id);
                    table.ForeignKey(
                        name: "fk_filpride_chart_of_accounts_filpride_chart_of_accounts_paren",
                        column: x => x.parent_account_id,
                        principalTable: "filpride_chart_of_accounts",
                        principalColumn: "account_id");
                });

            migrationBuilder.CreateTable(
                name: "filpride_check_voucher_details",
                columns: table => new
                {
                    check_voucher_detail_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    check_voucher_header_id = table.Column<int>(type: "integer", nullable: false),
                    account_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    account_no = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    amount = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    amount_paid = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    credit = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    debit = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    ewt_percent = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    is_display_entry = table.Column<bool>(type: "boolean", nullable: false),
                    is_user_selected = table.Column<bool>(type: "boolean", nullable: false),
                    is_vatable = table.Column<bool>(type: "boolean", nullable: false),
                    sub_account_id = table.Column<int>(type: "integer", nullable: true),
                    sub_account_name = table.Column<string>(type: "varchar(200)", nullable: true),
                    sub_account_type = table.Column<int>(type: "integer", nullable: true),
                    transaction_no = table.Column<string>(type: "character varying(13)", maxLength: 13, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_filpride_check_voucher_details", x => x.check_voucher_detail_id);
                    table.ForeignKey(
                        name: "fk_filpride_check_voucher_details_filpride_check_voucher_heade",
                        column: x => x.check_voucher_header_id,
                        principalTable: "filpride_check_voucher_headers",
                        principalColumn: "check_voucher_header_id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "filpride_cv_trade_payments",
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
                    table.PrimaryKey("pk_filpride_cv_trade_payments", x => x.id);
                    table.ForeignKey(
                        name: "fk_filpride_cv_trade_payments_filpride_check_voucher_headers_c",
                        column: x => x.check_voucher_id,
                        principalTable: "filpride_check_voucher_headers",
                        principalColumn: "check_voucher_header_id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "filpride_disbursement_books",
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
                    table.PrimaryKey("pk_filpride_disbursement_books", x => x.disbursement_book_id);
                });

            migrationBuilder.CreateTable(
                name: "filpride_employees",
                columns: table => new
                {
                    employee_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    address = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    birth_date = table.Column<DateOnly>(type: "date", nullable: true),
                    company = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    date_hired = table.Column<DateOnly>(type: "date", nullable: false),
                    date_resigned = table.Column<DateOnly>(type: "date", nullable: true),
                    department = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    employee_number = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    first_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    initial = table.Column<string>(type: "character varying(5)", maxLength: 5, nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    is_managerial = table.Column<bool>(type: "boolean", nullable: false),
                    last_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    middle_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    pagibig_no = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    paygrade = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    philhealth_no = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    position = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    salary = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    sss_no = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    suffix = table.Column<string>(type: "character varying(5)", maxLength: 5, nullable: true),
                    supervisor = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    tel_no = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    tin_no = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_filpride_employees", x => x.employee_id);
                });

            migrationBuilder.CreateTable(
                name: "filpride_inventories",
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
                    table.PrimaryKey("pk_filpride_inventories", x => x.inventory_id);
                    table.ForeignKey(
                        name: "fk_filpride_inventories_filpride_purchase_orders_po_id",
                        column: x => x.po_id,
                        principalTable: "filpride_purchase_orders",
                        principalColumn: "purchase_order_id");
                    table.ForeignKey(
                        name: "fk_filpride_inventories_products_product_id",
                        column: x => x.product_id,
                        principalTable: "products",
                        principalColumn: "product_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "filpride_journal_books",
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
                    table.PrimaryKey("pk_filpride_journal_books", x => x.journal_book_id);
                });

            migrationBuilder.CreateTable(
                name: "filpride_journal_voucher_details",
                columns: table => new
                {
                    journal_voucher_detail_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    journal_voucher_header_id = table.Column<int>(type: "integer", nullable: false),
                    account_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    account_no = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    credit = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    debit = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    sub_account_id = table.Column<int>(type: "integer", nullable: true),
                    sub_account_name = table.Column<string>(type: "varchar(200)", nullable: true),
                    sub_account_type = table.Column<int>(type: "integer", nullable: true),
                    transaction_no = table.Column<string>(type: "character varying(13)", maxLength: 13, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_filpride_journal_voucher_details", x => x.journal_voucher_detail_id);
                    table.ForeignKey(
                        name: "fk_filpride_journal_voucher_details_filpride_journal_voucher_h",
                        column: x => x.journal_voucher_header_id,
                        principalTable: "filpride_journal_voucher_headers",
                        principalColumn: "journal_voucher_header_id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "filpride_monthly_nibits",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    beginning_balance = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    company = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    ending_balance = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    is_valid = table.Column<bool>(type: "boolean", nullable: false),
                    month = table.Column<int>(type: "integer", nullable: false),
                    net_income = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    prior_period_adjustment = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    year = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_filpride_monthly_nibits", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "filpride_multiple_check_voucher_payments",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    check_voucher_header_invoice_id = table.Column<int>(type: "integer", nullable: false),
                    check_voucher_header_payment_id = table.Column<int>(type: "integer", nullable: false),
                    amount_paid = table.Column<decimal>(type: "numeric(18,4)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_filpride_multiple_check_voucher_payments", x => x.id);
                    table.ForeignKey(
                        name: "fk_filpride_multiple_check_voucher_payments_filpride_check_vou",
                        column: x => x.check_voucher_header_invoice_id,
                        principalTable: "filpride_check_voucher_headers",
                        principalColumn: "check_voucher_header_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_filpride_multiple_check_voucher_payments_filpride_check_vou1",
                        column: x => x.check_voucher_header_payment_id,
                        principalTable: "filpride_check_voucher_headers",
                        principalColumn: "check_voucher_header_id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "filpride_offsettings",
                columns: table => new
                {
                    off_setting_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    account_no = table.Column<string>(type: "text", nullable: false),
                    account_title = table.Column<string>(type: "text", nullable: false),
                    amount = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    company = table.Column<string>(type: "text", nullable: false),
                    created_by = table.Column<string>(type: "varchar(50)", nullable: true),
                    created_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    is_removed = table.Column<bool>(type: "boolean", nullable: false),
                    reference = table.Column<string>(type: "text", nullable: true),
                    source = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_filpride_offsettings", x => x.off_setting_id);
                });

            migrationBuilder.CreateTable(
                name: "filpride_po_actual_prices",
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
                    table.PrimaryKey("pk_filpride_po_actual_prices", x => x.id);
                    table.ForeignKey(
                        name: "fk_filpride_po_actual_prices_filpride_purchase_orders_purchase",
                        column: x => x.purchase_order_id,
                        principalTable: "filpride_purchase_orders",
                        principalColumn: "purchase_order_id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "filpride_purchase_books",
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
                    table.PrimaryKey("pk_filpride_purchase_books", x => x.purchase_book_id);
                });

            migrationBuilder.CreateTable(
                name: "filpride_receiving_reports",
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
                    table.PrimaryKey("pk_filpride_receiving_reports", x => x.receiving_report_id);
                    table.ForeignKey(
                        name: "fk_filpride_receiving_reports_filpride_delivery_receipts_deliv",
                        column: x => x.delivery_receipt_id,
                        principalTable: "filpride_delivery_receipts",
                        principalColumn: "delivery_receipt_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_filpride_receiving_reports_filpride_purchase_orders_po_id",
                        column: x => x.po_id,
                        principalTable: "filpride_purchase_orders",
                        principalColumn: "purchase_order_id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "filpride_sales_books",
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
                    table.PrimaryKey("pk_filpride_sales_books", x => x.sales_book_id);
                });

            migrationBuilder.CreateTable(
                name: "filpride_services",
                columns: table => new
                {
                    service_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    company = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    created_by = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    created_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    current_and_previous_no = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    current_and_previous_title = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    is_bienes = table.Column<bool>(type: "boolean", nullable: false),
                    is_filpride = table.Column<bool>(type: "boolean", nullable: false),
                    name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    percent = table.Column<int>(type: "integer", nullable: false),
                    service_no = table.Column<string>(type: "character varying(5)", maxLength: 5, nullable: true),
                    unearned_no = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    unearned_title = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_filpride_services", x => x.service_id);
                });

            migrationBuilder.CreateTable(
                name: "filpride_suppliers",
                columns: table => new
                {
                    supplier_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    branch = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    category = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    company = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    created_by = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    created_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    default_expense_number = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    edited_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    edited_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    is_bienes = table.Column<bool>(type: "boolean", nullable: false),
                    is_filpride = table.Column<bool>(type: "boolean", nullable: false),
                    proof_of_exemption_file_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    proof_of_exemption_file_path = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: true),
                    proof_of_registration_file_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    proof_of_registration_file_path = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: true),
                    reason_of_exemption = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    requires_price_adjustment = table.Column<bool>(type: "boolean", nullable: false),
                    supplier_address = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    supplier_code = table.Column<string>(type: "character varying(7)", maxLength: 7, nullable: true),
                    supplier_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    supplier_terms = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    supplier_tin = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    tax_type = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    trade_name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    validity = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    validity_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    vat_type = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    withholding_tax_percent = table.Column<decimal>(type: "numeric(18,4)", nullable: true),
                    withholding_tax_title = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    zip_code = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_filpride_suppliers", x => x.supplier_id);
                });

            migrationBuilder.CreateTable(
                name: "filpride_terms",
                columns: table => new
                {
                    terms_code = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    created_by = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    created_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    edited_by = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    edited_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    number_of_days = table.Column<int>(type: "integer", nullable: false),
                    number_of_months = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_filpride_terms", x => x.terms_code);
                });

            migrationBuilder.CreateTable(
                name: "filpride_general_ledger_books",
                columns: table => new
                {
                    general_ledger_book_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    account_id = table.Column<int>(type: "integer", nullable: false),
                    account_no = table.Column<string>(type: "varchar(50)", nullable: false),
                    account_title = table.Column<string>(type: "varchar(200)", nullable: false),
                    company = table.Column<string>(type: "varchar(50)", nullable: false),
                    created_by = table.Column<string>(type: "varchar(100)", nullable: false),
                    created_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    credit = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    date = table.Column<DateOnly>(type: "date", nullable: false),
                    debit = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    description = table.Column<string>(type: "text", nullable: false),
                    is_posted = table.Column<bool>(type: "boolean", nullable: false),
                    module_type = table.Column<string>(type: "varchar(50)", nullable: false),
                    reference = table.Column<string>(type: "varchar(20)", nullable: false),
                    sub_account_id = table.Column<int>(type: "integer", nullable: true),
                    sub_account_name = table.Column<string>(type: "varchar(200)", nullable: true),
                    sub_account_type = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_filpride_general_ledger_books", x => x.general_ledger_book_id);
                    table.ForeignKey(
                        name: "fk_filpride_general_ledger_books_filpride_chart_of_accounts_ac",
                        column: x => x.account_id,
                        principalTable: "filpride_chart_of_accounts",
                        principalColumn: "account_id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "filpride_gl_period_balances",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    account_id = table.Column<int>(type: "integer", nullable: false),
                    adjusted_ending_balance = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    adjustment_credit_total = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    adjustment_debit_total = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    beginning_balance = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    closed_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    company = table.Column<string>(type: "varchar(50)", nullable: false),
                    credit_total = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    debit_total = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    ending_balance = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    fiscal_period = table.Column<int>(type: "integer", nullable: false),
                    fiscal_year = table.Column<int>(type: "integer", nullable: false),
                    is_closed = table.Column<bool>(type: "boolean", nullable: false),
                    is_valid = table.Column<bool>(type: "boolean", nullable: false),
                    period_end_date = table.Column<DateOnly>(type: "date", nullable: false),
                    period_start_date = table.Column<DateOnly>(type: "date", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_filpride_gl_period_balances", x => x.id);
                    table.ForeignKey(
                        name: "fk_filpride_gl_period_balances_filpride_chart_of_accounts_acco",
                        column: x => x.account_id,
                        principalTable: "filpride_chart_of_accounts",
                        principalColumn: "account_id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "filpride_gl_sub_account_balances",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    account_id = table.Column<int>(type: "integer", nullable: false),
                    beginning_balance = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    company = table.Column<string>(type: "varchar(50)", nullable: false),
                    credit_total = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    debit_total = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    ending_balance = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    fiscal_period = table.Column<int>(type: "integer", nullable: false),
                    fiscal_year = table.Column<int>(type: "integer", nullable: false),
                    is_closed = table.Column<bool>(type: "boolean", nullable: false),
                    is_valid = table.Column<bool>(type: "boolean", nullable: false),
                    period_end_date = table.Column<DateOnly>(type: "date", nullable: false),
                    period_start_date = table.Column<DateOnly>(type: "date", nullable: false),
                    sub_account_id = table.Column<int>(type: "integer", nullable: false),
                    sub_account_name = table.Column<string>(type: "varchar(200)", nullable: false),
                    sub_account_type = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_filpride_gl_sub_account_balances", x => x.id);
                    table.ForeignKey(
                        name: "fk_filpride_gl_sub_account_balances_filpride_chart_of_accounts",
                        column: x => x.account_id,
                        principalTable: "filpride_chart_of_accounts",
                        principalColumn: "account_id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "filpride_provisional_receipts",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    bank_id = table.Column<int>(type: "integer", nullable: true),
                    employee_id = table.Column<int>(type: "integer", nullable: false),
                    bank_account_name = table.Column<string>(type: "text", nullable: true),
                    bank_account_no = table.Column<string>(type: "text", nullable: true),
                    batch_number = table.Column<string>(type: "text", nullable: true),
                    canceled_by = table.Column<string>(type: "varchar(50)", nullable: true),
                    canceled_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    cancellation_remarks = table.Column<string>(type: "varchar(255)", nullable: true),
                    cash_amount = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    check_amount = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    check_bank = table.Column<string>(type: "text", nullable: true),
                    check_branch = table.Column<string>(type: "text", nullable: true),
                    check_date = table.Column<DateOnly>(type: "date", nullable: true),
                    check_no = table.Column<string>(type: "text", nullable: true),
                    cleared_date = table.Column<DateOnly>(type: "date", nullable: true),
                    company = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    created_by = table.Column<string>(type: "varchar(100)", nullable: true),
                    created_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    deposited_date = table.Column<DateOnly>(type: "date", nullable: true),
                    ewt = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    edited_by = table.Column<string>(type: "varchar(50)", nullable: true),
                    edited_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    is_printed = table.Column<bool>(type: "boolean", nullable: false),
                    managers_check_amount = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    managers_check_bank = table.Column<string>(type: "text", nullable: true),
                    managers_check_branch = table.Column<string>(type: "text", nullable: true),
                    managers_check_date = table.Column<DateOnly>(type: "date", nullable: true),
                    managers_check_no = table.Column<string>(type: "text", nullable: true),
                    posted_by = table.Column<string>(type: "varchar(50)", nullable: true),
                    posted_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    reference_no = table.Column<string>(type: "text", nullable: false),
                    remarks = table.Column<string>(type: "text", nullable: false),
                    series_number = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    total = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    transaction_date = table.Column<DateOnly>(type: "date", nullable: false),
                    type = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    voided_by = table.Column<string>(type: "varchar(50)", nullable: true),
                    voided_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    wvat = table.Column<decimal>(type: "numeric(18,4)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_filpride_provisional_receipts", x => x.id);
                    table.ForeignKey(
                        name: "fk_filpride_provisional_receipts_filpride_bank_accounts_bank_id",
                        column: x => x.bank_id,
                        principalTable: "filpride_bank_accounts",
                        principalColumn: "bank_account_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_filpride_provisional_receipts_filpride_employees_employee_id",
                        column: x => x.employee_id,
                        principalTable: "filpride_employees",
                        principalColumn: "employee_id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "filpride_purchase_locked_records_queues",
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
                    table.PrimaryKey("pk_filpride_purchase_locked_records_queues", x => x.id);
                    table.ForeignKey(
                        name: "fk_filpride_purchase_locked_records_queues_filpride_receiving_",
                        column: x => x.receiving_report_id,
                        principalTable: "filpride_receiving_reports",
                        principalColumn: "receiving_report_id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "filpride_authority_to_loads",
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
                    table.PrimaryKey("pk_filpride_authority_to_loads", x => x.authority_to_load_id);
                    table.ForeignKey(
                        name: "fk_filpride_authority_to_loads_filpride_customer_order_slips_c",
                        column: x => x.customer_order_slip_id,
                        principalTable: "filpride_customer_order_slips",
                        principalColumn: "customer_order_slip_id");
                    table.ForeignKey(
                        name: "fk_filpride_authority_to_loads_filpride_suppliers_supplier_id",
                        column: x => x.supplier_id,
                        principalTable: "filpride_suppliers",
                        principalColumn: "supplier_id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "filpride_cos_appointed_suppliers",
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
                    table.PrimaryKey("pk_filpride_cos_appointed_suppliers", x => x.sequence_id);
                    table.ForeignKey(
                        name: "fk_filpride_cos_appointed_suppliers_filpride_customer_order_sl",
                        column: x => x.customer_order_slip_id,
                        principalTable: "filpride_customer_order_slips",
                        principalColumn: "customer_order_slip_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_filpride_cos_appointed_suppliers_filpride_purchase_orders_p",
                        column: x => x.purchase_order_id,
                        principalTable: "filpride_purchase_orders",
                        principalColumn: "purchase_order_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_filpride_cos_appointed_suppliers_filpride_suppliers_supplie",
                        column: x => x.supplier_id,
                        principalTable: "filpride_suppliers",
                        principalColumn: "supplier_id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "filpride_customers",
                columns: table => new
                {
                    customer_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    commissionee_id = table.Column<int>(type: "integer", nullable: true),
                    business_style = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    cluster_code = table.Column<int>(type: "integer", nullable: true),
                    commission_rate = table.Column<decimal>(type: "numeric", nullable: false),
                    company = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    created_by = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    created_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    credit_limit = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    credit_limit_as_of_today = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    customer_address = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    customer_code = table.Column<string>(type: "character varying(7)", maxLength: 7, nullable: true),
                    customer_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    customer_terms = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    customer_tin = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    customer_type = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    edited_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    edited_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    has_branch = table.Column<bool>(type: "boolean", nullable: false),
                    has_multiple_terms = table.Column<bool>(type: "boolean", nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    is_bienes = table.Column<bool>(type: "boolean", nullable: false),
                    is_filpride = table.Column<bool>(type: "boolean", nullable: false),
                    requires_price_adjustment = table.Column<bool>(type: "boolean", nullable: false),
                    retention_rate = table.Column<decimal>(type: "numeric(18,4)", nullable: true),
                    station_code = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: true),
                    type = table.Column<string>(type: "character varying(13)", maxLength: 13, nullable: false),
                    vat_type = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    with_holding_tax = table.Column<bool>(type: "boolean", nullable: false),
                    with_holding_vat = table.Column<bool>(type: "boolean", nullable: false),
                    zip_code = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_filpride_customers", x => x.customer_id);
                    table.ForeignKey(
                        name: "fk_filpride_customers_filpride_suppliers_commissionee_id",
                        column: x => x.commissionee_id,
                        principalTable: "filpride_suppliers",
                        principalColumn: "supplier_id");
                });

            migrationBuilder.CreateTable(
                name: "filpride_pick_up_points",
                columns: table => new
                {
                    pick_up_point_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    supplier_id = table.Column<int>(type: "integer", nullable: false),
                    company = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    created_by = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    created_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    depot = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    is_bienes = table.Column<bool>(type: "boolean", nullable: false),
                    is_filpride = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_filpride_pick_up_points", x => x.pick_up_point_id);
                    table.ForeignKey(
                        name: "fk_filpride_pick_up_points_filpride_suppliers_supplier_id",
                        column: x => x.supplier_id,
                        principalTable: "filpride_suppliers",
                        principalColumn: "supplier_id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "filpride_book_atl_details",
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
                    table.PrimaryKey("pk_filpride_book_atl_details", x => x.id);
                    table.ForeignKey(
                        name: "fk_filpride_book_atl_details_filpride_authority_to_loads_autho",
                        column: x => x.authority_to_load_id,
                        principalTable: "filpride_authority_to_loads",
                        principalColumn: "authority_to_load_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_filpride_book_atl_details_filpride_cos_appointed_suppliers_",
                        column: x => x.appointed_id,
                        principalTable: "filpride_cos_appointed_suppliers",
                        principalColumn: "sequence_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_filpride_book_atl_details_filpride_customer_order_slips_cus",
                        column: x => x.customer_order_slip_id,
                        principalTable: "filpride_customer_order_slips",
                        principalColumn: "customer_order_slip_id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "filpride_customer_branches",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    customer_id = table.Column<int>(type: "integer", nullable: false),
                    branch_address = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    branch_name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    branch_tin = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_filpride_customer_branches", x => x.id);
                    table.ForeignKey(
                        name: "fk_filpride_customer_branches_filpride_customers_customer_id",
                        column: x => x.customer_id,
                        principalTable: "filpride_customers",
                        principalColumn: "customer_id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "filpride_sales_invoices",
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
                    table.PrimaryKey("pk_filpride_sales_invoices", x => x.sales_invoice_id);
                    table.ForeignKey(
                        name: "fk_filpride_sales_invoices_filpride_customer_order_slips_custo",
                        column: x => x.customer_order_slip_id,
                        principalTable: "filpride_customer_order_slips",
                        principalColumn: "customer_order_slip_id");
                    table.ForeignKey(
                        name: "fk_filpride_sales_invoices_filpride_customers_customer_id",
                        column: x => x.customer_id,
                        principalTable: "filpride_customers",
                        principalColumn: "customer_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_filpride_sales_invoices_filpride_delivery_receipts_delivery",
                        column: x => x.delivery_receipt_id,
                        principalTable: "filpride_delivery_receipts",
                        principalColumn: "delivery_receipt_id");
                    table.ForeignKey(
                        name: "fk_filpride_sales_invoices_filpride_purchase_orders_purchase_o",
                        column: x => x.purchase_order_id,
                        principalTable: "filpride_purchase_orders",
                        principalColumn: "purchase_order_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_filpride_sales_invoices_products_product_id",
                        column: x => x.product_id,
                        principalTable: "products",
                        principalColumn: "product_id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "filpride_service_invoices",
                columns: table => new
                {
                    service_invoice_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    customer_id = table.Column<int>(type: "integer", nullable: false),
                    delivery_receipt_id = table.Column<int>(type: "integer", nullable: true),
                    service_id = table.Column<int>(type: "integer", nullable: false),
                    amount_paid = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    balance = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    canceled_by = table.Column<string>(type: "varchar(50)", nullable: true),
                    canceled_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    cancellation_remarks = table.Column<string>(type: "varchar(255)", nullable: true),
                    company = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    created_by = table.Column<string>(type: "varchar(100)", nullable: true),
                    created_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    current_and_previous_amount = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    customer_address = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    customer_business_style = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    customer_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    customer_tin = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    discount = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    due_date = table.Column<DateOnly>(type: "date", nullable: false),
                    edited_by = table.Column<string>(type: "varchar(50)", nullable: true),
                    edited_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    has_ewt = table.Column<bool>(type: "boolean", nullable: false),
                    has_wvat = table.Column<bool>(type: "boolean", nullable: false),
                    instructions = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    is_paid = table.Column<bool>(type: "boolean", nullable: false),
                    is_printed = table.Column<bool>(type: "boolean", nullable: false),
                    payment_status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    period = table.Column<DateOnly>(type: "date", nullable: false),
                    posted_by = table.Column<string>(type: "varchar(50)", nullable: true),
                    posted_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    service_invoice_no = table.Column<string>(type: "character varying(13)", maxLength: 13, nullable: false),
                    service_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    service_percent = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    total = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    type = table.Column<string>(type: "character varying(13)", maxLength: 13, nullable: false),
                    unearned_amount = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    vat_type = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    voided_by = table.Column<string>(type: "varchar(50)", nullable: true),
                    voided_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_filpride_service_invoices", x => x.service_invoice_id);
                    table.ForeignKey(
                        name: "fk_filpride_service_invoices_filpride_customers_customer_id",
                        column: x => x.customer_id,
                        principalTable: "filpride_customers",
                        principalColumn: "customer_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_filpride_service_invoices_filpride_delivery_receipts_delive",
                        column: x => x.delivery_receipt_id,
                        principalTable: "filpride_delivery_receipts",
                        principalColumn: "delivery_receipt_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_filpride_service_invoices_filpride_services_service_id",
                        column: x => x.service_id,
                        principalTable: "filpride_services",
                        principalColumn: "service_id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "filpride_freights",
                columns: table => new
                {
                    freight_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    pick_up_point_id = table.Column<int>(type: "integer", nullable: false),
                    cluster_code = table.Column<int>(type: "integer", nullable: false),
                    freight = table.Column<decimal>(type: "numeric(18,4)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_filpride_freights", x => x.freight_id);
                    table.ForeignKey(
                        name: "fk_filpride_freights_filpride_pick_up_points_pick_up_point_id",
                        column: x => x.pick_up_point_id,
                        principalTable: "filpride_pick_up_points",
                        principalColumn: "pick_up_point_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "filpride_collection_receipts",
                columns: table => new
                {
                    collection_receipt_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    bank_id = table.Column<int>(type: "integer", nullable: true),
                    customer_id = table.Column<int>(type: "integer", nullable: false),
                    sales_invoice_id = table.Column<int>(type: "integer", nullable: true),
                    service_invoice_id = table.Column<int>(type: "integer", nullable: true),
                    bank_account_name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    bank_account_number = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    batch_number = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    canceled_by = table.Column<string>(type: "varchar(50)", nullable: true),
                    canceled_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    cancellation_remarks = table.Column<string>(type: "varchar(255)", nullable: true),
                    cash_amount = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    check_amount = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    check_bank = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    check_branch = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    check_date = table.Column<DateOnly>(type: "date", nullable: true),
                    check_no = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    cleared_date = table.Column<DateOnly>(type: "date", nullable: true),
                    collection_receipt_no = table.Column<string>(type: "character varying(13)", maxLength: 13, nullable: true),
                    company = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    created_by = table.Column<string>(type: "varchar(100)", nullable: true),
                    created_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    deposited_date = table.Column<DateOnly>(type: "date", nullable: true),
                    ewt = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    edited_by = table.Column<string>(type: "varchar(50)", nullable: true),
                    edited_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    f2306file_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    f2306file_path = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    f2307file_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    f2307file_path = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    is_certificate_upload = table.Column<bool>(type: "boolean", nullable: false),
                    is_printed = table.Column<bool>(type: "boolean", nullable: false),
                    managers_check_amount = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    managers_check_bank = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    managers_check_branch = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    managers_check_date = table.Column<DateOnly>(type: "date", nullable: true),
                    managers_check_no = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    multiple_si = table.Column<string[]>(type: "text[]", nullable: true),
                    multiple_si_id = table.Column<int[]>(type: "integer[]", nullable: true),
                    multiple_transaction_date = table.Column<DateOnly[]>(type: "date[]", nullable: true),
                    posted_by = table.Column<string>(type: "varchar(50)", nullable: true),
                    posted_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    reference_no = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    remarks = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    si_multiple_amount = table.Column<decimal[]>(type: "numeric[]", nullable: true),
                    si_no = table.Column<string>(type: "character varying(13)", maxLength: 13, nullable: true),
                    sv_no = table.Column<string>(type: "character varying(13)", maxLength: 13, nullable: true),
                    status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    total = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    transaction_date = table.Column<DateOnly>(type: "date", nullable: false),
                    type = table.Column<string>(type: "character varying(13)", maxLength: 13, nullable: true),
                    voided_by = table.Column<string>(type: "varchar(50)", nullable: true),
                    voided_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    wvat = table.Column<decimal>(type: "numeric(18,4)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_filpride_collection_receipts", x => x.collection_receipt_id);
                    table.ForeignKey(
                        name: "fk_filpride_collection_receipts_filpride_bank_accounts_bank_id",
                        column: x => x.bank_id,
                        principalTable: "filpride_bank_accounts",
                        principalColumn: "bank_account_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_filpride_collection_receipts_filpride_customers_customer_id",
                        column: x => x.customer_id,
                        principalTable: "filpride_customers",
                        principalColumn: "customer_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_filpride_collection_receipts_filpride_sales_invoices_sales_",
                        column: x => x.sales_invoice_id,
                        principalTable: "filpride_sales_invoices",
                        principalColumn: "sales_invoice_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_filpride_collection_receipts_filpride_service_invoices_serv",
                        column: x => x.service_invoice_id,
                        principalTable: "filpride_service_invoices",
                        principalColumn: "service_invoice_id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "filpride_credit_memos",
                columns: table => new
                {
                    credit_memo_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    sales_invoice_id = table.Column<int>(type: "integer", nullable: true),
                    service_invoice_id = table.Column<int>(type: "integer", nullable: true),
                    adjusted_price = table.Column<decimal>(type: "numeric(18,4)", nullable: true),
                    amount = table.Column<decimal>(type: "numeric(18,4)", nullable: true),
                    canceled_by = table.Column<string>(type: "varchar(50)", nullable: true),
                    canceled_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    cancellation_remarks = table.Column<string>(type: "varchar(255)", nullable: true),
                    company = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    created_by = table.Column<string>(type: "varchar(100)", nullable: true),
                    created_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    credit_amount = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    credit_memo_no = table.Column<string>(type: "character varying(13)", maxLength: 13, nullable: true),
                    current_and_previous_amount = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    edited_by = table.Column<string>(type: "varchar(50)", nullable: true),
                    edited_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    is_printed = table.Column<bool>(type: "boolean", nullable: false),
                    period = table.Column<DateOnly>(type: "date", nullable: false),
                    posted_by = table.Column<string>(type: "varchar(50)", nullable: true),
                    posted_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    quantity = table.Column<decimal>(type: "numeric(18,4)", nullable: true),
                    remarks = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    source = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    transaction_date = table.Column<DateOnly>(type: "date", nullable: false),
                    type = table.Column<string>(type: "character varying(13)", maxLength: 13, nullable: true),
                    unearned_amount = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    voided_by = table.Column<string>(type: "varchar(50)", nullable: true),
                    voided_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_filpride_credit_memos", x => x.credit_memo_id);
                    table.ForeignKey(
                        name: "fk_filpride_credit_memos_filpride_sales_invoices_sales_invoice",
                        column: x => x.sales_invoice_id,
                        principalTable: "filpride_sales_invoices",
                        principalColumn: "sales_invoice_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_filpride_credit_memos_filpride_service_invoices_service_inv",
                        column: x => x.service_invoice_id,
                        principalTable: "filpride_service_invoices",
                        principalColumn: "service_invoice_id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "filpride_debit_memos",
                columns: table => new
                {
                    debit_memo_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    sales_invoice_id = table.Column<int>(type: "integer", nullable: true),
                    service_invoice_id = table.Column<int>(type: "integer", nullable: true),
                    adjusted_price = table.Column<decimal>(type: "numeric(18,4)", nullable: true),
                    amount = table.Column<decimal>(type: "numeric(18,4)", nullable: true),
                    canceled_by = table.Column<string>(type: "varchar(50)", nullable: true),
                    canceled_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    cancellation_remarks = table.Column<string>(type: "varchar(255)", nullable: true),
                    company = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    created_by = table.Column<string>(type: "varchar(100)", nullable: true),
                    created_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    current_and_previous_amount = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    debit_amount = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    debit_memo_no = table.Column<string>(type: "character varying(13)", maxLength: 13, nullable: true),
                    description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    edited_by = table.Column<string>(type: "varchar(50)", nullable: true),
                    edited_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    is_printed = table.Column<bool>(type: "boolean", nullable: false),
                    period = table.Column<DateOnly>(type: "date", nullable: false),
                    posted_by = table.Column<string>(type: "varchar(50)", nullable: true),
                    posted_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    quantity = table.Column<decimal>(type: "numeric(18,4)", nullable: true),
                    remarks = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    source = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    transaction_date = table.Column<DateOnly>(type: "date", nullable: false),
                    type = table.Column<string>(type: "character varying(13)", maxLength: 13, nullable: true),
                    unearned_amount = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    voided_by = table.Column<string>(type: "varchar(50)", nullable: true),
                    voided_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_filpride_debit_memos", x => x.debit_memo_id);
                    table.ForeignKey(
                        name: "fk_filpride_debit_memos_filpride_sales_invoices_sales_invoice_",
                        column: x => x.sales_invoice_id,
                        principalTable: "filpride_sales_invoices",
                        principalColumn: "sales_invoice_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_filpride_debit_memos_filpride_service_invoices_service_invo",
                        column: x => x.service_invoice_id,
                        principalTable: "filpride_service_invoices",
                        principalColumn: "service_invoice_id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "filpride_collection_receipt_details",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    collection_receipt_id = table.Column<int>(type: "integer", nullable: false),
                    amount = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    collection_receipt_no = table.Column<string>(type: "character varying(13)", maxLength: 13, nullable: false),
                    invoice_date = table.Column<DateOnly>(type: "date", nullable: false),
                    invoice_no = table.Column<string>(type: "character varying(13)", maxLength: 13, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_filpride_collection_receipt_details", x => x.id);
                    table.ForeignKey(
                        name: "fk_filpride_collection_receipt_details_filpride_collection_rec",
                        column: x => x.collection_receipt_id,
                        principalTable: "filpride_collection_receipts",
                        principalColumn: "collection_receipt_id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "ix_filpride_authority_to_loads_authority_to_load_no_company",
                table: "filpride_authority_to_loads",
                columns: new[] { "authority_to_load_no", "company" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_filpride_authority_to_loads_customer_order_slip_id",
                table: "filpride_authority_to_loads",
                column: "customer_order_slip_id");

            migrationBuilder.CreateIndex(
                name: "ix_filpride_authority_to_loads_supplier_id",
                table: "filpride_authority_to_loads",
                column: "supplier_id");

            migrationBuilder.CreateIndex(
                name: "ix_filpride_book_atl_details_appointed_id",
                table: "filpride_book_atl_details",
                column: "appointed_id");

            migrationBuilder.CreateIndex(
                name: "ix_filpride_book_atl_details_authority_to_load_id",
                table: "filpride_book_atl_details",
                column: "authority_to_load_id");

            migrationBuilder.CreateIndex(
                name: "ix_filpride_book_atl_details_customer_order_slip_id",
                table: "filpride_book_atl_details",
                column: "customer_order_slip_id");

            migrationBuilder.CreateIndex(
                name: "ix_filpride_chart_of_accounts_account_name",
                table: "filpride_chart_of_accounts",
                column: "account_name");

            migrationBuilder.CreateIndex(
                name: "ix_filpride_chart_of_accounts_account_number",
                table: "filpride_chart_of_accounts",
                column: "account_number",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_filpride_chart_of_accounts_parent_account_id",
                table: "filpride_chart_of_accounts",
                column: "parent_account_id");

            migrationBuilder.CreateIndex(
                name: "ix_filpride_check_voucher_details_check_voucher_header_id",
                table: "filpride_check_voucher_details",
                column: "check_voucher_header_id");

            migrationBuilder.CreateIndex(
                name: "ix_filpride_collection_receipt_details_collection_receipt_id",
                table: "filpride_collection_receipt_details",
                column: "collection_receipt_id");

            migrationBuilder.CreateIndex(
                name: "ix_filpride_collection_receipt_details_collection_receipt_no",
                table: "filpride_collection_receipt_details",
                column: "collection_receipt_no");

            migrationBuilder.CreateIndex(
                name: "ix_filpride_collection_receipt_details_invoice_no",
                table: "filpride_collection_receipt_details",
                column: "invoice_no");

            migrationBuilder.CreateIndex(
                name: "ix_filpride_collection_receipts_bank_id",
                table: "filpride_collection_receipts",
                column: "bank_id");

            migrationBuilder.CreateIndex(
                name: "ix_filpride_collection_receipts_collection_receipt_no_company",
                table: "filpride_collection_receipts",
                columns: new[] { "collection_receipt_no", "company" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_filpride_collection_receipts_customer_id",
                table: "filpride_collection_receipts",
                column: "customer_id");

            migrationBuilder.CreateIndex(
                name: "ix_filpride_collection_receipts_sales_invoice_id",
                table: "filpride_collection_receipts",
                column: "sales_invoice_id");

            migrationBuilder.CreateIndex(
                name: "ix_filpride_collection_receipts_service_invoice_id",
                table: "filpride_collection_receipts",
                column: "service_invoice_id");

            migrationBuilder.CreateIndex(
                name: "ix_filpride_cos_appointed_suppliers_customer_order_slip_id",
                table: "filpride_cos_appointed_suppliers",
                column: "customer_order_slip_id");

            migrationBuilder.CreateIndex(
                name: "ix_filpride_cos_appointed_suppliers_purchase_order_id",
                table: "filpride_cos_appointed_suppliers",
                column: "purchase_order_id");

            migrationBuilder.CreateIndex(
                name: "ix_filpride_cos_appointed_suppliers_supplier_id",
                table: "filpride_cos_appointed_suppliers",
                column: "supplier_id");

            migrationBuilder.CreateIndex(
                name: "ix_filpride_credit_memos_credit_memo_no_company",
                table: "filpride_credit_memos",
                columns: new[] { "credit_memo_no", "company" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_filpride_credit_memos_sales_invoice_id",
                table: "filpride_credit_memos",
                column: "sales_invoice_id");

            migrationBuilder.CreateIndex(
                name: "ix_filpride_credit_memos_service_invoice_id",
                table: "filpride_credit_memos",
                column: "service_invoice_id");

            migrationBuilder.CreateIndex(
                name: "ix_filpride_customer_branches_customer_id",
                table: "filpride_customer_branches",
                column: "customer_id");

            migrationBuilder.CreateIndex(
                name: "ix_filpride_customers_commissionee_id",
                table: "filpride_customers",
                column: "commissionee_id");

            migrationBuilder.CreateIndex(
                name: "ix_filpride_customers_customer_code",
                table: "filpride_customers",
                column: "customer_code");

            migrationBuilder.CreateIndex(
                name: "ix_filpride_customers_customer_name",
                table: "filpride_customers",
                column: "customer_name");

            migrationBuilder.CreateIndex(
                name: "ix_filpride_cv_trade_payments_check_voucher_id",
                table: "filpride_cv_trade_payments",
                column: "check_voucher_id");

            migrationBuilder.CreateIndex(
                name: "ix_filpride_debit_memos_debit_memo_no_company",
                table: "filpride_debit_memos",
                columns: new[] { "debit_memo_no", "company" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_filpride_debit_memos_sales_invoice_id",
                table: "filpride_debit_memos",
                column: "sales_invoice_id");

            migrationBuilder.CreateIndex(
                name: "ix_filpride_debit_memos_service_invoice_id",
                table: "filpride_debit_memos",
                column: "service_invoice_id");

            migrationBuilder.CreateIndex(
                name: "ix_filpride_employees_employee_number",
                table: "filpride_employees",
                column: "employee_number");

            migrationBuilder.CreateIndex(
                name: "ix_filpride_freights_pick_up_point_id",
                table: "filpride_freights",
                column: "pick_up_point_id");

            migrationBuilder.CreateIndex(
                name: "ix_filpride_general_ledger_books_account_id",
                table: "filpride_general_ledger_books",
                column: "account_id");

            migrationBuilder.CreateIndex(
                name: "ix_filpride_gl_period_balances_account_id",
                table: "filpride_gl_period_balances",
                column: "account_id");

            migrationBuilder.CreateIndex(
                name: "ix_filpride_gl_sub_account_balances_account_id",
                table: "filpride_gl_sub_account_balances",
                column: "account_id");

            migrationBuilder.CreateIndex(
                name: "ix_filpride_inventories_po_id",
                table: "filpride_inventories",
                column: "po_id");

            migrationBuilder.CreateIndex(
                name: "ix_filpride_inventories_product_id",
                table: "filpride_inventories",
                column: "product_id");

            migrationBuilder.CreateIndex(
                name: "ix_filpride_journal_voucher_details_journal_voucher_header_id",
                table: "filpride_journal_voucher_details",
                column: "journal_voucher_header_id");

            migrationBuilder.CreateIndex(
                name: "ix_filpride_monthly_nibits_company",
                table: "filpride_monthly_nibits",
                column: "company");

            migrationBuilder.CreateIndex(
                name: "ix_filpride_monthly_nibits_month",
                table: "filpride_monthly_nibits",
                column: "month");

            migrationBuilder.CreateIndex(
                name: "ix_filpride_monthly_nibits_year",
                table: "filpride_monthly_nibits",
                column: "year");

            migrationBuilder.CreateIndex(
                name: "ix_filpride_multiple_check_voucher_payments_check_voucher_head",
                table: "filpride_multiple_check_voucher_payments",
                column: "check_voucher_header_invoice_id");

            migrationBuilder.CreateIndex(
                name: "ix_filpride_multiple_check_voucher_payments_check_voucher_head1",
                table: "filpride_multiple_check_voucher_payments",
                column: "check_voucher_header_payment_id");

            migrationBuilder.CreateIndex(
                name: "ix_filpride_pick_up_points_company",
                table: "filpride_pick_up_points",
                column: "company");

            migrationBuilder.CreateIndex(
                name: "ix_filpride_pick_up_points_supplier_id",
                table: "filpride_pick_up_points",
                column: "supplier_id");

            migrationBuilder.CreateIndex(
                name: "ix_filpride_po_actual_prices_purchase_order_id",
                table: "filpride_po_actual_prices",
                column: "purchase_order_id");

            migrationBuilder.CreateIndex(
                name: "ix_filpride_provisional_receipts_bank_id",
                table: "filpride_provisional_receipts",
                column: "bank_id");

            migrationBuilder.CreateIndex(
                name: "ix_filpride_provisional_receipts_employee_id",
                table: "filpride_provisional_receipts",
                column: "employee_id");

            migrationBuilder.CreateIndex(
                name: "ix_filpride_provisional_receipts_series_number_company",
                table: "filpride_provisional_receipts",
                columns: new[] { "series_number", "company" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_filpride_purchase_locked_records_queues_locked_date",
                table: "filpride_purchase_locked_records_queues",
                column: "locked_date");

            migrationBuilder.CreateIndex(
                name: "ix_filpride_purchase_locked_records_queues_receiving_report_id",
                table: "filpride_purchase_locked_records_queues",
                column: "receiving_report_id");

            migrationBuilder.CreateIndex(
                name: "ix_filpride_receiving_reports_delivery_receipt_id",
                table: "filpride_receiving_reports",
                column: "delivery_receipt_id");

            migrationBuilder.CreateIndex(
                name: "ix_filpride_receiving_reports_po_id",
                table: "filpride_receiving_reports",
                column: "po_id");

            migrationBuilder.CreateIndex(
                name: "ix_filpride_receiving_reports_receiving_report_no_company",
                table: "filpride_receiving_reports",
                columns: new[] { "receiving_report_no", "company" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_filpride_sales_invoices_customer_id",
                table: "filpride_sales_invoices",
                column: "customer_id");

            migrationBuilder.CreateIndex(
                name: "ix_filpride_sales_invoices_customer_order_slip_id",
                table: "filpride_sales_invoices",
                column: "customer_order_slip_id");

            migrationBuilder.CreateIndex(
                name: "ix_filpride_sales_invoices_delivery_receipt_id",
                table: "filpride_sales_invoices",
                column: "delivery_receipt_id");

            migrationBuilder.CreateIndex(
                name: "ix_filpride_sales_invoices_product_id",
                table: "filpride_sales_invoices",
                column: "product_id");

            migrationBuilder.CreateIndex(
                name: "ix_filpride_sales_invoices_purchase_order_id",
                table: "filpride_sales_invoices",
                column: "purchase_order_id");

            migrationBuilder.CreateIndex(
                name: "ix_filpride_sales_invoices_sales_invoice_no_company",
                table: "filpride_sales_invoices",
                columns: new[] { "sales_invoice_no", "company" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_filpride_service_invoices_customer_id",
                table: "filpride_service_invoices",
                column: "customer_id");

            migrationBuilder.CreateIndex(
                name: "ix_filpride_service_invoices_delivery_receipt_id",
                table: "filpride_service_invoices",
                column: "delivery_receipt_id");

            migrationBuilder.CreateIndex(
                name: "ix_filpride_service_invoices_service_id",
                table: "filpride_service_invoices",
                column: "service_id");

            migrationBuilder.CreateIndex(
                name: "ix_filpride_service_invoices_service_invoice_no_company",
                table: "filpride_service_invoices",
                columns: new[] { "service_invoice_no", "company" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_filpride_suppliers_supplier_code",
                table: "filpride_suppliers",
                column: "supplier_code");

            migrationBuilder.CreateIndex(
                name: "ix_filpride_suppliers_supplier_name",
                table: "filpride_suppliers",
                column: "supplier_name");

            migrationBuilder.AddForeignKey(
                name: "fk_filpride_check_voucher_headers_filpride_bank_accounts_bank_",
                table: "filpride_check_voucher_headers",
                column: "bank_id",
                principalTable: "filpride_bank_accounts",
                principalColumn: "bank_account_id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "fk_filpride_check_voucher_headers_filpride_employees_employee_",
                table: "filpride_check_voucher_headers",
                column: "employee_id",
                principalTable: "filpride_employees",
                principalColumn: "employee_id");

            migrationBuilder.AddForeignKey(
                name: "fk_filpride_check_voucher_headers_filpride_suppliers_supplier_",
                table: "filpride_check_voucher_headers",
                column: "supplier_id",
                principalTable: "filpride_suppliers",
                principalColumn: "supplier_id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "fk_filpride_customer_order_slips_filpride_customers_customer_id",
                table: "filpride_customer_order_slips",
                column: "customer_id",
                principalTable: "filpride_customers",
                principalColumn: "customer_id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "fk_filpride_customer_order_slips_filpride_pick_up_points_pick_",
                table: "filpride_customer_order_slips",
                column: "pick_up_point_id",
                principalTable: "filpride_pick_up_points",
                principalColumn: "pick_up_point_id");

            migrationBuilder.AddForeignKey(
                name: "fk_filpride_customer_order_slips_filpride_purchase_orders_purc",
                table: "filpride_customer_order_slips",
                column: "purchase_order_id",
                principalTable: "filpride_purchase_orders",
                principalColumn: "purchase_order_id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "fk_filpride_customer_order_slips_filpride_suppliers_commission",
                table: "filpride_customer_order_slips",
                column: "commissionee_id",
                principalTable: "filpride_suppliers",
                principalColumn: "supplier_id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "fk_filpride_customer_order_slips_filpride_suppliers_hauler_id",
                table: "filpride_customer_order_slips",
                column: "hauler_id",
                principalTable: "filpride_suppliers",
                principalColumn: "supplier_id");

            migrationBuilder.AddForeignKey(
                name: "fk_filpride_customer_order_slips_filpride_suppliers_supplier_id",
                table: "filpride_customer_order_slips",
                column: "supplier_id",
                principalTable: "filpride_suppliers",
                principalColumn: "supplier_id");

            migrationBuilder.AddForeignKey(
                name: "fk_filpride_delivery_receipts_filpride_authority_to_loads_auth",
                table: "filpride_delivery_receipts",
                column: "authority_to_load_id",
                principalTable: "filpride_authority_to_loads",
                principalColumn: "authority_to_load_id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "fk_filpride_delivery_receipts_filpride_customers_customer_id",
                table: "filpride_delivery_receipts",
                column: "customer_id",
                principalTable: "filpride_customers",
                principalColumn: "customer_id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "fk_filpride_delivery_receipts_filpride_purchase_orders_purchas",
                table: "filpride_delivery_receipts",
                column: "purchase_order_id",
                principalTable: "filpride_purchase_orders",
                principalColumn: "purchase_order_id");

            migrationBuilder.AddForeignKey(
                name: "fk_filpride_delivery_receipts_filpride_suppliers_commissionee_",
                table: "filpride_delivery_receipts",
                column: "commissionee_id",
                principalTable: "filpride_suppliers",
                principalColumn: "supplier_id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "fk_filpride_delivery_receipts_filpride_suppliers_hauler_id",
                table: "filpride_delivery_receipts",
                column: "hauler_id",
                principalTable: "filpride_suppliers",
                principalColumn: "supplier_id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "fk_filpride_journal_voucher_headers_filpride_check_voucher_hea",
                table: "filpride_journal_voucher_headers",
                column: "cv_id",
                principalTable: "filpride_check_voucher_headers",
                principalColumn: "check_voucher_header_id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "fk_filpride_purchase_orders_filpride_customers_customer_id",
                table: "filpride_purchase_orders",
                column: "customer_id",
                principalTable: "filpride_customers",
                principalColumn: "customer_id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "fk_filpride_purchase_orders_filpride_pick_up_points_pick_up_po",
                table: "filpride_purchase_orders",
                column: "pick_up_point_id",
                principalTable: "filpride_pick_up_points",
                principalColumn: "pick_up_point_id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "fk_filpride_purchase_orders_filpride_suppliers_supplier_id",
                table: "filpride_purchase_orders",
                column: "supplier_id",
                principalTable: "filpride_suppliers",
                principalColumn: "supplier_id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "fk_filpride_purchase_orders_products_product_id",
                table: "filpride_purchase_orders",
                column: "product_id",
                principalTable: "products",
                principalColumn: "product_id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "fk_filpride_sales_locked_records_queues_filpride_delivery_rece",
                table: "filpride_sales_locked_records_queues",
                column: "delivery_receipt_id",
                principalTable: "filpride_delivery_receipts",
                principalColumn: "delivery_receipt_id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "fk_jv_amortization_settings_filpride_journal_voucher_headers_j",
                table: "jv_amortization_settings",
                column: "jv_id",
                principalTable: "filpride_journal_voucher_headers",
                principalColumn: "journal_voucher_header_id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
