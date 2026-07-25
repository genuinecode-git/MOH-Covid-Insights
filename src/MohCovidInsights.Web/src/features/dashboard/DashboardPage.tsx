import { Alert, Box, Skeleton } from '@mui/material';
import KpiRow from './components/KpiRow';
import ChartCard from './components/ChartCard';
import InfectionsChart from './components/InfectionsChart';
import HealthcarePressureChart from './components/HealthcarePressureChart';
import IcuBedBreakdown from './components/IcuBedBreakdown';
import InsightsPanel from './components/InsightsPanel';
import DataCoverageTable from './components/DataCoverageTable';
import { palette } from '../../theme/theme';
import type { DashboardData } from '../../types';

interface Props {
  data?: DashboardData;
  isPending: boolean;
  isError: boolean;
  error: unknown;
  showPercentChange: boolean;
}

export default function DashboardPage({ data, isPending, isError, error }: Props) {
  if (isError) {
    return <Alert severity="error">{(error as Error)?.message ?? 'Failed to load dashboard'}</Alert>;
  }

  const loading = isPending || !data;

  return (
    <Box sx={{ display: 'flex', flexDirection: 'column', gap: 2 }}>
      <KpiRow kpis={data?.kpis} loading={loading} />

      <Box sx={{ display: 'grid', gap: 2, gridTemplateColumns: { xs: '1fr', lg: '1fr 1fr' } }}>
        {loading ? (
          <>
            <Skeleton variant="rounded" height={400} />
            <Skeleton variant="rounded" height={400} />
          </>
        ) : (
          <>
            <ChartCard
              title="Infections, Hospital & ICU Admissions (Weekly)"
              hint="Left axis: estimated infections. Right axis: admissions."
              height={300}
              series={[
                { label: 'Estimated Infections', color: palette.infections },
                { label: 'Hospital Admissions', color: palette.hospital },
                { label: 'ICU Admissions', color: palette.icu },
              ]}
            >
              <InfectionsChart data={data.weekly} />
            </ChartCard>

            <ChartCard
              title="Healthcare Pressure (Weekly)"
              height={300}
              series={[
                { label: 'Avg Hospitalised Cases', color: palette.hospitalisedAvg },
                { label: 'Avg ICU Cases', color: palette.icuAvg },
                { label: 'ICU Utilisation %', color: palette.utilisation },
              ]}
            >
              <HealthcarePressureChart data={data.weekly} />
            </ChartCard>
          </>
        )}
      </Box>

      <Box sx={{
        display: 'grid', gap: 2,
        gridTemplateColumns: { xs: '1fr', md: '1fr 1fr', xl: '1.15fr 1fr 1.5fr' },
      }}>
        {loading ? (
          <>
            <Skeleton variant="rounded" height={300} />
            <Skeleton variant="rounded" height={300} />
            <Skeleton variant="rounded" height={300} />
          </>
        ) : (
          <>
            <IcuBedBreakdown data={data.bedUtilisation} />
            <InsightsPanel insights={data.insights} />
            <DataCoverageTable rows={data.coverage} />
          </>
        )}
      </Box>
    </Box>
  );
}