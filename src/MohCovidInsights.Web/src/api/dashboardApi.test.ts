import { afterEach, describe, expect, it, vi } from 'vitest';
import { fetchDashboard } from './dashboardApi';
import type { DashboardFilters } from '../types';

const filters: DashboardFilters = {
  fromYear: 2023,
  fromWeek: 1,
  toYear: 2024,
  toWeek: 4,
  ageGroup: 'All',
  clinicalStatus: 'All',
  showPercentChange: false,
};

afterEach(() => {
  vi.unstubAllGlobals();
});

describe('fetchDashboard', () => {
  it('requests the dashboard endpoint with epi-week-formatted filters', async () => {
    const fetchMock = vi.fn().mockResolvedValue({
      ok: true,
      json: () => Promise.resolve({ series: [] }),
    });
    vi.stubGlobal('fetch', fetchMock);

    const data = await fetchDashboard(filters);

    const requestedUrl = fetchMock.mock.calls[0][0] as string;
    expect(requestedUrl).toContain('/dashboard?');
    expect(requestedUrl).toContain('from=2023-W01');
    expect(requestedUrl).toContain('to=2024-W04');
    expect(data).toEqual({ series: [] });
  });

  it('throws the problem detail message when the request fails', async () => {
    vi.stubGlobal(
      'fetch',
      vi.fn().mockResolvedValue({
        ok: false,
        status: 400,
        json: () => Promise.resolve({ detail: 'Invalid date range' }),
      }),
    );

    await expect(fetchDashboard(filters)).rejects.toThrow('Invalid date range');
  });

  it('falls back to a generic message when the error body is not JSON', async () => {
    vi.stubGlobal(
      'fetch',
      vi.fn().mockResolvedValue({
        ok: false,
        status: 500,
        json: () => Promise.reject(new Error('not json')),
      }),
    );

    await expect(fetchDashboard(filters)).rejects.toThrow('Request failed (500)');
  });
});
