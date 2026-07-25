import type { DashboardData, WeeklyPoint } from '../types';
import { palette } from '../theme/theme';

function buildWeekly(): WeeklyPoint[] {
  const out: WeeklyPoint[] = [];
  const weeks: Array<[number, number]> = [];
  for (let w = 1; w <= 52; w++) weeks.push([2023, w]);
  for (let w = 1; w <= 4; w++) weeks.push([2024, w]);

  weeks.forEach(([year, week], i) => {
    const wave = Math.sin(i / 4.2) * 0.5 + Math.sin(i / 11) * 0.35 + 1;
    const infections = Math.round(38000 + wave * 24000);
    const hospital = Math.round(infections * 0.0032 + 90);
    const icu = Math.round(hospital * 0.13 + 4);
    out.push({
      epiWeek: `${year}-${String(week).padStart(2, '0')}`,
      estimatedInfections: infections,
      hospitalAdmissions: hospital,
      icuAdmissions: icu,
      avgHospitalisedCases: Math.round(hospital * 3.2),
      avgIcuCases: Math.round(icu * 4.1),
      icuUtilisationPct: Number((62 + wave * 9).toFixed(1)),
    });
  });
  return out;
}

export const mockDashboard: DashboardData = {
  rangeLabel: 'Jan 2023 – Jan 2024 (Latest)',
  lastUpdated: '6 Jun 2024',
  weekly: buildWeekly(),
  kpis: [
    {
      id: 'total-infections',
      label: 'Total Estimated Infections',
      value: '1.52M',
      tone: 'infections',
      trend: { changePct: 12.4, direction: 'up' },
    },
    {
      id: 'peak-infection-week',
      label: 'Peak Infection Week',
      value: '2023-09',
      tone: 'peak',
      caption: 'Week of 26 Feb – 4 Mar',
    },
    {
      id: 'total-hospital',
      label: 'Total Hospital Admissions',
      value: '44,892',
      tone: 'hospital',
      trend: { changePct: 8.7, direction: 'up' },
    },
    {
      id: 'total-icu',
      label: 'Total ICU Admissions',
      value: '6,125',
      tone: 'icu',
      trend: { changePct: 9.5, direction: 'up' },
    },
    {
      id: 'peak-icu-utilisation',
      label: 'Peak ICU Utilisation',
      value: '82.3%',
      tone: 'utilisation',
      caption: 'Week of 24 Dec – 30 Dec 2023',
    },
    {
      id: 'latest-week',
      label: 'Latest Data Week',
      value: '2024-04',
      tone: 'latest',
      caption: 'Week of 21 – 27 Jan 2024',
    },
  ],
  bedUtilisation: {
    utilisationPct: 72.8,
    totalCapacity: 1396,
    weekLabel: 'Week of 21 – 27 Jan 2024',
    segments: [
      { label: 'COVID Beds', beds: 312, pct: 22.3, color: palette.infections, tone: 'infections' },
      { label: 'Non-COVID Beds', beds: 707, pct: 50.5, color: palette.hospital, tone: 'hospital' },
      { label: 'Empty Beds', beds: 377, pct: 27.2, color: palette.utilisation, tone: 'utilisation' },
    ],
  },
  insights: [
    {
      id: 'peak-infections',
      tone: 'infections',
      headline: 'Peak infections occurred in',
      detail: 'Week 2023-09 (26 Feb – 4 Mar)',
    },
    {
      id: 'peak-hospital',
      tone: 'hospital',
      headline: 'Peak hospital admissions in',
      detail: 'Week 2023-10 (5 – 11 Mar)',
    },
    {
      id: 'peak-icu',
      tone: 'icu',
      headline: 'Peak ICU utilisation in',
      detail: 'Week 2023-51 (24 – 30 Dec)',
    },
    {
      id: 'avg-icu',
      tone: 'utilisation',
      headline: 'Average ICU utilisation',
      detail: '63.4% during selected period',
    },
  ],
  coverage: [
    { id: 'c1', name: 'Weekly Estimated Infections', coverage: 'Jan 2023 – Jan 2024', volume: '52 weeks', color: palette.infections },
    { id: 'c2', name: 'Hospital & ICU Admissions', coverage: 'Jan 2023 – Jan 2024', volume: '52 weeks', color: palette.hospital },
    { id: 'c3', name: 'Avg Hospitalised & ICU Cases', coverage: 'Jan 2023 – Jan 2024', volume: '52 weeks', color: '#2563EB' },
    { id: 'c4', name: 'ICU Bed Utilisation', coverage: 'Jan 2023 – Jan 2024', volume: '52 weeks', color: palette.utilisation },
    { id: 'c5', name: 'Vaccination Progress', coverage: 'Dec 2020 – Feb 2024', volume: '1,152 days', color: '#10B981' },
    { id: 'c6', name: 'Deaths by Month', coverage: 'Jan 2023 – Sep 2023', volume: '36 months', color: palette.icu },
    { id: 'c7', name: 'Vaccination by Age Group', coverage: 'Sep 2022 – Aug 2023', volume: '9 records', color: '#6366F1' },
  ],
};