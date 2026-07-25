import { Paper, Stack, Typography, Box } from '@mui/material';

interface TooltipItem { name?: string; value?: number; color?: string }

export function ChartTooltip({
  active, payload, label, suffixes = {},
}: {
  active?: boolean;
  payload?: TooltipItem[];
  label?: string;
  suffixes?: Record<string, string>;
}) {
  if (!active || !payload?.length) return null;
  return (
    <Paper sx={{ px: 1.5, py: 1, boxShadow: 3 }}>
      <Typography variant="caption" fontWeight={600} display="block" mb={0.5}>
        Epi Week {label}
      </Typography>
      {payload.map((p) => (
        <Stack key={p.name} direction="row" alignItems="center" spacing={1}>
          <Box sx={{ width: 8, height: 8, borderRadius: '50%', bgcolor: p.color }} />
          <Typography variant="caption" color="text.secondary" sx={{ flexGrow: 1 }}>
            {p.name}
          </Typography>
          <Typography variant="caption" fontWeight={600}>
            {p.value?.toLocaleString()}{suffixes[p.name ?? ''] ?? ''}
          </Typography>
        </Stack>
      ))}
    </Paper>
  );
}