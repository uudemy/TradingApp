import { useEffect, useRef } from 'react';
import {
  createChart,
  CandlestickSeries,
  HistogramSeries,
  LineSeries,
  ColorType,
  type IChartApi,
  type ISeriesApi,
  type UTCTimestamp,
} from 'lightweight-charts';
import type { CandleDto } from '../types/api';

interface Props {
  candles: CandleDto[];
  height?: number;
  showVolume?: boolean;
  showMa20?: boolean;
  showMa50?: boolean;
}

function sma(values: number[], period: number): (number | null)[] {
  const out: (number | null)[] = new Array(values.length).fill(null);
  let sum = 0;
  for (let i = 0; i < values.length; i++) {
    sum += values[i];
    if (i >= period) sum -= values[i - period];
    if (i >= period - 1) out[i] = sum / period;
  }
  return out;
}

export default function CandlestickChart({
  candles,
  height = 420,
  showVolume = true,
  showMa20 = true,
  showMa50 = false,
}: Props) {
  const containerRef = useRef<HTMLDivElement>(null);
  const chartRef = useRef<IChartApi | null>(null);
  const candleSeriesRef = useRef<ISeriesApi<'Candlestick'> | null>(null);
  const volumeSeriesRef = useRef<ISeriesApi<'Histogram'> | null>(null);
  const ma20SeriesRef = useRef<ISeriesApi<'Line'> | null>(null);
  const ma50SeriesRef = useRef<ISeriesApi<'Line'> | null>(null);

  // Chart'ı bir kez kur
  useEffect(() => {
    if (!containerRef.current) return;

    const chart = createChart(containerRef.current, {
      height,
      layout: {
        background: { type: ColorType.Solid, color: '#111a2e' },
        textColor: '#8b9bb4',
        attributionLogo: false,
      },
      grid: {
        vertLines: { color: '#1a2540' },
        horzLines: { color: '#1a2540' },
      },
      rightPriceScale: { borderColor: '#1f2a44' },
      timeScale: { borderColor: '#1f2a44', timeVisible: true, secondsVisible: false },
      crosshair: {
        mode: 1,
      },
    });

    chartRef.current = chart;

    candleSeriesRef.current = chart.addSeries(CandlestickSeries, {
      upColor: '#4ade80',
      downColor: '#f87171',
      borderUpColor: '#4ade80',
      borderDownColor: '#f87171',
      wickUpColor: '#4ade80',
      wickDownColor: '#f87171',
    });

    if (showVolume) {
      volumeSeriesRef.current = chart.addSeries(HistogramSeries, {
        color: '#334155',
        priceFormat: { type: 'volume' },
        priceScaleId: 'vol',
      });
      chart.priceScale('vol').applyOptions({
        scaleMargins: { top: 0.8, bottom: 0 },
      });
    }

    if (showMa20) {
      ma20SeriesRef.current = chart.addSeries(LineSeries, {
        color: '#4fc3f7',
        lineWidth: 1,
        priceLineVisible: false,
        lastValueVisible: false,
      });
    }

    if (showMa50) {
      ma50SeriesRef.current = chart.addSeries(LineSeries, {
        color: '#fbbf24',
        lineWidth: 1,
        priceLineVisible: false,
        lastValueVisible: false,
      });
    }

    const resizeObserver = new ResizeObserver(() => {
      if (containerRef.current) {
        chart.applyOptions({ width: containerRef.current.clientWidth });
      }
    });
    resizeObserver.observe(containerRef.current);

    return () => {
      resizeObserver.disconnect();
      chart.remove();
      chartRef.current = null;
      candleSeriesRef.current = null;
      volumeSeriesRef.current = null;
      ma20SeriesRef.current = null;
      ma50SeriesRef.current = null;
    };
  }, [height, showVolume, showMa20, showMa50]);

  // Candle verisi güncelle
  useEffect(() => {
    if (!candleSeriesRef.current || candles.length === 0) return;

    const candleData = candles.map((c) => ({
      time: Math.floor(new Date(c.openTime).getTime() / 1000) as UTCTimestamp,
      open: c.open,
      high: c.high,
      low: c.low,
      close: c.close,
    }));

    candleSeriesRef.current.setData(candleData);

    if (volumeSeriesRef.current) {
      const volumeData = candles.map((c) => ({
        time: Math.floor(new Date(c.openTime).getTime() / 1000) as UTCTimestamp,
        value: c.volume,
        color: c.close >= c.open ? 'rgba(74,222,128,0.4)' : 'rgba(248,113,113,0.4)',
      }));
      volumeSeriesRef.current.setData(volumeData);
    }

    if (ma20SeriesRef.current) {
      const closes = candles.map((c) => c.close);
      const ma = sma(closes, 20);
      ma20SeriesRef.current.setData(
        candles
          .map((c, i) => ({
            time: Math.floor(new Date(c.openTime).getTime() / 1000) as UTCTimestamp,
            value: ma[i],
          }))
          .filter((p) => p.value !== null) as any
      );
    }

    if (ma50SeriesRef.current) {
      const closes = candles.map((c) => c.close);
      const ma = sma(closes, 50);
      ma50SeriesRef.current.setData(
        candles
          .map((c, i) => ({
            time: Math.floor(new Date(c.openTime).getTime() / 1000) as UTCTimestamp,
            value: ma[i],
          }))
          .filter((p) => p.value !== null) as any
      );
    }

    chartRef.current?.timeScale().fitContent();
  }, [candles]);

  return <div ref={containerRef} className="w-full" />;
}