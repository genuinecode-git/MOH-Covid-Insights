export interface BedSegment {
  label: string;
  beds: number;
  pct: number;
  color: string;
  tone: 'infections' | 'hospital' | 'icu' | 'utilisation';
}
