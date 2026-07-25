import { describe, expect, it } from 'vitest';
import { fmtCompact } from './chartConfig.constants';

describe('fmtCompact', () => {
  it('formats millions with an M suffix', () => {
    expect(fmtCompact(2_500_000)).toBe('2.5M');
  });

  it('formats thousands with a K suffix', () => {
    expect(fmtCompact(4000)).toBe('4K');
  });

  it('returns plain numbers below 1000 unchanged', () => {
    expect(fmtCompact(42)).toBe('42');
  });
});
