import { Routes, Route } from 'react-router-dom';
import { Box, CssBaseline, ThemeProvider } from '@mui/material';
import Sidebar from './layout/Sidebar';
import TopBar from './layout/TopBar';
import Footer from './layout/Footer';
import DashboardPage from './features/dashboard/DashboardPage';
import HospitalisationPage from './features/hospitalisation/HospitalisationPage';
import { useFilters } from './hooks/useFilters';
import { useDashboardData } from './hooks/useDashboardData';
import theme from './theme/theme';

export default function App() {
  const { applied } = useFilters();
  const { data, isPending, isError, error } = useDashboardData(applied);

  return (
    <ThemeProvider theme={theme}>
      <CssBaseline />
      <Box sx={{ display: 'flex', minHeight: '100vh', bgcolor: 'background.default' }}>
        <Sidebar />
        <Box component="main" sx={{ flexGrow: 1, minWidth: 0, display: 'flex', flexDirection: 'column' }}>
          <TopBar rangeLabel={data?.rangeLabel ?? '—'} />
          <Box sx={{ flexGrow: 1, p: 3 }}>
            <Routes>
              <Route
                path="/"
                element={
                  <DashboardPage
                    data={data}
                    isPending={isPending}
                    isError={isError}
                    error={error}
                    showPercentChange={applied.showPercentChange}
                  />
                }
              />
              <Route path="/hospitalisation-cases" element={<HospitalisationPage />} />
            </Routes>
          </Box>
          <Footer lastUpdated={data?.lastUpdated ?? '—'} />
        </Box>
      </Box>
    </ThemeProvider>
  );
}