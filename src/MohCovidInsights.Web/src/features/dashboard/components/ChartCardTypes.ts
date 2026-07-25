import type { ReactNode } from 'react';

export interface Series {
  label: string;
  color: string;
  type?: 'line' | 'bar';
}

export interface Props {
  title: string;
  hint?: string;
  series?: Series[];
  height?: number;
  children: ReactNode;
}
