using Microsoft.ML.Data;

namespace Ml.Net
{
    public class HousingData
    {
        [LoadColumn(0)]
        public string City { get; set; }


        [LoadColumn(1)]
        public string Locality { get; set; }


        [LoadColumn(2)]
        public float Bedrooms { get; set; }


        [LoadColumn(3)]
        public float Bathrooms { get; set; }


        [LoadColumn(4)]
        public float SqftLiving { get; set; }


        [LoadColumn(5)]
        public float SqftLot { get; set; }


        [LoadColumn(6)]
        public float Floors { get; set; }


        [LoadColumn(7)]
        public float YearBuilt { get; set; }


        [LoadColumn(8)]
        public float YearRenovated { get; set; }


        [LoadColumn(9)]
        public string PropertyType { get; set; }


        [LoadColumn(10)]
        public float Parking { get; set; }


        [LoadColumn(11)]
        public string Facing { get; set; }


        [LoadColumn(12)]
        public string Lift { get; set; }


        [LoadColumn(13)]
        public string Furnishing { get; set; }


        [LoadColumn(14)]
        public float NearbySchools { get; set; }


        [LoadColumn(15)]
        public float NearbyHospitals { get; set; }


        [LoadColumn(16)]
        public string Society { get; set; }


        [LoadColumn(17)]
        public float Balcony { get; set; }


        [LoadColumn(18)]
        public string Garden { get; set; }

        [LoadColumn(19)]
        public float Price { get; set; }
    }

    public class Prediction
    {
        public float Score { get; set; }
    }
}
