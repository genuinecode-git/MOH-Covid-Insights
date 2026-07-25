import type { Kpi } from './Kpi';
import type { WeeklyPoint } from './WeeklyPoint';
import type { BedUtilisation } from './BedUtilisation';
import type { Insight } from './Insight';
import type { DatasetCoverage } from './DatasetCoverage';

export interface DashboardData {
  kpis: Kpi[];
  weekly: WeeklyPoint[];
  bedUtilisation: BedUtilisation;
  insights: Insight[];
  coverage: DatasetCoverage[];
  lastUpdated: string;
  rangeLabel: string;
}
