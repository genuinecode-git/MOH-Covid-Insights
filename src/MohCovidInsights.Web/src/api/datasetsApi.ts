import type { Dataset, DatasetObservations } from '../types';

const BASE_URL = import.meta.env.VITE_API_BASE_URL ?? '/api/v1';

async function get<T>(path: string): Promise<T> {
  const res = await fetch(`${BASE_URL}${path}`, { headers: { Accept: 'application/json' } });

  if (!res.ok) {
    const problem = await res.json().catch(() => null);
    throw new Error(problem?.detail ?? problem?.title ?? `Request failed (${res.status})`);
  }

  return (await res.json()) as T;
}

export const fetchDatasets = () => get<Dataset[]>('/datasets');

export const fetchObservations = (id: string, from: string, to: string) =>
  get<DatasetObservations>(`/datasets/${id}/observations?from=${from}&to=${to}`);