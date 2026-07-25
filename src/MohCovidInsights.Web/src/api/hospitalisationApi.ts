import type { HospitalisationCases } from '../types';

const BASE_URL = import.meta.env.VITE_API_BASE_URL ?? '/api/v1';

export interface HospitalisationFilters {
  from: string;
  to: string;
  clinicalStatus: string;
  ageGroup: string;
}

export async function fetchHospitalisationCases(
  filters: HospitalisationFilters,
): Promise<HospitalisationCases> {
  const params = new URLSearchParams({
    from: filters.from,
    to: filters.to,
    clinicalStatus: filters.clinicalStatus,
    ageGroup: filters.ageGroup,
  });

  const res = await fetch(`${BASE_URL}/hospitalisation-cases?${params}`, {
    headers: { Accept: 'application/json' },
  });

  if (!res.ok) {
    const problem = await res.json().catch(() => null);
    throw new Error(problem?.detail ?? problem?.title ?? `Request failed (${res.status})`);
  }

  return (await res.json()) as HospitalisationCases;
}