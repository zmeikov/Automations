using System;
using System.Collections.Generic;

namespace WaveAccountingIntegration.Models
{
	public class Invoice
	{
		public int id { get; set; }
		public string url { get; set; }
		public Address address { get; set; }
		public string subhead { get; set; }
		public string footer { get; set; }
		public string po_so_number { get; set; }
		public string memo { get; set; }
		public DateTime invoice_date { get; set; }
		public string invoice_number_label { get; set; }
		public string invoice_number { get; set; }
		public Invoice_Currency invoice_currency { get; set; }
		public double exchange_rate { get; set; }
		public double invoice_total { get; set; }
		public double invoice_tax_total { get; set; }
		public double invoice_amount_paid { get; set; }
		public double invoice_amount_due { get; set; }
		public DateTime due_date { get; set; }
		public DateTime? last_payment_date { get; set; }
		public string status { get; set; }
		public bool disable_credit_card_payments { get; set; }
		public bool disable_bank_payments { get; set; }
		public string readonly_url { get; set; }
		public string pdf_url { get; set; }
		public Sharing_Urls sharing_urls { get; set; }
		public string items_url { get; set; }
		public string payments_url { get; set; }
		public DateTime? last_sent { get; set; }
		public string last_sent_via { get; set; }
		public DateTime? last_viewed { get; set; }
		public DateTime date_created { get; set; }
		public DateTime date_modified { get; set; }
		public string source_type { get; set; }
		public object source_invoice_number { get; set; }
		public string source_url { get; set; }
		public string item_title { get; set; }
		public string description_title { get; set; }
		public string quantity_title { get; set; }
		public string price_title { get; set; }
		public string amount_title { get; set; }
		public bool? hide_item { get; set; }
		public bool? hide_description { get; set; }
		public bool? hide_quantity { get; set; }
		public bool? hide_price { get; set; }
		public bool? hide_amount { get; set; }
		public Customer customer { get; set; }
	}

	public class InvoiceDisablePayments
	{
		public bool disable_credit_card_payments { get; set; }
		public bool disable_bank_payments { get; set; }
	}

	public class InvoiceItem
	{
		public int id { get; set; }
		public string url { get; set; }
		public Product product { get; set; }
		public double price { get; set; }
		public double quantity { get; set; }
		public string description { get; set; }
		public object[] taxes { get; set; }
		public DateTime date_created { get; set; }
		public DateTime date_modified { get; set; }
	}

	public class Product
	{
		public int id { get; set; }
		public string url { get; set; }
		public string name { get; set; }
		public double price { get; set; }
		public string description { get; set; }
		public bool? is_sold { get; set; }
		public bool? is_bought { get; set; }
		public Income_Account income_account { get; set; }
		public Expense_Account expense_account { get; set; }
		public bool? active { get; set; }
		public DateTime date_created { get; set; }
		public DateTime date_modified { get; set; }
		public object[] default_sales_taxes { get; set; }
	}

	public class Income_Account
	{
		public int id { get; set; }
		public string url { get; set; }
	}

	public class Expense_Account
	{
		public int id { get; set; }
		public string url { get; set; }
	}

	public class Currency
	{
		public string url { get; set; }
		public string code { get; set; }
		public string symbol { get; set; }
		public string name { get; set; }
		public string plural { get; set; }
	}

	public class Invoice_Currency
	{
		public string url { get; set; }
		public string code { get; set; }
		public string symbol { get; set; }
		public string name { get; set; }
	}

	public class Sharing_Urls
	{
		public string gmail { get; set; }
		public string outlook { get; set; }
		public string _public { get; set; }
		public string yahoo_mail { get; set; }
	}

	public class Customer
	{
		public int id { get; set; }
		public string url { get; set; }
		public string account_number { get; set; }
		public bool active { get; set; }
		public string name { get; set; }
		public string first_name { get; set; }
		public string last_name { get; set; }
		public string email { get; set; }
		public string fax_number { get; set; }
		public string mobile_number { get; set; }
		public string phone_number { get; set; }
		public string toll_free_number { get; set; }
		public string website { get; set; }
		public Currency currency { get; set; }
		public string address1 { get; set; }
		public string address2 { get; set; }
		public string city { get; set; }
		public Province province { get; set; }
		public Country country { get; set; }
		public string postal_code { get; set; }
		public DateTime date_created { get; set; }
		public DateTime date_modified { get; set; }
		public Shipping_details shipping_details { get; set; }


	}

	public class CustomerUpdateSettings
	{
		public Update_Shipping_Details shipping_details { get; set; }

		public class Update_Shipping_Details
		{
			public string delivery_instructions { get; set; }

		}
	}

	public class Address
	{
		public string address1 { get; set; }
		public string address2 { get; set; }
		public string city { get; set; }
		public string province_name { get; set; }
		public string country_name { get; set; }
		public string postal_code { get; set; }
	}

	public class Shipping_details
	{
		public string ship_to_contact { get; set; }
		public string delivery_instructions { get; set; }
		public string phone_number { get; set; }
		public string address1 { get; set; }
		public string address2 { get; set; }
		public string city { get; set; }
		public Province province { get; set; }
		public Country country { get; set; }
		public string postal_code { get; set; }
	}

	public class Province
	{
		public string name { get; set; }
		public string slug { get; set; }
		public string code { get; set; }
	}

	public class Country
	{
		public string name { get; set; }
		public string country_code { get; set; }
		public Currency currency { get; set; }
		public string url { get; set; }
	}


	public class Payment
	{
		public int id { get; set; }
		public string url { get; set; }
		public string payment_date { get; set; }
		public string memo { get; set; }
		public double exchange_rate { get; set; }
		public double amount { get; set; }
		public Payment_Account payment_account { get; set; }
		public string payment_method { get; set; }
		public Payment_Details payment_details { get; set; }
		public DateTime date_created { get; set; }
		public DateTime date_modified { get; set; }
		public string readonly_url { get; set; }
	}

	public class Payment_Account
	{
		public int id { get; set; }
		public string url { get; set; }
	}

	public class Payment_Details
	{
	}



	public class RecurringInvoice
	{
		public Column_Settings column_settings { get; set; }
		public string notes { get; set; }
		public DateTime date_added { get; set; }
		public int? num_generated_to_date { get; set; }
		public RecurringInvoiceCustomer customer { get; set; }
		public string previous_invoice_date { get; set; }
		public List<Line_Items> line_items { get; set; }
		public double total { get; set; }
		public string currency_symbol { get; set; }
		public string footer { get; set; }
		public string currency_code { get; set; }
		public string subheading { get; set; }
		public object source_id { get; set; }
		public object[] notifications { get; set; }
		public object source_type { get; set; }
		public Schedule_Settings schedule_settings { get; set; }
		public DateTime date_modified { get; set; }
		public string state { get; set; }
		public string id { get; set; }
		public string business_id { get; set; }
		public string next_invoice_date { get; set; }
		public Payment_Settings payment_settings { get; set; }
		public string payment_terms { get; set; }
		public string first_invoice_date { get; set; }
		public string invoice_number_label { get; set; }
		public string purchase_order_id { get; set; }
		public Delivery_Settings delivery_settings { get; set; }
		public bool can_edit { get; set; }
	}

	public class Column_Settings
	{
		public string price_title { get; set; }
		public bool description_hidden { get; set; }
		public string item_title { get; set; }
		public string amount_title { get; set; }
		public bool quantity_hidden { get; set; }
		public bool amount_hidden { get; set; }
		public string description_title { get; set; }
		public bool item_hidden { get; set; }
		public bool price_hidden { get; set; }
		public string quantity_title { get; set; }
	}

	public class RecurringInvoiceCustomer
	{
		public string phone { get; set; }
		public string name { get; set; }
		public string id { get; set; }
		public string email { get; set; }
	}

	public class Schedule_Settings
	{
		public string timezone_id { get; set; }
		public object end_date { get; set; }
		public object repeat_on_day_of_week { get; set; }
		public int? recurrence_interval { get; set; }
		public object max_invoices { get; set; }
		public bool repeat_at_end_of_month { get; set; }
		public string start_date { get; set; }
		public string recurrence_unit { get; set; }
		public object repeat_on_month_of_year { get; set; }
		public int? repeat_on_day_of_month { get; set; }
		public object final_invoice_date { get; set; }
	}

	public class Payment_Settings
	{
		public bool enable_customer_payment_by_credit_card { get; set; }
		public string payment_offset_from_invoice_creation { get; set; }
		public object payment_method_id { get; set; }
		public bool auto_payment_enabled { get; set; }
	}

	public class Delivery_Settings
	{
		public object customer_emails { get; set; }
		public bool copy_myself { get; set; }
		public string custom_message { get; set; }
		public string what_to_deliver { get; set; }
		public bool attach_pdf { get; set; }
		public bool auto_send_enabled { get; set; }
		public string deliver_on { get; set; }
		public object sender_email { get; set; }
		public bool skip_weekends { get; set; }
	}

	public class Line_Items
	{
		public int product_id { get; set; }
		public object sort_order_index { get; set; }
		public double quantity { get; set; }
		public string description { get; set; }
		public double price { get; set; }
		public object[] taxes { get; set; }
	}




	public class User
	{
		public string id { get; set; }
		public string url { get; set; }
		public string first_name { get; set; }
		public string last_name { get; set; }
		public List<Email> emails { get; set; }
		public DateTime date_created { get; set; }
		public DateTime date_modified { get; set; }
		public DateTime last_login { get; set; }
	}

	public class Email
	{
		public string email { get; set; }
		public bool is_verified { get; set; }
		public bool is_default { get; set; }
	}



	public class Connected_Site
	{
		public int id { get; set; }
		public List<Contentservice_Set> contentservice_set { get; set; }
		public Yodlee_Site yodlee_site { get; set; }
		public string latest_update_time { get; set; }
		public Connected_Site_State connected_site_state { get; set; }
		public bool is_owner { get; set; }
		public DateTime created { get; set; }
		public bool has_unmapped_accounts { get; set; }
		public int site_account_id { get; set; }
	}

	public class Yodlee_Site
	{
		public int id { get; set; }
		public string display_name { get; set; }
		public string base_url { get; set; }
		public List<string> site_container_types { get; set; }
	}

	public class Connected_Site_State
	{
		public string state { get; set; }
		public string description { get; set; }
		public object user_message { get; set; }
		public bool state_is_error { get; set; }
	}

	public class Contentservice_Set
	{
		public int id { get; set; }
		public List<Account_Set> account_set { get; set; }
	}

	public class Account_Set
	{
		public int id { get; set; }
		public string display_name { get; set; }
		public string short_display_name { get; set; }
		public string account_number { get; set; }
		public string account_holder { get; set; }
		public bool? aggregating { get; set; }
		public string balance { get; set; }
		public string currency { get; set; }
		public string currency_code_with_symbol { get; set; }
		public DateTime? start_date { get; set; }
		public int? payment_account_pk { get; set; }
		public string business { get; set; }
		public string account_state { get; set; }
	}


	public class Business
	{
		public string id { get; set; }
		public string url { get; set; }
		public string company_name { get; set; }
		public Primary_Currency primary_currency { get; set; }
		public bool is_owner { get; set; }
		public bool is_default_business { get; set; }
		public bool is_personal { get; set; }
		public string business_group { get; set; }
		public string business_group_display { get; set; }
		public string business_type { get; set; }
		public string business_type_display { get; set; }
		public string organization_type { get; set; }
		public string organization_type_display { get; set; }
		public string address1 { get; set; }
		public string address2 { get; set; }
		public string city { get; set; }
		public Province province { get; set; }
		public Country country { get; set; }
		public string postal_code { get; set; }
		public string timezone_id { get; set; }
		public string phone_number { get; set; }
		public string mobile_phone_number { get; set; }
		public string toll_free_phone_number { get; set; }
		public string fax_number { get; set; }
		public string website { get; set; }
		public DateTime date_created { get; set; }
		public DateTime date_modified { get; set; }
		public App[] apps { get; set; }
		public string scopes { get; set; }
		public Attribs attribs { get; set; }
		public Invoice_Sending_Emails[] invoice_sending_emails { get; set; }
	}

	public class Primary_Currency
	{
		public string url { get; set; }
		public string code { get; set; }
		public string symbol { get; set; }
		public string name { get; set; }
		public string plural { get; set; }
	}


	public class Attribs
	{
		public string eligible_payment_service { get; set; }
	}

	public class App
	{
		public string app_name { get; set; }
		public string href { get; set; }
	}

	public class Invoice_Sending_Emails
	{
		public string email { get; set; }
		public bool _default { get; set; }
		public bool is_collaborator { get; set; }
		public bool verified { get; set; }
	}




	public class TransactionHistory
	{
		public List<Transaction_History> transaction_history { get; set; }
	}

	public class Transaction_History
	{
		public double starting_balance { get; set; }
		public double ending_balance { get; set; }
		public double payment_balance_for_period { get; set; }
		public double refund_balance_for_period { get; set; }
		public double invoice_balance_for_period { get; set; }
		public Statement_Currency statement_currency { get; set; }
		public List<Event> events { get; set; }
	}

	public class Statement_Currency
	{
		public string url { get; set; }
		public string code { get; set; }
		public string symbol { get; set; }
		public string name { get; set; }
	}

	public class Event
	{
		public string event_type { get; set; }
		public DateTime? date { get; set; }
		public double total { get; set; }
		public Invoice invoice { get; set; }
		public Payment payment { get; set; }
	}


}