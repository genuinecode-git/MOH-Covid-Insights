import type { BedSegment } from './BedSegment';

export interface BedUtilisation {
  utilisationPct: number;
  totalCapacity: number;
  weekLabel: string;
  segments: BedSegment[];
}
