import {
  Box, Button, FormControlLabel, MenuItem, Paper, Select, Stack, Switch, Typography,
} from '@mui/material';
import type { DashboardFilters } from '../types';

const YEARS = [2023, 2024];
const WEEKS = Array.from({ length: 52 }, (_, i) => i + 1);
const AGE_GROUPS = ['All', '0–11', '12–39', '40–59', '60+'];
const CLINICAL = ['All', 'Hospitalised', 'ICU', 'Recovered'];

interface Props {
  draft: DashboardFilters;
  isDirty: boolean;
  onChange: <K extends keyof DashboardFilters>(key: K, value: DashboardFilters[K]) => void;
  onApply: () => void;
  onClear: () => void;
}

function Label({ children }: { children: string }) {
  return (
    <Typography variant="caption" color="text.secondary" sx={{ display: 'block', mb: 0.75 }}>
      {children}
    </Typography>
  );
}

export default function FiltersPanel({ draft, isDirty, onChange, onApply, onClear }: Props) {
  return (
    <Paper sx={{ p: 2 }}>
      <Stack direction="row" justifyContent="space-between" alignItems="center" mb={2}>
        <Typography variant="subtitle2">Filters</Typography>
        <Button size="small" onClick={onClear} sx={{ minWidth: 0, p: 0.25 }}>
          Clear all
        </Button>
      </Stack>

      <Box mb={1.75}>
        <Label>From (Epi Week)</Label>
        <Stack direction="row" spacing={1}>
          <Select size="small" fullWidth value={draft.fromYear}
            onChange={(e) => onChange('fromYear', Number(e.target.value))}>
            {YEARS.map((y) => <MenuItem key={y} value={y}>{y}</MenuItem>)}
          </Select>
          <Select size="small" fullWidth value={draft.fromWeek}
            onChange={(e) => onChange('fromWeek', Number(e.target.value))}>
            {WEEKS.map((w) => (
              <MenuItem key={w} value={w}>Week {String(w).padStart(2, '0')}</MenuItem>
            ))}
          </Select>
        </Stack>
      </Box>

      <Box mb={1.75}>
        <Label>To (Epi Week)</Label>
        <Stack direction="row" spacing={1}>
          <Select size="small" fullWidth value={draft.toYear}
            onChange={(e) => onChange('toYear', Number(e.target.value))}>
            {YEARS.map((y) => <MenuItem key={y} value={y}>{y}</MenuItem>)}
          </Select>
          <Select size="small" fullWidth value={draft.toWeek}
            onChange={(e) => onChange('toWeek', Number(e.target.value))}>
            {WEEKS.map((w) => (
              <MenuItem key={w} value={w}>Week {String(w).padStart(2, '0')}</MenuItem>
            ))}
          </Select>
        </Stack>
      </Box>

      <Box mb={1.75}>
        <Label>Age Group</Label>
        <Select size="small" fullWidth value={draft.ageGroup}
          onChange={(e) => onChange('ageGroup', e.target.value)}>
          {AGE_GROUPS.map((a) => <MenuItem key={a} value={a}>{a}</MenuItem>)}
        </Select>
      </Box>

      <Box mb={1}>
        <Label>Clinical Status</Label>
        <Select size="small" fullWidth value={draft.clinicalStatus}
          onChange={(e) => onChange('clinicalStatus', e.target.value)}>
          {CLINICAL.map((c) => <MenuItem key={c} value={c}>{c}</MenuItem>)}
        </Select>
      </Box>

      <FormControlLabel
        sx={{ my: 1 }}
        control={
          <Switch size="small" checked={draft.showPercentChange}
            onChange={(e) => onChange('showPercentChange', e.target.checked)} />
        }
        label={<Typography variant="body2">Show as % change</Typography>}
      />

      <Button fullWidth variant="contained" size="large" onClick={onApply}
        sx={{ mt: 1, borderRadius: 2 }}>
        Apply Filters{isDirty ? ' •' : ''}
      </Button>
    </Paper>
  );
}