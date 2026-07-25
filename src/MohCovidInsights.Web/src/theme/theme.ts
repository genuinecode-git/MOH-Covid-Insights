import { createTheme, alpha } from '@mui/material/styles';

export const palette = {
  infections: '#3B82F6',
  hospital: '#8B5CF6',
  icu: '#F43F5E',
  utilisation: '#F59E0B',
  hospitalisedAvg: '#60A5FA',
  icuAvg: '#8B5CF6',
} as const;

const theme = createTheme({
  cssVariables: true,
  palette: {
    mode: 'light',
    primary: { main: '#4353E8', light: '#EEF1FE', dark: '#2E3BB8' },
    secondary: { main: '#8B5CF6' },
    success: { main: '#16A34A' },
    warning: { main: '#F59E0B' },
    error: { main: '#F43F5E' },
    background: { default: '#F6F7FB', paper: '#FFFFFF' },
    text: { primary: '#1A1D2B', secondary: '#6B7185' },
    divider: '#EDEFF5',
  },
  shape: { borderRadius: 12 },
  typography: {
    fontFamily: '"Inter", system-ui, -apple-system, "Segoe UI", sans-serif',
    h6: { fontSize: '1rem', fontWeight: 600 },
    subtitle2: { fontSize: '0.8125rem', fontWeight: 600 },
    body2: { fontSize: '0.8125rem' },
    caption: { fontSize: '0.75rem' },
  },
  components: {
    MuiPaper: {
      defaultProps: { elevation: 0 },
      styleOverrides: {
        root: ({ theme: t }) => ({
          border: `1px solid ${t.palette.divider}`,
          borderRadius: 14,
        }),
      },
    },
    MuiButton: {
      defaultProps: { disableElevation: true },
      styleOverrides: { root: { textTransform: 'none', fontWeight: 600 } },
    },
    MuiOutlinedInput: {
      styleOverrides: {
        root: ({ theme: t }) => ({
          backgroundColor: t.palette.background.paper,
          fontSize: '0.8125rem',
        }),
      },
    },
    MuiListItemButton: {
      styleOverrides: {
        root: ({ theme: t }) => ({
          borderRadius: 10,
          marginBottom: 2,
          '&.Mui-selected': {
            backgroundColor: t.palette.primary.light,
            color: t.palette.primary.main,
            '& .MuiListItemIcon-root': { color: t.palette.primary.main },
            '&:hover': { backgroundColor: alpha(t.palette.primary.main, 0.12) },
          },
        }),
      },
    },
    MuiListItemIcon: { styleOverrides: { root: { minWidth: 34, color: '#8A90A2' } } },
    MuiChip: { styleOverrides: { root: { fontWeight: 500 } } },
  },
});

export default theme;