import { useEffect, useRef } from 'react';
import {
  createChart,
  CandlestickSeries,
  HistogramSeries,
  LineSeries,
  ColorType,
  LineStyle,
  type IChartApi,
  type ISeriesApi,
  type UTCTimestamp,
} from 'lightweight-charts';
import type { CandleDto, IndicatorSeriesDto } from '../types/api';

interface Props {
  candles: CandleDto[];
  series: IndicatorSeriesDto | null;
  height?: number;
  showMa20?: boolean;
  showMa50?: boolean;
  showMa200?: boolean;
  showBollinger?: boolean;
  showMacd?: boolean;
  showRsi?: boolean;
}

const toTime = (iso: string): UTCTimestamp =>
  Math.floor(new Date(iso).getTime() / 1000) as UTCTimestamp;

export default function CandlestickChart({
  candles,
  series,
  height = 720,
  showMa20 = true,
  showMa50 = true,
  showMa200 = false,
  showBollinger = false,
  showMacd = true,
  showRsi = true,
}: Props) {
  const containerRef = useRef<HTMLDivElement>(null);
  const chartRef = useRef<IChartApi | null>(null);

  const candleRef = useRef<ISeriesApi<'Candlestick'> | null>(null);
  const volumeRef = useRef<ISeriesApi<'Histogram'> | null>(null);

  const ma20Ref = useRef<ISeriesApi<'Line'> | null>(null);
  const ma50Ref = useRef<ISeriesApi<'Line'> | null>(null);
  const ma200Ref = useRef<ISeriesApi<'Line'> | null>(null);

  const bbUpperRef = useRef<ISeriesApi<'Line'> | null>(null);
  const bbMidRef = useRef<ISeriesApi<'Line'> | null>(null);
  const bbLowerRef = useRef<ISeriesApi<'Line'> | null>(null);

  const macdHistRef = useRef<ISeriesApi<'Histogram'> | null>(null);
  const macdLineRef = useRef<ISeriesApi<'Line'> | null>(null);
  const signalRef = useRef<ISeriesApi<'Line'> | null>(null);

  const rsiRef = useRef<ISeriesApi<'Line'> | null>(null);

  // ---- Chart kur ----
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
      crosshair: { mode: 0 },
    });

    chartRef.current = chart;

    // ============ PANE 0: Candle + MA + Bollinger ============
    candleRef.current = chart.addSeries(CandlestickSeries, {
      upColor: '#4ade80', downColor: '#f87171',
      borderUpColor: '#4ade80', borderDownColor: '#f87171',
      wickUpColor: '#4ade80', wickDownColor: '#f87171',
    }, 0);

    volumeRef.current = chart.addSeries(HistogramSeries, {
      color: '#334155',
      priceFormat: { type: 'volume' },
      priceScaleId: 'vol',
    }, 0);
    chart.priceScale('vol').applyOptions({ scaleMargins: { top: 0.8, bottom: 0 } });

    ma20Ref.current = chart.addSeries(LineSeries, {
      color: '#4fc3f7', lineWidth: 1, priceLineVisible: false, lastValueVisible: false,
    }, 0);

    ma50Ref.current = chart.addSeries(LineSeries, {
      color: '#fbbf24', lineWidth: 1, priceLineVisible: false, lastValueVisible: false,
    }, 0);

    ma200Ref.current = chart.addSeries(LineSeries, {
      color: '#c084fc', lineWidth: 1, priceLineVisible: false, lastValueVisible: false,
    }, 0);

    bbUpperRef.current = chart.addSeries(LineSeries, {
      color: 'rgba(139,155,180,0.5)', lineWidth: 1, lineStyle: LineStyle.Dotted,
      priceLineVisible: false, lastValueVisible: false,
    }, 0);

    bbMidRef.current = chart.addSeries(LineSeries, {
      color: 'rgba(139,155,180,0.35)', lineWidth: 1, lineStyle: LineStyle.Dashed,
      priceLineVisible: false, lastValueVisible: false,
    }, 0);

    bbLowerRef.current = chart.addSeries(LineSeries, {
      color: 'rgba(139,155,180,0.5)', lineWidth: 1, lineStyle: LineStyle.Dotted,
      priceLineVisible: false, lastValueVisible: false,
    }, 0);

    // ============ PANE 1: MACD ============
    macdHistRef.current = chart.addSeries(HistogramSeries, {
      priceFormat: { type: 'price', precision: 4, minMove: 0.0001 },
      priceLineVisible: false, lastValueVisible: false,
    }, 1);

    macdLineRef.current = chart.addSeries(LineSeries, {
      color: '#4fc3f7', lineWidth: 1, priceLineVisible: false, lastValueVisible: false,
    }, 1);

    signalRef.current = chart.addSeries(LineSeries, {
      color: '#fbbf24', lineWidth: 1, priceLineVisible: false, lastValueVisible: false,
    }, 1);

    // ============ PANE 2: RSI ============
    rsiRef.current = chart.addSeries(LineSeries, {
      color: '#c084fc', lineWidth: 1, priceLineVisible: false, lastValueVisible: true,
    }, 2);

    // Pane boyutları
    const panes = chart.panes();
    if (panes.length >= 3) {
      panes[0].setHeight(height - 320);
      panes[1].setHeight(140);
      panes[2].setHeight(120);
    }

    const ro = new ResizeObserver(() => {
      if (containerRef.current) chart.applyOptions({ width: containerRef.current.clientWidth });
    });
    ro.observe(containerRef.current);

    return () => {
      ro.disconnect();
      chart.remove();
      chartRef.current = null;
    };
  }, [height]);

  // ---- Candle verisi ----
  useEffect(() => {
    if (!candleRef.current || candles.length === 0) return;

    candleRef.current.setData(candles.map((c) => ({
      time: toTime(c.openTime),
      open: c.open, high: c.high, low: c.low, close: c.close,
    })));

    if (volumeRef.current) {
      volumeRef.current.setData(candles.map((c) => ({
        time: toTime(c.openTime),
        value: c.volume,
        color: c.close >= c.open ? 'rgba(74,222,128,0.35)' : 'rgba(248,113,113,0.35)',
      })));
    }

    chartRef.current?.timeScale().fitContent();
  }, [candles]);

  // ---- MA/BB ----
  useEffect(() => {
    if (!series) return;

    const lineData = (pts: { time: string; value: number | null }[]) =>
      pts.filter((p) => p.value !== null).map((p) => ({ time: toTime(p.time), value: p.value as number }));

    if (showMa20) ma20Ref.current?.setData(lineData(series.ma20));
    else ma20Ref.current?.setData([]);

    if (showMa50) ma50Ref.current?.setData(lineData(series.ma50));
    else ma50Ref.current?.setData([]);

    if (showMa200) ma200Ref.current?.setData(lineData(series.ma200));
    else ma200Ref.current?.setData([]);

    if (showBollinger) {
      bbUpperRef.current?.setData(lineData(series.bollingerUpper));
      bbMidRef.current?.setData(lineData(series.bollingerMiddle));
      bbLowerRef.current?.setData(lineData(series.bollingerLower));
    } else {
      bbUpperRef.current?.setData([]);
      bbMidRef.current?.setData([]);
      bbLowerRef.current?.setData([]);
    }
  }, [series, showMa20, showMa50, showMa200, showBollinger]);

  // ---- MACD ----
  useEffect(() => {
    if (!series || !showMacd) {
      macdHistRef.current?.setData([]);
      macdLineRef.current?.setData([]);
      signalRef.current?.setData([]);
      return;
    }

    macdHistRef.current?.setData(
      series.macd
        .filter((m) => m.histogram !== null)
        .map((m) => ({
          time: toTime(m.time),
          value: m.histogram as number,
          color: (m.histogram as number) >= 0 ? 'rgba(74,222,128,0.7)' : 'rgba(248,113,113,0.7)',
        }))
    );

    macdLineRef.current?.setData(
      series.macd.filter((m) => m.macd !== null).map((m) => ({ time: toTime(m.time), value: m.macd as number }))
    );

    signalRef.current?.setData(
      series.macd.filter((m) => m.signal !== null).map((m) => ({ time: toTime(m.time), value: m.signal as number }))
    );
  }, [series, showMacd]);

  // ---- RSI ----
  useEffect(() => {
    if (!series || !showRsi) {
      rsiRef.current?.setData([]);
      return;
    }

    rsiRef.current?.setData(
      series.rsi.filter((p) => p.value !== null).map((p) => ({ time: toTime(p.time), value: p.value as number }))
    );

    // 30/70 seviyeleri
    rsiRef.current?.createPriceLine({
      price: 70, color: '#f87171', lineWidth: 1, lineStyle: LineStyle.Dashed,
      axisLabelVisible: true, title: '70',
    });
    rsiRef.current?.createPriceLine({
      price: 30, color: '#4ade80', lineWidth: 1, lineStyle: LineStyle.Dashed,
      axisLabelVisible: true, title: '30',
    });
  }, [series, showRsi]);

  return <div ref={containerRef} className="w-full" />;
}