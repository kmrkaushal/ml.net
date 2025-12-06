
using Microsoft.Data.Analysis;
using Microsoft.ML;
using Microsoft.ML.Data;
using Microsoft.ML.Transforms;
using Ml.Net;
using System.Diagnostics;
using System.Xml.Linq;
internal class Program
{
    private static void Main(string[] args)
    {
        // Step 1: Create MLContext
        MLContext mlContext = new MLContext();

        // Step 2: Load .csv File
        IDataView data = mlContext.Data.LoadFromTextFile<HousingData>(path: "HousePrices.csv", hasHeader: true, separatorChar: ',');

        // Step 3: Data Cleanup    
        var pipeline = mlContext.Transforms.ReplaceMissingValues("Price", "Price", MissingValueReplacingEstimator.ReplacementMode.Mean)
         .Append(mlContext.Transforms.ReplaceMissingValues("Bedrooms_o", "Bedrooms", MissingValueReplacingEstimator.ReplacementMode.Mean))
        .Append(mlContext.Transforms.ReplaceMissingValues("Bathrooms_o", "Bathrooms", MissingValueReplacingEstimator.ReplacementMode.Mean))
        .Append(mlContext.Transforms.ReplaceMissingValues("SqftLiving_o", "SqftLiving", MissingValueReplacingEstimator.ReplacementMode.Mean))
        .Append(mlContext.Transforms.ReplaceMissingValues("SqftLot_o", "SqftLot", MissingValueReplacingEstimator.ReplacementMode.Mean))
        .Append(mlContext.Transforms.ReplaceMissingValues("Floors_o", "Floors", MissingValueReplacingEstimator.ReplacementMode.Mean))
        .Append(mlContext.Transforms.ReplaceMissingValues("YearBuilt_o", "YearBuilt", MissingValueReplacingEstimator.ReplacementMode.Mean))
        .Append(mlContext.Transforms.ReplaceMissingValues("YearRenovated_o", "YearRenovated", MissingValueReplacingEstimator.ReplacementMode.Mean))
        .Append(mlContext.Transforms.ReplaceMissingValues("Parking_o", "Parking", MissingValueReplacingEstimator.ReplacementMode.Mean))
        .Append(mlContext.Transforms.ReplaceMissingValues("NearbySchools_o", "NearbySchools", MissingValueReplacingEstimator.ReplacementMode.Mean))
        .Append(mlContext.Transforms.ReplaceMissingValues("Balcony_o", "Balcony", MissingValueReplacingEstimator.ReplacementMode.Mean))
        .Append(mlContext.Transforms.NormalizeMinMax(
           new[]
           {
           new InputOutputColumnPair("Bedrooms"),
           //new InputOutputColumnPair("Bathrooms"),
           //new InputOutputColumnPair("SqftLiving"),
            new InputOutputColumnPair("SqftLot_scaled"),
            new InputOutputColumnPair("Floors_scaled", "Floors_o"),
            new InputOutputColumnPair("YearBuilt_scaled", "YearBuilt_o"),
            new InputOutputColumnPair("YearRenovated_scaled", "YearRenovated_o"),
            new InputOutputColumnPair("Parking_scaled", "Parking_o"),
            new InputOutputColumnPair("NearbySchools_scaled", "NearbySchools_o"),
            new InputOutputColumnPair("Balcony_scaled", "Balcony_o"),
        }))
        .Append(mlContext.Transforms.Categorical.OneHotEncoding(
           new[]
           {
               new InputOutputColumnPair("City"),
               //new InputOutputColumnPair("PropertyEncoded"),
               //new InputOutputColumnPair("FacingEncoded"),
               //new InputOutputColumnPair("FurnishingEncoded"),
               //new InputOutputColumnPair("LiftEncoded"),
               //new InputOutputColumnPair("GardenEncoded"),
               // new InputOutputColumnPair("LocalityEncoded"),
               //  new InputOutputColumnPair("SocietyEncoded")
           }))
        .Append(mlContext.Transforms.Concatenate("Features", "Bedrooms_scaled", "Bathrooms_scaled", "SqftLiving_scaled", "SqftLot_scaled", "Floors_scaled", "YearBuilt_scaled",
               "YearRenovated_scaled", "Parking_scaled", "NearbySchools_scaled", "Balcony_scaled", "City", "PropertyType", "Facing", "Furnishing", "Lift", "Garden", "Locality", "Society"))
        .Append(mlContext.Transforms.NormalizeMinMax("Features"));


        //// Step 4: Fit and Transform Data
        data = pipeline.Fit(data).Transform(data);

        // Step 5: Preview Data
        var preview = data.Preview(5);
        DisplayData(preview);

        ////// Split Train/Test
        var split = mlContext.Data.TrainTestSplit(data, testFraction: 0.2);

        //// Train
        var trainer = mlContext.Regression.Trainers.Sdca(labelColumnName: "Price", featureColumnName: "Features");
        var model = trainer.Fit(split.TrainSet);

        // Evaluate
        var predictions = model.Transform(split.TestSet);
        var metrics = mlContext.Regression.Evaluate(predictions, labelColumnName: "Price");

        Console.WriteLine("===== EVALUATION METRICS =====");
        Console.WriteLine($"RMSE: {metrics.RootMeanSquaredError}");
        Console.WriteLine($"R²  : {metrics.RSquared}");
        Console.WriteLine($"MAE : {metrics.MeanAbsoluteError}");
        Console.WriteLine("==============================\n");

        ////// Create prediction engine
        var engine = mlContext.Model.CreatePredictionEngine<HousingData, Prediction>(model);

        var sample = new HousingData
        {
           City = "Delhi",
           Locality = "Dwarka",
           Bedrooms = 3,
           Bathrooms = 2,
           SqftLiving = 1400,
           SqftLot = 0,
           Floors = 1,
           YearBuilt = 2015,
           YearRenovated = 0,
           PropertyType = "Apartment",
           Parking = 1,
           Facing = "East",
           Lift = "Yes",
           Furnishing = "Semi-Furnished",
           NearbySchools = 4,
           NearbyHospitals = 2,
           Society = "XYZ Society",
           Balcony = 1
        };

        var result = engine.Predict(sample);
        Console.WriteLine($"Predicted Price: {result.Score}");


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