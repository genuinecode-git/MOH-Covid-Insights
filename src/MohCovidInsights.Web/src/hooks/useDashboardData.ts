import { useQuery } from '@tanstack/react-query';
import { fetchDashboard } from '../api/dashboardApi';
import type { DashboardFilters } from '../types';

export function useDashboardData(filters: DashboardFilters) {
  return useQuery({
    queryKey: ['dashboard', filters],
    queryFn: () => fetchDashboard(filters),
    staleTime: 5 * 60 * 1000,
    placeholderData: (prev) => prev,
  });
}