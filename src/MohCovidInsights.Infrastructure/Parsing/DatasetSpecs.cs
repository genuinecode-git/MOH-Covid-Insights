using MohCovidInsights.Domain.ValueObjects;

namespace MohCovidInsights.Infrastructure.Parsing;

public static class DatasetSpecs
{
    public static readonly LongFormatSpec Infections = new()
    {
        ValueField = "est_count",
        DefaultMetric = MetricCode.EstimatedInfections,
    };


    public static readonly LongFormatSpec Admissions = new()
    {
        StatusField = "new_admisison_type",
        StatusMap = new Dictionary<string, MetricCode>
        {
            ["Hospitalised"] = MetricCode.HospitalAdmissions,
            ["ICU"] = MetricCode.IcuAdmissions,
        },
    };

    public static readonly LongFormatSpec AverageCases = new()
    {
        StatusField = "clinical_status",
        StatusMap = new Dictionary<string, MetricCode>
        {
            ["Hospitalised"] = MetricCode.AvgHospitalisedCases,
            ["ICU"] = MetricCode.AvgIcuCases,
        },
        DimensionField = "age_groups",
        EmitDimensionTotal = true,
    };

    public static readonly LongFormatSpec IcuBeds = new()
    {
        StatusField = "status",
        StatusMap = new Dictionary<string, MetricCode>
        {
            ["COVID"] = MetricCode.IcuBedsCovid,
            ["Non-COVID"] = MetricCode.IcuBedsNonCovid,
            ["Empty"] = MetricCode.IcuBedsEmpty,
        },
        TotalMetric = MetricCode.IcuBedsTotalCapacity,
    };
}