import type { KpiTrend } from './KpiTrend';

export interface Kpi {
  id: string;
  label: string;
  value: string;
  trend?: KpiTrend;
  caption?: string;
  tone: 'infections' | 'peak' | 'hospital' | 'icu' | 'utilisation' | 'latest';
}
