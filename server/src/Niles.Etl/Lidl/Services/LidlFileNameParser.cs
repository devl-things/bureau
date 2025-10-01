using Niles.Etl.Configurations;
using Niles.Etl.Extract;
using Niles.Models;
using System.Globalization;
using System.Text.RegularExpressions;

namespace Niles.Etl.Lidl.Services
{
    public sealed class LidlFileNameParser : IFileNameParser
    {
        public string RetailerName => Definitions.Retailers.Lidl;

        //TODO change signature into something better
        public (Store store, DateOnly date) Parse(string filePath)
        {
            string name = Path.GetFileNameWithoutExtension(filePath);
            string[] parts = name.Split('_', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

            if (parts.Length < 2) throw new FormatException("Unexpected filename: " + name);

            string storeName = parts[0];

            string? postal = null;
            string? city = null;
            for (int i = 0; i < parts.Length; i++)
            {
                if (Regex.IsMatch(parts[i], @"^\d{4,5}$"))
                {
                    postal = parts[i];
                    if (i + 1 < parts.Length) city = parts[i + 1];
                    break;
                }
            }

            string? address = null;
            if (parts.Length >= 3) address = string.Join(", ", parts.Skip(1).Take(parts.Length - 3));

            string dateToken = parts[parts.Length - 2]; // e.g. 01.08.2025
            DateTime date = DateTime.ParseExact(dateToken, "dd.MM.yyyy", CultureInfo.InvariantCulture);

            Store store = new Store
            {
                Name = storeName,
                Retailer = new Retailer { Name = RetailerName },
                Address = address,
                City = city,
                PostalCode = postal
            };

            return (store, DateOnly.FromDateTime(date));
        }
    }
}
