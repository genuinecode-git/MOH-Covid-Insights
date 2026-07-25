import { afterEach, describe, expect, it, vi } from 'vitest';
import { fetchHospitalisationCases } from './hospitalisationApi';

const filters = { from: '2023-W01', to: '2023-W10', clinicalStatus: 'All', ageGroup: 'All' };

afterEach(() => {
  vi.unstubAllGlobals();
});

describe('fetchHospitalisationCases', () => {
  it('requests the hospitalisation-cases endpoint with the given filters', async () => {
    const fetchMock = vi.fn().mockResolvedValue({
      ok: true,
      json: () => Promise.resolve({ cases: [] }),
    });
    vi.stubGlobal('fetch', fetchMock);

    const data = await fetchHospitalisationCases(filters);

    const requestedUrl = fetchMock.mock.calls[0][0] as string;
    expect(requestedUrl).toContain('/hospitalisation-cases?');
    expect(requestedUrl).toContain('from=2023-W01');
    expect(requestedUrl).toContain('to=2023-W10');
    expect(data).toEqual({ cases: [] });
  });

  it('throws the problem detail message when the request fails', async () => {
    vi.stubGlobal(
      'fetch',
      vi.fn().mockResolvedValue({
        ok: false,
        status: 400,
        json: () => Promise.resolve({ detail: 'Invalid filters' }),
      }),
    );

    await expect(fetchHospitalisationCases(filters)).rejects.toThrow('Invalid filters');
  });
});
