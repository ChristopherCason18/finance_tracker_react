using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Data.SqlClient;
using System.Data;
using System.Globalization;
using Microsoft.AspNetCore.Http;
using System;
using System.IO;

namespace finance_tracker.Models;

public static class seedData
{
    public static void Initialize(IServiceProvider serviceProvider)
    {

        var options = new DbContextOptionsBuilder<transactionsContext>()
        .UseSqlServer(
            "Server=localhost;Database=finances;Trusted_Connection=True;TrustServerCertificate=True;"
            ).Options;
        
        using(var context = new transactionsContext(options))
        {
            // Ensure database and tables are created
            context.Database.EnsureCreated();

            // Look for any existing stock.
            if (context.transactions.Any())
            {
                return;   // DB has been seeded
            }

            //Import the CSV:
            string csvFilePath = "";
            var stream = File.OpenRead(csvFilePath);
            StreamReader reader = new StreamReader(stream);
            reader.ReadLine();    //Skip the heading
            Console.WriteLine("Reading...");
            string line = "";
            string[] csvArray;
            //Read in the rest of the CSV
            while(!reader.EndOfStream)
            {
                line = reader.ReadLine();
                csvArray = line.Split(',');
                string type = csvArray[0];
                string details = csvArray[1];
                string particulars = csvArray[2];
                string code = csvArray[3];
                string reference = csvArray[4];
                decimal amount = decimal.Parse(csvArray[5]);
                string date = csvArray[6];
                
                //Create a new transaction to add to the database
                var newTransaction = new transactions{type = type,details=details,particulars=particulars,
                code=code,reference=reference,amount=amount,date=date};
                
                //Add items to database and save
                context.transactions.Add(newTransaction);
                context.SaveChanges();
            }
            context.SaveChanges();
            
        }
    }
}