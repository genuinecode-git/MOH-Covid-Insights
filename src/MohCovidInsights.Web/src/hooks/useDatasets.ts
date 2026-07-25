import { useQuery } from '@tanstack/react-query';
import { fetchDatasets, fetchObservations } from '../api/datasetsApi';

export function useDatasets() {
  return useQuery({
    queryKey: ['datasets'],
    queryFn: fetchDatasets,
    staleTime: 10 * 60 * 1000,
  });
}

export function useDatasetObservations(id: string | undefined, from: string, to: string) {
  return useQuery({
    queryKey: ['observations', id, from, to],
    queryFn: () => fetchObservations(id!, from, to),
    enabled: Boolean(id),
    placeholderData: (prev) => prev,
  });
}