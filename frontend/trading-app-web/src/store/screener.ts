import { create } from 'zustand';
import { persist } from 'zustand/middleware';
import type { ScreenerResultDto } from '../types/api';

interface ScreenerState {
  lastResult: ScreenerResultDto | null;
  lastFilters: {
    assetType: string;
    interval: string;
    minScore: number;
    maxRsi: number | '';
    category: string;
  } | null;
  setResult: (result: ScreenerResultDto, filters: ScreenerState['lastFilters']) => void;
  clear: () => void;
}

export const useScreenerStore = create<ScreenerState>()(
  persist(
    (set) => ({
      lastResult: null,
      lastFilters: null,
      setResult: (result, filters) => set({ lastResult: result, lastFilters: filters }),
      clear: () => set({ lastResult: null, lastFilters: null }),
    }),
    { name: 'trading-app-screener' }
  )
);