export const AXIS = {
  tick: { fontSize: 11, fill: '#8A90A2' },
  axisLine: false as const,
  tickLine: false as const,
};

export const GRID = { stroke: '#EDEFF5', vertical: false };

export const fmtCompact = (v: number) =>
  v >= 1_000_000 ? `${(v / 1_000_000).toFixed(1)}M`
  : v >= 1000 ? `${v / 1000}K`
  : String(v);
