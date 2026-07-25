export interface Insight {
  id: string;
  tone: 'infections' | 'hospital' | 'icu' | 'utilisation';
  headline: string;
  detail: string;
}
