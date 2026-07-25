import { act, renderHook } from '@testing-library/react';
import { beforeEach, describe, expect, it } from 'vitest';
import { DEFAULT_FILTERS, useFilters } from './useFilters';

beforeEach(() => {
  window.history.pushState({}, '', '/');
});

describe('useFilters', () => {
  it('initialises draft and applied to defaults when no URL params are present', () => {
    const { result } = renderHook(() => useFilters());
    expect(result.current.draft).toEqual(DEFAULT_FILTERS);
    expect(result.current.applied).toEqual(DEFAULT_FILTERS);
    expect(result.current.isDirty).toBe(false);
  });

  it('reads initial filters from the URL query string', () => {
    window.history.pushState({}, '', '/?fromYear=2022&ageGroup=Elderly&pct=1');
    const { result } = renderHook(() => useFilters());
    expect(result.current.applied.fromYear).toBe(2022);
    expect(result.current.applied.ageGroup).toBe('Elderly');
    expect(result.current.applied.showPercentChange).toBe(true);
  });

  it('setField updates only the draft and marks state as dirty', () => {
    const { result } = renderHook(() => useFilters());
    act(() => result.current.setField('ageGroup', 'Elderly'));
    expect(result.current.draft.ageGroup).toBe('Elderly');
    expect(result.current.applied.ageGroup).toBe(DEFAULT_FILTERS.ageGroup);
    expect(result.current.isDirty).toBe(true);
  });

  it('apply copies the draft into applied and clears the dirty flag', () => {
    const { result } = renderHook(() => useFilters());
    act(() => result.current.setField('ageGroup', 'Elderly'));
    act(() => result.current.apply());
    expect(result.current.applied.ageGroup).toBe('Elderly');
    expect(result.current.isDirty).toBe(false);
  });

  it('clear resets draft and applied back to defaults', () => {
    const { result } = renderHook(() => useFilters());
    act(() => result.current.setField('ageGroup', 'Elderly'));
    act(() => result.current.apply());
    act(() => result.current.clear());
    expect(result.current.draft).toEqual(DEFAULT_FILTERS);
    expect(result.current.applied).toEqual(DEFAULT_FILTERS);
  });
});
