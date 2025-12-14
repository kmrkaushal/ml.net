using Microsoft.ML.Data;

namespace Ml.Net
{
public class SalaryData
{
    [LoadColumn(0)]
    public float YearsExperience;

    [LoadColumn(1)]
    public float Salary;
}

public class SalaryPrediction
{
    [ColumnName("Score")]
    public float Salary;
}
}