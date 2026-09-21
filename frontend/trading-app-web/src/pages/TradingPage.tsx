import { useEffect, useState } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import { useAuthStore } from '../store/auth';
import { api } from '../lib/api';
import type { AssetDto, CandleDto } from '../types/api';
import CandlestickChart from '../components/CandlestickChart';
import AnalysisPanel from '../components/AnalysisPanel';
import { useMarketHub } from '../hooks/useMarketHub';

interface AnalysisResponse {
  assetId: string;
  symbol: string;
  name: string;
  currentPrice: number;
  changePercent: number;
  rsi14: number | null;
  macdHistogram: number | null;
  ma20: number | null;
  ma50: number | null;
  ma200: number | null;
  volumeRatio: number | null;
  ceilingScore: number;
  ceilingCategory: string;
  signals: any[];
}

const INTERVALS = ['1m', '5m', '15m', '1h', '4h', '1D', '1W'];

const fmt = (n: number | null | undefined, d = 2) =>
  n == null ? '—' : n.toLocaleString('tr-TR', { minimumFractionDigits: d, maximumFractionDigits: d });

export default function TradingPage() {
  const { symbol } = useParams<{ symbol: string }>();
  const navigate = useNavigate();
  const token = useAuthStore((s) => s.accessToken);

  const [asset, setAsset] = useState<AssetDto | null>(null);
  const [livePrice, setLivePrice] = useState<number | null>(null);
  const [candles, setCandles] = useState<CandleDto[]>([]);
  const [analysis, setAnalysis] = useState<AnalysisResponse | null>(null);
  const [interval, setInterval] = useState('1D');
  const [loading, setLoading] = useState(true);

  const { connected } = useMarketHub((u) => {
    if (u.symbol === symbol?.toUpperCase()) setLivePrice(u.price);
  });

  // Asset + analysis yükle
  useEffect(() => {
    if (!symbol) return;
    setLoading(true);
    Promise.all([
      api.get<AssetDto>(`/api/assets/${symbol}`, token),
      api.get<AnalysisResponse>(`/api/analysis/${symbol}?interval=1D&limit=200`, token).catch(() => null),
    ])
      .then(([a, an]) => {
        setAsset(a);
        setAnalysis(an);
      })
      .finally(() => setLoading(false));
  }, [symbol, token]);

  // Candle'ları yükle
  useEffect(() => {
    if (!symbol) return;
    api.get<CandleDto[]>(`/api/market/${symbol}/candles?interval=${interval}&limit=300`, token)
      .then((c) => setCandles(c || []))
      .catch((e) => console.error('Candles error:', e));
  }, [symbol, interval, token]);

  if (loading) return <div className="p-8 text-[#8b9bb4]">Yükleniyor...</div>;
  if (!asset) return <div className="p-8 text-[#f87171]">Sembol bulunamadı</div>;

  const price = livePrice ?? asset.currentPrice;
  const changePercent = asset.previousClose
    ? ((price - asset.previousClose) / asset.previousClose) * 100
    : 0;
  const positive = changePercent >= 0;

  return (
    <div className="p-8 space-y-6">
      {/* Header */}
      <div className="flex items-start justify-between">
        <div>
          <button
            onClick={() => navigate('/markets')}
            className="text-xs text-[#8b9bb4] hover:text-white mb-2"
          >
            ← Piyasalar
          </button>
          <div className="flex items-baseline gap-4">
            <h1 className="text-3xl font-bold text-white">{asset.symbol}</h1>
            <span className="text-lg text-[#8b9bb4]">{asset.name}</span>
            <span className="text-xs px-2 py-0.5 rounded bg-[#1f2a44] text-[#8b9bb4]">
              {asset.assetType} · {asset.currency}
            </span>
          </div>
        </div>

        <div className="text-right">
          <div className="flex items-center gap-2 justify-end">
            <span className={`text-3xl font-bold num ${positive ? 'text-[#4ade80]' : 'text-[#f87171]'}`}>
              {fmt(price, 4)}
            </span>
            <span className={`text-sm num ${positive ? 'text-[#4ade80]' : 'text-[#f87171]'}`}>
              {positive ? '+' : ''}{fmt(changePercent)}%
            </span>
          </div>
          <div className="flex items-center justify-end gap-2 mt-1">
            <span className={`w-2 h-2 rounded-full ${connected ? 'bg-[#4ade80] animate-pulse' : 'bg-[#f87171]'}`} />
            <span className="text-xs text-[#8b9bb4]">
              {connected ? 'Canlı' : 'Bağlantı yok'}
            </span>
          </div>
        </div>
      </div>

      {/* Grid: Chart + Analysis */}
      <div className="grid grid-cols-1 lg:grid-cols-[1fr_320px] gap-6">
        <div className="space-y-4">
          {/* Interval seçici */}
          <div className="flex gap-1 bg-[#111a2e] border border-[#1f2a44] rounded-lg p-1 w-fit">
            {INTERVALS.map((iv) => (
              <button
                key={iv}
                onClick={() => setInterval(iv)}
                className={`px-3 py-1.5 text-xs font-medium rounded-md transition ${
                  interval === iv ? 'bg-[#1d2b48] text-white' : 'text-[#8b9bb4] hover:text-white'
                }`}
              >
                {iv}
              </button>
            ))}
          </div>

          {/* Chart */}
          <div className="bg-[#111a2e] border border-[#1f2a44] rounded-lg overflow-hidden p-3">
            {candles.length > 0 ? (
              <CandlestickChart candles={candles} showVolume showMa20 />
            ) : (
              <div className="h-[420px] flex items-center justify-center text-[#8b9bb4]">
                Mum verisi yükleniyor...
              </div>
            )}
          </div>
        </div>

        {/* Analysis */}
        {analysis ? (
          <AnalysisPanel
            score={analysis.ceilingScore}
            category={analysis.ceilingCategory}
            rsi14={analysis.rsi14}
            macdHistogram={analysis.macdHistogram}
            ma20={analysis.ma20}
            ma50={analysis.ma50}
            volumeRatio={analysis.volumeRatio}
            signals={analysis.signals}
          />
        ) : (
          <div className="bg-[#111a2e] border border-[#1f2a44] rounded-lg p-5 text-[#8b9bb4] text-sm">
            Analiz yüklenemedi
          </div>
        )}
      </div>
    </div>
  );
}