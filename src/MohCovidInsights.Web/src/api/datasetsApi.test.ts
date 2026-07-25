import { afterEach, describe, expect, it, vi } from 'vitest';
import { fetchDatasets, fetchObservations } from './datasetsApi';

afterEach(() => {
  vi.unstubAllGlobals();
});

describe('fetchDatasets', () => {
  it('returns the parsed dataset list on success', async () => {
    vi.stubGlobal(
      'fetch',
      vi.fn().mockResolvedValue({
        ok: true,
        json: () => Promise.resolve([{ id: '1', name: 'Cases' }]),
      }),
    );

    await expect(fetchDatasets()).resolves.toEqual([{ id: '1', name: 'Cases' }]);
  });

  it('throws the problem detail message when the request fails', async () => {
    vi.stubGlobal(
      'fetch',
      vi.fn().mockResolvedValue({
        ok: false,
        status: 404,
        json: () => Promise.resolve({ title: 'Not found' }),
      }),
    );

    await expect(fetchDatasets()).rejects.toThrow('Not found');
  });
});

describe('fetchObservations', () => {
  it('requests the observations endpoint for the given dataset and date range', async () => {
    const fetchMock = vi.fn().mockResolvedValue({
      ok: true,
      json: () => Promise.resolve({ points: [] }),
    });
    vi.stubGlobal('fetch', fetchMock);

    const data = await fetchObservations('abc', '2023-W01', '2023-W10');

    const requestedUrl = fetchMock.mock.calls[0][0] as string;
    expect(requestedUrl).toContain('/datasets/abc/observations');
    expect(requestedUrl).toContain('from=2023-W01');
    expect(requestedUrl).toContain('to=2023-W10');
    expect(data).toEqual({ points: [] });
  });
});
