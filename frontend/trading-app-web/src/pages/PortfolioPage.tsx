import { useEffect, useState, useCallback } from 'react';
import { Link } from 'react-router-dom';
import { useAuthStore } from '../store/auth';
import { api } from '../lib/api';
import type { PortfolioSummaryDto, PositionDto } from '../types/api';
import { useMarketHub, type PriceUpdate } from '../hooks/useMarketHub';

const fmt = (n: number | null | undefined, d = 2) =>
  n == null ? '—' : n.toLocaleString('tr-TR', { minimumFractionDigits: d, maximumFractionDigits: d });

export default function PortfolioPage() {
  const token = useAuthStore((s) => s.accessToken);
  const [summary, setSummary] = useState<PortfolioSummaryDto | null>(null);
  const [loading, setLoading] = useState(true);
  const [showAdd, setShowAdd] = useState(false);

  const onUpdate = useCallback((_u: PriceUpdate) => {
    // Fiyat gelince portföyü yeniden çek (basit yaklaşım)
    api.get<PortfolioSummaryDto>('/api/portfolio', token).then(setSummary);
  }, [token]);
  useMarketHub(onUpdate);

  const load = useCallback(async () => {
    setLoading(true);
    try {
      const data = await api.get<PortfolioSummaryDto>('/api/portfolio', token);
      setSummary(data);
    } finally {
      setLoading(false);
    }
  }, [token]);

  useEffect(() => { load(); }, [load]);

  const handleDelete = async (id: string) => {
    if (!confirm('Bu pozisyonu silmek istediğine emin misin?')) return;
    await api.delete(`/api/portfolio/positions/${id}`, token);
    await load();
  };

  if (loading) return <div className="p-8 text-[#8b9bb4]">Yükleniyor...</div>;
  if (!summary) return <div className="p-8 text-[#f87171]">Portföy yüklenemedi</div>;

  const totalPnl = summary.totalUnrealizedPnl;

  return (
    <div className="p-8 space-y-6">
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-2xl font-bold text-white">Portföyüm</h1>
          <p className="text-sm text-[#8b9bb4] mt-1">{summary.positions.length} pozisyon</p>
        </div>
        <button
          onClick={() => setShowAdd(true)}
          className="px-5 py-2 rounded-lg bg-gradient-to-r from-[#4fc3f7] to-[#7c4dff] text-white text-sm font-semibold"
        >
          + Pozisyon Ekle
        </button>
      </div>

      {/* Özet kartları */}
      <div className="grid grid-cols-1 md:grid-cols-4 gap-4">
        <Card title="Portföy Değeri" value={`${fmt(summary.totalMarketValue)} ₺`} />
        <Card
          title="Toplam K/Z"
          value={`${fmt(totalPnl)} ₺`}
          sub={`${fmt(summary.totalUnrealizedPnlPercent)}%`}
          positive={totalPnl >= 0}
        />
        <Card
          title="Günlük K/Z"
          value={`${fmt(summary.totalDailyPnl)} ₺`}
          positive={summary.totalDailyPnl >= 0}
        />
        <Card title="Maliyet" value={`${fmt(summary.totalCostBasis)} ₺`} />
      </div>

      {/* Cash bakiyeler */}
      <div>
        <h2 className="text-sm font-semibold text-white mb-3">Nakit Bakiyeler</h2>
        <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
          {summary.cashBalances.map((c) => (
            <div key={c.currency} className="bg-[#111a2e] border border-[#1f2a44] rounded-lg p-4">
              <div className="flex items-center justify-between">
                <span className="text-xs text-[#8b9bb4]">{c.currency}</span>
                <span className="text-xs num text-white">{fmt(c.total)}</span>
              </div>
              <div className="mt-2 flex justify-between text-xs">
                <span className="text-[#8b9bb4]">Kullanılabilir</span>
                <span className="num text-[#4ade80]">{fmt(c.available)}</span>
              </div>
              {c.locked > 0 && (
                <div className="flex justify-between text-xs mt-1">
                  <span className="text-[#8b9bb4]">Kilitli</span>
                  <span className="num text-[#fbbf24]">{fmt(c.locked)}</span>
                </div>
              )}
            </div>
          ))}
        </div>
      </div>

      {/* Pozisyonlar */}
      {summary.positions.length > 0 && (
        <div className="bg-[#111a2e] border border-[#1f2a44] rounded-lg overflow-hidden">
          <table className="w-full">
            <thead className="bg-[#0f1729]">
              <tr className="text-xs text-[#8b9bb4] uppercase">
                <th className="px-5 py-3 text-left">Sembol</th>
                <th className="px-5 py-3 text-right">Adet</th>
                <th className="px-5 py-3 text-right">Ort. Maliyet</th>
                <th className="px-5 py-3 text-right">Fiyat</th>
                <th className="px-5 py-3 text-right">Değer</th>
                <th className="px-5 py-3 text-right">K/Z</th>
                <th className="px-5 py-3 text-right">K/Z %</th>
                <th className="px-5 py-3 text-right">Günlük</th>
                <th className="px-5 py-3"></th>
              </tr>
            </thead>
            <tbody>
              {summary.positions.map((p) => (
                <PositionRow key={p.id} p={p} onDelete={handleDelete} />
              ))}
            </tbody>
          </table>
        </div>
      )}

      {showAdd && (
        <AddPositionModal
          token={token}
          onClose={() => setShowAdd(false)}
          onSuccess={async () => { setShowAdd(false); await load(); }}
        />
      )}
    </div>
  );
}

function Card({ title, value, sub, positive }: { title: string; value: string; sub?: string; positive?: boolean }) {
  const color = positive === undefined ? 'text-white' : positive ? 'text-[#4ade80]' : 'text-[#f87171]';
  return (
    <div className="bg-[#111a2e] border border-[#1f2a44] rounded-lg p-5">
      <div className="text-xs text-[#8b9bb4]">{title}</div>
      <div className={`text-xl font-bold mt-1 num ${color}`}>{value}</div>
      {sub && <div className={`text-xs mt-1 num ${color}`}>{sub}</div>}
    </div>
  );
}

function PositionRow({ p, onDelete }: { p: PositionDto; onDelete: (id: string) => void }) {
  const positive = p.unrealizedPnl >= 0;
  return (
    <tr className="border-t border-[#131c30] hover:bg-[#172239]">
      <td className="px-5 py-3 text-sm font-semibold">
        <Link to={`/trading/${p.symbol}`} className="text-white hover:text-[#4fc3f7]">
          {p.symbol}
        </Link>
      </td>
      <td className="px-5 py-3 text-sm num text-right text-white">{fmt(p.quantity, 4)}</td>
      <td className="px-5 py-3 text-sm num text-right text-[#8b9bb4]">{fmt(p.averageCost, 4)}</td>
      <td className="px-5 py-3 text-sm num text-right text-white">{fmt(p.currentPrice, 4)}</td>
      <td className="px-5 py-3 text-sm num text-right text-white">{fmt(p.marketValue)}</td>
      <td className={`px-5 py-3 text-sm num text-right ${positive ? 'text-[#4ade80]' : 'text-[#f87171]'}`}>
        {positive ? '+' : ''}{fmt(p.unrealizedPnl)}
      </td>
      <td className={`px-5 py-3 text-sm num text-right ${positive ? 'text-[#4ade80]' : 'text-[#f87171]'}`}>
        {positive ? '+' : ''}{fmt(p.unrealizedPnlPercent)}%
      </td>
      <td className={`px-5 py-3 text-sm num text-right ${p.dailyPnl >= 0 ? 'text-[#4ade80]' : 'text-[#f87171]'}`}>
        {p.dailyPnl >= 0 ? '+' : ''}{fmt(p.dailyPnl)}
      </td>
      <td className="px-5 py-3 text-right">
        <button onClick={() => onDelete(p.id)} className="text-xs text-[#f87171] hover:underline">Sil</button>
      </td>
    </tr>
  );
}

function AddPositionModal({ token, onClose, onSuccess }: { token: string | null; onClose: () => void; onSuccess: () => void }) {
  const [symbol, setSymbol] = useState('THYAO');
  const [quantity, setQuantity] = useState('100');
  const [averageCost, setAverageCost] = useState('100');
  const [notes, setNotes] = useState('');
  const [error, setError] = useState<string | null>(null);
  const [saving, setSaving] = useState(false);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError(null);
    setSaving(true);
    try {
      await api.post('/api/portfolio/positions', {
        symbol: symbol.trim().toUpperCase(),
        quantity: parseFloat(quantity),
        averageCost: parseFloat(averageCost),
        purchaseDate: new Date().toISOString(),
        notes: notes || null,
      }, token);
      onSuccess();
    } catch (e: any) {
      setError(e.apiMessage || 'Kaydedilemedi');
    } finally {
      setSaving(false);
    }
  };

  const inputClass = "w-full px-3 py-2 rounded-lg bg-[#0b1220] border border-[#1f2a44] text-white focus:border-[#4fc3f7] focus:outline-none";

  return (
    <div className="fixed inset-0 bg-black/60 flex items-center justify-center z-50 p-4">
      <div className="bg-[#111a2e] border border-[#1f2a44] rounded-xl w-full max-w-md p-6">
        <h2 className="text-lg font-bold text-white mb-4">Pozisyon Ekle</h2>

        {error && <div className="mb-3 px-3 py-2 rounded-lg bg-[#2a1a1a] text-[#f87171] text-sm">{error}</div>}

        <form onSubmit={handleSubmit} className="space-y-4">
          <div>
            <label className="block text-xs text-[#8b9bb4] mb-1">Sembol</label>
            <input value={symbol} onChange={(e) => setSymbol(e.target.value.toUpperCase())} required className={inputClass} />
          </div>
          <div className="grid grid-cols-2 gap-3">
            <div>
              <label className="block text-xs text-[#8b9bb4] mb-1">Adet</label>
              <input type="number" step="any" value={quantity} onChange={(e) => setQuantity(e.target.value)} required className={inputClass} />
            </div>
            <div>
              <label className="block text-xs text-[#8b9bb4] mb-1">Ortalama Maliyet</label>
              <input type="number" step="any" value={averageCost} onChange={(e) => setAverageCost(e.target.value)} required className={inputClass} />
            </div>
          </div>
          <div>
            <label className="block text-xs text-[#8b9bb4] mb-1">Not (opsiyonel)</label>
            <textarea value={notes} onChange={(e) => setNotes(e.target.value)} rows={2} className={inputClass} />
          </div>
          <div className="flex gap-3 pt-2">
            <button type="button" onClick={onClose} className="flex-1 py-2 rounded-lg border border-[#1f2a44] text-[#8b9bb4]">İptal</button>
            <button type="submit" disabled={saving} className="flex-1 py-2 rounded-lg bg-gradient-to-r from-[#4fc3f7] to-[#7c4dff] text-white font-semibold disabled:opacity-50">
              {saving ? 'Kaydediliyor...' : 'Kaydet'}
            </button>
          </div>
        </form>
      </div>
    </div>
  );
}