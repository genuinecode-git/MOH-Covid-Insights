export interface WeeklyPoint {
  epiWeek: string;
  estimatedInfections: number;
  hospitalAdmissions: number;
  icuAdmissions: number;
  avgHospitalisedCases: number;
  avgIcuCases: number;
  icuUtilisationPct: number;
}
