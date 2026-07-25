import { useQuery } from '@tanstack/react-query';
import { fetchHospitalisationCases, type HospitalisationFilters } from '../api/hospitalisationApi';

export function useHospitalisationCases(filters: HospitalisationFilters) {
  return useQuery({
    queryKey: ['hospitalisation-cases', filters],
    queryFn: () => fetchHospitalisationCases(filters),
    staleTime: 5 * 60 * 1000,
    placeholderData: (prev) => prev,
  });
}