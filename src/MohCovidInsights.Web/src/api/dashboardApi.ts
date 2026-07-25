import type { DashboardData, DashboardFilters } from '../types';

const BASE_URL = import.meta.env.VITE_API_BASE_URL ?? '/api/v1';

const toEpiWeek = (year: number, week: number) =>
  `${year}-W${String(week).padStart(2, '0')}`;

export async function fetchDashboard(filters: DashboardFilters): Promise<DashboardData> {
  const params = new URLSearchParams({
    from: toEpiWeek(filters.fromYear, filters.fromWeek),
    to: toEpiWeek(filters.toYear, filters.toWeek),
    ageGroup: filters.ageGroup,
    clinicalStatus: filters.clinicalStatus,
  });

  const res = await fetch(`${BASE_URL}/dashboard?${params}`, {
    headers: { Accept: 'application/json' },
  });

  if (!res.ok) {
    const problem = await res.json().catch(() => null);
    throw new Error(problem?.detail ?? problem?.title ?? `Request failed (${res.status})`);
  }

  return (await res.json()) as DashboardData;
}