
using Microsoft.ML;
using Microsoft.ML.Data;
using Microsoft.ML.Transforms;
using Ml.Net;
internal class Program
{
    private static void Main(string[] args)
    {
        // Step 1: Create MLContext
        MLContext mlContext = new MLContext();

        // Step 2: Load .csv File
        IDataView data = mlContext.Data.LoadFromTextFile<HousingData>(path: "HousePrices.csv", hasHeader: true, separatorChar: ',');

        // Step 3: Data Cleanup    
        var pipeline = mlContext.Transforms.ReplaceMissingValues(new[]
        {
            new InputOutputColumnPair("Price")          
        }, MissingValueReplacingEstimator.ReplacementMode.Mean)
            .Append(mlContext.Transforms.Categorical.OneHotEncoding(new[]{
                new InputOutputColumnPair("City"),
                new InputOutputColumnPair("Locality"),
                new InputOutputColumnPair("Facing"),
                new InputOutputColumnPair("Lift"),
                new InputOutputColumnPair("Furnishing"),
                new InputOutputColumnPair("Society"),
                new InputOutputColumnPair("Garden")
                }))           
            .Append(mlContext.Transforms.Concatenate("Features", "City", "Locality", "Facing", "Lift", "Furnishing", "Society", "Garden", "City", 
            "Bedrooms", "Bathrooms", "SqftLiving", "SqftLot", "Floors", "YearBuilt", "YearRenovated", "Parking", "NearbySchools", "NearbyHospitals", "Balcony"))
            .Append(mlContext.Transforms.NormalizeMinMax("Features"))
            .Append(mlContext.Regression.Trainers.Sdca(labelColumnName: "Price",featureColumnName:"Features"));

        //Split /Train/Test
        var split = mlContext.Data.TrainTestSplit(data, testFraction: 0.2);

        //Train
        var model = pipeline.Fit(split.TrainSet);

        // Evaluate on test data
        var prediction = model.Transform(split.TestSet);
        var metrics = mlContext.Regression.Evaluate(prediction, labelColumnName: "Price");

        // Output metrics
        Console.WriteLine("===== MODEL METRICS =====");
        Console.WriteLine($"RMSE: {metrics.RootMeanSquaredError}");
        Console.WriteLine($"MAE: {metrics.MeanAbsoluteError}");
        Console.WriteLine($"R²  : {metrics.RSquared}");

        // Sample prediction
        var engine = mlContext.Model.CreatePredictionEngine<HousingData, Prediction>(model);
        var sample = new HousingData
        {
            City = "Delhi",
            Locality = "Vasant Kunj",
            Bedrooms = 3,
            Bathrooms = 3,
            SqftLiving = 3581,
            SqftLot = 5921,
            Floors = 2,
            YearBuilt = 1973,
            YearRenovated = 2008,
            PropertyType = "Apartment",
            Parking = 1,
            Facing = "North",
            Lift = "Yes",
            Furnishing = "Furnished",
            NearbySchools = 8,
            NearbyHospitals = 3,
            Balcony = 1,
            Garden = "Yes"
        };
        var pred = engine.Predict(sample);
        Console.WriteLine($"Sample Predicted Price: {pred.Score}");


        ////// Step 4: Fit and Transform Data
        //data = pipeline.Fit(data).Transform(data);

        //// Step 5: Preview Data
        //var preview = data.Preview(5);
        //DisplayData(preview);

    }
    static void DisplayData(DataDebuggerPreview preview)
    {
        Console.WriteLine("\n=== Transformed Data Preview ===\n");

        // Get column names
        var columnNames = preview.Schema.Select(c => c.Name).ToArray();

        // Print headers
        foreach (var col in columnNames)
            Console.Write($"{col,-20}");

        Console.WriteLine();
        Console.WriteLine(new string('-', columnNames.Length * 20));

        // Print rows
        foreach (var row in preview.RowView)
        {
            foreach (var column in row.Values)
            {
                string value = column.Value == null ? "NULL" : column.Value.ToString();
                if (value.Length > 18) value = value.Substring(0, 17) + "..."; // shorten long vector output
                Console.Write($"{value,-20}");
            }
            Console.WriteLine();
        }

        Console.WriteLine("\n============================\n");
    }
}