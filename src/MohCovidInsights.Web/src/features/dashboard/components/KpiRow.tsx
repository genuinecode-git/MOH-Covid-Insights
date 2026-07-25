import { Box, Paper, Skeleton, Stack, Typography } from '@mui/material';
import ArrowUpIcon from '@mui/icons-material/ArrowUpwardRounded';
import ArrowDownIcon from '@mui/icons-material/ArrowDownwardRounded';
import PeopleIcon from '@mui/icons-material/GroupsRounded';
import TrendIcon from '@mui/icons-material/TrendingUpRounded';
import HospitalIcon from '@mui/icons-material/ApartmentRounded';
import BedIcon from '@mui/icons-material/HotelRounded';
import GaugeIcon from '@mui/icons-material/DonutLargeRounded';
import CalendarIcon from '@mui/icons-material/EventRounded';
import type { Kpi } from '../../../types';

const TONES = {
  infections:  { bg: '#EEF2FF', fg: '#4353E8', icon: <PeopleIcon fontSize="small" /> },
  peak:        { bg: '#ECFDF5', fg: '#10B981', icon: <TrendIcon fontSize="small" /> },
  hospital:    { bg: '#F5F3FF', fg: '#8B5CF6', icon: <HospitalIcon fontSize="small" /> },
  icu:         { bg: '#FFF1F2', fg: '#F43F5E', icon: <BedIcon fontSize="small" /> },
  utilisation: { bg: '#FFF7ED', fg: '#F59E0B', icon: <GaugeIcon fontSize="small" /> },
  latest:      { bg: '#EEF2FF', fg: '#6366F1', icon: <CalendarIcon fontSize="small" /> },
} as const;

function KpiCard({ kpi }: { kpi: Kpi }) {
  const tone = TONES[kpi.tone];
  const up = kpi.trend?.direction === 'up';

  return (
    <Paper sx={{ p: 2, height: '100%' }}>
      <Typography variant="caption" color="text.secondary" align="center"
        sx={{ display: 'block', mb: 1.5, fontWeight: 500 }}>
        {kpi.label}
      </Typography>
      <Stack direction="row" spacing={1.5} alignItems="center">
        <Box sx={{
          width: 40, height: 40, borderRadius: '50%', flexShrink: 0,
          bgcolor: tone.bg, color: tone.fg, display: 'grid', placeItems: 'center',
        }}>
          {tone.icon}
        </Box>
        <Box minWidth={0}>
          <Typography sx={{ fontSize: '1.5rem', fontWeight: 700, lineHeight: 1.15 }}>
            {kpi.value}
          </Typography>
          {kpi.trend && (
            <Stack direction="row" alignItems="center" spacing={0.25}>
              {up ? <ArrowUpIcon sx={{ fontSize: 13, color: 'success.main' }} />
                  : <ArrowDownIcon sx={{ fontSize: 13, color: 'error.main' }} />}
              <Typography variant="caption" sx={{ color: up ? 'success.main' : 'error.main', fontWeight: 600 }}>
                {kpi.trend.changePct}%
              </Typography>
              <Typography variant="caption" color="text.secondary">vs prev period</Typography>
            </Stack>
          )}
          {kpi.caption && (
            <Typography variant="caption" color="text.secondary" sx={{ display: 'block' }}>
              {kpi.caption}
            </Typography>
          )}
        </Box>
      </Stack>
    </Paper>
  );
}

export default function KpiRow({ kpis, loading }: { kpis?: Kpi[]; loading?: boolean }) {
  return (
    <Box sx={{
      display: 'grid', gap: 2,
      gridTemplateColumns: {
        xs: '1fr', sm: 'repeat(2, 1fr)', md: 'repeat(3, 1fr)', xl: 'repeat(6, 1fr)',
      },
    }}>
      {loading || !kpis
        ? Array.from({ length: 6 }).map((_, i) => (
            <Skeleton key={i} variant="rounded" height={120} />
          ))
        : kpis.map((k) => <KpiCard key={k.id} kpi={k} />)}
    </Box>
  );
}