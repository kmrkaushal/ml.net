using Microsoft.ML.Data;

namespace WebAPI
{
    public class ModelInput
    {
        public float YearsExperience { get; set; }
    }

    public class ModelOutput
    {
        [ColumnName("Score")]
        public float Salary;
    }
}
