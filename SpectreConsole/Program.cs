using System;
using System.Collections.Generic;
using Spectre.Console;
using Bogus;
using System.Runtime.Serialization;
using System.Linq;

namespace spectre
{
    class Program
    {
        static void Main(string[] args)
        {
            var title = new Rule("[green]Products[/]");
            title.Alignment = Justify.Left;
            AnsiConsole.Render(title);

            AnsiConsole.WriteLine();

            var products = GenerateProducts(100);

            RenderTable(products);

            RenderChart(products);

        }

        private static IEnumerable<Product> GenerateProducts(int howMany)
        {
            return new Faker<Product>()
                            .WithRecord()
                            .RuleFor(p => p.Name, f => f.Commerce.Product())
                            .RuleFor(p => p.Price, f => f.Random.Decimal(1, 1000))
                            .RuleFor(p => p.Availability, f => f.Commerce.Random.Float())
                            .Generate(howMany);
        }

        private static void RenderTable(IEnumerable<Product> products)
        {
            var table = new Table();
            table.AddColumn(new TableColumn(new Markup("[yellow]Name[/]")));
            table.AddColumn(new TableColumn(new Markup("[yellow]Price[/]")).Centered());
            table.AddColumn(new TableColumn(new Markup("[yellow]Availability[/]")).Centered());
            foreach (var product in products)
            {
                var priceColor = product.Price > 500 ? "green" : "blue";
                var availabilityColor = product.Availability < 0.25 ?
                     "red" :
                     product.Availability < 0.5 ? "yellow" : "green";
                table.AddRow(
                    product.Name,
                    $"[{priceColor}]{product.Price}[/]",
                    $"[{availabilityColor}]{product.Availability}[/]"
                    );
            }
            AnsiConsole.Render(table);
        }

        private static void RenderChart(IEnumerable<Product> products)
        {
            Func<float,float> perc = a=> a/products.Count() * 100;

            var availabilityChartItems = new List<AvailabilityChartItem>
            {
                new AvailabilityChartItem("Low", perc(products.Count(p => p.Availability < 0.25)), Color.Red),
                new AvailabilityChartItem("Medium", perc(products.Count(p => p.Availability < 0.50 && p.Availability >= 0.25)), Color.Yellow),
                new AvailabilityChartItem("OK", perc(products.Count(p => p.Availability >= 0.5)), Color.Green)
            };

            var availabilityChart = new BarChart()
                .Width(60)
                .Label("Availability").CenterLabel()
                .AddItems(availabilityChartItems);
            
            AnsiConsole.Render(availabilityChart);
        }
    }

    public class AvailabilityChartItem : IBarChartItem
    {
        public AvailabilityChartItem(string label, double availability, Color? color = null)
        {
            Label = label;
            Value = availability;
            Color = color;
        }
        public string Label { get; set; }
        public double Value { get; set; }
        public Color? Color { get; set; }
    }

    public record Product(string Name, decimal Price, float Availability);

    public static class ExtensionsForBogus
    {
        public static Faker<T> WithRecord<T>(this Faker<T> faker) where T : class
        {
            faker.CustomInstantiator(_ => FormatterServices.GetUninitializedObject(typeof(T)) as T);
            return faker;
        }
    }

}
