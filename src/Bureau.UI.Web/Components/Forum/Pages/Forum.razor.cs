﻿using BlazorBootstrap;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Bureau.UI.Web.Components.Forum.Pages
{
    public partial class Forum
    {
        public record Customer(int id, string CustomerName);
        private string? customerName;
        public IEnumerable<Customer>? customers;

        private async Task<AutoCompleteDataProviderResult<Customer>> CustomersDataProvider(AutoCompleteDataProviderRequest<Customer> request)
        {
            if (customers is null) // pull customers only one time for client-side autocomplete
                customers = GetCustomers(); // call a service or an API to pull the customers

            return await Task.FromResult(request.ApplyTo(customers.OrderBy(customer => customer.CustomerName)));
        }

        private IEnumerable<Customer> GetCustomers()
        {
            return new List<Customer> {
            new(1, "Pich S"),
            new(2, "sfh Sobi"),
            new(3, "Jojo chan"),
            new(4, "Jee ja"),
            new(5, "Rose Canon"),
            new(6, "Manju A"),
            new(7, "Bandita PA"),
            new(8, "Sagar Adil"),
            new(9, "Isha Wang"),
            new(10, "Daina JJ"),
            new(11, "Komala Mug"),
            new(12, "Dikshita BD"),
            new(13, "Neha Gosar"),
            new(14, "Preeti S"),
            new(15, "Sagar Seth"),
            new(16, "Vinayak MM"),
            new(17, "Vijaya Lakhsmi"),
            new(18, "Jahan K"),
            new(19, "Joy B"),
            new(20, "Zaraiah C"),
            new(21, "Laura L"),
            new(22, "Punith ES")
        };
        }

        private void OnAutoCompleteChanged(Customer customer)
        {
            // TODO: handle your own logic

            // NOTE: do null check
            Console.WriteLine($"'{customer?.CustomerName}' selected.");
        }
    }
}
