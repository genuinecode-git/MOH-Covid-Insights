import { useMemo, useState } from 'react';
import {
  Alert, Chip, MenuItem, Paper, Select, Skeleton, Stack, Typography,
} from '@mui/material';
import { DataGrid, GridToolbar, type GridColDef } from '@mui/x-data-grid';
import { useHospitalisationCases } from '../../hooks/useHospitalisationCases';

const DEFAULTS = { from: '2023-W09', to: '2024-W08' };

const AGE_OPTIONS = [
  { value: 'All', label: 'All age groups' },
  { value: '0-11', label: '0 – 11 years' },
  { value: '12-59', label: '12 – 59 years' },
  { value: '60+', label: '60 years and above' },
];

const STATUS_OPTIONS = [
  { value: 'All', label: 'All statuses' },
  { value: 'Hospitalised', label: 'Hospitalised' },
  { value: 'ICU', label: 'ICU' },
];

export default function HospitalisationPage() {
  const [clinicalStatus, setClinicalStatus] = useState('All');
  const [ageGroup, setAgeGroup] = useState('All');

  const { data, isPending, isError, error } = useHospitalisationCases({
    ...DEFAULTS,
    clinicalStatus,
    ageGroup,
  });

  const columns = useMemo<GridColDef[]>(
    () => [
      { field: 'epiYear', headerName: 'Epi Year', width: 100, type: 'number' },
      { field: 'epiWeek', headerName: 'Epi Week', width: 110 },
      {
        field: 'weekStart',
        headerName: 'Week Of',
        width: 150,
        valueFormatter: (value: string) =>
          new Date(value).toLocaleDateString('en-SG', {
            day: 'numeric', month: 'short', year: 'numeric',
          }),
      },
      {
        field: 'clinicalStatus',
        headerName: 'Clinical Status',
        width: 150,
        renderCell: (params) => (
          <Chip
            size="small"
            label={params.value}
            color={params.value === 'ICU' ? 'error' : 'secondary'}
            variant="outlined"
          />
        ),
      },
      { field: 'ageGroupLabel', headerName: 'Age Group', flex: 1, minWidth: 170 },
      {
        field: 'averageDailyCases',
        headerName: 'Avg Daily Cases',
        width: 160,
        type: 'number',
        valueFormatter: (value: number) => value.toFixed(1),
      },
    ],
    [],
  );

  const rows = useMemo(
    () =>
      data?.rows.map((r, i) => ({
        id: `${r.epiWeek}-${r.clinicalStatus}-${r.ageGroup}-${i}`,
        ...r,
      })) ?? [],
    [data],
  );

  return (
    <Stack spacing={2}>
      <Paper sx={{ p: 2.5 }}>
        <Typography variant="h6" mb={0.5}>
          Average Daily Hospitalised / ICU Cases
        </Typography>
        <Typography variant="body2" color="text.secondary" mb={2}>
          By epi week, clinical status and age group. Source: MOH via data.gov.sg.
        </Typography>

        <Stack direction={{ xs: 'column', sm: 'row' }} spacing={1.5}>
          <Select
            size="small"
            value={clinicalStatus}
            onChange={(e) => setClinicalStatus(e.target.value)}
            sx={{ minWidth: 180 }}
          >
            {STATUS_OPTIONS.map((o) => (
              <MenuItem key={o.value} value={o.value}>{o.label}</MenuItem>
            ))}
          </Select>

          <Select
            size="small"
            value={ageGroup}
            onChange={(e) => setAgeGroup(e.target.value)}
            sx={{ minWidth: 200 }}
          >
            {AGE_OPTIONS.map((o) => (
              <MenuItem key={o.value} value={o.value}>{o.label}</MenuItem>
            ))}
          </Select>

          {data && <Chip size="small" label={`${data.rows.length} rows`} sx={{ alignSelf: 'center' }} />}
        </Stack>
      </Paper>

      {isError && (
        <Alert severity="error">{(error as Error)?.message ?? 'Failed to load data'}</Alert>
      )}

      {isPending && !data && <Skeleton variant="rounded" height={520} />}

      {data && (
        <Paper sx={{ p: 1 }}>
          <DataGrid
            rows={rows}
            columns={columns}
            density="compact"
            autoHeight
            disableRowSelectionOnClick
            initialState={{ pagination: { paginationModel: { pageSize: 25 } } }}
            pageSizeOptions={[10, 25, 50, 100]}
            slots={{ toolbar: GridToolbar }}
            slotProps={{
              toolbar: {
                showQuickFilter: true,
                csvOptions: { fileName: 'avg-hospitalised-icu-cases' },
              },
            }}
            sx={{ border: 0 }}
          />
        </Paper>
      )}
    </Stack>
  );
}